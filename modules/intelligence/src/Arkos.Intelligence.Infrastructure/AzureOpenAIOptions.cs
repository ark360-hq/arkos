using Microsoft.Extensions.Configuration;
using Arkos.Intelligence;

namespace Arkos.Intelligence.Infrastructure;

/// <summary>SPEC-0012: Key Vault / env binding for the customer Azure OpenAI resource. Not provisioned here.</summary>
public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    public string Endpoint { get; set; } = string.Empty;
    public string Deployment { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string TenantMode { get; set; } = AzureOpenAIGuard.ClientTenantMode;
    public string DeploymentType { get; set; } = AzureOpenAIGuard.RegionalStandardDeploymentType;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = AzureOpenAIGuard.DefaultApiVersion;

    public bool IsFullyConfigured
        => !string.IsNullOrWhiteSpace(Endpoint)
           && !string.IsNullOrWhiteSpace(Deployment)
           && !string.IsNullOrWhiteSpace(Region);

    public bool HasAnyValue
        => !string.IsNullOrWhiteSpace(Endpoint)
           || !string.IsNullOrWhiteSpace(Deployment)
           || !string.IsNullOrWhiteSpace(Region)
           || !string.IsNullOrWhiteSpace(ApiKey)
           || !AzureOpenAIGuard.IsClientTenant(TenantMode)
           || !AzureOpenAIGuard.IsRegionalStandard(DeploymentType);

    public static AzureOpenAIOptions FromConfiguration(IConfiguration configuration)
        => new()
        {
            Endpoint = configuration["AzureOpenAI:Endpoint"] ?? string.Empty,
            Deployment = configuration["AzureOpenAI:Deployment"] ?? string.Empty,
            Region = configuration["AzureOpenAI:Region"] ?? string.Empty,
            TenantMode = string.IsNullOrWhiteSpace(configuration["AzureOpenAI:TenantMode"])
                ? AzureOpenAIGuard.ClientTenantMode
                : configuration["AzureOpenAI:TenantMode"]!,
            DeploymentType = string.IsNullOrWhiteSpace(configuration["AzureOpenAI:DeploymentType"])
                ? AzureOpenAIGuard.RegionalStandardDeploymentType
                : configuration["AzureOpenAI:DeploymentType"]!,
            ApiKey = configuration["AzureOpenAI:ApiKey"] ?? string.Empty,
            ApiVersion = string.IsNullOrWhiteSpace(configuration["AzureOpenAI:ApiVersion"])
                ? AzureOpenAIGuard.DefaultApiVersion
                : configuration["AzureOpenAI:ApiVersion"]!,
        };

    /// <summary>
    /// SPEC-0012: a partial or illegal binding fails startup. An absent binding does not
    /// fail startup: no caller exists yet, and taking down every environment would be the wrong fail-closed.
    /// </summary>
    public void ThrowIfMisconfiguredAtStartup()
    {
        if (!HasAnyValue)
        {
            return;
        }

        AzureOpenAIGuard.EnsureCanSend(Endpoint, Deployment, Region, TenantMode, DeploymentType);
    }

    public bool IsReadyForClientData()
    {
        try
        {
            AzureOpenAIGuard.EnsureCanSend(Endpoint, Deployment, Region, TenantMode, DeploymentType);
            return true;
        }
        catch (LanguageModelException)
        {
            return false;
        }
    }
}
