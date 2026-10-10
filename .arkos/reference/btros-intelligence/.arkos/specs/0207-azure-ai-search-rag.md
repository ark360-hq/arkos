---
id: SPEC-0207
slug: azure-ai-search-rag
title: Azure AI Search RAG slice (client tenant, Australian-region)
status: Approved
owner: "@parkjadev"
created: 2026-10-10
touches-personal-data: true
trust-boundaries-crossed: true
linked-adrs: [ADR-0050]
linked-threat-model: .arkos/threat-models/azure-ai-search-rag.md
---

# SPEC-0207: Azure AI Search RAG slice (client tenant, Australian-region)

## Problem

Nothing in `src/` can retrieve indexed site documents for grounded generation. Issue #466 is the
RAG slice: later intelligence work (I1 / #419, and the reusable Intelligence module in
ark360-hq/arkos#18) needs a fail-closed retrieve-then-ground path over **the customer's** Azure AI
Search service in an Australian region. A shared or ARK360-owned search tenant, or a service
outside `australiaeast` / `australiasoutheast`, would process client document text off-tenant or
off-residency.

Chat completions already have a seam in SPEC-0206 / ADR-0049 (draft PR #472). This spec must not
duplicate that adapter. It adds retrieval and prompt grounding only. James approved this
tech-strategy item on 10 Oct 2026.

## Out of scope

1. Azure OpenAI chat completions, embeddings, or any `*.openai.azure.com` client (SPEC-0206 /
   issue #465 / PR #472). This spec consumes that seam later; it does not reimplement it.
2. I1 drafting agent, operator review UI, MCP tools, or any product caller (issue #419).
3. Provisioning an Azure AI Search service, index, replica, semantic-ranker SKU, Private Link, or
   embedding deployment. This repository must not create that billable resource. The customer
   provisions the service in **their** subscription.
4. `Azure.Search.Documents` SDK, Semantic Kernel, or a second search vendor.
5. Document ingest UI, blob-to-index pipeline, or user-facing "AI-native" copy (SPEC-0189).

## Requirements

### Ubiquitous

> REQ-001: The system shall expose a provider-neutral `IRetrievalClient` and `IRagClient` in
> `BTROS.Domain` and an Azure AI Search HTTP adapter in `BTROS.Infrastructure` that is the only
> production retrieval implementation.

> REQ-002: The adapter shall query only an `https://*.search.windows.net/` resource over TLS 1.2+,
> using the Azure AI Search documents-search REST contract (`.arkos/contracts/azure-ai-search.md`).

### State-driven

> REQ-003: WHILE the host environment is Development or Testing the system shall register a
> deterministic retrieval stub that performs no outbound HTTP.

> REQ-004: WHILE Azure AI Search is configured the system shall require `TenantMode=Client` and
> `Region` in `{australiaeast, australiasoutheast}`.

### Event-driven

> REQ-005: WHEN any Azure AI Search setting is present outside Development/Testing and the binding
> is incomplete or violates REQ-004 / REQ-002, startup shall fail before the host accepts traffic.

> REQ-006: WHEN `SearchAsync` is invoked without a valid client-tenant AU binding, or without a
> non-empty site id, the adapter shall throw and shall NOT send a request.

> REQ-007: WHEN retrieval succeeds, `IRagClient.GroundAsync` shall compose a grounded prompt from
> the returned chunks (citations included) without calling a language model.

### Unwanted behaviour

> REQ-008: IF the endpoint is a non-Azure host, a non-Australian region, or `TenantMode` other than
> `Client`, THEN the system shall NOT send client data to that search service.

> REQ-009: IF a search is attempted, THEN the request shall include an equality filter on `siteId`
> matching the caller's site, and results whose `siteId` does not match shall be dropped.

> REQ-010: IF a search is attempted, THEN logs, metrics, and `GET /api/integrations/status` shall
> NOT contain query text, document content, or the API key.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | `InfrastructureRegistrationTests` resolves `IRetrievalClient` to the Azure adapter outside Dev/Testing | unit |
| REQ-002 | Adapter POST path, host suffix, and api-version match the contract | unit |
| REQ-003 | Dev/Testing register `StubRetrievalClient` | unit |
| REQ-004 | `australiaeast`/`australiasoutheast` + `Client` accepted; `eastus` and `Shared` rejected | unit |
| REQ-005 | Production registration with `Region=eastus` or `TenantMode=Shared` throws | unit |
| REQ-006 | Unconfigured or empty-site `SearchAsync` throws and the handler is not called | unit |
| REQ-007 | `GroundAsync` returns system/user text plus citations and does not send HTTP to a model host | unit |
| REQ-008 | Source scan: no `Microsoft.Search/searchServices` in `infra/**/*.bicep`; no hardcoded search URL in `src/` | unit |
| REQ-009 | Posted body includes `siteId eq '{guid}'`; a mismatched hit is omitted from the result | unit |
| REQ-010 | Capturing logger has no query/content/api-key; integration snapshot is a boolean only | unit |

## Privacy notes

Indexed chunks and queries may include personal or operational client data (lease text, names,
addresses, work-order notes). This slice does not persist queries or retrieved text. The only
outbound processor is the **customer's** Azure AI Search resource in an Australian region.
Generation, when a later caller adds it, goes through SPEC-0206's client-tenant AU OpenAI adapter.

| Principle | Obligation | How addressed |
|---|---|---|
| APP 1 | Open and transparent management of personal information | Spec + onboarding state that site documents are queried only against the customer's AU-region search service when a later feature calls this slice. |
| APP 5 | Notification of collection of personal information | No new collection surface. Callers (I1 and later) must disclose search/model processing before they send personal information. |
| APP 11 | Security of personal information | Client-tenant only; AU region allowlist; HTTPS Azure AI Search host allowlist; mandatory site filter; secrets in Key Vault; logs omit content and keys; no shared/ARK360 search tenant. |

## Accessibility notes

No user interface in this spec.
