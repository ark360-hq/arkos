# Intelligence module

In-process Azure OpenAI chat adapter and Azure AI Search retrieve-then-ground slice for ARK OS products.

This module is **extracted** from btros main `a04a31a`:

- [ark360-hq/btros#472](https://github.com/ark360-hq/btros/pull/472) Azure OpenAI adapter
- [ark360-hq/btros#473](https://github.com/ark360-hq/btros/pull/473) AI Search RAG slice

It is **not a fork**. Do not copy the btros application tree here and do not develop a separate Intelligence tree that then diverges. When those sources change, extract again.

Spec: [SPEC-0012](../../.arkos/specs/0012-intelligence-module.md). Issue: [#18](https://github.com/ark360-hq/arkos/issues/18). Decision: [ADR-0006](../../.arkos/adr/0006-intelligence-in-process-extract.md).

## What you get

| Path | Purpose |
|---|---|
| `src/Arkos.Intelligence/` | `ILanguageModelClient`, `IRetrievalClient`, `IRagClient`, guards, `RagGroundingService`, `RagPromptComposer` |
| `src/Arkos.Intelligence.Infrastructure/` | HttpClient adapters, fail-closed options, stubs, `AddArkosIntelligence` |
| `tests/Arkos.Intelligence.Tests/` | Guard, adapter, grounding, and registration tests |
| [examples/worked-example/](examples/worked-example/) | How to register and call the seams without cloning btros |
| [SOURCE.md](SOURCE.md) | Provenance for the extract |

There is no template Bicep. There is no product HTTP `/v1` gateway. There is no embeddings deployment and no eval goldens. This repository does not provision Azure OpenAI or Azure AI Search.

## Rules

1. Product AI calls go through `ILanguageModelClient`, `IRetrievalClient`, and `IRagClient`. Do not call a raw Azure OpenAI or Azure AI Search SDK from domain code.
2. Client-tenant mode only. Regions: `australiaeast` or `australiasoutheast`. Azure OpenAI deployment type: regional `Standard` only.
3. Endpoints must be `https://*.openai.azure.com/` and `https://*.search.windows.net/`.
4. Every search requires a `siteId` filter. Default `top` is 5. `top` is capped at 20. Mismatched hits are dropped.
5. A partial or illegal binding fails startup outside Development/Testing. An absent binding does not. `CompleteAsync` and `SearchAsync` still fail closed.
6. Auth is an `api-key` header or `DefaultAzureCredential`.
7. Logs omit prompt, completion, query, document content, and API keys. Product callers own accept/edit/reject audit.
8. This repository does not create Azure, Entra, or Stripe resources.

## Adopt without reading btros

1. Copy `modules/intelligence/src/` into your product, or add a project reference to the two projects.
2. Call `services.AddArkosIntelligence(configuration, environmentName)` at host startup.
3. Follow [examples/worked-example/README.md](examples/worked-example/README.md).
4. Provision Azure OpenAI (regional Standard GPT) and Azure AI Search in **your** tenant. Bind `AzureOpenAI:*` and `AzureAiSearch:*` from your secret store.

## Verify

```
bash modules/intelligence/tests/check-module.sh
dotnet test modules/intelligence/Arkos.Intelligence.sln
```

## Out of scope here

- Issue #19 gate checks
- The v0.3 Microsoft agentic module
- Template Bicep or production deploys from this repository
