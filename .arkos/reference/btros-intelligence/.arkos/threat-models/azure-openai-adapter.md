# Threat Model: Azure OpenAI adapter

**Linked spec:** SPEC-0206

**Date:** 2026-10-10

**Author:** @parkjadev

---

## System sketch

```
[Later caller: I1 / other]
        |
        v
[ILanguageModelClient] ----(trust boundary)----> [Customer Azure OpenAI]
        |                                              australiaeast|southeast
[BTROS.Api / Worker]                                   regional Standard GPT
        |
   (no persist of prompt/completion)
   (Key Vault: Endpoint, Deployment, Region, ApiKey)
```

## Trust boundaries

1. BTROS process → customer Azure OpenAI Chat Completions HTTPS endpoint.
2. Operator/process configuration → Key Vault secrets that grant model invocation.

## STRIDE analysis

| Category | Threat | Likelihood | Impact | Mitigation | Status |
|---|---|---|---|---|---|
| Spoofing | Caller pointed at `api.openai.com` or a lookalike host | M | H | HTTPS + `*.openai.azure.com` allowlist; public OpenAI rejected | Mitigated |
| Tampering | MITM on the completion request | L | H | HTTPS only; user-info/query/fragment rejected | Mitigated |
| Repudiation | Model call with no later audit | M | M | Adapter does not audit (no caller yet). I1 must write accept/edit/reject audit; not this spec | Accepted |
| Information disclosure | Prompt/completion/API key in logs or status API | M | H | Structured logs omit content and secrets; integration status is a boolean | Mitigated |
| Information disclosure | Shared/ARK360 or Global Standard model processes client data off-tenant or off-AU | H | H | `TenantMode=Client`, AU region allowlist, `DeploymentType=Standard` only; startup fail-closed | Mitigated |
| Denial of service | 429/5xx retry storm runs up token cost | M | M | No automatic retry; failure is thrown to the caller | Mitigated |
| Elevation of privilege | Model used as an authorizer or given MCP tools | L | H | Adapter has no tools, no MCP credentials, no command surface | Mitigated |

## Residual risk

| Risk | Rationale for acceptance | Re-evaluation trigger |
|---|---|---|
| `Region` is a configured claim, not Azure Resource Manager proof of the account's location | CI cannot call ARM; hostname has no region | First customer resource is provisioned — verify location and deployment type in the Azure portal |
| No completion audit until a caller exists | No product feature sends data yet | I1 (issue #419) or any other caller ships |
| API key or managed-identity token grants the process the customer's model | Same pattern as ACS/Stripe; secret stays in that environment's Key Vault | Key rotation / MI scope review at go-live |
