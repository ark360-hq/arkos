---
id: SPEC-0010
slug: roadmap-contributing-tidy
title: Align framework roadmap and CONTRIBUTING spec rule
status: Approved
owner: "@ark360-hq"
created: 2026-10-09
touches-personal-data: false
trust-boundaries-crossed: false
linked-adrs: []
linked-threat-model: null
---

# SPEC-0010: Align framework roadmap and CONTRIBUTING spec rule

## Problem

Two published documents are out of date with the released repository and with `AGENTS.md`:

1. `docs/framework.md` section 13 still describes Stage 1 as shipping the template and tagging `v0.1.0`. ArkOS v0.2.1 is already released. The next milestone is v0.3 (Microsoft agentic module: Foundry Agent Service, Copilot Studio, Entra Agent ID, approvals, evals and tracing) and that work is plan stage only.
2. `CONTRIBUTING.md` tells contributors that a spec is not required for minor corrections. `AGENTS.md` requires an approved spec and a feature branch for every file change, including documentation updates, config tweaks, typo fixes, and other small changes.

Readers who follow those two files get the wrong release status and the wrong contribution rule.

## Out of scope

1. Implementing the v0.3 Microsoft agentic module, including Foundry Agent Service, Copilot Studio, Entra Agent ID, approvals, evals, or tracing.
2. Merging this change, creating a git tag, or publishing a GitHub Release.
3. Closing, commenting on, or changing issue #14 (SPEC-0008). That issue stays open.
4. Rewriting historical specs, ADRs, or the `[0.2.1]` and `[0.1.0]` CHANGELOG sections.
5. Implementing `npx create-arkos`, `npx arkos upgrade`, or `npx arkos proof`.

## Requirements

### Ubiquitous (always true, no trigger)

> REQ-001: `docs/framework.md` section 13 shall state that ArkOS v0.2.1 is released.

> REQ-002: `docs/framework.md` section 13 shall describe v0.3 as the Microsoft agentic module covering Foundry Agent Service, Copilot Studio, Entra Agent ID, approvals, evals and tracing, and shall mark that work as plan stage only.

> REQ-003: `CONTRIBUTING.md` shall require an approved spec and a feature branch for every file change, including documentation updates, config tweaks, typo fixes, CHANGELOG entries, and other small changes.

### Unwanted behaviour (what must not happen)

> REQ-004: IF a reader opens `docs/framework.md` section 13 THEN the document shall NOT present Stage 1 as shipping or tagging `v0.1.0`.

> REQ-005: IF a contributor reads `CONTRIBUTING.md` THEN the document shall NOT say that a spec is not required for corrections.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | `docs/framework.md` section 13 contains `v0.2.1` and `released` | manual |
| REQ-002 | `docs/framework.md` section 13 contains `Foundry Agent Service`, `Copilot Studio`, `Entra Agent ID`, `approvals`, `evals`, `tracing`, and `plan stage only` | manual |
| REQ-003 | `CONTRIBUTING.md` states that every file change needs an approved spec and a feature branch, with no exception for small or documentation fixes | manual |
| REQ-004 | `grep -n "Tag \`v0.1.0\`\\|tag \`v0.1.0\`\\|Stage 1 (next 2 weeks)" docs/framework.md` returns no matches | manual |
| REQ-005 | `grep -n "spec is not required" CONTRIBUTING.md` returns no matches | manual |

## Privacy notes

Not applicable. This spec covers documentation only; no personal data is touched.

## Accessibility notes

Not applicable. This spec produces no user interface.
