---
id: SPEC-0002
slug: agents-md-hardening
title: Harden AGENTS.md against task-momentum governance bypass
status: Approved
owner: "@ark360-hq"
created: 2026-05-13
touches-personal-data: false
trust-boundaries-crossed: false
linked-adrs: []
linked-threat-model: null
---

# SPEC-0002: Harden AGENTS.md against task-momentum governance bypass

## Problem

The v0.1 AGENTS.md instructs the agent to run pre-flight checks before every code-changing action, but the instruction is too easy to rationalise past under task momentum. Three failure modes were observed during the v0.1 build:

1. After receiving audit findings, the agent transitioned directly from research to implementation without pausing to check for a spec or create a branch.
2. Documentation and configuration changes were treated as exempt from the spec-first requirement because they do not feel like "features."
3. Branch creation appears at step 8 of the new-feature workflow - after the agent has already started drafting content - so it was skipped.

The result was the agent editing files on a merged branch without a spec, which is exactly the governance failure ARK OS is designed to prevent. A developer using this template should not have to manually redirect the agent back to the process. The framework should catch itself.

## Out of scope

1. Changes to CI scripts or gate definitions - AGENTS.md is an agent instruction file, not a CI config.
2. Changes to `docs/framework.md` beyond what is needed to stay consistent with AGENTS.md.
3. Stack-specific banned patterns or command placeholders - those are adopter responsibilities.

## Requirements

> REQ-001: AGENTS.md shall open with a hard-stop block (before any other content) that names the three mandatory checks - approved spec exists, on a feature branch, pre-flight table clear - and explicitly states that documentation, config, and "small" changes are not exempt.

> REQ-002: WHEN an agent receives research findings or an audit result that implies file changes, AGENTS.md shall explicitly instruct the agent to pause, return to the pre-flight checks, and propose a spec before implementing any finding.

> REQ-003: The "Starting a new feature" workflow shall instruct the agent to create the feature branch immediately after spec approval and before editing any file.

> REQ-004: AGENTS.md shall include an explicit pushback script for requests framed as "just fix it", "just update the docs", or "just a small change" - equivalent to the existing pushback for "skip the spec."

> REQ-005: The pre-flight checks table shall include an explicit check: "Are you on a feature branch (not `main` or a previously merged branch)?" with a clear failure action.

> REQ-006: The "Starting a new feature" workflow shall instruct the agent to create a GitHub issue before creating the feature branch, consistent with the constitution rule "Issue before branch."

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | Hard-stop block appears before `## Operating model` and before any other section | manual |
| REQ-002 | Section covering research-to-implementation transition exists in AGENTS.md | manual |
| REQ-003 | Branch creation step appears before "Then implement" in the new-feature workflow | manual |
| REQ-004 | Pushback script for "just fix it" / "just update the docs" exists in AGENTS.md | manual |
| REQ-005 | Pre-flight table includes a branch check row | manual |
| REQ-006 | New-feature workflow includes a GitHub issue creation step before branch creation | manual |

## Privacy notes

Not applicable. This spec covers agent instruction files only; no personal data is touched.

## Accessibility notes

Not applicable. This spec produces no user interface.
