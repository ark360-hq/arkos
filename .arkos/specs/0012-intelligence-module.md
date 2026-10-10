---
id: SPEC-0012
slug: intelligence-module
title: Reusable Intelligence module
status: Approved
owner: "@ark360-hq"
created: 2026-10-10
touches-personal-data: true
trust-boundaries-crossed: true
linked-adrs: [ADR-0005]
linked-threat-model: .arkos/threat-models/intelligence-module.md
---

# SPEC-0012: Reusable Intelligence module

GitHub issue: [#18](https://github.com/ark360-hq/arkos/issues/18).

## Problem

ARK360 products that need product AI each stand up Azure OpenAI, Azure AI Search, and their own call path. Identity, audit, and infrastructure then split. Adopters of ARK OS have no reusable module for those pieces.

James Park approved, on 10 October 2026, templating the Azure OpenAI and Azure AI Search Bicep plus a governed gateway contract so other ARK OS products reuse them. The reuse source is the btros Azure OpenAI adapter and RAG slice. A fork that then diverges would recreate the same split.

This spec is issue #18. Implementation waits until those btros sources have landed. This token cannot see `ark360-hq/btros` (repository 404), so no btros issue number is cited here.

## Out of scope

1. Landing Intelligence module implementation (Bicep, example stacks, or gateway runtime) before the btros Azure OpenAI adapter and RAG slice have landed.
2. Forking btros and developing a separate Intelligence tree.
3. Issue #19 gate checks for non-Azure product AI and missing gateway audit.
4. Merging to `main`, creating a git tag, or publishing a GitHub Release.
5. Creating billable Azure, Entra, or Stripe resources.
6. Production deploys.
7. The v0.3 Microsoft agentic module (Foundry Agent Service, Copilot Studio, Entra Agent ID, approvals, evals, and tracing).

## Requirements

### Ubiquitous (always true, no trigger)

> REQ-001: The repository shall contain this Approved spec for a reusable Intelligence module that covers Azure OpenAI Bicep, Azure AI Search Bicep, and a governed gateway contract.

> REQ-002: The Intelligence module shall be extracted from the btros Azure OpenAI adapter and RAG slice. It shall not be a fork that then diverges.

> REQ-003: The Intelligence module shall include documentation and a worked example that an adopter can follow without reading the btros repository.

> REQ-004: Product AI calls that use this module shall go through the governed gateway, not a raw provider SDK.

> REQ-005: The governed gateway contract shall require an audit call on every gateway tool invocation.

> REQ-006: `CHANGELOG.md` `[Unreleased]` shall record this spec and shall reference SPEC-0012.

### Event-driven (triggered by an event)

> REQ-007: WHEN the btros Azure OpenAI adapter and RAG slice have landed, the implementation work shall extract Bicep and the gateway contract from those sources rather than rewriting them.

> REQ-008: WHEN this spec lands, the repository shall include ADR-0005, a STRIDE-lite threat model at `.arkos/threat-models/intelligence-module.md`, and a gateway contract at `.arkos/contracts/intelligence-gateway.md`.

### State-driven (active while in a state)

> REQ-009: WHILE the btros Azure OpenAI adapter or RAG slice has not landed, the repository shall not contain Intelligence module implementation code (Bicep modules, example stacks, or gateway runtime).

### Optional feature (conditional on feature being present)

> REQ-010: WHERE a later machine-readable OpenAPI, JSON Schema, or Bicep extract is taken from btros, it shall replace or extend the human-readable gateway contract without changing the audit-call obligation.

### Unwanted behaviour (what must not happen)

> REQ-011: IF implementation is proposed before the btros sources have landed THEN the agent shall NOT land that implementation.

> REQ-012: IF a change would create a billable Azure, Entra, or Stripe resource THEN the change shall NOT create it.

> REQ-013: IF this spec is applied THEN the change shall NOT implement issue #19.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | `.arkos/specs/0012-intelligence-module.md` exists with `status: Approved` and names Azure OpenAI Bicep, Azure AI Search Bicep, and a governed gateway contract | manual |
| REQ-002 | Implementation PR description and module README state extract-from-btros and forbid a diverging fork | manual |
| REQ-003 | Implementation PR adds module docs and a worked example that do not require cloning btros | manual |
| REQ-004 | Gateway contract and example route product AI calls through the gateway, not a raw provider SDK | manual |
| REQ-005 | `.arkos/contracts/intelligence-gateway.md` requires an audit call on every gateway tool invocation | manual |
| REQ-006 | `CHANGELOG.md` `[Unreleased]` contains SPEC-0012 | manual |
| REQ-007 | Implementation commit extracts from the landed btros adapter and RAG slice rather than a rewrite | manual |
| REQ-008 | ADR-0005, `.arkos/threat-models/intelligence-module.md`, and `.arkos/contracts/intelligence-gateway.md` exist and this spec links them | manual |
| REQ-009 | `git grep -E 'azurerm_cognitive|Microsoft.Search|azure-openai' -- ':!.arkos/specs/0012-intelligence-module.md' ':!.arkos/adr/0005-intelligence-module-extract.md' ':!.arkos/threat-models/intelligence-module.md' ':!.arkos/contracts/intelligence-gateway.md' ':!CHANGELOG.md'` is empty until btros sources have landed | manual |
| REQ-010 | Any later machine-readable contract still requires the audit call | manual |
| REQ-011 | This PR adds no Bicep modules, example stacks, or gateway runtime | manual |
| REQ-012 | This PR creates no Azure, Entra, or Stripe resources | manual |
| REQ-013 | This PR does not add issue #19 gate checks or a SPEC-0013 file | manual |

## Privacy notes

The module does not collect personal information by itself. Products that adopt it will send prompts, documents, and tool payloads to Azure OpenAI and Azure AI Search through the gateway. Those payloads may contain personal information.

| Principle | Obligation | How addressed |
|---|---|---|
| APP 1 | Open and transparent management of personal information | Adopter products must say in their own privacy notice that product AI may send prompts and documents to Azure OpenAI and Azure AI Search through this gateway. This spec does not add a public-facing collection notice in ARK OS. |
| APP 5 | Notification of collection of personal information | The gateway contract forbids silent third-party providers. Only the documented Azure services are in scope. Adopters notify their users at the product boundary. |
| APP 11 | Security of personal information | Calls go through the governed gateway with an audit record per tool invocation. Credentials stay in the adopter's secret store. No billable Azure resources are created by this repository. The STRIDE-lite model in `.arkos/threat-models/intelligence-module.md` covers spoofing, tampering, and disclosure at the gateway boundary. |

## Accessibility notes

Not applicable. This spec produces no user interface. Module documentation is prose. The worked example is infrastructure, not an interactive UI.
