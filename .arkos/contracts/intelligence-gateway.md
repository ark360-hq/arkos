# Contract: Intelligence governed gateway

## Summary

| Field | Value |
|---|---|
| Interface name | Intelligence governed gateway |
| Owner | @ark360-hq |
| Spec | SPEC-0012 |
| Direction | outbound from the adopter product through the gateway to Azure |
| Protocol | HTTPS JSON operations extracted from the btros Azure OpenAI adapter ([#472](https://github.com/ark360-hq/btros/pull/472)) and AI Search RAG slice ([#473](https://github.com/ark360-hq/btros/pull/473)) |
| Auth | adopter-configured identity; no anonymous product AI path |
| Machine-readable | [.arkos/contracts/intelligence-gateway.openapi.yaml](intelligence-gateway.openapi.yaml) |

This file is the human-readable contract required by constitution principle 3 and SPEC-0012 REQ-008. The OpenAPI file extends it (SPEC-0012 REQ-010 and REQ-016). The audit-call obligation is not optional.

## Endpoints / operations

| Method / event | Path / topic | Request | Response / payload |
|---|---|---|---|
| Product AI completion | `POST /v1/complete` | prompt, optional tool context, correlation id | model output plus required `audit` record |
| Retrieval / RAG query | `POST /v1/retrieve` | query, optional filters, correlation id | search hits plus required `audit` record |
| Gateway tool invocation | `POST /v1/tools/{toolName}` | tool name and arguments, correlation id | tool result plus required `audit` record |

Product AI calls use these gateway operations. They do not call a raw Azure OpenAI or Azure AI Search SDK (SPEC-0012 REQ-004).

## Obligations

1. Product AI calls that use this module go through this gateway, not a raw Azure OpenAI or Azure AI Search SDK (SPEC-0012 REQ-004).
2. Every gateway tool invocation, including `complete` and `retrieve`, emits an audit call before the result is returned to the product (SPEC-0012 REQ-005). The `audit` object is required on every success response.
3. Allowed backends are Azure OpenAI and Azure AI Search only.
4. The gateway does not create Azure, Entra, or Stripe resources (SPEC-0012 REQ-012).
5. Credentials stay in the adopter secret store. The extracted Bicep outputs endpoints only.

## Audit record

Every operation persists this record before the product receives a result:

| Field | Type | Required | Notes |
|---|---|---|---|
| `correlationId` | string | yes | Caller-supplied or gateway-minted id |
| `toolName` | string | yes | `complete`, `retrieve`, or the invoked tool name |
| `actor` | string | yes | Adopter product identity |
| `occurredAt` | string (date-time) | yes | UTC timestamp |
| `backend` | string | yes | `azure-openai` or `azure-ai-search` |
| `outcome` | string | yes | `success` or `error` |

## Schema stub

See [intelligence-gateway.openapi.yaml](intelligence-gateway.openapi.yaml). The audit-call obligation is not optional.

## Gate reference

This contract satisfies constitution principle 3 ("Contracts before code") and
ship gate criterion CRT-022 where this interface crosses a trust boundary.
