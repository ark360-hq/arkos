# Intelligence module

Reusable Azure OpenAI Bicep, Azure AI Search Bicep, and a governed gateway contract for ARK OS products.

This module is **extracted** from the landed btros sources:

- [ark360-hq/btros#472](https://github.com/ark360-hq/btros/pull/472) Azure OpenAI adapter
- [ark360-hq/btros#473](https://github.com/ark360-hq/btros/pull/473) AI Search RAG slice

It is **not a fork**. Do not copy the btros application tree here and do not develop a separate Intelligence tree that then diverges. When those sources change, extract again.

Spec: [SPEC-0012](../../.arkos/specs/0012-intelligence-module.md). Issue: [#18](https://github.com/ark360-hq/arkos/issues/18).

## What you get

| Path | Purpose |
|---|---|
| [bicep/openai.bicep](bicep/openai.bicep) | Azure OpenAI account plus chat and embeddings deployments |
| [bicep/search.bicep](bicep/search.bicep) | Azure AI Search service for RAG retrieval |
| [examples/worked-example/](examples/worked-example/) | Compose the modules and call the gateway |
| [SOURCE.md](SOURCE.md) | Provenance for the extract |
| `.arkos/contracts/intelligence-gateway.md` | Human-readable gateway contract |
| `.arkos/contracts/intelligence-gateway.openapi.yaml` | Machine-readable contract (audit required) |

You do not need to clone btros to adopt this module.

## Rules

1. Product AI calls go through the governed gateway (`POST /v1/complete`, `POST /v1/retrieve`, `POST /v1/tools/{toolName}`). Do not call a raw Azure OpenAI or Azure AI Search SDK.
2. Every gateway tool invocation, including complete and retrieve, emits an audit record before the result is returned.
3. Allowed backends are Azure OpenAI and Azure AI Search only.
4. This repository does not deploy the Bicep and does not create Azure, Entra, or Stripe resources. Adopters deploy in their own tenant.
5. Credentials stay in the adopter secret store. The Bicep outputs endpoints only.

## Adopt without reading btros

1. Copy `modules/intelligence/bicep/` into your product repository, or reference these files from your own Bicep.
2. Read [examples/worked-example/README.md](examples/worked-example/README.md) and copy `main.bicep` plus `parameters.example.json`.
3. Replace every `REPLACE_*` parameter in your tenant. Do not deploy the example values from this repository.
4. Point your product AI client at the gateway operations in `.arkos/contracts/intelligence-gateway.openapi.yaml`.
5. Keep the audit object on every success response.

## Verify

```
bash modules/intelligence/tests/check-module.sh
```

## Out of scope here

- Issue #19 gate checks
- The v0.3 Microsoft agentic module
- Production deploys from this repository
