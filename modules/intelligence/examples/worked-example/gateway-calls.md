# Gateway calls

Product AI uses the governed gateway. Do not call a raw Azure OpenAI SDK or a raw Azure AI Search SDK.

Contract: `.arkos/contracts/intelligence-gateway.openapi.yaml`.

## Complete (from btros#472)

```
POST /v1/complete
```

```json
{
  "prompt": "Summarise the open service requests for site 12",
  "actor": "product-api",
  "correlationId": "corr-example-001"
}
```

The gateway sends the prompt to Azure OpenAI and returns:

```json
{
  "output": "There are two open service requests.",
  "audit": {
    "correlationId": "corr-example-001",
    "toolName": "complete",
    "actor": "product-api",
    "occurredAt": "2026-10-10T00:00:00Z",
    "backend": "azure-openai",
    "outcome": "success"
  }
}
```

The `audit` object is required. Persist it before the product receives the result.

## Retrieve (from btros#473)

```
POST /v1/retrieve
```

```json
{
  "query": "fire safety certificate for building A",
  "actor": "product-api",
  "correlationId": "corr-example-002",
  "filters": {
    "siteId": "12"
  }
}
```

The gateway queries Azure AI Search and returns hits plus `audit`. Do not query Search from the product.

## Tool invocation

```
POST /v1/tools/{toolName}
```

Every tool the gateway exposes follows the same rule: emit the audit record, then return the result. Missing audit is a contract failure (SPEC-0012 REQ-005).
