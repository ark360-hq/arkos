using Azure.Core;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using BTROS.Domain.Abstractions;
using BTROS.Domain.AccountVerification;
using BTROS.Domain.Access;
using BTROS.Domain.Amenities;
using BTROS.Domain.Analytics;
using BTROS.Domain.Announcements;
using BTROS.Domain.Auditing;
using BTROS.Domain.Auth;
using BTROS.Domain.Documents;
using BTROS.Domain.Events;
using BTROS.Domain.Feedback;
using BTROS.Domain.Identity;
using BTROS.Domain.Leasing;
using BTROS.Domain.Notifications;
using BTROS.Domain.Onboarding;
using BTROS.Domain.Parcels;
using BTROS.Domain.Payments;
using BTROS.Domain.Residential;
using BTROS.Domain.ResidentNotifications;
using BTROS.Domain.ServiceManagement;
using BTROS.Domain.Outbox;
using BTROS.Domain.Sites;
using BTROS.Domain.Units;
using BTROS.Domain.Users;
using BTROS.Domain.Waitlist;
using BTROS.Infrastructure.Outbox;
using BTROS.Infrastructure.Security;
using BTROS.Infrastructure.AccountVerification;
using BTROS.Infrastructure.Amenities;
using BTROS.Infrastructure.Analytics;
using BTROS.Infrastructure.Announcements;
using BTROS.Infrastructure.Auditing;
using BTROS.Infrastructure.Auth;
using BTROS.Infrastructure.BioStar;
using BTROS.Infrastructure.Data;
using BTROS.Infrastructure.Access;
using BTROS.Infrastructure.Documents;
using BTROS.Infrastructure.Events;
using BTROS.Infrastructure.Feedback;
using BTROS.Infrastructure.Identity;
using BTROS.Infrastructure.Leasing;
using BTROS.Infrastructure.Notifications;
using BTROS.Infrastructure.Onboarding;
using BTROS.Infrastructure.ResidentNotifications;
using BTROS.Infrastructure.Parcels;
using BTROS.Infrastructure.Payments;
using BTROS.Infrastructure.Residential;
using BTROS.Infrastructure.ServiceManagement;
using BTROS.Infrastructure.Sites;
using BTROS.Infrastructure.Units;
using BTROS.Infrastructure.Pms;
using BTROS.Infrastructure.Waitlist;
using BTROS.Infrastructure.Intelligence;
using BTROS.Domain.Intelligence;
using BTROS.Domain.Pms;

namespace BTROS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBtrosInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? environmentName = null)
    {
        var connectionString = ConnectionStringNormalizer.ForNpgsql(
            configuration.GetConnectionString("BtrosDb") ?? configuration["DATABASE_URL"]);
        var allowSqliteFallback =
            string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase)
            || string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(connectionString) && !allowSqliteFallback)
        {
            // Fail fast rather than silently running production against a local SQLite file
            // (SPEC-0038 REQ-004). Mirrors the JWT/key fail-fast in the API and Gateway hosts.
            throw new InvalidOperationException(
                "No database connection string configured. Set ConnectionStrings:BtrosDb or " +
                "DATABASE_URL. The local SQLite fallback is permitted only in the Development and " +
                "Testing environments.");
        }

        // SPEC-0092/ADR-0033: Azure Database for PostgreSQL Flexible Server uses passwordless Entra
        // auth (ADR-0031) — the connection string carries no password, so Npgsql must present an Entra
        // access token (obtained via the container's managed identity) as the password. Build the data
        // source once with a periodic token provider. Local Postgres (with a password) and SQLite are
        // unaffected, so `dotnet run` and tests keep working with no Azure dependency at runtime.
        NpgsqlDataSource? azurePgDataSource = null;
        if (!string.IsNullOrWhiteSpace(connectionString)
            && IsPasswordlessAzurePostgres(connectionString))
        {
            azurePgDataSource = BuildAzureAdNpgsqlDataSource(connectionString);
            services.AddSingleton(azurePgDataSource);
        }

        // SPEC-0181 REQ-001: async-local site context for worker/design-time; API host overrides with HttpSiteContextAccessor.
        // Under Testing, suppress filters so the existing integration suite can seed/query without X-Site-Id on every
        // DbContext call — SiteQueryFilterTests exercises filters with an explicit AmbientSiteContextAccessor.
        if (string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<ISiteContextAccessor>(DesignTimeSiteContextAccessor.Instance);
        }
        else
        {
            services.AddSingleton<ISiteContextAccessor, AmbientSiteContextAccessor>();
        }

        services.AddDbContext<BtrosDbContext>((sp, options) =>
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlite("Data Source=btros-dev.db");
            }
            else if (connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else if (azurePgDataSource is not null)
            {
                options.UseNpgsql(azurePgDataSource);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }

            // SPEC-0181 REQ-005: the interceptor enforces context on reads in production. The SQLite test
            // suite seeds scoped rows directly via DbContext (without HTTP site middleware), and EF issues
            // internal SELECTs during SaveChanges — so registering it under Testing breaks hundreds of
            // integration tests without adding coverage (SiteQueryFilterTests exercises filters explicitly).
            if (!string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
            {
                options.AddInterceptors(new SiteContextQueryInterceptor(sp.GetRequiredService<ISiteContextAccessor>()));
            }
        });

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, Domain.Auth.AuthService>();
        services.AddScoped<ISiteRepository, EfSiteRepository>();
        services.AddSingleton<ISiteAccessEvaluator, SiteAccessEvaluator>();
        services.AddScoped<ISiteMembershipGuard, SiteMembershipGuard>();
        // SPEC-0177 REQ-009: the Airfob demo-API default is a convenience for Development/Demo/Testing
        // only. A production/staging instance with no configured BioStar base URL must surface the
        // integration as unconfigured (fail closed), never silently talk to the demo backend.
        var allowDemoBioStarDefault =
            allowSqliteFallback
            || string.Equals(environmentName, "Demo", StringComparison.OrdinalIgnoreCase);
        services.AddScoped<ISiteConfigResolver>(_ => new SiteConfigResolver(configuration, allowDemoBioStarDefault));
        services.AddScoped<ISiteService, SiteService>();

        services.Configure<BioStarOptions>(options =>
        {
            configuration.GetSection(BioStarOptions.SectionName).Bind(options);
            options.ApiKey = configuration["BIOSTAR_API_KEY"] ?? options.ApiKey;
        });
        services.AddSingleton<BioStarTokenCache>();
        services.AddHttpClient(nameof(BioStarHttpClient));
        services.AddHttpClient(AzureOpenAILanguageModelClient.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddHttpClient(AzureAiSearchRetrievalClient.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // SPEC-0206 / ADR-0049: Azure OpenAI adapter. Stub in Dev/Testing. A partial or illegal
        // binding fails startup; an absent binding does not (no caller yet) but CompleteAsync
        // still fails closed.
        var azureOpenAI = AzureOpenAIOptions.FromConfiguration(configuration);
        if (!allowSqliteFallback)
        {
            azureOpenAI.ThrowIfMisconfiguredAtStartup();
        }

        services.AddSingleton(azureOpenAI);
        if (allowSqliteFallback)
        {
            services.AddScoped<ILanguageModelClient, StubLanguageModelClient>();
        }
        else
        {
            services.AddScoped<ILanguageModelClient, AzureOpenAILanguageModelClient>();
        }

        // SPEC-0207 / ADR-0050: Azure AI Search RAG slice. Stub in Dev/Testing. A partial or
        // illegal binding fails startup; an absent binding does not (no caller yet) but
        // SearchAsync still fails closed. Does not reimplement SPEC-0206's model adapter.
        var azureAiSearch = AzureAiSearchOptions.FromConfiguration(configuration);
        if (!allowSqliteFallback)
        {
            azureAiSearch.ThrowIfMisconfiguredAtStartup();
        }

        services.AddSingleton(azureAiSearch);
        if (allowSqliteFallback)
        {
            services.AddScoped<IRetrievalClient, StubRetrievalClient>();
        }
        else
        {
            services.AddScoped<IRetrievalClient, AzureAiSearchRetrievalClient>();
        }

        services.AddScoped<IRagClient, RagGroundingService>();
        services.AddScoped<IBioStarClientFactory, BioStarClientFactory>();
        services.AddScoped<IBioStarSiteOperations, BioStarSiteOperations>();
        services.AddScoped<IBioStarDoorOperations, BioStarDoorOperations>();

        if (string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IBioStarUserOperations, StubBioStarUserOperations>();
            services.AddScoped<IBioStarEventOperations, StubBioStarEventOperations>();
            services.AddScoped<IBioStarProxyOperations, StubBioStarProxyOperations>();
        }
        else
        {
            services.AddScoped<IBioStarUserOperations, BioStarUserOperations>();
            services.AddScoped<IBioStarEventOperations, BioStarEventOperations>();
            services.AddScoped<IBioStarProxyOperations, BioStarProxyOperations>();
        }

        services.AddScoped<IResidentRepository, EfResidentRepository>();
        services.AddScoped<IVisitorRepository, EfVisitorRepository>();
        services.AddScoped<IDoorAccessRepository, EfDoorAccessRepository>();
        services.AddScoped<IResidentDataPurger, ResidentDataPurger>();
        // SPEC-0156: SAR export — the read-side mirror of the purger's table map.
        services.AddScoped<IResidentDataExportService, ResidentDataExportService>();
        services.AddScoped<IResidentService, ResidentService>();
        services.AddScoped<IVisitorService, VisitorService>();
        services.AddScoped<IServiceRequestRepository, EfServiceRequestRepository>();
        services.AddScoped<IServiceRequestService, ServiceRequestService>();
        services.AddScoped<IUnitRepository, EfUnitRepository>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<ITenancyRepository, EfTenancyRepository>();
        services.AddScoped<ITenancyService, TenancyService>();
        services.AddScoped<ITenancyNotifier, NullTenancyNotifier>();
        services.AddScoped<IPaymentRepository, EfPaymentRepository>();
        // SPEC-0166: transaction seam for exactly-once webhook processing (mark + mutation in one commit).
        services.AddScoped<BTROS.Domain.Abstractions.IUnitOfWork, BTROS.Infrastructure.Data.EfUnitOfWork>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentEventPublisher, NullPaymentEventPublisher>();
        if (allowSqliteFallback)
        {
            // Development/Testing: deterministic provider, no real Stripe calls (ADR-0021).
            services.AddScoped<IPaymentProvider, StubPaymentProvider>();
        }
        else
        {
            var stripeOptions = new StripePaymentOptions
            {
                SecretKey = configuration["Stripe:SecretKey"] ?? string.Empty,
                WebhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty,
            };
            services.AddSingleton(stripeOptions);
            services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        }

        // Community announcements (SPEC-0050): operator noticeboard authoring + resident feed.
        services.AddScoped<IAnnouncementRepository, EfAnnouncementRepository>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();

        // Parcels and package management (SPEC-0051): operator worklist + resident own-parcels.
        services.AddScoped<IParcelRepository, EfParcelRepository>();
        services.AddScoped<IParcelService, ParcelService>();

        // Operator analytics (SPEC-0056): derived aggregates over existing tables, site-scoped.
        services.AddScoped<IAnalyticsRepository, EfAnalyticsRepository>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // Multi-channel resident notifications (SPEC-0053, ADR-0025): device tokens, preferences, dispatch.
        services.AddScoped<IResidentDeviceTokenRepository, EfResidentDeviceTokenRepository>();
        services.AddScoped<IResidentNotificationPreferenceRepository, EfResidentNotificationPreferenceRepository>();
        services.AddScoped<ResidentNotificationService>();
        services.AddScoped<IResidentNotificationService>(sp => sp.GetRequiredService<ResidentNotificationService>());
        // The dispatcher is wrapped so its non-throwing contract (ADR-0025/REQ-007) is structurally enforced:
        // a relay/DB/broadcast failure is logged and swallowed and never fails the originating action.
        services.AddScoped<IResidentNotificationDispatcher>(sp => new ResilientResidentNotificationDispatcher(
            sp.GetRequiredService<ResidentNotificationService>(),
            sp.GetRequiredService<ILogger<ResilientResidentNotificationDispatcher>>()));

        // SPEC-0184 (ADR-0043): field encryption for the two secrets a database read would otherwise hand
        // over — the per-site BioStar admin password and the webhook secret. Outside Development/Testing the
        // key is MANDATORY: encryption that silently degrades to plaintext is the failure this guards
        // against, and it would be invisible until someone read the table.
        var fieldKey = configuration["Secrets:FieldEncryptionKey"];
        if (!string.IsNullOrWhiteSpace(fieldKey))
        {
            services.AddSingleton<ISecretProtector>(new AesGcmSecretProtector(Convert.FromBase64String(fieldKey)));
        }
        else if (!allowSqliteFallback)
        {
            throw new InvalidOperationException(
                "Secrets:FieldEncryptionKey is required outside Development/Testing (SPEC-0184). "
                + "Set a base64 32-byte key in Key Vault; without it the per-site BioStar admin password "
                + "and webhook secret would be stored in plaintext.");
        }

        // SPEC-0186 (ADR-0042): the outbox. Registered as both interfaces from one instance so the enqueue
        // side shares the caller's DbContext — an outbox row must commit in the same transaction as the
        // state change that caused it, which is the whole reason it exists.
        services.AddScoped<EfOutboxStore>();
        services.AddScoped<IOutbox>(sp => sp.GetRequiredService<EfOutboxStore>());
        services.AddScoped<IOutboxStore>(sp => sp.GetRequiredService<EfOutboxStore>());

        // SPEC-0155 (ADR-0040): the audit log's write side is best-effort — same structural wrapping.
        // SPEC-0185 REQ-007: security-relevant writes use the raw sink via ISecurityAuditLog.
        services.AddScoped<EfAuditLog>();
        services.AddScoped<ISecurityAuditLog>(sp => sp.GetRequiredService<EfAuditLog>());
        services.AddScoped<IAuditLog>(sp => new ResilientAuditLog(
            sp.GetRequiredService<EfAuditLog>(),
            sp.GetRequiredService<ILogger<ResilientAuditLog>>()));
        if (allowSqliteFallback)
        {
            // Development/Testing: deterministic transport, no real FCM/APNs/email (ADR-0025).
            services.AddScoped<IResidentPushTransport, StubResidentPushTransport>();
        }
        else
        {
            // SPEC-0122 / ADR-0037: prefer the Firebase Admin (FCM) transport when a service-account
            // credential is configured; otherwise fall back to the generic relay seam.
            var fcmCredentialsJson = configuration["ResidentPush:FirebaseCredentialsJson"] ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(fcmCredentialsJson))
            {
                services.AddSingleton(new FcmPushOptions { ServiceAccountJson = fcmCredentialsJson });
                services.AddScoped<IResidentPushTransport, FcmResidentPushTransport>();
            }
            else
            {
                var relayOptions = new ResidentPushRelayOptions
                {
                    RelayUrl = configuration["ResidentPush:RelayUrl"] ?? string.Empty,
                    ApiKey = configuration["ResidentPush:ApiKey"] ?? string.Empty,
                };
                services.AddSingleton(relayOptions);
                services.AddScoped<IResidentPushTransport, HttpResidentPushTransport>();
            }
        }

        // Email/phone verification + password reset (SPEC-0058, ADR-0026): hashed OTP/reset artifacts + a
        // transactional messaging provider (stub in Dev/Testing, real relay elsewhere).
        services.AddScoped<IVerificationCodeRepository, EfVerificationCodeRepository>();
        services.AddSingleton<IOtpHasher, Sha256OtpHasher>();
        // SPEC-0177 REQ-010: the public web origin used in outbound email links/assets (PublicWeb:BaseUrl).
        services.AddSingleton<IPublicWebLinks, ConfiguredPublicWebLinks>();
        services.AddScoped<IAccountVerificationService, AccountVerificationService>();
        if (allowSqliteFallback)
        {
            // Development/Testing: deterministic, no real email/SMS send (ADR-0026).
            services.AddScoped<IMessagingProvider, StubMessagingProvider>();
        }
        else
        {
            // SPEC-0102 / ADR-0034: prefer Azure Communication Services email when configured;
            // otherwise fall back to the generic relay seam (ADR-0026). Both degrade to Skipped when
            // unconfigured, so an unprovisioned environment never errors.
            var acsEmailOptions = new AcsEmailOptions
            {
                ConnectionString = configuration["Email:Acs:ConnectionString"] ?? string.Empty,
                SenderAddress = configuration["Email:Acs:SenderAddress"] ?? string.Empty,
            };
            if (acsEmailOptions.IsConfigured)
            {
                services.AddSingleton(acsEmailOptions);
                services.AddScoped<IMessagingProvider, AcsEmailMessagingProvider>();
            }
            else
            {
                var messagingOptions = new MessagingRelayOptions
                {
                    RelayUrl = configuration["Messaging:RelayUrl"] ?? string.Empty,
                    ApiKey = configuration["Messaging:ApiKey"] ?? string.Empty,
                };
                services.AddSingleton(messagingOptions);
                services.AddScoped<IMessagingProvider, HttpMessagingProvider>();
            }
        }

        // Resident documents (SPEC-0052, ADR-0024): metadata + object-storage signed-URL boundary.
        services.AddScoped<IDocumentRepository, EfDocumentRepository>();
        services.AddScoped<IDocumentService, DocumentService>();
        if (allowSqliteFallback)
        {
            // Development/Testing: deterministic fake signed URLs, no real Blob Storage (ADR-0024).
            services.AddScoped<IDocumentStore, StubDocumentStore>();
        }
        else
        {
            var blobOptions = new AzureBlobDocumentOptions
            {
                AccountName = configuration["DocumentStorage:AccountName"] ?? string.Empty,
                AccountKey = configuration["DocumentStorage:AccountKey"] ?? string.Empty,
                ContainerName = configuration["DocumentStorage:Container"] ?? "documents",
            };
            services.AddSingleton(blobOptions);
            services.AddScoped<IDocumentStore, AzureBlobDocumentStore>();
        }

        // Resident onboarding / move-in (SPEC-0054): orchestration tracker + PMS reconciler.
        services.AddScoped<IMoveInRepository, EfMoveInRepository>();
        services.AddScoped<OnboardingService>();
        services.AddScoped<IOnboardingService>(sp => sp.GetRequiredService<OnboardingService>());
        services.AddScoped<IMoveInReconciler>(sp => sp.GetRequiredService<OnboardingService>());

        // Resident feedback + service-request threading (SPEC-0055).
        services.AddScoped<IFeedbackRepository, EfFeedbackRepository>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IServiceRequestThreadRepository, EfServiceRequestThreadRepository>();
        services.AddScoped<IServiceRequestThreadService, ServiceRequestThreadService>();

        // Amenities and facility bookings (SPEC-0049): atomic capacity claim (ADR-0023) + real-time notifier.
        services.AddScoped<IAmenityRepository, EfAmenityRepository>();
        services.AddScoped<IBookingRepository, EfBookingRepository>();
        services.AddScoped<IAmenityService, AmenityService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddSingleton<IBookingNotifier, NullBookingNotifier>();

        // Resident identity verification (SPEC-0048): status-only record + tenancy-activation gate.
        services.AddScoped<IIdentityVerificationRepository, EfIdentityVerificationRepository>();
        services.AddScoped<IIdentityVerificationService, IdentityVerificationService>();
        services.AddScoped<IResidentVerificationGate, IdentityVerificationGate>();
        if (allowSqliteFallback)
        {
            // Development/Testing: deterministic provider, no real Stripe Identity calls (ADR-0022).
            services.AddScoped<IIdentityVerificationProvider, StubIdentityVerificationProvider>();
        }
        else
        {
            var stripeIdentityOptions = new StripeIdentityOptions
            {
                SecretKey = configuration["Stripe:SecretKey"] ?? string.Empty,
                WebhookSecret = configuration["Stripe:IdentityWebhookSecret"] ?? string.Empty,
            };
            services.AddSingleton(stripeIdentityOptions);
            services.AddScoped<IIdentityVerificationProvider, StripeIdentityVerificationProvider>();
        }
        services.AddScoped<IEventRepository, EfEventRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddSingleton<IEventBroadcaster, NullEventBroadcaster>();
        services.AddScoped<INotificationRepository, EfNotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationGenerator, NotificationGenerator>();
        services.AddSingleton<INotificationBroadcaster, NullNotificationBroadcaster>();
        services.AddScoped<IWebhookSecretResolver, ConfigurationWebhookSecretResolver>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IAccessLevelService, AccessLevelService>();
        services.AddScoped<IWaitlistRepository, EfWaitlistRepository>();
        services.AddScoped<IWaitlistService, WaitlistService>();
        services.AddScoped<IPmsWebhookProcessor, PmsWebhookProcessor>();

        // Reserved seam pending PMS vendor selection (SPEC-0017). No production code consumes
        // IPmsOutboundClient yet, so register the no-op stub only under Testing — production hosts
        // do not carry a silent no-op outbound client (SPEC-0038 REQ-005).
        if (string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IPmsOutboundClient, StubPmsOutboundClient>();
        }

        services.AddSingleton<IPmsSignatureValidator, PmsSignatureValidator>();

        return services;
    }

    // SPEC-0092: an Azure Database for PostgreSQL Flexible Server host with no password in the
    // connection string means passwordless Entra auth (ADR-0031) — the app authenticates with a
    // managed-identity access token instead of a password.
    private static bool IsPasswordlessAzurePostgres(string connectionString) =>
        connectionString.Contains("postgres.database.azure.com", StringComparison.OrdinalIgnoreCase)
        && !connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase);

    // Builds an Npgsql data source that authenticates to Azure PostgreSQL with an Entra (Azure AD)
    // access token, refreshed periodically. DefaultAzureCredential honours AZURE_CLIENT_ID, so it
    // uses the container app's user-assigned managed identity in Azure (and a developer's az/VS login
    // elsewhere). The token is fetched lazily on first connection.
    private static NpgsqlDataSource BuildAzureAdNpgsqlDataSource(string connectionString)
    {
        var credential = new DefaultAzureCredential();
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UsePeriodicPasswordProvider(
            async (_, cancellationToken) =>
            {
                var token = await credential.GetTokenAsync(
                    new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]),
                    cancellationToken).ConfigureAwait(false);
                return token.Token;
            },
            TimeSpan.FromMinutes(55),
            TimeSpan.FromSeconds(5));
        return builder.Build();
    }
}
