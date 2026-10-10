# Threat Model: Intelligence module

**Linked spec:** SPEC-0012

**Date:** 2026-10-10

**Author:** @ark360-hq

---

## System sketch

```
[Adopter product caller]
        |
        v
[ILanguageModelClient] ----(trust boundary)----> [Customer Azure OpenAI]
        |                                         australiaeast|southeast
[IRetrievalClient / IRagClient] --(trust boundary)--> [Customer Azure AI Search]
        |                                              siteId filter, top <= 20
[RagPromptComposer] --> grounded prompt --> [ILanguageModelClient, later]
        |
   (no persist of prompt/query/chunks)
   (adopter secret store: Endpoint, Deployment/Index, Region, ApiKey)
```

Actors: adopter product service, adopter engineer, customer Azure tenant, unauthenticated attacker.

Extracted from btros main `a04a31a` (#472, #473). No Bicep. No product HTTP `/v1` gateway.

## Trust boundaries

1. Adopter process to the customer's Azure OpenAI Chat Completions HTTPS endpoint.
2. Adopter process to the customer's Azure AI Search documents-search HTTPS endpoint.
3. Site A query must not read Site B documents in the same index.
4. Operator configuration to secrets that grant model invocation or index query.

## STRIDE analysis

| Category | Threat | Likelihood | Impact | Mitigation | Status |
|---|---|---|---|---|---|
| Spoofing | Caller pointed at `api.openai.com`, a lookalike OpenAI host, or a lookalike Search host | M | H | HTTPS plus `*.openai.azure.com` and `*.search.windows.net` allowlists | Mitigated |
| Tampering | MITM on the completion or search request | L | H | HTTPS only; user-info, query, and fragment rejected | Mitigated |
| Tampering | Caller-supplied filter drops `siteId` | M | H | Adapter builds the filter; raw filter is not accepted | Mitigated |
| Repudiation | Model or search call with no later audit | M | M | Adapters do not write accept/edit/reject audit. Product callers own that trail (SPEC-0012 REQ-005) | Accepted |
| Information disclosure | Prompt, completion, query, chunk, or API key in logs or status | M | H | Structured logs omit content and secrets; ready-state is a boolean | Mitigated |
| Information disclosure | Shared, platform, Global Standard, or non-AU processing of client data | H | H | `TenantMode=Client`; AU region allowlist; `DeploymentType=Standard`; startup fail-closed | Mitigated |
| Information disclosure | Cross-site hit returned from a shared index | M | H | Mandatory `siteId` equality filter; drop mismatched hits | Mitigated |
| Denial of service | 429/5xx retry storm runs up cost | M | M | No automatic retry; failure is thrown to the caller | Mitigated |
| Elevation of privilege | Model or search used as an authorizer or given MCP tools | L | H | Adapters have no tools, no MCP credentials, no command surface | Mitigated |

## Residual risk

| Risk | Rationale for acceptance | Re-evaluation trigger |
|---|---|---|
| `Region` is a configured claim, not Azure Resource Manager proof of location | CI cannot call ARM; hostnames have no region | First customer resource is provisioned. Verify location and deployment type in the Azure portal |
| No completion or retrieval audit until a product caller exists | No template feature sends client data | A product caller ships and must add accept/edit/reject audit |
| API key or managed-identity token grants the process the customer's model or index | Secret stays in the adopter environment | Key rotation and managed-identity scope review at go-live |
| Keyword-only retrieval until a vector is supplied | This slice must not add an embeddings deployment | A later embeddings extension of the OpenAI adapter |
