using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using BTROS.Infrastructure;
using BTROS.Tests.Auth;

namespace BTROS.Tests.Api;

/// <summary>
/// SPEC-0114: integration configuration state is visible — the snapshot reflects config presence, and the
/// status endpoint exposes it (booleans only, never secret values).
/// <para>
/// SPEC-0178 REQ-006: the same endpoint now carries the instance mode, because the operator console runs in
/// its own container and had no channel at all to API state. It is readable by Manager as well as Admin: a
/// site operator needs to know whether document storage is real before approving a resident's document, and
/// withholding that from the role that does the approving is what made the gap invisible.
/// </para>
/// </summary>
public sealed class IntegrationStatusTests
{
    [Fact] // SPEC-0114 REQ-002: snapshot is not-configured when keys are absent, configured when present
    public void Snapshot_reflects_configuration_presence()
    {
        var empty = new ConfigurationBuilder().Build();
        Assert.All(IntegrationStatus.Snapshot(empty), item => Assert.False(item.Configured));

        var configured = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Email:Acs:ConnectionString"] = "endpoint=...",
                ["Email:Acs:SenderAddress"] = "no-reply@btros.local",
                ["Stripe:SecretKey"] = "sk_test_x",
                ["DocumentStorage:AccountName"] = "acct",
                ["DocumentStorage:AccountKey"] = "key",
                ["ResidentPush:RelayUrl"] = "https://relay",
                ["AzureOpenAI:Endpoint"] = "https://customer-resource.openai.azure.com/",
                ["AzureOpenAI:Deployment"] = "gpt-4o",
                ["AzureOpenAI:Region"] = "australiaeast",
                ["AzureOpenAI:TenantMode"] = "Client",
                ["AzureOpenAI:DeploymentType"] = "Standard",
                ["AzureAiSearch:Endpoint"] = "https://customer-search.search.windows.net/",
                ["AzureAiSearch:IndexName"] = "site-docs",
                ["AzureAiSearch:Region"] = "australiaeast",
                ["AzureAiSearch:TenantMode"] = "Client",
            })
            .Build();
        Assert.All(IntegrationStatus.Snapshot(configured), item => Assert.True(item.Configured));
    }

    [Fact] // SPEC-0178 REQ-006: operators can read it — an Admin-only channel was the original mistake
    public async Task Status_endpoint_is_readable_by_operators()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"btros-test-{Guid.NewGuid():N}.db");
        using var factory = TestWebApplicationFactory.Create(dbPath);

        foreach (var client in new[]
        {
            await AuthTestHelper.CreateAdminClientAsync(factory),
            await AuthTestHelper.CreateManagerClientAsync(factory),
        })
        {
            using var ok = await client.GetAsync("/api/integrations/status");
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

            var body = await ok.Content.ReadFromJsonAsync<InstanceStatus>();
            Assert.NotNull(body);
            Assert.Contains(body!.Integrations, i => i.Name == "Payments");
            Assert.Contains(body.Integrations, i => i.Name == "AzureOpenAI");
            Assert.Contains(body.Integrations, i => i.Name == "AzureAiSearch");
        }
    }

    [Fact] // SPEC-0178 REQ-006: widening to Manager does not open it to residents
    public async Task Status_endpoint_is_forbidden_to_residents()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"btros-test-{Guid.NewGuid():N}.db");
        using var factory = TestWebApplicationFactory.Create(dbPath);

        var resident = factory.CreateClient();
        var reg = await resident.PostAsJsonAsync("/api/auth/register",
            new { email = $"res-{Guid.NewGuid():N}@test.btros", name = "Res", password = "Password123!" });
        reg.EnsureSuccessStatusCode();
        var auth = await reg.Content.ReadFromJsonAsync<AuthTestHelper.AuthResponse>();
        resident.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        using var forbidden = await resident.GetAsync("/api/integrations/status");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact] // SPEC-0178 REQ-005: mode is derived from the environment, and Testing is not a demo instance
    public async Task Instance_mode_is_customer_outside_demo_environments()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"btros-test-{Guid.NewGuid():N}.db");
        using var factory = TestWebApplicationFactory.Create(dbPath);

        var admin = await AuthTestHelper.CreateAdminClientAsync(factory);
        var body = await admin.GetFromJsonAsync<InstanceStatus>("/api/integrations/status");

        // The suite runs as Testing, which DemoEnvironment.IsEnabled deliberately excludes.
        Assert.Equal("customer", body!.Mode);
    }

    [Fact] // SPEC-0178 REQ-005: a showcase instance reports itself as one
    public async Task Instance_mode_is_demo_on_a_demo_instance()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"btros-test-{Guid.NewGuid():N}.db");
        using var factory = TestWebApplicationFactory.Create(
            dbPath, b => b.UseEnvironment(BTROS.Api.DemoEnvironment.Name));

        var admin = await AuthTestHelper.CreateAdminClientAsync(factory);
        var body = await admin.GetFromJsonAsync<InstanceStatus>("/api/integrations/status");

        Assert.Equal("demo", body!.Mode);
    }

    private sealed record InstanceStatus(string Mode, IReadOnlyList<IntegrationStatusItem> Integrations);

    private sealed record IntegrationStatusItem(string Name, bool Configured);
}
