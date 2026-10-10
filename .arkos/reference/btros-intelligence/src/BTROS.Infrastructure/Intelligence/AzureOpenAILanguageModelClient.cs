using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using BTROS.Domain.Intelligence;

namespace BTROS.Infrastructure.Intelligence;

/// <summary>
/// SPEC-0206 / ADR-0049: Azure OpenAI Chat Completions over HttpClient. Fail-closed on tenant,
/// region, deployment type, and host. Never logs prompt or completion text.
/// </summary>
public sealed class AzureOpenAILanguageModelClient : ILanguageModelClient
{
    public const string HttpClientName = nameof(AzureOpenAILanguageModelClient);
    private const string CognitiveServicesScope = "https://cognitiveservices.azure.com/.default";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AzureOpenAIOptions _options;
    private readonly ILogger<AzureOpenAILanguageModelClient> _logger;
    private TokenCredential? _credential;

    public AzureOpenAILanguageModelClient(
        IHttpClientFactory httpClientFactory,
        AzureOpenAIOptions options,
        ILogger<AzureOpenAILanguageModelClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<LanguageModelCompletion> CompleteAsync(
        LanguageModelRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Messages is null || request.Messages.Count == 0)
        {
            throw new ArgumentException("At least one message is required.", nameof(request));
        }

        AzureOpenAIGuard.EnsureCanSend(
            _options.Endpoint,
            _options.Deployment,
            _options.Region,
            _options.TenantMode,
            _options.DeploymentType);

        if (!AzureOpenAIGuard.TryGetAllowedEndpoint(_options.Endpoint, out var endpoint) || endpoint is null)
        {
            throw new LanguageModelException(
                LanguageModelFailureKind.InvalidEndpoint,
                "Azure OpenAI Endpoint must be an https://*.openai.azure.com/ resource URL.");
        }

        var uri = new Uri(
            endpoint,
            $"openai/deployments/{Uri.EscapeDataString(_options.Deployment)}/chat/completions?api-version={Uri.EscapeDataString(_options.ApiVersion)}");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(BuildBody(request), options: JsonOptions),
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
                "Azure OpenAI request failed region={Region} deployment={Deployment}",
                _options.Region,
                _options.Deployment);
            throw new LanguageModelException(
                LanguageModelFailureKind.RequestFailed,
                "Azure OpenAI request failed; no completion was produced.",
                ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Azure OpenAI chat completion failed region={Region} deployment={Deployment} status={Status}",
                    _options.Region,
                    _options.Deployment,
                    (int)response.StatusCode);
                throw new LanguageModelException(
                    LanguageModelFailureKind.RequestFailed,
                    $"Azure OpenAI request failed with HTTP {(int)response.StatusCode}.");
            }

            AzureChatCompletion? completion;
            try
            {
                completion = await response.Content
                    .ReadFromJsonAsync<AzureChatCompletion>(JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (JsonException ex)
            {
                throw new LanguageModelException(
                    LanguageModelFailureKind.RequestFailed,
                    "Azure OpenAI returned an invalid response envelope.",
                    ex);
            }

            var content = completion?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new LanguageModelException(
                    LanguageModelFailureKind.RequestFailed,
                    "Azure OpenAI returned no completion content.");
            }

            _logger.LogInformation(
                "Azure OpenAI chat completion succeeded region={Region} deployment={Deployment} status={Status}",
                _options.Region,
                _options.Deployment,
                (int)response.StatusCode);

            return new LanguageModelCompletion(content, _options.Deployment, _options.Region);
        }
    }

    private static object BuildBody(LanguageModelRequest request)
    {
        var messages = request.Messages.Select(m => new { role = m.Role, content = m.Content }).ToArray();
        if (request.MaxTokens is null && request.Temperature is null)
        {
            return new { messages };
        }

        if (request.MaxTokens is not null && request.Temperature is not null)
        {
            return new { messages, max_tokens = request.MaxTokens, temperature = request.Temperature };
        }

        if (request.MaxTokens is not null)
        {
            return new { messages, max_tokens = request.MaxTokens };
        }

        return new { messages, temperature = request.Temperature };
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
                new TokenRequestContext([CognitiveServicesScope]),
                cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }
        catch (AuthenticationFailedException ex)
        {
            throw new LanguageModelException(
                LanguageModelFailureKind.RequestFailed,
                "Azure OpenAI authentication failed; no completion was produced.",
                ex);
        }
    }

    private sealed record AzureChatCompletion(
        [property: JsonPropertyName("choices")] IReadOnlyList<AzureChatChoice>? Choices);

    private sealed record AzureChatChoice(
        [property: JsonPropertyName("message")] AzureChatMessage? Message);

    private sealed record AzureChatMessage(
        [property: JsonPropertyName("content")] string? Content);
}
