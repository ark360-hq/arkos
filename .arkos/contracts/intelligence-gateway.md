# Contract: Intelligence governed seams

## Summary

| Field | Value |
|---|---|
| Interface name | Intelligence governed seams |
| Owner | @ark360-hq |
| Spec | SPEC-0012 |
| Direction | in-process from the adopter product to the adapters, then outbound HTTPS to the customer's Azure resources |
| Protocol | C# interfaces in `Arkos.Intelligence`; Azure REST shapes in the linked contracts |
| Auth | adopter-configured identity on the product side; `api-key` or `DefaultAzureCredential` on the Azure side |

This is the governed product seam extracted from btros main `a04a31a` (#472, #473). It is **not** a product HTTP `/v1` gateway. Domain callers use these interfaces. They do not reference Azure types or call a raw provider SDK.

Outbound REST shapes:

- [azure-openai-chat.md](azure-openai-chat.md)
- [azure-ai-search.md](azure-ai-search.md)

## Operations

| Interface | Method | Request | Response |
|---|---|---|---|
| `ILanguageModelClient` | `CompleteAsync` | `LanguageModelRequest` (messages, optional max tokens and temperature) | `LanguageModelCompletion` |
| `IRetrievalClient` | `SearchAsync` | `RetrievalQuery` (`siteId` required, search text, optional top and vector) | `RetrievalResult` |
| `IRagClient` | `GroundAsync` | `RetrievalQuery` | `RagGroundedPrompt` (system instruction, user message, citations). Does not call a model. |

## Obligations

1. Product AI calls that use this module go through these interfaces, not a raw Azure OpenAI or Azure AI Search SDK (SPEC-0012 REQ-004).
2. Logs, metrics, and status snapshots omit prompt, completion, query, document content, and API keys (SPEC-0012 REQ-005). The adapters do not write an accept/edit/reject audit. Product callers own that trail.
3. Allowed backends are the customer's Azure OpenAI and Azure AI Search resources only, in `australiaeast` or `australiasoutheast`, `TenantMode=Client`.
4. Azure OpenAI `DeploymentType` must be regional `Standard`. Search queries include a required `siteId` equality filter.
5. This module does not create Azure, Entra, or Stripe resources (SPEC-0012 REQ-012).

## Gate reference

This contract satisfies constitution principle 3 and ship gate criterion CRT-022 at the product-to-adapter boundary. The Azure trust boundaries are contracted in the two REST files.
