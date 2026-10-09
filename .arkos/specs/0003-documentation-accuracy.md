---
id: SPEC-0003
slug: documentation-accuracy
title: Fix documentation inaccuracies, add contracts template, wire issue auto-close
status: Approved
owner: "@ark360-hq"
created: 2026-05-13
touches-personal-data: false
trust-boundaries-crossed: false
linked-adrs: []
linked-threat-model: null
---

# SPEC-0003: Fix documentation inaccuracies, add contracts template, wire issue auto-close

## Problem

A documentation audit of the v0.1 repository found five gaps that would mislead a new adopter or cause confusion during onboarding:

1. `docs/framework.md` line 352 references `.arkos/proof-essential-eight.md` - that file does not exist. The correct file is `.arkos/proof-sheet.md`.
2. `AGENTS.md` pre-flight checks and `docs/framework.md` both reference `.arkos/contracts/` but no directory or template exists in the repository. An adopter following the pre-flight check has nowhere to put a contract.
3. `README.md` quickstart step 6 says "set `project` and `created` date" without specifying the required date format. `QUICKSTART.md` Step 6 does not mention the `created` field at all.
4. The `README.md` "What ARK OS gives you" table omits the contracts directory despite it being a recommended artefact in the framework doc and referenced in AGENTS.md.
5. GitHub issues opened for a spec do not auto-close when the PR merges because the agent does not include a closing keyword in the PR body.

## Out of scope

1. Changes to CI scripts or gate definitions.
2. Adding actual contract files for any specific project - that is the adopter's responsibility.
3. Changes to `docs/framework.md` beyond the single broken file reference on line 352.

## Requirements

> REQ-001: `docs/framework.md` shall not reference `.arkos/proof-essential-eight.md`. The reference shall be updated to `.arkos/proof-sheet.md`.

> REQ-002: `.arkos/contracts/_template.md` shall exist and provide a minimal starting point (format stub, guidance on OpenAPI / JSON Schema / Protobuf, and a link to the gate criterion).

> REQ-003: `README.md` quickstart step 6 and `QUICKSTART.md` Step 6 shall both instruct the adopter to set the `created` field to today's date in YYYY-MM-DD format.

> REQ-004: `README.md` "What ARK OS gives you" table shall include a row for `.arkos/contracts/_template.md`.

> REQ-005: The "Commit and PR" section of `AGENTS.md` shall instruct the agent to include `Closes #N` in the PR body referencing the issue opened in the new-feature workflow, so the issue auto-closes on merge.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | `grep -r "proof-essential-eight" .` returns no matches | manual |
| REQ-002 | `ls .arkos/contracts/_template.md` succeeds | manual |
| REQ-003 | `grep "YYYY-MM-DD" README.md QUICKSTART.md` returns a match in both files | manual |
| REQ-004 | `grep "contracts" README.md` returns a match in the "What ARK OS gives you" table | manual |
| REQ-005 | `grep "Closes" AGENTS.md` returns a match in the "When committing or opening a PR" section | manual |

## Privacy notes

Not applicable. This spec covers documentation and templates only; no personal data is touched.

## Accessibility notes

Not applicable. This spec produces no user interface.
