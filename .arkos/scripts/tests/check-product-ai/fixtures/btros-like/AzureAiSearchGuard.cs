namespace Arkos.Intelligence;

public static class AzureAiSearchGuard
{
    public static readonly string[] AllowedRegions = ["australiaeast", "australiasoutheast"];

    public const string ClientTenantMode = "Client";
    public const string AzureAiSearchHostSuffix = ".search.windows.net";
    public const int DefaultTop = 5;
    public const int MaxTop = 20;

    public static void EnsureCanSend(
        string? endpoint,
        string? indexName,
        string? region,
        string? tenantMode,
        Guid siteId)
    {
        _ = endpoint;
        _ = indexName;
        _ = region;
        _ = tenantMode;
        _ = siteId;
    }

    public static int ClampTop(int top)
        => top < 1 ? DefaultTop : Math.Min(top, MaxTop);
}
