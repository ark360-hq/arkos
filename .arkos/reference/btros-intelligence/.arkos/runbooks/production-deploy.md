# Runbook — Production deployment (SPEC-0083 / ADR-0031)

Continuous deployment: a green `arkos` run on `main` triggers `.github/workflows/deploy.yml`, which
waits for approval on the `production` GitHub Environment, then runs `azd provision` + `azd deploy`
to Azure Container Apps (api, web, gateway) with a Postgres Flexible Server + Key Vault.

The pipeline is in code. The **one-time setup below is operator work** (creating Azure identities,
assigning roles, and storing secrets) — it cannot be automated from the repo.

## 1. One-time Azure setup

Pick the subscription + region. Then create a deploy identity with an OIDC **federated credential**
scoped to this repo's `production` environment, and grant it least-privilege roles.

```bash
# Variables
SUB=<azure-subscription-id>
RG=rg-production                # azd creates rg-$AZURE_ENV_NAME (AZURE_ENV_NAME=production)
LOC=australiaeast
APP_NAME=btros-deploy
REPO=ark360-hq/btros           # the repo lives in the ark360 GitHub Enterprise org

az account set --subscription "$SUB"

# App registration + service principal for the deploy identity
APP_ID=$(az ad app create --display-name "$APP_NAME" --query appId -o tsv)
az ad sp create --id "$APP_ID"
OBJ_ID=$(az ad sp show --id "$APP_ID" --query id -o tsv)

# Federated credential: only GitHub Actions running in THIS repo's `production` environment
# can obtain a token (matches the workflow's `environment: production`).
az ad app federated-credential create --id "$APP_ID" --parameters '{
  "name": "github-production",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:'"$REPO"':environment:production",
  "audiences": ["api://AzureADTokenExchange"]
}'

# Least-privilege roles on the subscription (or scope to the RG once it exists)
az role assignment create --assignee "$APP_ID" --role "Contributor" --scope "/subscriptions/$SUB"
az role assignment create --assignee "$APP_ID" --role "Key Vault Secrets Officer" --scope "/subscriptions/$SUB"
az role assignment create --assignee "$APP_ID" --role "AcrPush" --scope "/subscriptions/$SUB"

echo "AZURE_CLIENT_ID=$APP_ID"
az account show --query '{tenantId:tenantId, subId:id}' -o tsv
```

## 2. First provision (bootstrap the env + generate infra)

From a machine with `azd` + the above identity (or `azd auth login` as yourself):

```bash
azd env new production --subscription "$SUB" --location "$LOC"
# Regenerate infra so the committed Bicep includes the Postgres + Key Vault from the AppHost
# (SPEC-0087 committed this; only re-run after AppHost infra changes):
azd config set alpha.infraSynth on
azd infra generate       # review the diff under infra/ + src/BTROS.AppHost/infra/ and COMMIT it (PR)
# Secret params (only needed for a LOCAL provision; the pipeline sets these from GitHub secrets):
azd env set AZURE_JWT_SECRET "$(openssl rand -base64 48)"
azd env set AZURE_INTERNAL_SERVICE_KEY "$(openssl rand -base64 32)"
azd provision            # stands up ACR, Container Apps env, Log Analytics, Postgres, Key Vault
azd deploy               # first rollout of api, web, gateway
```

Add a bicep output `WEB_BASE_URL` (the web container app's FQDN) so the pipeline's `/health` smoke
check runs; without it the check is skipped with a warning.

## 3. GitHub configuration

**Repository → Settings → Secrets and variables → Actions → Variables:**

| Variable | Value |
|---|---|
| `AZURE_CLIENT_ID` | the `$APP_ID` above |
| `AZURE_TENANT_ID` | your tenant id |
| `AZURE_SUBSCRIPTION_ID` | `$SUB` |
| `AZURE_ENV_NAME` | `production` |
| `AZURE_LOCATION` | e.g. `australiaeast` |

**Secrets** (masked): `JWT_SECRET` (≥32 chars), `INTERNAL_SERVICE_KEY`. **`BIOSTAR_API_KEY`** is an
**optional** Actions secret — when set, the pipeline delivers it to the `api` container so the BioStar
integration can be configured (see "BioStar Air" below); when unset, BioStar stays unconfigured and the
deploy still succeeds. Other integration secrets (`Stripe__*`, `DocumentStorage__*`, push/messaging
relays) should be stored in **Key Vault** (`az keyvault secret set`) using `--`-for-`:` names
(e.g. `BioStar--ApiKey`); they stay stubbed until set.

**Repository → Settings → Environments → New environment `production`:** add a **required reviewer**
(you). This is the deploy approval gate.

## 4. Deploy flow (steady state)

1. Merge a PR to `main`. `arkos` runs the gates.
2. On success, `deploy` starts and **pauses for approval** on the `production` environment.
3. Approve in GitHub → `azd provision` (idempotent) + `azd deploy` run → `/health` smoke check.

Manual/dry-run: **Actions → deploy → Run workflow** (`workflow_dispatch`), optionally with a
`staging` env name to validate before production.

## 5. Rollback

```bash
# Fastest: shift traffic to the previous Container Apps revision
az containerapp revision list -n <web-app> -g "$RG" -o table
az containerapp ingress traffic set -n <web-app> -g "$RG" --revision-weight <prev-revision>=100
# Or redeploy a known-good commit
git checkout <good-sha> && azd deploy
```

## Seeding demo data (pooled demo instance, SPEC-0091)

The prod DB starts with schema only. To seed the idempotent demo dataset (operator
`demo.ops@btros.local` / `Demo1234!`, resident roster, demo site, operator-wide data) on the
**demo** instance, set the opt-in flag on the api container app and let it restart:

```bash
az containerapp update -n api -g "rg-${AZURE_ENV_NAME}" \
  --set-env-vars DemoData__SeedOnStartup=true        # new revision restarts → seeds (idempotent)
# verify: sign in at the web URL as demo.ops@btros.local / Demo1234!
# when done, turn it back off:
az containerapp update -n api -g "rg-${AZURE_ENV_NAME}" --set-env-vars DemoData__SeedOnStartup=false
```

Off by default — never seed a real-customer deployment (the demo accounts use a known password).

## Custom domain — btros.ark360.com.au (SPEC-0099)

The web app is served at **https://btros.ark360.com.au** with an Azure-managed TLS cert.

**DNS (at the external `ark360.com.au` provider):**

| Type | Name | Value |
|---|---|---|
| CNAME | `btros` | `web.reddesert-01d2f0d0.australiaeast.azurecontainerapps.io` |
| TXT | `asuid.btros` | the web app's `customDomainVerificationId` (`az containerapp show -n web -g rg-production --query properties.customDomainVerificationId -o tsv`) |

**Bind (one-time; the pipeline re-asserts it idempotently every deploy):**

```bash
env="$(az containerapp env list -g rg-production --query '[0].name' -o tsv)"
az containerapp hostname add  -n web -g rg-production --hostname btros.ark360.com.au
az containerapp hostname bind -n web -g rg-production --hostname btros.ark360.com.au \
  --environment "$env" --validation-method CNAME      # creates + binds a managed cert (≤20 min)
```

`AllowedHosts` already includes the domain (SPEC-0094) and canonical/OG URLs are request-host-derived,
so no app config change is needed. **Durability:** `azd provision`/`deploy` reconcile the web app from
bicep + `web.tmpl.yaml` (neither declares the custom domain), so the deploy workflow has a
*Re-assert custom domain binding* step that re-adds + re-binds only when it is not already
`SniEnabled` — the domain survives every deploy without manual intervention.

## Public API subdomain — api.btros.ark360.com.au (SPEC-0113)

The **api** is internet-facing (ingress `external: true`) so BioStar/Stripe **webhooks** and the
**resident mobile app** can reach it; the operator console still uses btros.ark360.com.au → BFF → api
over the internal hop. Protected by JWT auth, per-webhook secret validation, `AllowedHosts`, and a
per-IP auth rate limiter (SPEC-0113).

**DNS (at the external `ark360.com.au` provider):**

| Type | Name | Value |
|---|---|---|
| CNAME | `api.btros` | the api app's FQDN (`az containerapp show -n api -g rg-production --query properties.configuration.ingress.fqdn -o tsv`) |
| TXT | `asuid.api.btros` | the api app's `customDomainVerificationId` (`az containerapp show -n api -g rg-production --query properties.customDomainVerificationId -o tsv`) |

**Bind:** the deploy workflow's *Re-assert API custom domain binding* step adds + binds the managed
cert idempotently every deploy (warns, never fails, until the DNS records exist). To bind manually once
the DNS is in place:

```bash
env="$(az containerapp env list -g rg-production --query '[0].name' -o tsv)"
az containerapp hostname add  -n api -g rg-production --hostname api.btros.ark360.com.au
az containerapp hostname bind -n api -g rg-production --hostname api.btros.ark360.com.au \
  --environment "$env" --validation-method CNAME
```

**After it resolves:** verify `https://api.btros.ark360.com.au/health` → 200, then re-point the BioStar
webhook (Settings → Webhooks) and Stripe webhook to `https://api.btros.ark360.com.au/...`, and use this
host as the mobile app's API base URL (SPEC-0115).

## Demo instance config posture (SPEC-0099)

What a pooled-demo instance needs (the public-domain demo, not a real-customer deployment):
- `JWT_SECRET` / `INTERNAL_SERVICE_KEY` — strong values from GitHub secrets (seeded into Container Apps
  by the pipeline; never the dev defaults — see SPEC-0087).
- `DemoData:SeedOnStartup=true` to seed, then back to `false` (above). Demo accounts use the **known**
  password `Demo1234!` — acceptable only for synthetic demo data on a throwaway instance.
- BioStar Air is **stubbed** in demo (no real credentials needed); BioStar admin creds, if used, live in
  the gitignored `appsettings.Development.json` locally, never committed. The seeder defaults a
  placeholder BioStar site id when `BIOSTAR_SITE_ID` is unset (SPEC-0098), so seeding never no-ops.
- Stripe / storage / relays are stubbed/fixture-backed in demo.

## BioStar Air (access control) — real integration (SPEC-0108)

BioStar/Webhooks need the global **`BIOSTAR_API_KEY`** (sent as the `moca-api-key` header) **and** the
per-site connection fields. The pipeline now delivers the key to the `api` container as an optional
secured parameter (`biostar-api-key`), so the only steps are:

1. **Add the GitHub Actions secret** `BIOSTAR_API_KEY` (repo → Settings → Secrets and variables →
   Actions → New repository secret) with the BioStar Air / Airfob developer API key. (Unset = BioStar
   stays unconfigured; the deploy still succeeds.) Do this **before** approving the next deploy.
2. **Redeploy** (push to `main` or re-run `deploy`) so the api container picks up `BIOSTAR_API_KEY`.
   Verify: `az containerapp show -n api -g rg-production --query "properties.template.containers[0].env[?name=='BIOSTAR_API_KEY']"` shows a `secretRef`.
3. In the operator UI **Settings → BioStar connection**, set the **API base URL** (real Airfob tenant,
   or the demo `https://sp-demo-api.airfob.com/v1`), **admin email**, and **admin password**, then
   **Save BioStar config**. (`BioStarSiteId` is fixed at site creation.)
4. Click **Test connection** — it calls BioStar `GET /sites/{bioStarSiteId}`; success confirms the API
   key + credentials are all valid. (A `503` means a field or the API key is still missing.)
5. **Settings → Webhooks**: set the callback URL `https://btros.ark360.com.au/api/webhooks/events` +
   a strong secret, then **Register webhook** (registers with BioStar; the secret is stored and
   verified on inbound events). Webhooks require BioStar to be configured first.

Notes: the per-site admin password is stored masked (`••••••••` in the UI, never returned); leave it
blank on later saves to keep the existing value. Rotating the key = update the Actions secret + redeploy.

## Transactional email — Azure Communication Services (SPEC-0102 / ADR-0034 / SPEC-0204)

Email verification + password-reset send via ACS **when configured**; until then delivery safely falls
back to a no-op (the flows still work, no email leaves). **SPEC-0204** provisions the Communication
Service, Email Communication Service, and Azure Managed Domain from `infra/acs-email/` and writes
`Email--Acs--ConnectionString` + `Email--Acs--SenderAddress` into the environment Key Vault on
`azd provision`. Restart the api after the first provision so it re-reads the vault.

A custom sender domain (DNS + `CustomerManaged`) is still an operator follow-up; the managed domain
sends as `DoNotReply@<guid>.azurecomm.net`. Do not create a second ACS account by hand unless you
intend to replace the Bicep-managed one — a later provision overwrites those Key Vault secrets.

When `Email:Acs:ConnectionString` + `Email:Acs:SenderAddress` are both set (non-Development), the
`AcsEmailMessagingProvider` is selected automatically; otherwise the generic relay seam is used.
ACS **SMS** (phone-channel verification) is not implemented yet — a follow-up.

## Pilot integrations via Key Vault (SPEC-0114 / ADR-0036)

The api now loads the **`secrets` Key Vault** into configuration (managed identity), so every
integration secret is supplied by setting a Key Vault secret — **no per-secret deploy plumbing**. Key
Vault secret names use `--` where config uses `:` (e.g. `Stripe--SecretKey` → `Stripe:SecretKey`).
After setting secrets, **restart the api** (new revision) to re-read them.

```bash
kv="$(az keyvault list -g rg-production --query '[0].name' -o tsv)"
# Email (ACS) and document storage (Blob) are written by infra/acs-email and infra/blob-storage
# on azd provision (SPEC-0204). Do not paste replacement values here unless you are rotating
# away from the Bicep-managed resources. Confirm with:
#   az keyvault secret show --vault-name "$kv" --name Email--Acs--SenderAddress --query name
#   az keyvault secret show --vault-name "$kv" --name DocumentStorage--AccountName --query name
# Payments (Stripe TEST for the pilot) — keys from the Stripe dashboard; webhook → api subdomain:
az keyvault secret set --vault-name "$kv" --name Stripe--SecretKey     --value "sk_test_..."
az keyvault secret set --vault-name "$kv" --name Stripe--WebhookSecret --value "whsec_..."   # webhook endpoint: https://api.btros.ark360.com.au/api/webhooks/stripe
# Resident push — Android FCM (SPEC-0122): Firebase console -> Project settings -> Service accounts ->
# Generate new private key -> the downloaded JSON is the credential. Store the WHOLE json as one secret:
az keyvault secret set --vault-name "$kv" --name ResidentPush--FirebaseCredentialsJson --file <service-account>.json
# (legacy alternative: a generic relay via ResidentPush--RelayUrl / ResidentPush--ApiKey)
# iOS push (APNs) needs the paid Apple Developer Program — deferred. Verify push on a real Android device.
# Azure OpenAI (SPEC-0206): the customer's own AU-region regional-Standard GPT deployment.
# Do not point this at a shared or ARK360-owned model tenant. Do not use Global Standard.
# This repository does not provision the account. Restart the api after setting secrets.
az keyvault secret set --vault-name "$kv" --name AzureOpenAI--Endpoint --value "https://<customer-resource>.openai.azure.com/"
az keyvault secret set --vault-name "$kv" --name AzureOpenAI--Deployment --value "<deployment-name>"
az keyvault secret set --vault-name "$kv" --name AzureOpenAI--Region --value "australiaeast"
az keyvault secret set --vault-name "$kv" --name AzureOpenAI--TenantMode --value "Client"
az keyvault secret set --vault-name "$kv" --name AzureOpenAI--DeploymentType --value "Standard"
az keyvault secret set --vault-name "$kv" --name AzureOpenAI--ApiKey --value "<customer-key>"
# Omit AzureOpenAI--ApiKey to authenticate with the container managed identity instead.
# Azure AI Search (SPEC-0207): the customer's own AU-region search service + index.
# Do not point this at a shared or ARK360-owned search tenant. This repository does not
# provision the service. Restart the api after setting secrets.
az keyvault secret set --vault-name "$kv" --name AzureAiSearch--Endpoint --value "https://<customer-service>.search.windows.net/"
az keyvault secret set --vault-name "$kv" --name AzureAiSearch--IndexName --value "<index-name>"
az keyvault secret set --vault-name "$kv" --name AzureAiSearch--Region --value "australiaeast"
az keyvault secret set --vault-name "$kv" --name AzureAiSearch--TenantMode --value "Client"
az keyvault secret set --vault-name "$kv" --name AzureAiSearch--ApiKey --value "<customer-query-key>"
# Omit AzureAiSearch--ApiKey to authenticate with the container managed identity instead.
```

**Verify (no silent no-ops):** the api logs each integration's configured state at startup
(`Integration {Name}: configured / NOT configured`), and `GET /api/integrations/status` (Admin) returns
the booleans — confirm each shows configured before relying on it in the pilot. (BioStar is per-site —
verify via Settings → Test connection.) Secret values are never returned; rotation = update the secret +
restart the api.

## Staging environment (SPEC-0103)

A non-gating pre-prod environment driven by `.github/workflows/deploy-staging.yml`. It is **dormant**
until you enable it (the job is skipped while `STAGING_ENABLED` ≠ `true`, so no failed runs), then
deploys `main` to a separate azd env (`AZURE_ENV_NAME=staging` → its own `rg-staging-*`) on each green
`main`, seeds the demo dataset, and never gates production. One-time setup:

```bash
# 1. Reuse the prod subscription/OIDC app; add a federated credential for the staging environment:
az ad app federated-credential create --id "$APP_ID" --parameters '{
  "name":"github-staging","issuer":"https://token.actions.githubusercontent.com",
  "subject":"repo:ark360-hq/btros:environment:staging","audiences":["api://AzureADTokenExchange"]}'

# 2. Create the azd staging env + provision its resources (run locally once, or let the workflow do it):
azd env new staging --subscription "$AZURE_SUBSCRIPTION_ID" --location "$AZURE_LOCATION"
azd env set AZURE_JWT_SECRET "<staging-jwt>"; azd env set AZURE_INTERNAL_SERVICE_KEY "<staging-isk>"
azd provision

# 3. In GitHub: create a `staging` Environment (reviewer optional — leaving it off makes staging a true
#    auto-deploy dry-run), and set the repo variable STAGING_ENABLED=true to arm the workflow.
```

Staging carries demo data (`DemoData__SeedOnStartup=true`, set by the workflow); production stays clean.
Tear it down the same way as prod (`azd env select staging && azd down --purge`).

## EF Core migrations — one-time cutover (SPEC-0101 / ADR-0035)

Schema is now versioned by EF Core migrations: on startup the **PostgreSQL** path runs
`Database.MigrateAsync()` (the SQLite dev/test path keeps `EnsureCreated`). The existing prod DB was
created by `EnsureCreated` and has **no `__EFMigrationsHistory`**, so `MigrateAsync` would fail trying to
create tables that already exist. **Before the first migration-enabled deploy**, drop the prod schema
once (it holds only synthetic, re-seedable demo data):

```bash
# Connect to the Postgres Flexible Server as the Entra admin (e.g. via `psql` with an AAD token, or the
# Azure Portal query editor) and reset the public schema, then let the deploy + seeder rebuild it:
#   DROP SCHEMA public CASCADE; CREATE SCHEMA public;
# Then approve the deploy (MigrateAsync recreates the schema from InitialCreate), and re-seed:
az containerapp update -n api -g rg-production --set-env-vars DemoData__SeedOnStartup=true
```

After the cutover, future schema changes ship as new migrations (`dotnet ef migrations add <Name>
--project src/BTROS.Infrastructure --startup-project src/BTROS.Infrastructure`) applied automatically on
deploy. **Order matters:** drop the schema *before* approving the deploy that carries this change.

## Pilot ops basics — SLOs, alerting, on-call, backup drill (SPEC-0115 / SPEC-0177)

Lightweight operability for the pilot. Full multi-person rotation and DB HA are deferred (P2 backlog);
the numeric objectives, the single on-call contact, and the risk register below are the run-gate
baseline (CRT-023–026).

### 0. Service level objectives (pilot)

| SLO | Target | Measured by |
|---|---|---|
| API availability (`GET /health` on api.btros.ark360.com.au) | ≥ 99.5% monthly | Azure Monitor availability / Log Analytics over the health endpoint |
| API 5xx rate | < 1% of requests, monthly | Container Apps `Requests` metric, 5xx category (the alert below) |
| Console availability (`/login` renders) | ≥ 99.5% monthly | Azure Monitor availability test on the web app |
| Deploy recovery | Rollback to previous revision < 30 min | Rollback procedure in §5, rehearsed |

One metric per SLO exists (Requests/5xx + availability); one alert per failure mode is the 5xx alert
below plus an availability alert on `/health` (portal, Log Analytics query). Structured logs flow to
Log Analytics via the Container Apps environment (ILogger JSON console output).

**On-call (pilot):** James Park (`james.park@ark360.com.au`) — the `btros-oncall` action group below
pages this address. Broaden the action group as the team grows.

### 0.1 Go-live risk register

| Risk | Impact | Mitigation / plan |
|---|---|---|
| **API is pinned to one replica** (Airfob allows one session per admin account; the BioStar token cache + login gate are per-process — `api.tmpl.yaml` `minReplicas: 1 / maxReplicas: 1`, guarded by `ApiSingleReplicaTests`) | No horizontal scale; a replica restart is a brief outage (readiness probe holds traffic until warm) | Accepted for pilot scale. Unlock path: move the BioStar token cache + login single-flight to a shared store (Redis) so replicas share one Airfob session |
| **Unconfigured integrations fail at first use, not at startup** (Stripe/ACS/Blob/FCM register real providers even when unconfigured; startup logs a warning per gap and Admins see `GET /api/integrations/status`) | A missed secret surfaces as a runtime error on the first payment/email/upload | Check `/api/integrations/status` as a go-live step (pilot-onboarding doc); alarms catch the resulting 5xx |
| **48-hour operator JWT lifetime** (`JWT_EXPIRY`) | A leaked token is valid up to 48h | Mitigated by the per-request account-status re-check (SPEC-0164) — suspend/revoke takes effect immediately; shorter-lived tokens + refresh is post-MVP |
| **Single-region deployment, no DB HA** | Regional outage = downtime; RPO bounded by PITR backups | Accepted for pilot; backup-restore drill below is rehearsed |

### 1. Health / 5xx alert (Azure Monitor)

The api exposes an anonymous `GET /health` (returns `{ "status": "ok" }`). Wire a metric alert on the
api Container App's 5xx rate so a pilot incident pages you instead of being discovered by the operator.

```bash
# An action group that emails you (one-time):
az monitor action-group create -g rg-production -n btros-oncall \
  --short-name btros --action email primary james.park@ark360.com.au

# Alert: api 5xx responses over 5 in 5 min (adjust to taste). Scope = the api Container App resource id.
API_ID=$(az containerapp show -n api -g rg-production --query id -o tsv)
az monitor metrics alert create -g rg-production -n "api-5xx" \
  --scopes "$API_ID" \
  --condition "total Requests where StatusCodeCategory == '5xx' > 5" \
  --window-size 5m --evaluation-frequency 1m \
  --action btros-oncall \
  --description "BTROS api 5xx spike"
```

If the metric name differs in your region's Container Apps schema, create the alert in the portal
(Monitor → Alerts → Create) against the api app, signal = "Requests", split/filter on the 5xx status
category. A simple availability alert on the `/health` endpoint via a Log Analytics query works too.

### 2. PostgreSQL backup-restore drill (do this once before go-live)

The Flexible Server keeps automated backups; **a backup you have never restored is not a backup.**
Rehearse a point-in-time restore into a *throwaway* server (never overwrite prod), verify, then delete.

```bash
SRC=$(az postgres flexible-server list -g rg-production --query "[0].name" -o tsv)
# Restore to a new server as of 5 minutes ago (PITR):
az postgres flexible-server restore -g rg-production \
  --name "${SRC}-restoretest" --source-server "$SRC" \
  --restore-time "$(date -u -v-5M '+%Y-%m-%dT%H:%M:%SZ' 2>/dev/null || date -u -d '5 minutes ago' '+%Y-%m-%dT%H:%M:%SZ')"

# Verify: connect to the restored server (AAD token) and confirm core tables + row counts look sane,
# e.g. \dt and SELECT count(*) FROM "Residents"; — then tear the restore-test server down:
az postgres flexible-server delete -g rg-production --name "${SRC}-restoretest" --yes
```

Record the date of the last successful drill in the deploy log. Re-run after any major schema change.

## Teardown + cost

```bash
azd down --purge --force-and-purge      # deletes the azd-managed resources (incl. Key Vault purge)
# or, for the whole group:
az group delete --name rg-production --yes --no-wait
```

`--purge` is needed so the soft-deleted Key Vault name can be reused. Rough monthly cost of the
minimal demo footprint (australiaeast, 1 replica × 3 Container Apps on Consumption, a Burstable
Postgres Flexible Server, Basic ACR, Log Analytics): on the order of **~AUD 60–90/month** at idle —
dominated by the always-on `minReplicas: 1` and the Postgres compute. To cut cost without tearing down,
scale the apps to `minReplicas: 0` (cold-start tradeoff) and stop/downsize the Postgres server. Use
Azure Cost Management + a budget alert on `rg-production`.

## Notes / follow-ups
- Schema is created by `EnsureCreatedAsync()` on first start. **Add EF Core migrations before real
  production data** (separate spec) — `EnsureCreated` does not version or migrate.
- **Re-apply after `azd infra generate`:** the generated `infra/main.bicep` is hand-edited to append
  `;Username=${...MANAGED_IDENTITY_NAME}` to `BTROS_PG_CONNECTIONSTRING` (SPEC-0093 — the passwordless
  Entra connection needs the MI name as the PG role). `azd infra generate` overwrites `main.bicep`, so
  this edit (and the Postgres + Key Vault modules, SPEC-0087, and the Blob + ACS Email modules,
  SPEC-0204) must be re-applied after any regenerate.
  See `.arkos/threat-models/cd-pipeline.md` / the azd-CD gotchas. The custom-domain binding is instead
  made durable automatically by the *Re-assert custom domain binding* deploy step (above).
- **Web session affinity (SPEC-0125):** the web ingress sets `stickySessions.affinity: sticky`
  (`web.tmpl.yaml`) so multi-replica Blazor Server circuits survive websocket reconnects. Verify after a
  deploy: `az containerapp show -n web -g rg-production --query properties.configuration.ingress.stickySessions`
  → `{ "affinity": "sticky" }`. If a circuit still drops on a replica restart/scale-down the user just
  refreshes; the complete multi-replica fix is an Azure SignalR backplane (P2).
- **Demo mode durability (SPEC-0161, replacing the SPEC-0128 flag):** the pilot/demo instance runs as
  `ASPNETCORE_ENVIRONMENT=Demo` — the only thing that enables demo seeding, self-signup provisioning,
  and (SPEC-0177) the web signup surface (`Signup__Enabled=true` on the web app). Neither is declared
  in the tmpl.yaml files (they are reused per instance and customer instances run demo-off), so
  `deploy.yml` re-asserts both after each deploy (idempotent), like the custom-domain bind. Verify:
  `az containerapp show -n api -g rg-production --query "properties.template.containers[0].env[?name=='ASPNETCORE_ENVIRONMENT'].value"`
  → `Demo`. **Per-customer instances must NOT carry either** — they run with real data, demo off, and
  anonymous registration refused (SPEC-0177).
- `staging` environment (SPEC-0103): dormant until the repo variable `STAGING_ENABLED=true` arms
  `deploy-staging.yml`; see §Staging above. Arm it ahead of risky releases for a pre-prod dry run.
- Consider moving all app secrets to Key Vault references and adding CODEOWNERS-based approval as the
  team grows (see `.arkos/threat-models/cd-pipeline.md`).
