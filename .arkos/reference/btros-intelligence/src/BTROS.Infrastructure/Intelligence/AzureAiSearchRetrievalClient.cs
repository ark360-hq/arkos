using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using BTROS.Domain.Intelligence;

namespace BTROS.Infrastructure.Intelligence;

/// <summary>
/// SPEC-0207 / ADR-0050: Azure AI Search documents-search over HttpClient. Fail-closed on tenant,
/// region, host, and site scope. Never logs query text or document content.
/// </summary>
public sealed class AzureAiSearchRetrievalClient : IRetrievalClient
{
    public const string HttpClientName = nameof(AzureAiSearchRetrievalClient);
    private const string SearchScope = "https://search.azure.com/.default";
    private const string SelectFields = "id,siteId,title,content,source";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AzureAiSearchOptions _options;
    private readonly ILogger<AzureAiSearchRetrievalClient> _logger;
    private TokenCredential? _credential;

    public AzureAiSearchRetrievalClient(
        IHttpClientFactory httpClientFactory,
        AzureAiSearchOptions options,
        ILogger<AzureAiSearchRetrievalClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<RetrievalResult> SearchAsync(
        RetrievalQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.SearchText))
        {
            throw new ArgumentException("Search text is required.", nameof(query));
        }

        AzureAiSearchGuard.EnsureCanSend(
            _options.Endpoint,
            _options.IndexName,
            _options.Region,
            _options.TenantMode,
            query.SiteId);

        if (!AzureAiSearchGuard.TryGetAllowedEndpoint(_options.Endpoint, out var endpoint) || endpoint is null)
        {
            throw new RetrievalException(
                RetrievalFailureKind.InvalidEndpoint,
                "Azure AI Search Endpoint must be an https://*.search.windows.net/ resource URL.");
        }

        var top = AzureAiSearchGuard.ClampTop(query.Top);
        var uri = new Uri(
            endpoint,
            $"indexes/{Uri.EscapeDataString(_options.IndexName)}/docs/search?api-version={Uri.EscapeDataString(_options.ApiVersion)}");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(BuildBody(query, top), options: JsonOptions),
        };
        await ApplyAuthAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        var client = _httpClientFactory.CreateClient(HttpClientName);
        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or AuthenticationFailedException)
        {
            _logger.LogWarning(
                ex,
                "Azure AI Search request failed region={Region} index={Index}",
                _options.Region,
                _options.IndexName);
            throw new RetrievalException(
                RetrievalFailureKind.RequestFailed,
                "Azure AI Search request failed; no documents were retrieved.",
                ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Azure AI Search query failed region={Region} index={Index} status={Status}",
                    _options.Region,
                    _options.IndexName,
                    (int)response.StatusCode);
                throw new RetrievalException(
                    RetrievalFailureKind.RequestFailed,
                    $"Azure AI Search request failed with HTTP {(int)response.StatusCode}.");
            }

            AzureSearchEnvelope? envelope;
            try
            {
                envelope = await response.Content
                    .ReadFromJsonAsync<AzureSearchEnvelope>(JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (JsonException ex)
            {
                throw new RetrievalException(
                    RetrievalFailureKind.RequestFailed,
                    "Azure AI Search returned an invalid response envelope.",
                    ex);
            }

            var chunks = (envelope?.Value ?? [])
                .Select(hit => MapHit(hit, query.SiteId))
                .OfType<RetrievedChunk>()
                .ToArray();

            _logger.LogInformation(
                "Azure AI Search query succeeded region={Region} index={Index} status={Status} hits={Hits}",
                _options.Region,
                _options.IndexName,
                (int)response.StatusCode,
                chunks.Length);

            return new RetrievalResult(query.SiteId, query.SearchText, chunks);
        }
    }

    private SearchRequestBody BuildBody(RetrievalQuery query, int top)
    {
        IReadOnlyList<VectorQueryBody>? vectors = null;
        if (query.Vector is { Count: > 0 })
        {
            vectors =
            [
                new VectorQueryBody("vector", query.Vector, _options.VectorField, top),
            ];
        }

        return new SearchRequestBody(
            query.SearchText,
            AzureAiSearchGuard.SiteFilter(query.SiteId),
            top,
            SelectFields,
            vectors);
    }

    private static RetrievedChunk? MapHit(AzureSearchHit hit, Guid expectedSiteId)
    {
        if (string.IsNullOrWhiteSpace(hit.Id)
            || !Guid.TryParse(hit.SiteId, out var siteId)
            || siteId != expectedSiteId)
        {
            return null;
        }

        return new RetrievedChunk(
            hit.Id,
            siteId,
            hit.Title ?? string.Empty,
            hit.Content ?? string.Empty,
            hit.Source ?? string.Empty,
            hit.Score ?? 0);
    }

    private async Task ApplyAuthAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.Add("api-key", _options.ApiKey);
            return;
        }

        try
        {
            _credential ??= new DefaultAzureCredential();
            var token = await _credential.GetTokenAsync(
                new TokenRequestContext([SearchScope]),
                cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }
        catch (AuthenticationFailedException ex)
        {
            throw new RetrievalException(
                RetrievalFailureKind.RequestFailed,
                "Azure AI Search authentication failed; no documents were retrieved.",
                ex);
        }
    }

    private sealed record SearchRequestBody(
        [property: JsonPropertyName("search")] string Search,
        [property: JsonPropertyName("filter")] string Filter,
        [property: JsonPropertyName("top")] int Top,
        [property: JsonPropertyName("select")] string Select,
        [property: JsonPropertyName("vectorQueries")] IReadOnlyList<VectorQueryBody>? VectorQueries);

    private sealed record VectorQueryBody(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("vector")] IReadOnlyList<float> Vector,
        [property: JsonPropertyName("fields")] string Fields,
        [property: JsonPropertyName("k")] int K);

    private sealed record AzureSearchEnvelope(
        [property: JsonPropertyName("value")] IReadOnlyList<AzureSearchHit>? Value);

    private sealed record AzureSearchHit(
        [property: JsonPropertyName("@search.score")] double? Score,
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("siteId")] string? SiteId,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("source")] string? Source);
}
