# Threat Model: Intelligence module

**Linked spec:** SPEC-0012

**Date:** 2026-10-10

**Author:** @ark360-hq

---

## System sketch

```
[Adopter product] --> [Governed gateway] --> [Azure OpenAI]
         |                    |                      |
         |                    +------> [Azure AI Search]
         |                    |
         |                    v
         |              [Audit record]
         |
    (Trust boundary: product -> gateway)
    (Trust boundary: gateway -> Azure)
```

Actors: adopter product service, adopter engineer, Azure tenant, unauthenticated attacker.

This model covers the extracted module at `modules/intelligence/`. Sources: ark360-hq/btros#472 and #473.

## Trust boundaries

1. Adopter product to the governed gateway (prompts, documents, tool names, and tool arguments leave the product trust zone).
2. Gateway to Azure OpenAI and Azure AI Search (payloads leave the adopter network to Microsoft Azure).
3. Gateway to the audit store (who called which tool, when, and with which correlation id).

## STRIDE analysis

| Category | Threat | Likelihood | Impact | Mitigation | Status |
|---|---|---|---|---|---|
| Spoofing | Caller invokes the gateway without a valid product identity | M | H | Gateway accepts only the adopter's configured identity. No anonymous product AI path. | Open |
| Tampering | Tool arguments are altered in transit or at the gateway | M | H | TLS to Azure. Gateway does not accept a raw provider SDK bypass (SPEC-0012 REQ-004). | Open |
| Repudiation | A tool call has no audit record | M | H | Contract requires an audit call on every gateway tool invocation (SPEC-0012 REQ-005). | Open |
| Information disclosure | Prompts or documents containing personal information leak via logs or a second provider | M | H | Only Azure OpenAI and Azure AI Search are in scope. Secrets stay in the adopter store. APP 11 notes in SPEC-0012. | Open |
| Denial of service | Azure quota or a flood of tool calls blocks product AI | M | M | Adopter sets quotas in their tenant. This template does not create billable resources. | Open |
| Elevation of privilege | A tool call reaches Azure resources outside the product scope | L | H | Extracted Bicep scopes OpenAI and Search to the product resource group. Local auth is disabled. No Entra or Stripe resources from this repo. | Open |

## Residual risk

| Risk | Rationale for acceptance | Re-evaluation trigger |
|---|---|---|
| Azure regional outage | Single-cloud product AI is the approved stack. Multi-region failover is not in SPEC-0012. | A product SLA requires an active-active AI path. |
| This GitHub App installation cannot read the private btros tree | James named #472 and #473 as the landed sources. The extract uses that named surface (OpenAI Bicep, Search Bicep, gateway plus audit). | A later run that can read those pulls finds a different resource or operation shape. |
