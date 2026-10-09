---
id: SPEC-0011
slug: brand-rename-ark-os
title: Rename brand ArkOS to ARK OS in prose and branding
status: Approved
owner: "@ark360-hq"
created: 2026-10-09
touches-personal-data: false
trust-boundaries-crossed: false
linked-adrs: []
linked-threat-model: null
---

# SPEC-0011: Rename brand ArkOS to ARK OS in prose and branding

## Problem

Published prose and branding still present the product as ArkOS (camel case) or ARKOS (smashed wordmark). James Park approved the brand as ARK OS: two words, all caps, in body copy and in the wordmark.

Readers, adopters, and agents who copy the current name into new writing will keep shipping the retired form. The audit trail should show one approved rename rather than ad-hoc edits.

## Out of scope

1. Merging this change, creating a git tag, or publishing a GitHub Release.
2. Retitling the v0.2.1 GitHub Release, or editing release notes on GitHub.
3. Renaming the repository slug `arkos`, the `.arkos/` folder or paths, `arkos.yml` or its keys, the `arkos` workflow or its check names, rules file names, or the `create-arkos` and `arkos` command and package names.
4. Rewriting historical `[0.2.1]` and `[0.1.0]` CHANGELOG entries.
5. Implementing `npx create-arkos`, `npx arkos upgrade`, or `npx arkos proof`.

## Requirements

### Ubiquitous (always true, no trigger)

> REQ-001: Current prose that names the product shall use ARK OS (two words, all caps), not camel-case ArkOS or the smashed wordmark ARKOS.

> REQ-002: `docs/framework.md` shall use ARK OS as the document title wordmark and as the Wordmark heading value.

> REQ-003: `docs/framework.md` approved product terms shall list ARK OS as the framework name and shall state that body copy uses two words, all caps, and never "Arkos".

> REQ-004: `CHANGELOG.md` `[Unreleased]` shall record the brand rename and reference SPEC-0011.

### Unwanted behaviour (what must not happen)

> REQ-005: IF a reader opens the `[0.2.1]` or `[0.1.0]` sections of `CHANGELOG.md` THEN those historical entries shall NOT be rewritten.

> REQ-006: IF the rename is applied THEN the repository shall NOT rename the slug `arkos`, the `.arkos/` folder or paths, `arkos.yml` keys, the `arkos` workflow name, the `plan-gate` / `build-gate` / `ship-gate` / `run-readiness` check names, rules file names, or the `create-arkos` and `arkos` command names.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | After excluding `.arkos/specs/0011-brand-rename-ark-os.md` and `CHANGELOG.md`, `grep -nE 'ArkOS\|ARKOS'` across the repository returns no matches | manual |
| REQ-002 | `docs/framework.md` line 1 is `# ARK OS` and the Wordmark subsection presents `**ARK OS**` | manual |
| REQ-003 | The approved product terms table in `docs/framework.md` has an `ARK OS` row that forbids "Arkos" in body copy and states the wordmark is ARK OS | manual |
| REQ-004 | `CHANGELOG.md` `[Unreleased]` contains `ARK OS` and `SPEC-0011` | manual |
| REQ-005 | `git diff main -- CHANGELOG.md` does not change any line at or after `## [0.2.1]` | manual |
| REQ-006 | `.github/workflows/arkos.yml` still has `name: arkos`; `.arkos/arkos.yml` still has keys `arkos-version` and `project: "arkos"`; `README.md` still documents `npx create-arkos` and `npx arkos upgrade`; `.cursor/rules/arkos.mdc` and `.claude/rules/arkos.md` filenames are unchanged | manual |

## Privacy notes

Not applicable. This spec covers branding and documentation only; no personal data is touched.

## Accessibility notes

Not applicable. This spec produces no user interface.
