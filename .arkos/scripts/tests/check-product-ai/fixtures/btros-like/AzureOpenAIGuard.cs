namespace Arkos.Intelligence;

public static class AzureOpenAIGuard
{
    public static readonly string[] AllowedRegions = ["australiaeast", "australiasoutheast"];

    public const string ClientTenantMode = "Client";
    public const string RegionalStandardDeploymentType = "Standard";
    public const string AzureOpenAIHostSuffix = ".openai.azure.com";

    public static void EnsureCanSend(
        string? endpoint,
        string? deployment,
        string? region,
        string? tenantMode,
        string? deploymentType)
    {
        _ = endpoint;
        _ = deployment;
        _ = region;
        _ = tenantMode;
        _ = deploymentType;
    }
}
