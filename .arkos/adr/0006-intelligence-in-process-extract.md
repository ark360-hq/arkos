# ADR-0006: Extract Intelligence as in-process C# adapters

**Status:** Accepted

**Date:** 2026-10-10

**Supersedes:** ADR-0005

**Superseded by:** (none)

---

## Context

James Park pushed the landed btros Azure OpenAI adapter and RAG slice (btros main `a04a31a`, pulls #472 and #473) onto the SPEC-0012 branch as a reference tree. Those sources have no Azure OpenAI or Azure AI Search Bicep, no product HTTP `/v1` gateway, no embeddings deployment, and no eval goldens.

The real design is in-process C#: provider-neutral `ILanguageModelClient`, `IRetrievalClient`, and `IRagClient`; `AzureOpenAIGuard` and `AzureAiSearchGuard`; HttpClient adapters with api-key or `DefaultAzureCredential`; Development/Testing stubs; options that fail closed on a partial or illegal binding.

ADR-0005 assumed an extract of Bicep plus a governed HTTP gateway. Following that decision would rewrite the landed sources and recreate the split ADR-0005 was meant to prevent.

## Decision

This project will extract the Intelligence module from btros commit `a04a31a` as in-process C# adapters and guards, generalised from BTR OS namespaces to `Arkos.Intelligence` / `Arkos.Intelligence.Infrastructure`. The module will not include template Bicep, will not provision Azure OpenAI or Azure AI Search, and will not expose a product HTTP `/v1` gateway. Product AI calls go through the provider-neutral interfaces, not a raw Azure SDK. No billable Azure, Entra, or Stripe resources will be created from this repository.

## Consequences

Plus: The template matches the landed btros adapter and RAG slice instead of inventing infrastructure.

Plus: Domain callers stay free of Azure types. Client-tenant, Australian-region, and host allowlists are enforced in code.

Minus: Adopters must provision Azure OpenAI and Azure AI Search in their own tenant. This repository does not ship Bicep for those resources.

Minus: The adapters do not write an accept/edit/reject audit. Product callers own that trail when they send client data.
