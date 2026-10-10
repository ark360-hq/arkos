---
id: SPEC-0012
slug: intelligence-module
title: Reusable Intelligence module
status: Approved
owner: "@ark360-hq"
created: 2026-10-10
touches-personal-data: true
trust-boundaries-crossed: true
linked-adrs: [ADR-0005, ADR-0006]
linked-threat-model: .arkos/threat-models/intelligence-module.md
---

# SPEC-0012: Reusable Intelligence module

GitHub issue: [#18](https://github.com/ark360-hq/arkos/issues/18).

## Problem

ARK360 products that need product AI each stand up Azure OpenAI, Azure AI Search, and their own call path. Identity and residency rules then split. Adopters of ARK OS have no reusable module for the governed chat and retrieve-then-ground seams.

James Park approved, on 10 October 2026, extracting those seams so other ARK OS products reuse them. The landed sources are [ark360-hq/btros#472](https://github.com/ark360-hq/btros/pull/472) and [ark360-hq/btros#473](https://github.com/ark360-hq/btros/pull/473) on btros main `a04a31a`. Those sources are in-process C# adapters. They do not provision Azure resources and they do not expose a product HTTP `/v1` gateway. A fork that then diverges, or a rewrite as Bicep, would recreate the same split.

## Out of scope

1. Copying the full btros application (operator console, residents, payments, BioStar, or other product features) into this template.
2. Forking btros and developing a separate Intelligence tree that then diverges.
3. Issue #19 gate checks for non-Azure product AI and missing gateway audit.
4. Merging to `main`, creating a git tag, or publishing a GitHub Release.
5. Creating billable Azure, Entra, or Stripe resources, including provisioning Azure OpenAI or Azure AI Search.
6. Template Bicep for Azure OpenAI or Azure AI Search.
7. A product HTTP `/v1` gateway, embeddings deployment, or eval goldens.
8. Production deploys and the v0.3 Microsoft agentic module.

## Requirements

### Ubiquitous (always true, no trigger)

> REQ-001: The repository shall contain this Approved spec for a reusable Intelligence module that covers the in-process Azure OpenAI chat adapter, the Azure AI Search retrieve-then-ground slice, and the governed client seams.

> REQ-002: The Intelligence module shall be extracted from btros main `a04a31a` (pulls #472 and #473). It shall not be a fork that then diverges.

> REQ-003: The Intelligence module shall include documentation and a worked example that an adopter can follow without reading the btros repository.

> REQ-004: Product AI calls that use this module shall go through `ILanguageModelClient`, `IRetrievalClient`, and `IRagClient`, not a raw Azure OpenAI or Azure AI Search SDK.

> REQ-005: Logs, metrics, and status snapshots shall not contain prompt text, completion text, query text, document content, or API keys. The adapters shall not emit an accept/edit/reject audit; product callers own that trail.

> REQ-006: `CHANGELOG.md` `[Unreleased]` shall record this spec and shall reference SPEC-0012.

### Event-driven (triggered by an event)

> REQ-007: WHEN the btros Azure OpenAI adapter and RAG slice have landed, the implementation shall extract the in-process C# core from those sources rather than rewriting them as Bicep or an HTTP `/v1` gateway.

> REQ-008: WHEN this spec lands, the repository shall include ADR-0005, ADR-0006, a STRIDE-lite threat model at `.arkos/threat-models/intelligence-module.md`, and contracts for the in-process seams plus the Azure OpenAI and Azure AI Search REST shapes.

> REQ-014: The Intelligence module shall live at `modules/intelligence/` and its README and SOURCE.md shall state extract-from-btros, cite btros commit `a04a31a`, and forbid a diverging fork.

> REQ-015: The module shall include `ILanguageModelClient`, `IRetrievalClient`, `IRagClient`, `RagGroundingService`, `RagPromptComposer`, `AzureOpenAIGuard`, `AzureAiSearchGuard`, HttpClient adapters, options that fail closed on partial or illegal config, Development/Testing stubs, and tests.

> REQ-016: Azure OpenAI shall accept only `TenantMode=Client`, `Region` in `{australiaeast, australiasoutheast}`, `DeploymentType=Standard` (regional), and `https://*.openai.azure.com/` endpoints.

> REQ-017: Azure AI Search shall accept only `TenantMode=Client`, `Region` in `{australiaeast, australiasoutheast}`, and `https://*.search.windows.net/` endpoints. Every search shall include a required `siteId` equality filter, default `top` 5, capped at 20. Mismatched `siteId` hits shall be dropped.

> REQ-018: Auth shall be an `api-key` header or `DefaultAzureCredential`. Development and Testing shall register stubs that perform no outbound HTTP.

### State-driven (active while in a state)

> REQ-009: WHILE the host environment is Development or Testing the module shall register deterministic stubs and shall not send outbound HTTP.

### Optional feature (conditional on feature being present)

> REQ-010: WHERE a later machine-readable contract is taken from btros, it shall extend the Azure OpenAI and Azure AI Search contracts without adding a product HTTP `/v1` gateway or template Bicep.

### Unwanted behaviour (what must not happen)

> REQ-011: IF implementation is proposed as template Bicep, an HTTP `/v1` gateway, an embeddings deployment, or eval goldens THEN the agent shall NOT land that implementation.

> REQ-012: IF a change would create a billable Azure, Entra, or Stripe resource THEN the change shall NOT create it.

> REQ-013: IF this spec is applied THEN the change shall NOT implement issue #19.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | `.arkos/specs/0012-intelligence-module.md` exists with `status: Approved` and names the in-process adapters and seams | manual |
| REQ-002 | README and SOURCE.md state extract-from-btros, cite `a04a31a`, and forbid a diverging fork | `modules/intelligence/tests/check-module.sh` |
| REQ-003 | `modules/intelligence/examples/worked-example/README.md` does not require cloning btros | `modules/intelligence/tests/check-module.sh` |
| REQ-004 | Domain project has no Azure SDK package; adapters use HttpClient | `dotnet test` |
| REQ-005 | Adapter tests assert logs omit prompt, completion, query, chunk text, and API key | `dotnet test` |
| REQ-006 | `CHANGELOG.md` `[Unreleased]` contains SPEC-0012 | `modules/intelligence/tests/check-module.sh` |
| REQ-007 | SOURCE.md cites btros#472, btros#473, and commit `a04a31a`; no Bicep and no `/v1/complete` in the module | `modules/intelligence/tests/check-module.sh` |
| REQ-008 | ADR-0006, threat model, and `.arkos/contracts/azure-openai-chat.md` plus `.arkos/contracts/azure-ai-search.md` exist | `modules/intelligence/tests/check-module.sh` |
| REQ-009 | Dev/Testing registration tests resolve stubs | `dotnet test` |
| REQ-010 | No `intelligence-gateway.openapi.yaml` product `/v1` contract | `modules/intelligence/tests/check-module.sh` |
| REQ-011 | Module contains no `.bicep` files | `modules/intelligence/tests/check-module.sh` |
| REQ-012 | This change creates no Azure, Entra, or Stripe resources | manual |
| REQ-013 | No SPEC-0013 file and no issue #19 gate checks | `modules/intelligence/tests/check-module.sh` |
| REQ-014 | `modules/intelligence/README.md` and `SOURCE.md` exist with provenance | `modules/intelligence/tests/check-module.sh` |
| REQ-015 | Domain and infrastructure types listed in REQ-015 exist | `dotnet test` |
| REQ-016 | Guard and adapter tests reject `eastus`, `Shared`, `GlobalStandard`, and `api.openai.com` | `dotnet test` |
| REQ-017 | Search tests require `siteId`, default/clamp top, and drop mismatched hits | `dotnet test` |
| REQ-018 | Valid binding posts with `api-key`; stubs do not call HTTP | `dotnet test` |

## Privacy notes

The module does not collect personal information by itself. Products that adopt it will send prompts and site documents to the **customer's** Azure OpenAI and Azure AI Search resources in an Australian region. Those payloads may contain personal information.

| Principle | Obligation | How addressed |
|---|---|---|
| APP 1 | Open and transparent management of personal information | Adopter products must say in their own privacy notice that product AI may send prompts and documents to the customer's Australian-region Azure OpenAI and Azure AI Search resources. This spec does not add a public-facing collection notice in ARK OS. |
| APP 5 | Notification of collection of personal information | Only the documented Azure services in the customer's tenant are in scope. Shared or platform tenants are rejected. Adopters notify their users at the product boundary. |
| APP 11 | Security of personal information | Client-tenant only; `australiaeast` / `australiasoutheast` only; HTTPS host allowlists; mandatory site filter; secrets in the adopter store; logs omit content and keys. No billable Azure resources are created by this repository. |

## Accessibility notes

Not applicable. This spec produces no user interface. Module documentation is prose. The worked example is registration and call-path guidance, not an interactive UI.
