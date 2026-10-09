---
id: SPEC-0006
slug: org-migration
title: Update repository references after migration to ark360-hq
status: Approved
owner: "@ark360-hq"
created: 2026-06-26
touches-personal-data: false
trust-boundaries-crossed: false
linked-adrs: []
linked-threat-model: null
---

# SPEC-0006: Update repository references after migration to ark360-hq

## Problem

The repository was transferred from the personal account `parkjadev/arkos` to the
enterprise organisation `ark360-hq/arkos`. Several committed governance records still name
the previous owner in their spec frontmatter, and this repository's own self-referential
metadata (the CHANGELOG release link and the `arkos.yml` project identifier) still carries
template defaults. For the ARK OS audit trail to stay trustworthy, these records must reflect
the new canonical home. This is a metadata correction only; no behaviour, dependency, or
interface changes.

## Out of scope

1. Adopter-facing template placeholders. `QUICKSTART.md` deliberately instructs downstream
   adopters to clone their own repository with `<your-org>/<your-repo>`; those generic
   placeholders are left intact.
2. Personalising other `arkos.yml` template defaults such as the `created` date.
3. GitHub-side configuration (branch protection, Actions enablement, teams, GitHub App
   installs). These are repository settings, not source files, and are handled separately.
4. Any deployment workflow or secrets migration. None exist in this repository.

## Requirements

### Ubiquitous (always true, no trigger)

> REQ-001: The spec records under `.arkos/specs/` shall name `@ark360-hq` as `owner`.

> REQ-002: The `CHANGELOG.md` `[0.1.0]` release link shall resolve to
> `https://github.com/ark360-hq/arkos`.

> REQ-003: The `.arkos/arkos.yml` `project` field shall identify the project as `arkos`.

### Unwanted behaviour (what must not happen)

> REQ-004: IF a file provides adopter-facing template instructions (for example the
> `QUICKSTART.md` clone command) THEN the system shall NOT replace its generic
> `<your-org>/<your-repo>` placeholders with this repository's slug.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | `git grep -n "owner:" .arkos/specs/00*.md` shows `@ark360-hq` for specs 0001-0006 and no `@parkjadev` or `@ARK360` | manual |
| REQ-002 | `CHANGELOG.md` `[0.1.0]` link reads `https://github.com/ark360-hq/arkos/releases/tag/v0.1.0` | manual |
| REQ-003 | `.arkos/arkos.yml` `project` equals `"arkos"` | manual |
| REQ-004 | `QUICKSTART.md` still contains `<your-org>/<your-repo>` in its `git clone` example | manual |

## Accessibility notes

| Criterion | WCAG reference | How addressed |
|---|---|---|
| Metadata-only change | N/A | No user interface component; documentation and config metadata only. |
