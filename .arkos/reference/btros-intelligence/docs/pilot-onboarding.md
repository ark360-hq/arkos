# ARK360 Pilot Onboarding

A step-by-step path to get the first operator from a deployed environment to a running pilot. This is the
"day one" runbook for the business; the infrastructure/secret steps live in the
[production deploy runbook](../.arkos/runbooks/production-deploy.md) and are cross-linked where needed.

**Customer pilots:** use [pilot-customer-onboarding.md](pilot-customer-onboarding.md) for provisioning
without demo seeding or database role flips. This document remains the operator-facing day-one guide once
the instance is live.

Audience: whoever stands up the pilot (you). Most steps are in the operator console; a few are one-time
Azure setup.

## 0. Prerequisites (one-time, infra)

These are done once per environment by whoever owns the Azure subscription — see the
[deploy runbook](../.arkos/runbooks/production-deploy.md):

- Platform deployed to Azure Container Apps; web on **https://btros.ark360.com.au**, API on
  **https://api.btros.ark360.com.au** (managed TLS).
- Integration secrets set in Key Vault and the API restarted (Email/ACS and Blob storage are written
  by Bicep on provision — SPEC-0204; Stripe **test** is still set by hand).
  Verify with the Admin endpoint `GET https://api.btros.ark360.com.au/api/integrations/status` — each
  should read `configured: true` (push is enabled later, with the mobile rollout). Azure OpenAI
  (`AzureOpenAI` on that status payload) is optional until a model caller ships: it must be the
  **customer's** Australian-region **regional Standard** GPT deployment (`australiaeast` or
  `australiasoutheast`, `TenantMode=Client`). A shared or Global Standard endpoint is rejected.
  Azure AI Search (`AzureAiSearch` on that status payload) is optional until a RAG caller ships:
  it must be the **customer's** Australian-region search service (`australiaeast` or
  `australiasoutheast`, `TenantMode=Client`). A shared tenant or non-AU region is rejected.
- The ACS sender display name is **ARK360** so emails don't read as "DoNotReply".

If those aren't done yet, do them first — the console works without them, but email/payments/document
upload won't function until their secrets are set.

## 1. Get your admin account

- **Guided demo:** the seeded demo admin is `demo.ops@btros.local` / `Demo1234!`. Use this for the demo
  environment only. **These known-password demo accounts must be rotated/removed before real resident
  data goes in** (see Go-live).
- **Real operator:** create your own account via **Create account** on the sign-in page, then have an
  existing admin set your role to **Admin** under **Users** (or set it directly for the first admin).

Sign in at **https://btros.ark360.com.au** and verify your email under **Profile**.

## 2. Configure the site (Settings → Site details)

As an Admin, open **Settings**:

- Set **name**, **address**, **country**, and **timezone**, then **Save site**.

## 3. Connect BioStar Air (Settings → BioStar connection)

Door access flows from BioStar Air. You'll need your BioStar Air admin credentials and the numeric
**site ID** from your BioStar Air account. The global API key is delivered as a secret (runbook).

1. Enter the **API base URL**, **admin email**, **admin password**, and **BioStar site ID**.
2. **Save BioStar config**.
3. **Test connection** — the status badge should turn **configured** and the test should succeed.
4. Under **Webhooks**, set the callback to `https://api.btros.ark360.com.au/api/webhooks/events` and a
   signing secret, then **Register webhook** — so door events flow back into BTROS.

Now the **Access** pages light up: use **Sync from BioStar** on Doors / Devices / Access Levels /
Activity to pull your access-control data.

## 4. Verify the live integrations

- **Email** — trigger a password reset for a test account and confirm a branded **ARK360** email arrives.
- **Payments (Stripe test)** — in Stripe, confirm the webhook endpoint
  `https://api.btros.ark360.com.au/api/webhooks/stripe` is set for `payment_intent.succeeded`,
  `payment_intent.payment_failed`, `charge.refunded`, `charge.dispute.created`.
- **Documents** — a resident document upload should store and re-open via a signed link.

## 5. Set up the building

In the console:

1. **Units** — add the building's units (number, building, floor).
2. **Users** — add your on-site managers (role **Manager**).
3. **Access Levels / Schedules** — confirm the access rules synced from BioStar match your building.

## 6. Onboard residents

1. **Residents → add a resident** (assign a unit + move-in date). This starts their move-in.
2. **Onboarding** — walk each resident through the six stages: confirm unit → documents →
   ID verification → lease → payment → key provisioning. Residents complete the first two in the app;
   you advance the rest. **Block** a stage with a reason if something's outstanding; **Finalise** once
   all six are complete (the system refuses to finalise early).
3. **Leasing** — record the tenancy (rent, cadence, dates).

## 7. Distribute the resident app

- Send residents the **TestFlight (iOS)** / **Play beta (Android)** invite link (see the mobile
  enablement spec/runbook once available).
- Residents sign in with the account you created; their digital key activates when move-in is finalised.

## 8. Demo / first-run script (end to end)

A good order to demonstrate the whole platform:

1. **Operator:** sign in → onboard a demo resident → finalise move-in.
2. **Resident (app):** sign in → open a door (cloud + BLE) → invite a visitor → pay rent with a Stripe
   **test card** → upload a document → receive the branded email / notification.
3. **Operator:** see the live door event in **Activity**, approve the uploaded document, log a parcel
   (resident gets notified), respond to a service request.
4. **Cross-system:** a BioStar door event → webhook → **Activity** feed → resident notification.

## 9. Go-live checklist

Before real residents and real data:

- [ ] Confirm the instance is **not** running as `Demo`. Instance mode derives solely from
      `ASPNETCORE_ENVIRONMENT` (SPEC-0178), so this one check covers all of it: no demo seeding, no
      `POST /api/admin/demo/reset`, no self-signup operator provisioning, and no danger zone in Settings.
      `GET /api/integrations/status` reports `"mode": "customer"` when it is right. There is no flag to
      turn off — the earlier instruction here named `DemoData:SeedOnStartup`, which SPEC-0161 deleted.
- [ ] Confirm no demo accounts exist (`demo.ops@btros.local` and the seeded roster, password
      `Demo1234!`). They can only have been created by a demo-mode instance; on a customer instance the
      seeder refuses to run.
- [ ] Switch Stripe from **test** to live keys (Key Vault) and update the webhook.
- [ ] Confirm each integration shows `configured` at `GET /api/integrations/status`.
- [ ] Enable resident **push** (with the mobile rollout).
- [ ] Review the post-pilot hardening backlog — the audit trail (SPEC-0155) and resident data
      export/erasure (SPEC-0134/0156) have shipped; what remains is a custom verified email sender
      domain for deliverability and DB HA — tracked as fast-follow.
- [ ] Run the backup-restore drill + set the health/5xx alert (see the runbook "Pilot ops basics").

## Related

- [Customer pilot provisioning](pilot-customer-onboarding.md) · [Operator guide](operator-guide.md) · [Resident app guide](resident-app-guide.md)
- [Production deploy runbook](../.arkos/runbooks/production-deploy.md) — infra, secrets, ops basics.
- [Disaster recovery runbook](../.arkos/runbooks/disaster-recovery.md) — RTO/RPO and restore steps.
