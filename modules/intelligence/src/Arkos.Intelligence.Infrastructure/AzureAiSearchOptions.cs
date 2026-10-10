using Microsoft.Extensions.Configuration;
using Arkos.Intelligence;

namespace Arkos.Intelligence.Infrastructure;

/// <summary>SPEC-0012: Key Vault / env binding for the customer Azure AI Search service. Not provisioned here.</summary>
public sealed class AzureAiSearchOptions
{
    public const string SectionName = "AzureAiSearch";

    private static readonly Guid StartupSitePlaceholder = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public string Endpoint { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string TenantMode { get; set; } = AzureAiSearchGuard.ClientTenantMode;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = AzureAiSearchGuard.DefaultApiVersion;
    public string VectorField { get; set; } = AzureAiSearchGuard.DefaultVectorField;

    public bool IsFullyConfigured
        => !string.IsNullOrWhiteSpace(Endpoint)
           && !string.IsNullOrWhiteSpace(IndexName)
           && !string.IsNullOrWhiteSpace(Region);

    public bool HasAnyValue
        => !string.IsNullOrWhiteSpace(Endpoint)
           || !string.IsNullOrWhiteSpace(IndexName)
           || !string.IsNullOrWhiteSpace(Region)
           || !string.IsNullOrWhiteSpace(ApiKey)
           || !AzureAiSearchGuard.IsClientTenant(TenantMode);

    public static AzureAiSearchOptions FromConfiguration(IConfiguration configuration)
        => new()
        {
            Endpoint = configuration["AzureAiSearch:Endpoint"] ?? string.Empty,
            IndexName = configuration["AzureAiSearch:IndexName"] ?? string.Empty,
            Region = configuration["AzureAiSearch:Region"] ?? string.Empty,
            TenantMode = string.IsNullOrWhiteSpace(configuration["AzureAiSearch:TenantMode"])
                ? AzureAiSearchGuard.ClientTenantMode
                : configuration["AzureAiSearch:TenantMode"]!,
            ApiKey = configuration["AzureAiSearch:ApiKey"] ?? string.Empty,
            ApiVersion = string.IsNullOrWhiteSpace(configuration["AzureAiSearch:ApiVersion"])
                ? AzureAiSearchGuard.DefaultApiVersion
                : configuration["AzureAiSearch:ApiVersion"]!,
            VectorField = string.IsNullOrWhiteSpace(configuration["AzureAiSearch:VectorField"])
                ? AzureAiSearchGuard.DefaultVectorField
                : configuration["AzureAiSearch:VectorField"]!,
        };

    /// <summary>
    /// SPEC-0012: a partial or illegal binding fails startup. An absent binding does not
    /// fail startup: no caller exists yet, and taking down every environment would be the wrong fail-closed.
    /// Site id is not known at startup, so the guard is invoked with a non-empty placeholder.
    /// </summary>
    public void ThrowIfMisconfiguredAtStartup()
    {
        if (!HasAnyValue)
        {
            return;
        }

        AzureAiSearchGuard.EnsureCanSend(Endpoint, IndexName, Region, TenantMode, StartupSitePlaceholder);
    }

    public bool IsReadyForClientData()
    {
        try
        {
            AzureAiSearchGuard.EnsureCanSend(Endpoint, IndexName, Region, TenantMode, StartupSitePlaceholder);
            return true;
        }
        catch (RetrievalException)
        {
            return false;
        }
    }
}
