# Threat Model: Azure AI Search RAG

**Linked spec:** SPEC-0207

**Date:** 2026-10-10

**Author:** @parkjadev

---

## System sketch

```
[Later caller: I1 / other]
        |
        v
[IRagClient / IRetrievalClient] ----(trust boundary)----> [Customer Azure AI Search]
        |                                                      australiaeast|southeast
[BTROS.Api / Worker]                                           index filtered by siteId
        |
   [RagPromptComposer] -- grounded prompt --> [SPEC-0206 ILanguageModelClient, later]
   (no persist of query/chunks)
   (Key Vault: Endpoint, IndexName, Region, ApiKey)
```

## Trust boundaries

1. BTROS process → customer Azure AI Search documents-search HTTPS endpoint.
2. Operator/process configuration → Key Vault secrets that grant search-index query.
3. Site A query → must not read Site B documents in the same index.

## STRIDE analysis

| Category | Threat | Likelihood | Impact | Mitigation | Status |
|---|---|---|---|---|---|
| Spoofing | Caller pointed at a lookalike search host | M | H | HTTPS + `*.search.windows.net` allowlist | Mitigated |
| Tampering | MITM on the search request | L | H | HTTPS only; user-info/query/fragment rejected | Mitigated |
| Tampering | Caller-supplied filter drops `siteId` | M | H | Adapter builds the filter; raw filter is not accepted | Mitigated |
| Repudiation | Search with no later audit | M | M | Adapter does not audit (no caller yet). I1 must write accept/edit/reject audit | Accepted |
| Information disclosure | Query/chunk/API key in logs or status API | M | H | Structured logs omit content and secrets; integration status is a boolean | Mitigated |
| Information disclosure | Shared/ARK360 or non-AU search processes client documents | H | H | `TenantMode=Client`, AU region allowlist; startup fail-closed | Mitigated |
| Information disclosure | Cross-site hit returned from a shared index | M | H | Mandatory `siteId` equality filter; drop mismatched hits | Mitigated |
| Denial of service | 429/5xx retry storm runs up query cost | M | M | No automatic retry; failure is thrown to the caller | Mitigated |
| Elevation of privilege | Search used as an authorizer or given MCP tools | L | H | Adapter has no tools, no MCP credentials, no command surface | Mitigated |

## Residual risk

| Risk | Rationale for acceptance | Re-evaluation trigger |
|---|---|---|
| `Region` is a configured claim, not Azure Resource Manager proof of the service location | CI cannot call ARM; hostname has no region | First customer service is provisioned — verify location in the Azure portal |
| No retrieval audit until a caller exists | No product feature sends data yet | I1 (issue #419) or any other caller ships |
| API key or managed-identity token grants the process query rights on the customer's index | Same pattern as ACS/Stripe; secret stays in that environment's Key Vault | Key rotation / MI scope review at go-live |
| Keyword-only retrieval until a vector is supplied | Embeddings are SPEC-0206's Azure OpenAI resource; this slice must not duplicate that adapter | I1 or a later embeddings extension of SPEC-0206 |
