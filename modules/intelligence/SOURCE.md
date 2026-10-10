# Intelligence module source map

Extract, not a fork. SPEC-0012 REQ-002 and REQ-007.

Provenance: btros main `a04a31a`, [ark360-hq/btros#472](https://github.com/ark360-hq/btros/pull/472) and [ark360-hq/btros#473](https://github.com/ark360-hq/btros/pull/473). James Park placed those files on this branch under `.arkos/reference/btros-intelligence/` (commit `e7e03a8`) for extraction. That reference tree is not part of the finished module.

| Landed source | btros path | Extracted to |
|---|---|---|
| #472 Azure OpenAI adapter | `src/BTROS.Domain/Intelligence/ILanguageModelClient.cs`, `AzureOpenAIGuard.cs` | `src/Arkos.Intelligence/` |
| #472 | `src/BTROS.Infrastructure/Intelligence/AzureOpenAIOptions.cs`, `AzureOpenAILanguageModelClient.cs`, `StubLanguageModelClient.cs` | `src/Arkos.Intelligence.Infrastructure/` |
| #473 AI Search RAG slice | `src/BTROS.Domain/Intelligence/IRetrievalClient.cs` (includes `IRagClient`), `AzureAiSearchGuard.cs`, `RagGroundingService.cs`, `RagPromptComposer.cs` | `src/Arkos.Intelligence/` |
| #473 | `src/BTROS.Infrastructure/Intelligence/AzureAiSearchOptions.cs`, `AzureAiSearchRetrievalClient.cs`, `StubRetrievalClient.cs` | `src/Arkos.Intelligence.Infrastructure/` |
| #472 / #473 registration | Intelligence block of `DependencyInjection.cs` | `IntelligenceServiceCollectionExtensions.AddArkosIntelligence` |
| #472 / #473 tests | `tests/BTROS.Tests/Intelligence/*.cs` plus the Intelligence cases from `InfrastructureRegistrationTests.cs` | `tests/Arkos.Intelligence.Tests/` |
| #472 / #473 contracts | `.arkos/contracts/azure-openai-chat.md`, `azure-ai-search.md` | `.arkos/contracts/` (owner `@ark360-hq`, spec SPEC-0012) |

Generalisation from BTR OS:

- Namespaces `BTROS.Domain.Intelligence` and `BTROS.Infrastructure.Intelligence` became `Arkos.Intelligence` and `Arkos.Intelligence.Infrastructure`.
- `AddBtrosInfrastructure` / `allowSqliteFallback` became `AddArkosIntelligence` / Development-or-Testing stubs.
- Dropped BioStar, payments, PMS, integration-status API, production-deploy runbook, and other product features.
- No template Bicep. No product HTTP `/v1` gateway. No embeddings deployment. No eval goldens.

Not extracted (do not fork these into ARK OS):

- The btros operator console and other product features
- Entra or Stripe resources
- Any deploy step that would create a billable resource from this repository
