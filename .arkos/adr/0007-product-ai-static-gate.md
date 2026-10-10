# ADR-0007: Enforce product AI policy with a static build-gate scan

**Status:** Accepted

**Date:** 2026-10-10

**Supersedes:** (none)

**Superseded by:** (none)

---

## Context

Issue #19 requires the build gate to fail CI when a product calls a non-Azure AI provider, or when a product AI path skips the governed Intelligence checks. CONTRIBUTING.md treats a change to `.arkos/gates/` as a structural change, so this decision is recorded before the spec is applied.

The landed Intelligence extract (SPEC-0012 on pull request #21, btros main `a04a31a`) is in-process C#. `AzureOpenAIGuard` and `AzureAiSearchGuard` fail closed on tenant, region, host, and search site scope. Adapters do not emit an accept/edit/reject audit. Product callers own that trail. There is no product HTTP `/v1` gateway and no template Bicep.

A scan that required a gateway audit call, or that treated the Intelligence adapters as a raw SDK, would fail legitimate btros code. Access OS is an Entra and audit-trail product without product AI; a noisy scan would fail it too. The ARK360 marketing site may name third-party models without being product AI.

## Decision

This project will enforce product AI policy with a whole-tree static scan in the build gate (`.arkos/scripts/check-product-ai.sh`). The scan will fail on non-Azure product AI providers and on Azure OpenAI or Azure AI Search send sites that do not call `AzureOpenAIGuard.EnsureCanSend` or `AzureAiSearchGuard.EnsureCanSend` in the same file. A documented allow-list will skip marketing-site paths. The scan will not require an HTTP `/v1` gateway, Bicep, or an adapter-emitted audit record.

## Consequences

Plus: CI matches the in-process guards instead of the retired Bicep and HTTP-gateway assumption.

Plus: btros-style adapters and Access OS trees without product AI can pass.

Minus: The scan is syntactic. A send site that calls the guard in another file, or that hides a provider behind an unusual wrapper, can evade it. Runtime enforcement stays in the guards.
