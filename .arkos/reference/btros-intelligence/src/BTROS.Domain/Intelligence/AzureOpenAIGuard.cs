namespace BTROS.Domain.Intelligence;

/// <summary>
/// SPEC-0206: client-tenant, Australian-region, regional-Standard rules for Azure OpenAI.
/// Region cannot be read from <c>*.openai.azure.com</c>; it is a configured claim checked here.
/// </summary>
public static class AzureOpenAIGuard
{
    public static readonly string[] AllowedRegions = ["australiaeast", "australiasoutheast"];

    public const string ClientTenantMode = "Client";
    public const string RegionalStandardDeploymentType = "Standard";
    public const string DefaultApiVersion = "2024-10-21";
    public const string AzureOpenAIHostSuffix = ".openai.azure.com";

    public static bool IsAllowedRegion(string? region)
        => !string.IsNullOrWhiteSpace(region)
           && AllowedRegions.Contains(region.Trim(), StringComparer.OrdinalIgnoreCase);

    public static bool IsClientTenant(string? tenantMode)
        => string.Equals((tenantMode ?? ClientTenantMode).Trim(), ClientTenantMode, StringComparison.OrdinalIgnoreCase);

    public static bool IsRegionalStandard(string? deploymentType)
        => string.Equals(
            (deploymentType ?? RegionalStandardDeploymentType).Trim(),
            RegionalStandardDeploymentType,
            StringComparison.OrdinalIgnoreCase);

    public static bool TryGetAllowedEndpoint(string? endpoint, out Uri? uri)
    {
        uri = null;
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var parsed))
        {
            return false;
        }

        if (parsed.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(parsed.UserInfo)
            || !string.IsNullOrEmpty(parsed.Query)
            || !string.IsNullOrEmpty(parsed.Fragment)
            || parsed.Port != 443
            || parsed.AbsolutePath != "/"
            || !parsed.IdnHost.EndsWith(AzureOpenAIHostSuffix, StringComparison.OrdinalIgnoreCase)
            || parsed.IdnHost.Equals("openai.azure.com", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        uri = new Uri(parsed.GetLeftPart(UriPartial.Authority) + "/", UriKind.Absolute);
        return true;
    }

    public static void EnsureCanSend(
        string? endpoint,
        string? deployment,
        string? region,
        string? tenantMode,
        string? deploymentType)
    {
        if (!IsClientTenant(tenantMode))
        {
            throw new LanguageModelException(
                LanguageModelFailureKind.NonClientTenant,
                "Azure OpenAI must use TenantMode=Client (the customer's Azure tenant). Shared or platform model tenants are not permitted.");
        }

        if (!string.IsNullOrWhiteSpace(region) && !IsAllowedRegion(region))
        {
            throw new LanguageModelException(
                LanguageModelFailureKind.NonAustralianRegion,
                $"Client data may only be sent to an Australian-region GPT deployment. '{region}' is not australiaeast or australiasoutheast.");
        }

        if (!IsRegionalStandard(deploymentType))
        {
            throw new LanguageModelException(
                LanguageModelFailureKind.InvalidDeploymentType,
                "Azure OpenAI DeploymentType must be Standard (regional). Global Standard, Data Zone, and provisioned types are not permitted for client data.");
        }

        if (!string.IsNullOrWhiteSpace(endpoint) && !TryGetAllowedEndpoint(endpoint, out _))
        {
            throw new LanguageModelException(
                LanguageModelFailureKind.InvalidEndpoint,
                "Azure OpenAI Endpoint must be an https://*.openai.azure.com/ resource URL.");
        }

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(deployment)
            || string.IsNullOrWhiteSpace(region))
        {
            throw new LanguageModelException(
                LanguageModelFailureKind.NotConfigured,
                "Azure OpenAI Endpoint, Deployment and Region are required before a completion can be sent.");
        }

        if (!IsAllowedRegion(region))
        {
            throw new LanguageModelException(
                LanguageModelFailureKind.NonAustralianRegion,
                $"Client data may only be sent to an Australian-region GPT deployment. '{region}' is not australiaeast or australiasoutheast.");
        }

        if (!TryGetAllowedEndpoint(endpoint, out _))
        {
            throw new LanguageModelException(
                LanguageModelFailureKind.InvalidEndpoint,
                "Azure OpenAI Endpoint must be an https://*.openai.azure.com/ resource URL.");
        }
    }
}
