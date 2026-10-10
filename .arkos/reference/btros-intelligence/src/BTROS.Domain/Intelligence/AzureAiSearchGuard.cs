namespace BTROS.Domain.Intelligence;

/// <summary>
/// SPEC-0207: client-tenant, Australian-region rules for Azure AI Search.
/// Region cannot be read from <c>*.search.windows.net</c>; it is a configured claim checked here.
/// </summary>
public static class AzureAiSearchGuard
{
    public static readonly string[] AllowedRegions = ["australiaeast", "australiasoutheast"];

    public const string ClientTenantMode = "Client";
    public const string DefaultApiVersion = "2024-07-01";
    public const string DefaultVectorField = "contentVector";
    public const string AzureAiSearchHostSuffix = ".search.windows.net";
    public const int DefaultTop = 5;
    public const int MaxTop = 20;

    public static bool IsAllowedRegion(string? region)
        => !string.IsNullOrWhiteSpace(region)
           && AllowedRegions.Contains(region.Trim(), StringComparer.OrdinalIgnoreCase);

    public static bool IsClientTenant(string? tenantMode)
        => string.Equals((tenantMode ?? ClientTenantMode).Trim(), ClientTenantMode, StringComparison.OrdinalIgnoreCase);

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
            || !parsed.IdnHost.EndsWith(AzureAiSearchHostSuffix, StringComparison.OrdinalIgnoreCase)
            || parsed.IdnHost.Equals("search.windows.net", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        uri = new Uri(parsed.GetLeftPart(UriPartial.Authority) + "/", UriKind.Absolute);
        return true;
    }

    public static int ClampTop(int top)
        => top < 1 ? DefaultTop : Math.Min(top, MaxTop);

    public static string SiteFilter(Guid siteId)
    {
        if (siteId == Guid.Empty)
        {
            throw new RetrievalException(
                RetrievalFailureKind.MissingSiteScope,
                "A site id is required before a search can be sent.");
        }

        return $"siteId eq '{siteId:D}'";
    }

    public static void EnsureCanSend(
        string? endpoint,
        string? indexName,
        string? region,
        string? tenantMode,
        Guid siteId)
    {
        if (!IsClientTenant(tenantMode))
        {
            throw new RetrievalException(
                RetrievalFailureKind.NonClientTenant,
                "Azure AI Search must use TenantMode=Client (the customer's Azure tenant). Shared or platform search tenants are not permitted.");
        }

        if (!string.IsNullOrWhiteSpace(region) && !IsAllowedRegion(region))
        {
            throw new RetrievalException(
                RetrievalFailureKind.NonAustralianRegion,
                $"Client data may only be sent to an Australian-region Azure AI Search service. '{region}' is not australiaeast or australiasoutheast.");
        }

        if (!string.IsNullOrWhiteSpace(endpoint) && !TryGetAllowedEndpoint(endpoint, out _))
        {
            throw new RetrievalException(
                RetrievalFailureKind.InvalidEndpoint,
                "Azure AI Search Endpoint must be an https://*.search.windows.net/ resource URL.");
        }

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(indexName)
            || string.IsNullOrWhiteSpace(region))
        {
            throw new RetrievalException(
                RetrievalFailureKind.NotConfigured,
                "Azure AI Search Endpoint, IndexName and Region are required before a search can be sent.");
        }

        if (!IsAllowedRegion(region))
        {
            throw new RetrievalException(
                RetrievalFailureKind.NonAustralianRegion,
                $"Client data may only be sent to an Australian-region Azure AI Search service. '{region}' is not australiaeast or australiasoutheast.");
        }

        if (!TryGetAllowedEndpoint(endpoint, out _))
        {
            throw new RetrievalException(
                RetrievalFailureKind.InvalidEndpoint,
                "Azure AI Search Endpoint must be an https://*.search.windows.net/ resource URL.");
        }

        if (siteId == Guid.Empty)
        {
            throw new RetrievalException(
                RetrievalFailureKind.MissingSiteScope,
                "A site id is required before a search can be sent.");
        }
    }
}
