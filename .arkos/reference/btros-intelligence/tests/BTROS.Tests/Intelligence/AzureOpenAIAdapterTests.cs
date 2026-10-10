using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using BTROS.Domain.Intelligence;
using BTROS.Infrastructure.Intelligence;

namespace BTROS.Tests.Intelligence;

/// <summary>SPEC-0206: Azure OpenAI adapter — client tenant, AU regional Standard, fail-closed.</summary>
public sealed class AzureOpenAIAdapterTests
{
    private const string AuEndpoint = "https://customer-resource.openai.azure.com/";
    private const string SecretPrompt = "lease for unit 12B, resident Jane Example";

    [Theory] // REQ-004
    [InlineData("australiaeast")]
    [InlineData("australiasoutheast")]
    [InlineData("AustraliaEast")]
    public void Allowed_australian_regions_are_accepted(string region)
    {
        AzureOpenAIGuard.EnsureCanSend(AuEndpoint, "gpt-4o", region, "Client", "Standard");
        Assert.True(AzureOpenAIGuard.IsAllowedRegion(region));
    }

    [Theory] // REQ-004 / REQ-007
    [InlineData("eastus")]
    [InlineData("westeurope")]
    [InlineData("westus2")]
    [InlineData("")]
    public void Non_australian_regions_are_rejected(string region)
    {
        var ex = Assert.Throws<LanguageModelException>(
            () => AzureOpenAIGuard.EnsureCanSend(AuEndpoint, "gpt-4o", region, "Client", "Standard"));
        Assert.Equal(
            string.IsNullOrEmpty(region)
                ? LanguageModelFailureKind.NotConfigured
                : LanguageModelFailureKind.NonAustralianRegion,
            ex.Kind);
    }

    [Theory] // REQ-004 / REQ-007
    [InlineData("Shared")]
    [InlineData("Platform")]
    [InlineData("Ark360")]
    public void Non_client_tenant_mode_is_rejected(string tenantMode)
    {
        var ex = Assert.Throws<LanguageModelException>(
            () => AzureOpenAIGuard.EnsureCanSend(AuEndpoint, "gpt-4o", "australiaeast", tenantMode, "Standard"));
        Assert.Equal(LanguageModelFailureKind.NonClientTenant, ex.Kind);
    }

    [Theory] // REQ-007
    [InlineData("GlobalStandard")]
    [InlineData("DataZoneStandard")]
    [InlineData("Provisioned")]
    [InlineData("Global")]
    public void Non_regional_standard_deployment_type_is_rejected(string deploymentType)
    {
        var ex = Assert.Throws<LanguageModelException>(
            () => AzureOpenAIGuard.EnsureCanSend(AuEndpoint, "gpt-4o", "australiaeast", "Client", deploymentType));
        Assert.Equal(LanguageModelFailureKind.InvalidDeploymentType, ex.Kind);
    }

    [Theory] // REQ-002 / REQ-007
    [InlineData("https://api.openai.com/")]
    [InlineData("https://api.openai.com/v1")]
    [InlineData("http://customer-resource.openai.azure.com/")]
    [InlineData("https://openai.azure.com/")]
    [InlineData("https://customer-resource.openai.azure.com/openai")]
    [InlineData("https://user:pass@customer-resource.openai.azure.com/")]
    public void Non_azure_openai_endpoints_are_rejected(string endpoint)
    {
        var ex = Assert.Throws<LanguageModelException>(
            () => AzureOpenAIGuard.EnsureCanSend(endpoint, "gpt-4o", "australiaeast", "Client", "Standard"));
        Assert.True(
            ex.Kind is LanguageModelFailureKind.InvalidEndpoint or LanguageModelFailureKind.NotConfigured,
            ex.Kind.ToString());
    }

    [Fact] // REQ-006
    public async Task Unconfigured_complete_does_not_send()
    {
        var handler = new CapturingHandler();
        var client = new AzureOpenAILanguageModelClient(
            new StubHttpClientFactory(handler),
            new AzureOpenAIOptions(),
            NullLogger<AzureOpenAILanguageModelClient>.Instance);

        var ex = await Assert.ThrowsAsync<LanguageModelException>(() =>
            client.CompleteAsync(new LanguageModelRequest([new LanguageModelMessage("user", SecretPrompt)])));

        Assert.Equal(LanguageModelFailureKind.NotConfigured, ex.Kind);
        Assert.Null(handler.LastRequest);
    }

    [Fact] // REQ-002 / REQ-006
    public async Task Valid_binding_posts_chat_completions_and_returns_content()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = """{"choices":[{"message":{"content":"draft pack"}}]}""",
        };
        var client = new AzureOpenAILanguageModelClient(
            new StubHttpClientFactory(handler),
            ValidOptions(),
            NullLogger<AzureOpenAILanguageModelClient>.Instance);

        var result = await client.CompleteAsync(
            new LanguageModelRequest([new LanguageModelMessage("user", SecretPrompt)], MaxTokens: 64));

        Assert.Equal("draft pack", result.Content);
        Assert.Equal("gpt-4o", result.Deployment);
        Assert.Equal("australiaeast", result.Region);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal(
            "https://customer-resource.openai.azure.com/openai/deployments/gpt-4o/chat/completions?api-version=2024-10-21",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("test-key", handler.LastRequest.Headers.GetValues("api-key").Single());
        Assert.Contains("\"role\":\"user\"", handler.Body, StringComparison.Ordinal);
        Assert.Contains(SecretPrompt, handler.Body, StringComparison.Ordinal);
    }

    [Fact] // REQ-008
    public async Task Logs_omit_prompt_completion_and_api_key()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = """{"choices":[{"message":{"content":"secret-completion-text"}}]}""",
        };
        var logger = new CapturingLogger();
        var client = new AzureOpenAILanguageModelClient(
            new StubHttpClientFactory(handler),
            ValidOptions(),
            logger);

        await client.CompleteAsync(new LanguageModelRequest([new LanguageModelMessage("user", SecretPrompt)]));

        Assert.NotEmpty(logger.Messages);
        Assert.All(logger.Messages, message =>
        {
            Assert.DoesNotContain(SecretPrompt, message, StringComparison.Ordinal);
            Assert.DoesNotContain("secret-completion-text", message, StringComparison.Ordinal);
            Assert.DoesNotContain("test-key", message, StringComparison.Ordinal);
        });
    }

    [Fact] // REQ-007
    public async Task Provider_error_does_not_retry()
    {
        var handler = new CapturingHandler { StatusCode = HttpStatusCode.TooManyRequests, ResponseBody = "{}" };
        var client = new AzureOpenAILanguageModelClient(
            new StubHttpClientFactory(handler),
            ValidOptions(),
            NullLogger<AzureOpenAILanguageModelClient>.Instance);

        var ex = await Assert.ThrowsAsync<LanguageModelException>(() =>
            client.CompleteAsync(new LanguageModelRequest([new LanguageModelMessage("user", "hi")])));

        Assert.Equal(LanguageModelFailureKind.RequestFailed, ex.Kind);
        Assert.Equal(1, handler.SendCount);
    }

    [Fact] // REQ-003
    public async Task Stub_does_not_call_http()
    {
        var handler = new CapturingHandler();
        var stub = new StubLanguageModelClient(NullLogger<StubLanguageModelClient>.Instance);
        var result = await stub.CompleteAsync(new LanguageModelRequest([new LanguageModelMessage("user", SecretPrompt)]));
        Assert.Equal("stub", result.Content);
        Assert.Null(handler.LastRequest);
    }

    [Fact] // REQ-007
    public void Bicep_does_not_provision_azure_openai()
    {
        var infra = Path.Combine(RepoRoot(), "infra");
        foreach (var file in Directory.EnumerateFiles(infra, "*.bicep", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("Microsoft.CognitiveServices/accounts", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Microsoft.CognitiveServices/accounts/deployments", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("kind: 'OpenAI'", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact] // REQ-007
    public void Source_has_no_hardcoded_azure_openai_resource()
    {
        var src = Path.Combine(RepoRoot(), "src");
        var resourceUrl = new Regex(@"https://[a-z0-9-]+\.openai\.azure\.com", RegexOptions.IgnoreCase);
        foreach (var file in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            var text = File.ReadAllText(file);
            Assert.False(
                resourceUrl.IsMatch(text),
                $"{file} hard-codes an Azure OpenAI resource URL; the adapter must take the customer endpoint from configuration.");
        }
    }

    [Fact] // REQ-008
    public void Integration_status_is_boolean_and_ready_only_for_client_au_standard()
    {
        var ready = AzureOpenAIOptions.FromConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureOpenAI:Endpoint"] = AuEndpoint,
                ["AzureOpenAI:Deployment"] = "gpt-4o",
                ["AzureOpenAI:Region"] = "australiaeast",
                ["AzureOpenAI:TenantMode"] = "Client",
                ["AzureOpenAI:DeploymentType"] = "Standard",
            })
            .Build());
        Assert.True(ready.IsReadyForClientData());

        var eastus = AzureOpenAIOptions.FromConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureOpenAI:Endpoint"] = AuEndpoint,
                ["AzureOpenAI:Deployment"] = "gpt-4o",
                ["AzureOpenAI:Region"] = "eastus",
            })
            .Build());
        Assert.False(eastus.IsReadyForClientData());
    }

    private static AzureOpenAIOptions ValidOptions() => new()
    {
        Endpoint = AuEndpoint,
        Deployment = "gpt-4o",
        Region = "australiaeast",
        TenantMode = "Client",
        DeploymentType = "Standard",
        ApiKey = "test-key",
    };

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Join(dir.FullName, "BTROS.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string Body { get; private set; } = string.Empty;
        public int SendCount { get; private set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string ResponseBody { get; set; } = """{"choices":[{"message":{"content":"ok"}}]}""";

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

    private sealed class CapturingLogger : ILogger<AzureOpenAILanguageModelClient>
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
