---
id: SPEC-0206
slug: azure-openai-adapter
title: Azure OpenAI adapter (client tenant, Australian-region GPT)
status: Approved
owner: "@parkjadev"
created: 2026-10-10
touches-personal-data: true
trust-boundaries-crossed: true
linked-adrs: [ADR-0049]
linked-threat-model: .arkos/threat-models/azure-openai-adapter.md
---

# SPEC-0206: Azure OpenAI adapter (client tenant, Australian-region GPT)

## Problem

Nothing in `src/` can call a language model. The decided pilot includes I1 (issue #419) — a
compliance-deadlines drafting agent — and later intelligence work (issue #466, Azure AI Search RAG)
depends on a governed chat path. Client data sent to a model must stay in an **Australian-region GPT
deployment** in the **customer's** Azure tenant. A shared or ARK360-owned model tenant, a public
OpenAI endpoint, or a Global Standard deployment would process that data outside the residency and
tenancy rules.

James approved this tech-strategy item on 10 Oct 2026 (issue #465). This spec is the adapter only.

## Out of scope

1. Azure AI Search / RAG (issue #466) — a later spec.
2. I1 drafting agent, operator review UI, or any MCP tool that calls the model (issue #419).
3. Provisioning an Azure OpenAI account, GPT deployment, quota, or Private Link. This repository
   must not create that billable resource. The customer provisions a **regional Standard** GPT
   deployment in `australiaeast` or `australiasoutheast` in **their** subscription.
4. Semantic Kernel, `IChatClient`, Azure.AI.OpenAI SDK, or a second model vendor.
5. User-facing "AI-native" copy (SPEC-0189). Shipping an unused adapter does not make those claims true.

## Requirements

### Ubiquitous

> REQ-001: The system shall expose a provider-neutral `ILanguageModelClient` in `BTROS.Domain` and an
> Azure OpenAI chat-completions adapter in `BTROS.Infrastructure` that is the only production
> implementation.

> REQ-002: The adapter shall send chat completions only to an `https://*.openai.azure.com/` resource
> over TLS 1.2+, using the Azure OpenAI Chat Completions REST contract
> (`.arkos/contracts/azure-openai-chat.md`).

### State-driven

> REQ-003: WHILE the host environment is Development or Testing the system shall register a
> deterministic stub that performs no outbound HTTP.

> REQ-004: WHILE Azure OpenAI is configured the system shall require `TenantMode=Client`,
> `Region` in `{australiaeast, australiasoutheast}`, and `DeploymentType=Standard` (regional).

### Event-driven

> REQ-005: WHEN any Azure OpenAI setting is present outside Development/Testing and the binding is
> incomplete or violates REQ-004 / REQ-002, startup shall fail before the host accepts traffic.

> REQ-006: WHEN `CompleteAsync` is invoked without a valid client-tenant AU Standard binding the
> adapter shall throw and shall NOT send a request.

### Unwanted behaviour

> REQ-007: IF the endpoint is `api.openai.com`, a non-Azure host, a Global or Data-Zone deployment
> type, a non-Australian region, or `TenantMode` other than `Client`, THEN the system shall NOT send
> client data to that model.

> REQ-008: IF a completion is attempted, THEN logs, metrics, and `GET /api/integrations/status`
> shall NOT contain prompt text, completion text, or the API key.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | `InfrastructureRegistrationTests` resolves `ILanguageModelClient` to the Azure adapter outside Dev/Testing | unit |
| REQ-002 | Adapter POST path, host suffix, and api-version match the contract; `api.openai.com` is rejected | unit |
| REQ-003 | Dev/Testing register `StubLanguageModelClient` | unit |
| REQ-004 | `australiaeast`/`australiasoutheast` + `Client` + `Standard` accepted; `eastus`, `Shared`, `GlobalStandard` rejected | unit |
| REQ-005 | Production registration with `Region=eastus` or `TenantMode=Shared` throws | unit |
| REQ-006 | Unconfigured `CompleteAsync` throws `LanguageModelException` (`NotConfigured`) and the handler is not called | unit |
| REQ-007 | Source scan: no Cognitive Services / OpenAI account in `infra/**/*.bicep`; no hardcoded resource URL in `src/` | unit |
| REQ-008 | Capturing logger has no prompt/completion/api-key; integration snapshot is a boolean only | unit |

## Privacy notes

Prompts may include personal or operational client data (lease text, names, addresses). The adapter
does not persist prompts or completions. The only outbound processor is the **customer's** Azure
OpenAI resource in an Australian region.

| Principle | Obligation | How addressed |
|---|---|---|
| APP 1 | Open and transparent management of personal information | Spec + onboarding state that client data is sent only to the customer's AU-region GPT deployment when a later feature calls the adapter. |
| APP 5 | Notification of collection of personal information | No new collection surface in this spec. Callers (I1 and later) must disclose model processing before they send personal information. |
| APP 11 | Security of personal information | Client-tenant only; AU regional Standard only; HTTPS Azure OpenAI host allowlist; secrets in Key Vault; logs omit content and keys; no shared/ARK360 model tenant. |

## Accessibility notes

No user interface in this spec.
