namespace BTROS.Tests.Architecture;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using BTROS.Domain.AccountVerification;
using BTROS.Domain.Intelligence;
using BTROS.Domain.Pms;
using BTROS.Domain.ResidentNotifications;
using BTROS.Infrastructure;
using BTROS.Infrastructure.AccountVerification;
using BTROS.Infrastructure.Intelligence;
using BTROS.Infrastructure.ResidentNotifications;

/// <summary>
/// SPEC-0038 REQ-004 / REQ-005: infrastructure registration fails fast when no database connection
/// string is configured outside Development/Testing, and the reserved PMS outbound stub is gated to
/// the Testing environment.
/// SPEC-0092: a passwordless Azure PostgreSQL connection string registers an Entra-token
/// <see cref="NpgsqlDataSource"/>; a local/password one does not.
/// </summary>
public sealed class InfrastructureRegistrationTests
{
    /// <summary>
    /// SPEC-0184: these tests exercise non-Development environments, where the field-encryption key is
    /// mandatory. Supplied here so the registration assertions stay about what they are about — the key
    /// guard has its own tests in <see cref="Spec0184FieldEncryptionTests"/>.
    /// </summary>
    private const string TestFieldKey = "YnRyb3MtdGVzdC1maWVsZC1lbmNyeXB0aW9uLWtleSE=";

    private static IConfiguration Config(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(
                values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value))
                    .Append(new KeyValuePair<string, string?>("Secrets:FieldEncryptionKey", TestFieldKey)))
            .Build();

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void Missing_connection_string_fails_fast_outside_dev_and_testing(string environment)
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(
            () => services.AddBtrosInfrastructure(Config(), environment));
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Missing_connection_string_is_allowed_in_dev_and_testing(string environment)
    {
        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddBtrosInfrastructure(Config(), environment));

        Assert.Null(exception);
    }

    [Fact]
    public void Pms_outbound_stub_is_registered_only_under_testing()
    {
        var testing = new ServiceCollection();
        testing.AddBtrosInfrastructure(Config(), "Testing");
        Assert.Contains(testing, d => d.ServiceType == typeof(IPmsOutboundClient));

        // Supply a connection string so the REQ-004 fail-fast does not trip under Production.
        var production = new ServiceCollection();
        production.AddBtrosInfrastructure(
            Config(("ConnectionStrings:BtrosDb", "Data Source=spec0038-prod.db")),
            "Production");
        Assert.DoesNotContain(production, d => d.ServiceType == typeof(IPmsOutboundClient));
    }

    [Fact] // SPEC-0092: passwordless Azure PostgreSQL registers an Entra-token NpgsqlDataSource
    public void Azure_passwordless_postgres_registers_aad_token_datasource()
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(
            Config(("ConnectionStrings:BtrosDb",
                "Host=btros-pg.postgres.database.azure.com;Database=BtrosDb;Username=app-identity")),
            "Production");

        Assert.Contains(services, d => d.ServiceType == typeof(NpgsqlDataSource));
    }

    [Fact] // SPEC-0092: a local/password PostgreSQL connection string uses plain Npgsql (no token source)
    public void Password_postgres_does_not_register_aad_token_datasource()
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(
            Config(("ConnectionStrings:BtrosDb",
                "Host=localhost;Database=btros;Username=btros;Password=btros")),
            "Production");

        Assert.DoesNotContain(services, d => d.ServiceType == typeof(NpgsqlDataSource));
    }

    [Fact] // SPEC-0102: ACS email is selected when configured (non-Dev)
    public void Acs_email_provider_selected_when_configured()
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(
            Config(
                ("ConnectionStrings:BtrosDb", "Data Source=spec0102.db"),
                ("Email:Acs:ConnectionString", "endpoint=https://x.communication.azure.com/;accesskey=ZmFrZQ=="),
                ("Email:Acs:SenderAddress", "donotreply@example.com")),
            "Production");

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IMessagingProvider));
        Assert.Equal(typeof(AcsEmailMessagingProvider), descriptor.ImplementationType);
    }

    [Fact] // SPEC-0102: without ACS config, the generic relay seam is used (non-Dev)
    public void Relay_provider_used_when_acs_not_configured()
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(
            Config(("ConnectionStrings:BtrosDb", "Data Source=spec0102-norelay.db")),
            "Production");

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IMessagingProvider));
        Assert.Equal(typeof(HttpMessagingProvider), descriptor.ImplementationType);
    }

    [Theory] // SPEC-0102 (REQ-003): Dev/Testing keep the deterministic stub (no real send)
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Stub_messaging_provider_in_dev_and_testing(string environment)
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(Config(), environment);

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IMessagingProvider));
        Assert.Equal(typeof(StubMessagingProvider), descriptor.ImplementationType);
    }

    [Fact] // SPEC-0122: FCM transport is selected when a Firebase credential is configured (non-Dev)
    public void Fcm_push_transport_selected_when_configured()
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(
            Config(
                ("ConnectionStrings:BtrosDb", "Data Source=spec0122.db"),
                ("ResidentPush:FirebaseCredentialsJson", "{\"type\":\"service_account\"}")),
            "Production");

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IResidentPushTransport));
        Assert.Equal(typeof(FcmResidentPushTransport), descriptor.ImplementationType);
    }

    [Fact] // SPEC-0122: without a Firebase credential, the generic relay transport is used (non-Dev)
    public void Relay_push_transport_when_fcm_not_configured()
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(
            Config(("ConnectionStrings:BtrosDb", "Data Source=spec0122-norelay.db")),
            "Production");

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IResidentPushTransport));
        Assert.Equal(typeof(HttpResidentPushTransport), descriptor.ImplementationType);
    }

    [Theory] // SPEC-0053/0122: Dev/Testing keep the deterministic stub push transport
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Stub_push_transport_in_dev_and_testing(string environment)
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(Config(), environment);

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IResidentPushTransport));
        Assert.Equal(typeof(StubResidentPushTransport), descriptor.ImplementationType);
    }

    [Theory] // SPEC-0206 REQ-003
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Stub_language_model_in_dev_and_testing(string environment)
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(Config(), environment);

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ILanguageModelClient));
        Assert.Equal(typeof(StubLanguageModelClient), descriptor.ImplementationType);
    }

    [Fact] // SPEC-0206 REQ-001
    public void Azure_openai_adapter_registered_outside_dev_and_testing()
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(
            Config(("ConnectionStrings:BtrosDb", "Data Source=spec0206.db")),
            "Production");

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ILanguageModelClient));
        Assert.Equal(typeof(AzureOpenAILanguageModelClient), descriptor.ImplementationType);
    }

    [Fact] // SPEC-0206 REQ-005
    public void Non_australian_openai_region_fails_startup_outside_dev()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<LanguageModelException>(
            () => services.AddBtrosInfrastructure(
                Config(
                    ("ConnectionStrings:BtrosDb", "Data Source=spec0206-eastus.db"),
                    ("AzureOpenAI:Endpoint", "https://customer-resource.openai.azure.com/"),
                    ("AzureOpenAI:Deployment", "gpt-4o"),
                    ("AzureOpenAI:Region", "eastus")),
                "Production"));

        Assert.Equal(LanguageModelFailureKind.NonAustralianRegion, ex.Kind);
    }

    [Fact] // SPEC-0206 REQ-005
    public void Shared_tenant_mode_fails_startup_outside_dev()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<LanguageModelException>(
            () => services.AddBtrosInfrastructure(
                Config(
                    ("ConnectionStrings:BtrosDb", "Data Source=spec0206-shared.db"),
                    ("AzureOpenAI:TenantMode", "Shared")),
                "Production"));

        Assert.Equal(LanguageModelFailureKind.NonClientTenant, ex.Kind);
    }

    [Theory] // SPEC-0207 REQ-003
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Stub_retrieval_in_dev_and_testing(string environment)
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(Config(), environment);

        var retrieval = Assert.Single(services, d => d.ServiceType == typeof(IRetrievalClient));
        Assert.Equal(typeof(StubRetrievalClient), retrieval.ImplementationType);
        var rag = Assert.Single(services, d => d.ServiceType == typeof(IRagClient));
        Assert.Equal(typeof(RagGroundingService), rag.ImplementationType);
    }

    [Fact] // SPEC-0207 REQ-001
    public void Azure_ai_search_adapter_registered_outside_dev_and_testing()
    {
        var services = new ServiceCollection();
        services.AddBtrosInfrastructure(
            Config(("ConnectionStrings:BtrosDb", "Data Source=spec0207.db")),
            "Production");

        var retrieval = Assert.Single(services, d => d.ServiceType == typeof(IRetrievalClient));
        Assert.Equal(typeof(AzureAiSearchRetrievalClient), retrieval.ImplementationType);
        var rag = Assert.Single(services, d => d.ServiceType == typeof(IRagClient));
        Assert.Equal(typeof(RagGroundingService), rag.ImplementationType);
    }

    [Fact] // SPEC-0207 REQ-005
    public void Non_australian_search_region_fails_startup_outside_dev()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<RetrievalException>(
            () => services.AddBtrosInfrastructure(
                Config(
                    ("ConnectionStrings:BtrosDb", "Data Source=spec0207-eastus.db"),
                    ("AzureAiSearch:Endpoint", "https://customer-search.search.windows.net/"),
                    ("AzureAiSearch:IndexName", "site-docs"),
                    ("AzureAiSearch:Region", "eastus")),
                "Production"));

        Assert.Equal(RetrievalFailureKind.NonAustralianRegion, ex.Kind);
    }

    [Fact] // SPEC-0207 REQ-005
    public void Shared_search_tenant_mode_fails_startup_outside_dev()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<RetrievalException>(
            () => services.AddBtrosInfrastructure(
                Config(
                    ("ConnectionStrings:BtrosDb", "Data Source=spec0207-shared.db"),
                    ("AzureAiSearch:TenantMode", "Shared")),
                "Production"));

        Assert.Equal(RetrievalFailureKind.NonClientTenant, ex.Kind);
    }
}
