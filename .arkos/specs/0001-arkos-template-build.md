---
id: SPEC-0001
slug: arkos-template-build
title: ARK OS v0.1 template repository build
status: Approved
owner: "@ark360-hq"
created: 2026-05-13
touches-personal-data: false
trust-boundaries-crossed: false
linked-adrs: [ADR-0001, ADR-0002, ADR-0003]
linked-threat-model: null
---

# SPEC-0001: ARK OS v0.1 template repository build

## Problem

ARK360 needs a reusable, opinionated starting point for any project that requires AI coding agents to behave like enterprise engineers. Every existing alternative (CLAUDE.md, .cursorrules, Spec Kit, internal team wikis) is either IDE-specific, governance-light, or not portable. Solo developers and small enterprise teams have no ready-made framework that ships specs, ADRs, compliance baselines, and CI gates in one codebase-native package.

The first deliverable is the template itself: a GitHub template repository containing 38 files that wire AGENTS.md, a project constitution, EARS-based specs, Nygard ADRs, STRIDE-lite threat models, runbooks, machine-readable gates, three CI enforcement scripts, a GitHub Actions workflow, IDE adapters for Claude Code, Copilot, and Cursor, and a procurement proof sheet.

## Out of scope

1. The `npx create-arkos@latest` CLI scaffolder (v0.2).
2. Stack-specific modules such as `arkos-module-nextjs` (v0.2).
3. An `arkos proof` command that generates a populated proof sheet from the repository (v0.3).
4. IRAP, APRA CPS 234, Essential Eight ML2+, SOC 2 Type II conformance (out of v1 scope entirely).
5. Deployment workflows or hosting opinions of any kind.
6. A worked example project demonstrating ARK OS on a real stack (deferred to v0.2).

## Requirements

### Ubiquitous

> REQ-001: The repository shall be configured as a GitHub template repository so that any user can scaffold a new project from it without forking.

> REQ-002: The repository shall contain `AGENTS.md` at the root as the single source of truth for agent instructions, and shall not duplicate that content in any IDE adapter file.

> REQ-003: The repository shall contain a `.arkos/` directory housing all governance artefacts (constitution, specs, ADRs, threat models, runbooks, gates, scripts, proof sheet, arkos.yml).

> REQ-004: The repository shall be stack-agnostic. Base template files shall not contain runtime-specific code, package manager names, or hardcoded tool invocations. Documentation files may contain stack-specific examples only when clearly labelled as examples.

> REQ-005: The repository shall be IDE-agnostic. AGENTS.md shall be readable natively by any AGENTS.md-compatible IDE. IDE-specific adapter files (CLAUDE.md, .cursor/rules/arkos.mdc, .github/copilot-instructions.md) shall be thin pointers containing no more than ten lines of original content each.

> REQ-006: All documentation shall be written in Australian English and shall not contain em dash characters (U+2014), with the sole exception of `docs/framework.md` which is preserved verbatim per the source framework document.

> REQ-007: The repository shall be licensed under Apache 2.0 with copyright held by ARK360.

### Event-driven

> REQ-008: WHEN a pull request is opened against `main`, the CI workflow shall run the plan gate (spec reference check) and the build gate (lint, typecheck, tests, accessibility, contracts, banned patterns, CHANGELOG).

> REQ-009: WHEN a commit is pushed to `main`, the CI workflow shall run the ship gate (SBOM generation, SAST, dependency scan) and the run-readiness checklist in addition to the plan and build gates.

> REQ-010: WHEN the banned-pattern script runs in CI, it shall scan files changed in the diff (PR base vs head, or HEAD~1..HEAD for direct pushes), not the working tree.

### State-driven

> REQ-011: WHILE a spec is in `Draft` status, the active enforcer (the agent) shall refuse to merge code that depends on it and shall prompt the user to approve the spec first.

### Optional feature

> REQ-012: WHERE the project adopts an IDE that does not natively support AGENTS.md (Claude Code, Aider), the repository shall provide a thin adapter file that points to AGENTS.md.

### Unwanted behaviour

> REQ-013: IF a commit message or PR lacks a `SPEC-[0-9]+` reference THEN the plan gate shall fail with a human-readable error directing the developer to add a reference.

> REQ-014: IF a banned pattern (`console.log`, `: any`, or bare `TODO` without an issue reference) appears in a changed source file THEN the build gate shall fail and identify the file and line.

> REQ-015: IF the CHANGELOG.md is not updated in the PR diff THEN the build gate shall fail with a human-readable error.

> REQ-016: IF the CI workflow attempts to deploy to any environment THEN that workflow shall be rejected at review. Deployment is explicitly out of scope.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | GitHub repository settings show "Template repository" enabled | manual |
| REQ-002 | `wc -l AGENTS.md` returns < 150; `grep` for AGENTS.md content in adapter files returns empty | manual |
| REQ-003 | `ls -la .arkos/` shows constitution, arkos.yml, specs, adr, threat-models, runbooks, gates, scripts, contracts, proof-sheet | manual |
| REQ-004 | `grep -E "pnpm\|npm\|pip\|cargo" AGENTS.md .arkos/ .github/workflows/arkos.yml` returns no unlabelled matches | manual |
| REQ-005 | Each adapter file is under 10 lines; none contain copied content from AGENTS.md | manual |
| REQ-006 | `grep -r "—" .` returns matches only in `docs/framework.md` | automated (lint script, future) |
| REQ-007 | `LICENSE` is Apache 2.0; copyright line names ARK360 | manual |
| REQ-008 | PR triggers plan-gate and build-gate jobs in GitHub Actions | manual |
| REQ-009 | Push to main triggers ship-gate and run-readiness jobs in addition | manual |
| REQ-010 | `check-banned-patterns.sh` test cases: empty staging area in CI does not silently pass; staged file is scanned locally | shell test (future) |
| REQ-011 | Reviewer-enforced; AGENTS.md "Pre-flight checks" documents the requirement | manual |
| REQ-012 | `CLAUDE.md` contains `@AGENTS.md`; `.cursor/rules/arkos.mdc` references AGENTS.md; `.github/copilot-instructions.md` references AGENTS.md | manual |
| REQ-013 | Commit without `SPEC-[0-9]+` triggers plan-gate failure with message containing "No spec reference found" | shell test |
| REQ-014 | Adding `console.log` to a `.ts` file triggers build-gate failure | shell test |
| REQ-015 | Commit without modifying CHANGELOG.md triggers build-gate failure | shell test |
| REQ-016 | Reviewer-enforced; workflow file contains explicit comment stating deployment is out of scope | manual |

## Privacy notes

Not applicable. This spec covers the framework repository build itself; it does not touch personal data.

## Accessibility notes

Not applicable. This spec produces documentation and CI tooling, not a user interface. Accessibility obligations apply to projects built on top of ARK OS, not the framework repository.
