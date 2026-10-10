using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Arkos.Intelligence;
using Arkos.Intelligence.Infrastructure;

namespace Arkos.Intelligence.Tests;

/// <summary>SPEC-0012: Azure AI Search RAG: client tenant, AU region, site-scoped, fail-closed.</summary>
public sealed class AzureAiSearchRagTests
{
    private const string AuEndpoint = "https://customer-search.search.windows.net/";
    private const string SecretQuery = "lease for unit 12B, resident Jane Example";
    private static readonly Guid SiteA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SiteB = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Theory] // REQ-004
    [InlineData("australiaeast")]
    [InlineData("australiasoutheast")]
    [InlineData("AustraliaEast")]
    public void Allowed_australian_regions_are_accepted(string region)
    {
        AzureAiSearchGuard.EnsureCanSend(AuEndpoint, "site-docs", region, "Client", SiteA);
        Assert.True(AzureAiSearchGuard.IsAllowedRegion(region));
    }

    [Theory] // REQ-004 / REQ-008
    [InlineData("eastus")]
    [InlineData("westeurope")]
    [InlineData("westus2")]
    [InlineData("")]
    public void Non_australian_regions_are_rejected(string region)
    {
        var ex = Assert.Throws<RetrievalException>(
            () => AzureAiSearchGuard.EnsureCanSend(AuEndpoint, "site-docs", region, "Client", SiteA));
        Assert.Equal(
            string.IsNullOrEmpty(region)
                ? RetrievalFailureKind.NotConfigured
                : RetrievalFailureKind.NonAustralianRegion,
            ex.Kind);
    }

    [Theory] // REQ-004 / REQ-008
    [InlineData("Shared")]
    [InlineData("Platform")]
    [InlineData("Ark360")]
    public void Non_client_tenant_mode_is_rejected(string tenantMode)
    {
        var ex = Assert.Throws<RetrievalException>(
            () => AzureAiSearchGuard.EnsureCanSend(AuEndpoint, "site-docs", "australiaeast", tenantMode, SiteA));
        Assert.Equal(RetrievalFailureKind.NonClientTenant, ex.Kind);
    }

    [Theory] // REQ-002 / REQ-008
    [InlineData("https://api.openai.com/")]
    [InlineData("https://search.windows.net/")]
    [InlineData("http://customer-search.search.windows.net/")]
    [InlineData("https://customer-search.search.windows.net/indexes")]
    [InlineData("https://user:pass@customer-search.search.windows.net/")]
    [InlineData("https://customer-search.cognitiveservices.azure.com/")]
    public void Non_azure_ai_search_endpoints_are_rejected(string endpoint)
    {
        var ex = Assert.Throws<RetrievalException>(
            () => AzureAiSearchGuard.EnsureCanSend(endpoint, "site-docs", "australiaeast", "Client", SiteA));
        Assert.True(
            ex.Kind is RetrievalFailureKind.InvalidEndpoint or RetrievalFailureKind.NotConfigured,
            ex.Kind.ToString());
    }

    [Fact] // REQ-017
    public void Clamp_top_defaults_and_caps()
    {
        Assert.Equal(5, AzureAiSearchGuard.ClampTop(0));
        Assert.Equal(5, AzureAiSearchGuard.ClampTop(-1));
        Assert.Equal(8, AzureAiSearchGuard.ClampTop(8));
        Assert.Equal(20, AzureAiSearchGuard.ClampTop(50));
    }

    [Fact] // REQ-006
    public async Task Unconfigured_search_does_not_send()
    {
        var handler = new CapturingHandler();
        var client = new AzureAiSearchRetrievalClient(
            new StubHttpClientFactory(handler),
            new AzureAiSearchOptions(),
            NullLogger<AzureAiSearchRetrievalClient>.Instance);

        var ex = await Assert.ThrowsAsync<RetrievalException>(() =>
            client.SearchAsync(new RetrievalQuery(SiteA, SecretQuery)));

        Assert.Equal(RetrievalFailureKind.NotConfigured, ex.Kind);
        Assert.Null(handler.LastRequest);
    }

    [Fact] // REQ-006
    public async Task Empty_site_does_not_send()
    {
        var handler = new CapturingHandler();
        var client = new AzureAiSearchRetrievalClient(
            new StubHttpClientFactory(handler),
            ValidOptions(),
            NullLogger<AzureAiSearchRetrievalClient>.Instance);

        var ex = await Assert.ThrowsAsync<RetrievalException>(() =>
            client.SearchAsync(new RetrievalQuery(Guid.Empty, SecretQuery)));

        Assert.Equal(RetrievalFailureKind.MissingSiteScope, ex.Kind);
        Assert.Null(handler.LastRequest);
    }

    [Fact] // REQ-002 / REQ-009
    public async Task Valid_binding_posts_site_filtered_search()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = $$"""
                {"value":[
                  {"@search.score":1.5,"id":"doc-1","siteId":"{{SiteA:D}}","title":"House rules","content":"Quiet hours after 10pm.","source":"rules.pdf"},
                  {"@search.score":0.2,"id":"doc-x","siteId":"{{SiteB:D}}","title":"Other site","content":"must-not-return","source":"other.pdf"}
                ]}
                """,
        };
        var client = new AzureAiSearchRetrievalClient(
            new StubHttpClientFactory(handler),
            ValidOptions(),
            NullLogger<AzureAiSearchRetrievalClient>.Instance);

        var result = await client.SearchAsync(new RetrievalQuery(SiteA, SecretQuery, Top: 8));

        Assert.Equal(SiteA, result.SiteId);
        var chunk = Assert.Single(result.Chunks);
        Assert.Equal("doc-1", chunk.Id);
        Assert.Equal("House rules", chunk.Title);
        Assert.Equal("Quiet hours after 10pm.", chunk.Content);
        Assert.DoesNotContain(result.Chunks, c => c.Content.Contains("must-not-return", StringComparison.Ordinal));

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal(
            "https://customer-search.search.windows.net/indexes/site-docs/docs/search?api-version=2024-07-01",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("test-key", handler.LastRequest.Headers.GetValues("api-key").Single());
        Assert.Contains($"siteId eq '{SiteA:D}'", handler.Body, StringComparison.Ordinal);
        Assert.Contains(SecretQuery, handler.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("vectorQueries", handler.Body, StringComparison.Ordinal);
    }

    [Fact] // REQ-002
    public async Task Caller_supplied_vector_is_posted_without_calling_openai()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = """{"value":[]}""",
        };
        var client = new AzureAiSearchRetrievalClient(
            new StubHttpClientFactory(handler),
            ValidOptions(),
            NullLogger<AzureAiSearchRetrievalClient>.Instance);

        await client.SearchAsync(new RetrievalQuery(SiteA, "hours", Vector: [0.1f, 0.2f]));

        Assert.Contains("\"kind\":\"vector\"", handler.Body, StringComparison.Ordinal);
        Assert.Contains("contentVector", handler.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("openai.azure.com", handler.LastRequest!.RequestUri!.Host, StringComparison.OrdinalIgnoreCase);
    }

    [Fact] // REQ-010
    public async Task Logs_omit_query_content_and_api_key()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = $$"""
                {"value":[{"@search.score":1,"id":"doc-1","siteId":"{{SiteA:D}}","title":"t","content":"secret-chunk-text","source":"s"}]}
                """,
        };
        var logger = new CapturingLogger();
        var client = new AzureAiSearchRetrievalClient(
            new StubHttpClientFactory(handler),
            ValidOptions(),
            logger);

        await client.SearchAsync(new RetrievalQuery(SiteA, SecretQuery));

        Assert.NotEmpty(logger.Messages);
        Assert.All(logger.Messages, message =>
        {
            Assert.DoesNotContain(SecretQuery, message, StringComparison.Ordinal);
            Assert.DoesNotContain("secret-chunk-text", message, StringComparison.Ordinal);
            Assert.DoesNotContain("test-key", message, StringComparison.Ordinal);
        });
    }

    [Fact] // REQ-008
    public async Task Provider_error_does_not_retry()
    {
        var handler = new CapturingHandler { StatusCode = HttpStatusCode.TooManyRequests, ResponseBody = "{}" };
        var client = new AzureAiSearchRetrievalClient(
            new StubHttpClientFactory(handler),
            ValidOptions(),
            NullLogger<AzureAiSearchRetrievalClient>.Instance);

        var ex = await Assert.ThrowsAsync<RetrievalException>(() =>
            client.SearchAsync(new RetrievalQuery(SiteA, "hours")));

        Assert.Equal(RetrievalFailureKind.RequestFailed, ex.Kind);
        Assert.Equal(1, handler.SendCount);
    }

    [Fact] // REQ-003
    public async Task Stub_does_not_call_http()
    {
        var handler = new CapturingHandler();
        var stub = new StubRetrievalClient(NullLogger<StubRetrievalClient>.Instance);
        var result = await stub.SearchAsync(new RetrievalQuery(SiteA, SecretQuery));
        Assert.Empty(result.Chunks);
        Assert.Null(handler.LastRequest);
    }

    [Fact] // REQ-007
    public async Task Ground_composes_citations_without_calling_a_model()
    {
        var handler = new CapturingHandler();
        var retrieval = new FixedRetrievalClient(new RetrievalResult(
            SiteA,
            SecretQuery,
            [
                new RetrievedChunk("doc-1", SiteA, "House rules", "Quiet hours after 10pm.", "rules.pdf", 1.2),
                new RetrievedChunk("doc-x", SiteB, "Other", "cross-site", "other.pdf", 0.1),
            ]));
        var rag = new RagGroundingService(retrieval);

        var prompt = await rag.GroundAsync(new RetrievalQuery(SiteA, SecretQuery));

        Assert.Contains("only the retrieved site documents", prompt.SystemInstruction, StringComparison.Ordinal);
        Assert.Contains(SecretQuery, prompt.UserMessage, StringComparison.Ordinal);
        Assert.Contains("Quiet hours after 10pm.", prompt.UserMessage, StringComparison.Ordinal);
        Assert.Contains("[1] House rules (rules.pdf)", prompt.UserMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("cross-site", prompt.UserMessage, StringComparison.Ordinal);
        Assert.Equal("doc-1", Assert.Single(prompt.Citations).Id);
        Assert.Null(handler.LastRequest);
    }

    [Fact] // REQ-007
    public void Empty_retrieval_tells_the_model_it_does_not_know()
    {
        var query = new RetrievalQuery(SiteA, "unknown topic");
        var prompt = RagPromptComposer.Compose(query, new RetrievalResult(SiteA, query.SearchText, []));
        Assert.Contains("none", prompt.UserMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not know", prompt.UserMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(prompt.Citations);
    }

    [Fact] // REQ-008
    public void Bicep_does_not_provision_azure_ai_search()
    {
        var infra = Path.Combine(RepoRoot(), "infra");
        if (!Directory.Exists(infra))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(infra, "*.bicep", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("Microsoft.Search/searchServices", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Microsoft.CognitiveServices/accounts", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact] // REQ-008
    public void Source_has_no_hardcoded_azure_ai_search_resource()
    {
        var src = Path.Combine(RepoRoot(), "src");
        var resourceUrl = new Regex(@"https://[a-z0-9-]+\.search\.windows\.net", RegexOptions.IgnoreCase);
        foreach (var file in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            var text = File.ReadAllText(file);
            Assert.False(
                resourceUrl.IsMatch(text),
                $"{file} hard-codes an Azure AI Search resource URL; the adapter must take the customer endpoint from configuration.");
        }
    }

    [Fact] // REQ-010
    public void Integration_status_is_boolean_and_ready_only_for_client_au()
    {
        var ready = AzureAiSearchOptions.FromConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAiSearch:Endpoint"] = AuEndpoint,
                ["AzureAiSearch:IndexName"] = "site-docs",
                ["AzureAiSearch:Region"] = "australiaeast",
                ["AzureAiSearch:TenantMode"] = "Client",
            })
            .Build());
        Assert.True(ready.IsReadyForClientData());

        var eastus = AzureAiSearchOptions.FromConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAiSearch:Endpoint"] = AuEndpoint,
                ["AzureAiSearch:IndexName"] = "site-docs",
                ["AzureAiSearch:Region"] = "eastus",
            })
            .Build());
        Assert.False(eastus.IsReadyForClientData());
    }

    private static AzureAiSearchOptions ValidOptions() => new()
    {
        Endpoint = AuEndpoint,
        IndexName = "site-docs",
        Region = "australiaeast",
        TenantMode = "Client",
        ApiKey = "test-key",
    };

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Join(dir.FullName, "Arkos.Intelligence.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed class FixedRetrievalClient : IRetrievalClient
    {
        private readonly RetrievalResult _result;

        public FixedRetrievalClient(RetrievalResult result) => _result = result;

        public Task<RetrievalResult> SearchAsync(RetrievalQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(_result);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string Body { get; private set; } = string.Empty;
        public int SendCount { get; private set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string ResponseBody { get; set; } = """{"value":[]}""";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendCount++;
            LastRequest = request;
            Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(StatusCode)
            {
                Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public StubHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }

    private sealed class CapturingLogger : ILogger<AzureAiSearchRetrievalClient>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose()
            {
            }
        }
    }
}
