using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Arkos.Intelligence;
using Arkos.Intelligence.Infrastructure;

namespace Arkos.Intelligence.Tests;

/// <summary>SPEC-0012: Development/Testing stubs; production adapter; fail-closed startup.</summary>
public sealed class IntelligenceRegistrationTests
{
    private static IConfiguration Config(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    [Theory] // REQ-009 / REQ-018
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Stub_language_model_in_dev_and_testing(string environment)
    {
        var services = new ServiceCollection();
        services.AddArkosIntelligence(Config(), environment);

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ILanguageModelClient));
        Assert.Equal(typeof(StubLanguageModelClient), descriptor.ImplementationType);
    }

    [Fact] // REQ-015 / REQ-018
    public void Azure_openai_adapter_registered_outside_dev_and_testing()
    {
        var services = new ServiceCollection();
        services.AddArkosIntelligence(Config(), "Production");

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ILanguageModelClient));
        Assert.Equal(typeof(AzureOpenAILanguageModelClient), descriptor.ImplementationType);
    }

    [Fact] // REQ-016
    public void Non_australian_openai_region_fails_startup_outside_dev()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<LanguageModelException>(
            () => services.AddArkosIntelligence(
                Config(
                    ("AzureOpenAI:Endpoint", "https://customer-resource.openai.azure.com/"),
                    ("AzureOpenAI:Deployment", "gpt-4o"),
                    ("AzureOpenAI:Region", "eastus")),
                "Production"));

        Assert.Equal(LanguageModelFailureKind.NonAustralianRegion, ex.Kind);
    }

    [Fact] // REQ-016
    public void Shared_tenant_mode_fails_startup_outside_dev()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<LanguageModelException>(
            () => services.AddArkosIntelligence(
                Config(("AzureOpenAI:TenantMode", "Shared")),
                "Production"));

        Assert.Equal(LanguageModelFailureKind.NonClientTenant, ex.Kind);
    }

    [Theory] // REQ-009 / REQ-018
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Stub_retrieval_in_dev_and_testing(string environment)
    {
        var services = new ServiceCollection();
        services.AddArkosIntelligence(Config(), environment);

        var retrieval = Assert.Single(services, d => d.ServiceType == typeof(IRetrievalClient));
        Assert.Equal(typeof(StubRetrievalClient), retrieval.ImplementationType);
        var rag = Assert.Single(services, d => d.ServiceType == typeof(IRagClient));
        Assert.Equal(typeof(RagGroundingService), rag.ImplementationType);
    }

    [Fact] // REQ-015 / REQ-018
    public void Azure_ai_search_adapter_registered_outside_dev_and_testing()
    {
        var services = new ServiceCollection();
        services.AddArkosIntelligence(Config(), "Production");

        var retrieval = Assert.Single(services, d => d.ServiceType == typeof(IRetrievalClient));
        Assert.Equal(typeof(AzureAiSearchRetrievalClient), retrieval.ImplementationType);
        var rag = Assert.Single(services, d => d.ServiceType == typeof(IRagClient));
        Assert.Equal(typeof(RagGroundingService), rag.ImplementationType);
    }

    [Fact] // REQ-017
    public void Non_australian_search_region_fails_startup_outside_dev()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<RetrievalException>(
            () => services.AddArkosIntelligence(
                Config(
                    ("AzureAiSearch:Endpoint", "https://customer-search.search.windows.net/"),
                    ("AzureAiSearch:IndexName", "site-docs"),
                    ("AzureAiSearch:Region", "eastus")),
                "Production"));

        Assert.Equal(RetrievalFailureKind.NonAustralianRegion, ex.Kind);
    }

    [Fact] // REQ-017
    public void Shared_search_tenant_mode_fails_startup_outside_dev()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<RetrievalException>(
            () => services.AddArkosIntelligence(
                Config(("AzureAiSearch:TenantMode", "Shared")),
                "Production"));

        Assert.Equal(RetrievalFailureKind.NonClientTenant, ex.Kind);
    }
}
