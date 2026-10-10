---
id: SPEC-0013
slug: product-ai-gates
title: Product AI gate checks
status: Approved
owner: "@ark360-hq"
created: 2026-10-10
touches-personal-data: false
trust-boundaries-crossed: false
linked-adrs: [ADR-0007]
linked-threat-model: null
---

# SPEC-0013: Product AI gate checks

GitHub issue: [#19](https://github.com/ark360-hq/arkos/issues/19).

## Problem

ARK360 products that add product AI can call a public model provider, or call Azure OpenAI and Azure AI Search without the in-process Intelligence guards. The build gate today only has generic banned-pattern checks. Nothing fails CI for a non-Azure product AI path, and nothing fails CI when a send site skips `AzureOpenAIGuard` or `AzureAiSearchGuard`.

The Intelligence extract on pull request #21 (SPEC-0012, btros main `a04a31a`) is in-process C#, not Bicep and not an HTTP `/v1` gateway. A gate written against the retired gateway-audit assumption would block that legitimate code. Access OS is an Entra and entry-trail product without product AI; a noisy scan would block it too. The ARK360 marketing site may name third-party models without those mentions being product AI.

James Park approved this work on 10 October 2026 after pull request #21 was green.

## Out of scope

1. Merging to `main`, creating a git tag, or publishing a GitHub Release.
2. Creating billable Azure, Entra, or Stripe resources.
3. Changing the Intelligence module extract, SPEC-0012, or ADR-0006.
4. Implementing a product HTTP `/v1` gateway or template Bicep for Azure OpenAI or Azure AI Search.
5. The v0.3 Microsoft agentic module (Foundry Agent Service, Copilot Studio, Entra Agent ID, approvals, evals, and tracing).
6. Runtime interception of outbound HTTP. This spec is a static scan.

## Requirements

### Ubiquitous (always true, no trigger)

> REQ-001: The repository shall contain this Approved spec and ADR-0007 for a build-gate product AI scan that matches the in-process Intelligence guards (`AzureOpenAIGuard`, `AzureAiSearchGuard`), not an HTTP `/v1` gateway and not Bicep.

> REQ-002: The build gate shall run `.arkos/scripts/check-product-ai.sh` over the repository tree.

> REQ-003: The repository shall contain a documented allow-list at `.arkos/product-ai-allowlist.yml` that names marketing-site path prefixes which are not product AI.

> REQ-004: `CHANGELOG.md` `[Unreleased]` shall record this change and shall reference SPEC-0013.

### Event-driven (triggered by an event)

> REQ-005: WHEN the product AI scan finds a non-Azure product AI provider, SDK, or public endpoint in application or manifest source, the scan shall fail and name the file.

> REQ-006: WHEN the product AI scan finds an Azure OpenAI send site that does not call `AzureOpenAIGuard.EnsureCanSend` in the same file, the scan shall fail and name the file.

> REQ-007: WHEN the product AI scan finds an Azure AI Search send site that does not call `AzureAiSearchGuard.EnsureCanSend` in the same file, the scan shall fail and name the file.

> REQ-008: WHEN the product AI scan is pointed at a btros-like Intelligence extract (in-process guards, HttpClient adapters, fail-closed options, api-key or `DefaultAzureCredential`, site-scoped search) the scan shall pass.

> REQ-009: WHEN the product AI scan is pointed at an Access OS-like tree with Entra identity and an audit trail and no product AI, the scan shall pass.

### State-driven (active while in a state)

> REQ-010: WHILE a path prefix is listed in `.arkos/product-ai-allowlist.yml`, the scan shall treat files under that prefix as the marketing site (or another documented non-product surface) and shall not fail them for naming a third-party model.

### Optional feature (conditional on feature being present)

> REQ-011: WHERE a file is a guard-backed Intelligence test (path under a test directory and the file names `AzureOpenAIGuard` or `AzureAiSearchGuard`), the scan shall allow rejected-provider fixtures such as `api.openai.com`.

### Unwanted behaviour (what must not happen)

> REQ-012: IF a change would require a product HTTP `/v1` gateway, template Bicep, or an adapter-emitted accept/edit/reject audit THEN the scan shall NOT require it.

> REQ-013: IF a change would create a billable Azure, Entra, or Stripe resource THEN the change shall NOT create it.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | `.arkos/specs/0013-product-ai-gates.md` has `status: Approved` and names the in-process guards; ADR-0007 exists | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-002 | `.github/workflows/arkos.yml` build-gate runs `check-product-ai.sh`; `.arkos/gates/build.yml` lists CRT-018 | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-003 | `.arkos/product-ai-allowlist.yml` exists and documents the marketing-site rule | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-004 | `CHANGELOG.md` `[Unreleased]` contains SPEC-0013 | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-005 | Fixture `non-azure-openai` fails the scan | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-006 | Fixture `azure-openai-without-guard` fails the scan | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-007 | Fixture `azure-search-without-guard` fails the scan | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-008 | Fixture `btros-like` and the SPEC-0012 extract tree pass the scan | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-009 | Fixture `access-os-like` passes the scan | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-010 | Fixture `marketing-site` with an allow-listed path passes; the same file outside the allow-list fails | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-011 | Fixture `btros-like` tests may name `api.openai.com` as a rejected endpoint and still pass | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-012 | Scanner help and errors do not require `/v1/complete`, Bicep, or an adapter audit call | `bash .arkos/scripts/tests/check-product-ai/run.sh` |
| REQ-013 | This change creates no Azure, Entra, or Stripe resources | manual |

## Privacy notes

This spec does not collect, store, or transmit personal information. It scans committed source in CI.

## Accessibility notes

Not applicable. This spec produces no user interface. The scan is a CI script. Documentation is prose.
