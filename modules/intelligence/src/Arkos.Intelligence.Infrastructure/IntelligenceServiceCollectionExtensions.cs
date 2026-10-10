using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Arkos.Intelligence;

namespace Arkos.Intelligence.Infrastructure;

/// <summary>
/// SPEC-0012 / ADR-0006: register the extracted Azure OpenAI and Azure AI Search adapters.
/// Development and Testing use stubs. A partial or illegal binding fails startup outside those
/// environments. An absent binding does not fail startup, but CompleteAsync and SearchAsync
/// still fail closed.
/// </summary>
public static class IntelligenceServiceCollectionExtensions
{
    public static IServiceCollection AddArkosIntelligence(
        this IServiceCollection services,
        IConfiguration configuration,
        string? environmentName = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var useStubs =
            string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase)
            || string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase);

        services.AddHttpClient(AzureOpenAILanguageModelClient.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddHttpClient(AzureAiSearchRetrievalClient.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        var azureOpenAI = AzureOpenAIOptions.FromConfiguration(configuration);
        if (!useStubs)
        {
            azureOpenAI.ThrowIfMisconfiguredAtStartup();
        }

        services.AddSingleton(azureOpenAI);
        if (useStubs)
        {
            services.AddScoped<ILanguageModelClient, StubLanguageModelClient>();
        }
        else
        {
            services.AddScoped<ILanguageModelClient, AzureOpenAILanguageModelClient>();
        }

        var azureAiSearch = AzureAiSearchOptions.FromConfiguration(configuration);
        if (!useStubs)
        {
            azureAiSearch.ThrowIfMisconfiguredAtStartup();
        }

        services.AddSingleton(azureAiSearch);
        if (useStubs)
        {
            services.AddScoped<IRetrievalClient, StubRetrievalClient>();
        }
        else
        {
            services.AddScoped<IRetrievalClient, AzureAiSearchRetrievalClient>();
        }

        services.AddScoped<IRagClient, RagGroundingService>();
        return services;
    }
}
