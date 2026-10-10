# Contract: Intelligence governed gateway

## Summary

| Field | Value |
|---|---|
| Interface name | Intelligence governed gateway |
| Owner | @ark360-hq |
| Spec | SPEC-0012 |
| Direction | outbound from the adopter product through the gateway to Azure |
| Protocol | to be extracted from the btros Azure OpenAI adapter and RAG slice |
| Auth | adopter-configured identity; no anonymous product AI path |

This file is the human-readable contract required by constitution principle 3 and SPEC-0012 REQ-008. A machine-readable OpenAPI or JSON Schema file replaces or extends this file when those btros sources have landed (SPEC-0012 REQ-010). Do not invent paths or payloads here.

## Endpoints / operations

| Method / event | Path / topic | Request | Response / payload |
|---|---|---|---|
| Product AI completion | extracted from btros | prompt and tool context | model output via the gateway |
| Retrieval / RAG query | extracted from btros | query and filters | search hits via the gateway |
| Gateway tool invocation | every tool the gateway exposes | tool name and arguments | tool result plus audit record |

## Obligations

1. Product AI calls that use this module go through this gateway, not a raw Azure OpenAI or Azure AI Search SDK (SPEC-0012 REQ-004).
2. Every gateway tool invocation emits an audit call before the result is returned to the product (SPEC-0012 REQ-005).
3. Allowed backends are Azure OpenAI and Azure AI Search only.
4. The gateway does not create Azure, Entra, or Stripe resources (SPEC-0012 REQ-012).

## Schema stub

```yaml
openapi: "3.1.0"
info:
  title: Intelligence governed gateway
  version: "0.0.0"
  description: >
    Placeholder only. Extract paths, schemas, and error codes from the
    btros Azure OpenAI adapter and RAG slice when those sources land.
    The audit-call obligation is not optional.
paths: {}
```

## Gate reference

This contract satisfies constitution principle 3 ("Contracts before code") and
ship gate criterion CRT-022 where this interface crosses a trust boundary.
Replace or extend this file once a machine-readable extract from btros is in place.
