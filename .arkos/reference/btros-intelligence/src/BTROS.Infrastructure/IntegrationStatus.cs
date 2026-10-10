using Microsoft.Extensions.Configuration;
using BTROS.Infrastructure.Intelligence;

namespace BTROS.Infrastructure;

/// <summary>SPEC-0114: the configured/not-configured state of a global integration.</summary>
public sealed record IntegrationStatusItem(string Name, bool Configured);

/// <summary>
/// SPEC-0114: reports whether each global integration's configuration is present, so a misconfiguration
/// is visible (startup log + the Admin status endpoint) rather than a silent no-op discovered only when
/// a feature fails. Mirrors the real-vs-stub selection in <c>DependencyInjection</c>. (BioStar is
/// per-site and surfaced separately via Settings → Test connection.)
/// </summary>
public static class IntegrationStatus
{
    public static IReadOnlyList<IntegrationStatusItem> Snapshot(IConfiguration configuration) =>
    [
        new("Email", Has(configuration, "Email:Acs:ConnectionString") && Has(configuration, "Email:Acs:SenderAddress")),
        new("Payments", Has(configuration, "Stripe:SecretKey")),
        new("DocumentStorage", Has(configuration, "DocumentStorage:AccountName") && Has(configuration, "DocumentStorage:AccountKey")),
        new("ResidentPush", Has(configuration, "ResidentPush:FirebaseCredentialsJson") || Has(configuration, "ResidentPush:RelayUrl")),
        new("AzureOpenAI", AzureOpenAIOptions.FromConfiguration(configuration).IsReadyForClientData()),
        new("AzureAiSearch", AzureAiSearchOptions.FromConfiguration(configuration).IsReadyForClientData()),
    ];

    private static bool Has(IConfiguration configuration, string key)
        => !string.IsNullOrWhiteSpace(configuration[key]);
}
