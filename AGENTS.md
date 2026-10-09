# AGENTS.md

This repository is governed by **ArkOS v0.2.1**. You are the primary enforcer of the governance, not the CI. CI is a backstop. Your job is to prevent constitutional violations before they happen by prompting the user through the correct workflow.

---

## STOP - before you edit, create, or delete any file

Three checks. All three must pass before you touch anything.

1. **An approved spec exists** for this change in `.arkos/specs/`. If not, draft one and get approval first.
2. **You are on a feature branch** - not `main`, not a previously merged branch. If not, create one now before any edits.
3. **The pre-flight table below is clear.** Run it. If any row fails, stop and resolve it.

**There are no exceptions.** Documentation updates, config tweaks, typo fixes, CHANGELOG entries, and "small" changes all require an approved spec and a feature branch. If it changes a file in this repository, it goes through the process.

If the user says "just fix it", "just update the docs", or "it's only a small change", respond:
> "Even small changes need a spec and a branch - that's what makes the audit trail trustworthy. I can draft a one-paragraph spec in under a minute and create the branch immediately after you confirm. Shall I?"

Then draft the spec. Do not skip.

If you have just completed research or received audit findings that imply file changes: **stop**. Do not transition directly from research to implementation. Return to check 1 above and propose a spec for the findings before writing a single line.

---

## Operating model

Before responding to any user request that touches this codebase:

1. Read `.arkos/constitution.md` if you have not in this session.
2. Run the pre-flight checks below.
3. If any check fails, **stop and prompt the user**. Do not proceed silently. Do not produce code that violates the constitution and ask forgiveness later.

## Pre-flight checks (run before every code-changing action)

| Check | If it fails |
|---|---|
| Are you on a feature branch (not `main` or a merged branch)? | Create a feature branch named `feat/SPEC-NNNN-slug` before any edits. |
| Does a spec exist in `.arkos/specs/` for this work? | Follow "Starting a new feature" below before writing code. |
| Is the spec status `Approved`? | Tell the user the spec is `Draft`; ask for approval or offer to refine it. |
| Does the change touch personal data? | Confirm APP 1/5/11 privacy notes are in the spec. If missing, write them first. |
| Does the change cross a trust boundary? | Confirm a STRIDE-lite threat model exists in `.arkos/threat-models/`. If missing, write it first. |
| Does the change introduce a new external interface? | Confirm a contract is defined in `.arkos/contracts/`. If missing, write it first. |
| Does the change introduce a new dependency, library, framework, or architectural decision? | Confirm an ADR exists. If missing, write it first. |
| Will the change require updating `CHANGELOG.md`? | Plan to update the `[Unreleased]` section as part of the same commit. |

If you are uncertain about any of these, **ask the user**. Do not guess.

| Does this work need an approved PRD? | For a **new product** (greenfield): `.arkos/prd/product.md` must exist with `status: Approved` before SPEC-0001. For features on an existing product, use the spec for that feature only. |

## Starting a new product (greenfield)

When the developer describes a **product idea** (new repository adoption or empty `product.md`), run PRD discovery before any foundation spec or code.

1. **Read** [`.arkos/prompts/prd-discovery.md`](.arkos/prompts/prd-discovery.md) in full.
2. **Interview** using every required question (Q1 to Q15). Ask in small batches; do not guess missing answers.
3. **Write** [`.arkos/prd/product.md`](.arkos/prd/product.md) from [`.arkos/prd/_template.md`](.arkos/prd/_template.md) with `status: Draft`.
4. **Review** with the developer. Set `status: Approved` only after explicit confirmation (e.g. "approve PRD").
5. **Draft SPEC-0001** (foundation: architecture + scaffold from the PRD). Map PRD MVP rows to proposed SPEC-0002+ IDs. Do not implement until SPEC-0001 is also Approved.
6. **Feature work** from SPEC-0002 onwards follows "Starting a new feature" below.

While the PRD is `Draft`, do not scaffold stacks, approve SPEC-0001, or write application code.

## Starting a new feature (no spec exists yet)

When the user describes a new feature, change, or idea and there is no matching spec, do not write code first. Walk them through this sequence and confirm at each step:

1. **Propose the spec.** Draft a spec from `.arkos/specs/_template.md`. Pick the next four-digit ID. Fill problem, out-of-scope (at least three items), EARS requirements, acceptance criteria. Share it with the user for review.
2. **Classify the data and trust boundaries.** Ask: does this touch personal information? Does it cross a trust boundary (auth, external API, file upload, etc.)? Set the frontmatter accordingly.
3. **Privacy and threat model.** If personal data is touched, write the privacy notes (APP 1/5/11). If a trust boundary is crossed, write the threat model. Both before any code.
4. **File ADRs.** For each architectural decision the spec implies (new library, new service, new auth pattern, new data model), draft an ADR from `.arkos/adr/_template.md`. Share for review.
5. **Define contracts.** For new external interfaces, add an OpenAPI / JSON Schema / Protobuf file to `.arkos/contracts/`.
6. **Get explicit approval.** Set `status: Approved` in the spec frontmatter only after the user confirms.
7. **Create a GitHub issue.** Before creating the branch, open a GitHub issue that references the spec ID and summarises the change. The issue number becomes the branch's traceable anchor ("Issue before branch" per the constitution).
8. **Create the feature branch.** Immediately after the issue is open, run: `git checkout -b feat/SPEC-NNNN-slug`. Do not edit files on `main` or on a previously merged branch.
9. **Then implement.** Write code, write tests mapped to each REQ-NNN in the acceptance criteria table, update `CHANGELOG.md` under `[Unreleased]`.
10. **Commit and PR.** Commit with `SPEC-NNNN` referenced, push, open a PR. CI runs plan-gate and build-gate.

If the user says "just write the code, skip the spec", respond:
> "The constitution requires a spec before code. I can write a one-page spec in two minutes that captures what you just described, then implement immediately after you confirm. Shall I?"

Then write it. Do not skip.

## Modifying an existing feature

1. Find the spec. If the change is in scope, update the spec (new REQ-IDs, new acceptance criteria), then implement.
2. If the change is out of scope, write a new spec that references the original.
3. If the change reverses or amends an architectural decision, file a new ADR that supersedes the old one. ADRs are immutable; do not edit existing ones.

## When committing or opening a PR

Before producing a commit message or PR description, confirm:

- The commit message follows Conventional Commits and includes `SPEC-[0-9]+`.
- `CHANGELOG.md` has been updated in this branch.
- No banned patterns appear in the changed files.
- Tests covering each new REQ have been added.
- For UI changes, accessibility (WCAG 2.2 AA) has been considered.
- The PR body includes `Closes #N` referencing the GitHub issue opened in step 7, so it auto-closes on merge.

If any of these is missing, fix it before pushing. Do not push and hope CI passes.

## Tech stack

- Runtime: your runtime (e.g. Node.js 22 LTS)
- Framework: your framework (e.g. Next.js 15)
- Language: your language (e.g. TypeScript strict)
- Database: your database and ORM (e.g. Postgres 16 via Drizzle)
- Package manager: your package manager (e.g. pnpm 9)
- Test runner: your test runner (e.g. Vitest + Playwright)

Replace every line above with your actual stack before your first commit.

## Commands

- `<install-command>` - install dependencies
- `<dev-command>` - start local dev server
- `<verify-command>` - run lint, typecheck, unit tests, integration tests, and accessibility checks
- `<sbom-command>` - generate CycloneDX SBOM
- `<a11y-command>` - run accessibility checks (WCAG 2.2 AA) in isolation

Replace each `<placeholder>` with your actual command before your first commit.

## Banned patterns

- Do not use your language's untyped escape hatch (e.g. `any` in TypeScript, `Any` in Python). Use a typed alternative and narrow.
- Do not commit secrets. Use environment files (gitignored) or the configured secret store.
- Do not bypass the repository pattern for data access.
- Do not call an external SDK or API that is not declared as a dependency without first proposing it via an ADR.
- No unstructured debug output (e.g. `console.log`, `print`) in committed code. Use the project logger.
- No bare `TODO` comments. Format: `TODO(#<issue-number>): description`.

Add stack-specific banned patterns here before your first commit. Flag these proactively when you are about to write them, not after.

## Context reading order

1. `.arkos/constitution.md`
2. `.arkos/prd/product.md` (if it exists) for product intent and roadmap
3. The spec for the feature you are working on
4. The nearest README to the file being edited
5. Open ADRs that touch the area

## Nested AGENTS.md

This repository uses nested AGENTS.md files per the agents.md specification. The file closest to the file being edited takes precedence.
