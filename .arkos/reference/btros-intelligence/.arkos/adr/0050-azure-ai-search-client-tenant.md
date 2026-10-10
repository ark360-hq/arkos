# ADR-0050: Azure AI Search RAG — client tenant, Australian region

**Status:** Accepted

**Date:** 2026-10-10

**Supersedes:** none

**Superseded by:** none

---

## Context

Issue #466 needs a retrieve-then-ground path so later intelligence work (I1 / #419, arkos#18) can
answer from site documents. SPEC-0206 / ADR-0049 already decided the chat-completions adapter:
client tenant, Australian-region regional Standard, HttpClient, no model SDK. That adapter must
not be duplicated here.

Forces: Domain stays dependency-free; `Azure.Identity` is pinned at 1.13.1 (ADR-0033), so
`Azure.Search.Documents` is unwelcome if it bumps `Azure.Core`; SPEC-0179 already pins platform
regions to `australiaeast` / `australiasoutheast`; SPEC-0181 requires site scope by construction;
this repository must not provision the billable Azure AI Search account.

Vector embeddings would call Azure OpenAI. That is SPEC-0206's resource, not this one. Until a
caller supplies a vector, retrieval is keyword search plus a mandatory `siteId` filter.

## Decision

We will add provider-neutral `IRetrievalClient` and `IRagClient` in `BTROS.Domain` and one
production adapter that POSTs the Azure AI Search documents-search REST API over `HttpClient`
(no search SDK). `IRagClient.GroundAsync` retrieves, then composes a grounded prompt. It does
not call a language model; later callers pass the composed messages to SPEC-0206's
`ILanguageModelClient`.

The binding is configuration-only (`AzureAiSearch:*` from Key Vault / env, ADR-0036). It is
valid only when all of the following hold:

1. **Client tenant.** `TenantMode` is `Client`. Shared, platform, or ARK360-owned tenants are rejected.
2. **Australian region.** `Region` is `australiaeast` or `australiasoutheast` (same pin as SPEC-0179).
3. **Azure AI Search host.** `Endpoint` is `https://{service}.search.windows.net/` — not a
   non-Azure host and not a bare apex host.
4. **Site scope.** Every query includes `filter: siteId eq '{siteId}'`. Hits with another site id
   are dropped.

Development and Testing use a deterministic stub. Outside those environments, a partial or
illegal binding fails startup. An absent binding does not fail startup (no caller exists yet)
but `SearchAsync` fails closed and sends nothing.

This repository does not create the Azure AI Search service or index. The customer provisions
both in their subscription; keys go in that environment's Key Vault.

## Consequences

Plus: RAG and later features share one fail-closed retrieval seam; CI never calls Azure AI
Search; residency, tenancy, and site-scope rules are enforced in code; no new NuGet package;
SPEC-0206 remains the only chat path.

Minus: `Region` is a configured claim — the `*.search.windows.net` hostname has no region.
Keyword search without an embeddings deployment is weaker than hybrid vector retrieval; that
waits on a caller-supplied vector or a later embeddings extension of SPEC-0206. Live search
against a real service is not exercised by CI.
