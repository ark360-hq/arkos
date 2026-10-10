# Changelog

All notable changes to ARK OS are documented in this file.

This project follows [Keep a Changelog 1.1](https://keepachangelog.com/en/1.1.0/) and [Semantic Versioning 2.0](https://semver.org/).

---

## [Unreleased]

### Changed

- Brand prose and wordmark now use ARK OS (two words, all caps); camel-case and smashed-wordmark forms retired in current docs (SPEC-0011)
- `docs/framework.md` roadmap now records v0.2.1 as released and lists v0.3 (Microsoft agentic module: Foundry Agent Service, Copilot Studio, Entra Agent ID, approvals, evals and tracing) as plan stage only; stale Stage 1 ship/tag `v0.1.0` wording removed (SPEC-0010)
- `CONTRIBUTING.md` now matches `AGENTS.md`: every file change needs an approved spec and a feature branch, including small and documentation fixes (SPEC-0010)

### Added

- Intelligence module extract at `modules/intelligence/` from btros main `a04a31a` (#472 Azure OpenAI adapter, #473 AI Search RAG slice): in-process C# clients, guards, stubs, and tests. No template Bicep and no product HTTP `/v1` gateway (SPEC-0012)
- `.arkos/specs/0012-intelligence-module.md`, ADR-0005, `.arkos/threat-models/intelligence-module.md`, and `.arkos/contracts/intelligence-gateway.md` - Approved spec for a reusable Intelligence module (Azure OpenAI and Azure AI Search Bicep plus governed gateway), extracted from btros after that adapter and RAG slice land (SPEC-0012)
- `.arkos/specs/0011-brand-rename-ark-os.md` - spec for the brand rename in prose and branding (SPEC-0011)
- `.arkos/specs/0010-roadmap-contributing-tidy.md` - spec for the post-v0.2.1 roadmap and contributing tidy-up (SPEC-0010)

## [0.2.1] - 2026-10-09

### Changed

- Framework version labels aligned to `0.2.1` in `.arkos/arkos.yml`, `AGENTS.md`, `.arkos/proof-sheet.md`, `docs/standards.md`, `docs/framework.md`, `README.md`, `QUICKSTART.md`, and `SECURITY.md` (SPEC-0009)
- `npx create-arkos` and `npx arkos upgrade` documented as planned and not available; GitHub **Use this template** (or clone of this repository) is the current adoption path (SPEC-0009)
- `.arkos/arkos.yml` bumped to `arkos-version: "0.2"` and now declares `claude-extension: true` under `ide-adapters` (SPEC-0005)
- `.arkos/specs/0001` to `0004` `owner` fields updated from `@parkjadev` / `@ARK360` to `@ark360-hq` after the repository moved to the `ark360-hq` organisation (SPEC-0006)
- `CHANGELOG.md` `[0.1.0]` release link now points to `ark360-hq/arkos` (SPEC-0006)
- `.arkos/arkos.yml` `project` set to `arkos` (SPEC-0006)
- `.arkos/scripts/check-changelog.sh` now fetches `PR_BASE_SHA` and `PR_HEAD_SHA` so shallow PR merge checkouts detect CHANGELOG updates reliably (SPEC-0004)
- `AGENTS.md` adds greenfield "Starting a new product" workflow and PRD pre-flight check (SPEC-0004)
- `.arkos/constitution.md` adds "PRD before foundation" process rule (SPEC-0004)
- `QUICKSTART.md` adds Step 7 PRD discovery before foundation spec; renumbers later steps (SPEC-0004)
- `AGENTS.md` "When committing or opening a PR" checklist now requires `Closes #N` in the PR body so GitHub issues auto-close on merge (SPEC-0003)
- `AGENTS.md` restructured with a hard-stop block at the top, explicit branch check in the pre-flight table, GitHub issue creation step before branch creation, branch creation moved before file editing in the new-feature workflow, pushback script for "just fix it" requests, and explicit instruction to pause after research findings before implementing (SPEC-0002)
- `README.md` quickstart step 6 and `QUICKSTART.md` Step 4 now specify YYYY-MM-DD format for the `created` field in `arkos.yml` (SPEC-0003)

### Added

- `.arkos/specs/0009-v0.2.1-cleanup-release.md` - spec for the v0.2.1 cleanup release (SPEC-0009)
- `NOTICE` - ARK360 copyright and attribution moved out of `LICENSE` so GitHub can detect Apache-2.0 (SPEC-0009)
- `.claude/` Claude Code extension layer: `README.md`, `settings.json` (hooks disabled by default), `settings.local.json.example`, `rules/arkos.md`, `commands/governance-check.md`, `commands/scaffold-spec.md`, `hooks/block-edit-on-main.sh`, and `hooks/README.md` (SPEC-0005)
- `.arkos/specs/0005-claude-code-extension-layer.md` and `.arkos/adr/0004-claude-code-extension-layer.md` - spec and ADR for the optional Claude Code extension layer (SPEC-0005)
- `.mcp.json.example` example-only MCP template, and `.gitignore` ignoring `.claude/settings.local.json`, `.mcp.json`, and `CLAUDE.local.md` (SPEC-0005)
- `docs/v0.2-enhancement-proposal.md` - v0.2 enhancement proposal for the extension layer (SPEC-0005)
- `.arkos/specs/0007-ci-check-name-consistency.md` - spec covering CI check-name alignment with documented branch-protection contexts (SPEC-0007)
- `.arkos/specs/0006-org-migration.md` - spec covering repository reference updates after the ark360-hq migration (SPEC-0006)
- `.arkos/prd/_template.md` and `.arkos/prd/README.md` for product requirements before SPEC-0001 (SPEC-0004)
- `.arkos/prompts/prd-discovery.md` with required clarification questions (Q1 to Q15) for agent interviews (SPEC-0004)
- `.arkos/prompts/README.md` index for agent playbooks (SPEC-0004)
- `.arkos/specs/0004-prd-discovery-workflow.md` - spec for PRD template and discovery flow (SPEC-0004)
- `README.md` table rows for PRD template and discovery prompt (SPEC-0004)
- `.arkos/contracts/_template.md` - minimal contract stub for OpenAPI / JSON Schema / Protobuf; satisfies the AGENTS.md pre-flight check for new external interfaces (SPEC-0003)
- `.arkos/specs/0003-documentation-accuracy.md` - spec covering documentation fixes and contracts template
- `.arkos/specs/0002-agents-md-hardening.md` - spec covering the AGENTS.md governance bypass hardening
- `README.md` "What ArkOS gives you" table now includes the contracts template row (SPEC-0003)

### Fixed

- `LICENSE` restored to the unmodified Apache License 2.0 text so GitHub detects Apache-2.0 (SPEC-0009)
- `CHANGELOG.md` `[0.1.0]` footer no longer points at the non-existent `v0.1.0` tag; Keep a Changelog compare links added for `[Unreleased]` and `[0.2.1]` (SPEC-0009)
- `.github/workflows/arkos.yml` gate jobs no longer override `name:`, so their check-run names match the `plan-gate` / `build-gate` / `ship-gate` / `run-readiness` contexts documented in `QUICKSTART.md` and `README.md`; previously branch protection requiring those contexts could never match and blocked merges (SPEC-0007)
- `docs/framework.md` conformance summary referenced non-existent `.arkos/proof-essential-eight.md`; corrected to `.arkos/proof-sheet.md` (SPEC-0003)
- `check-banned-patterns.sh` bare-TODO check now excludes `.md`, `.sh`, `.yml`, and `.yaml` files via a configurable `TODO_EXCLUDED_REGEX`, preventing false positives when documentation or tooling files describe the TODO pattern in prose or enforce it in code

---

## [0.1.0] - 2026-05-13

### Added

- `.arkos/specs/0001-arkos-template-build.md` - canonical spec for the v0.1 build that all post-build commits reference
- `permissions: contents: read` at workflow level for least-privilege CI
- Pre-flight checks table and "Starting a new feature" workflow in `AGENTS.md` so the agent actively prompts the user through the constitution rather than relying on CI failures
- Stack-agnostic enforcement scoping in `check-banned-patterns.sh` (SOURCE_EXTENSIONS_REGEX prevents false positives in markdown / YAML)
- First-commit handling in `check-banned-patterns.sh` and `check-changelog.sh`
- PR-range commit scanning in `check-spec-link.sh` so merge commits are handled correctly
- New gate criterion CRT-004 (spec reference in PR) on the plan gate; CRT-017 (CHANGELOG updated) on the build gate
- `QUICKSTART.md` "What is not in this workflow" section making the deployment boundary explicit
- Top-of-file comment in `arkos.yml` explaining the governance-only / no-deployment posture
- `AGENTS.md` - portable agent instruction file, single source of truth for all agentic IDEs
- `CLAUDE.md` - Claude Code adapter with @import pointer to AGENTS.md
- `.cursor/rules/arkos.mdc` - Cursor adapter pointing to AGENTS.md
- `.github/copilot-instructions.md` - GitHub Copilot adapter pointing to AGENTS.md
- `.arkos/constitution.md` - project constitution with five non-negotiable principles
- `.arkos/arkos.yml` - framework configuration file
- `.arkos/specs/_template.md` - EARS-based spec template
- `.arkos/adr/_template.md` - Nygard ADR template
- `.arkos/adr/0001-agents-md-as-portable-instruction-standard.md` - ADR: AGENTS.md as the portable standard
- `.arkos/adr/0002-ears-as-requirements-notation.md` - ADR: EARS as requirements notation
- `.arkos/adr/0003-apache-2-licence.md` - ADR: Apache 2.0 licensing
- `.arkos/threat-models/_template.md` - STRIDE-lite threat model template
- `.arkos/runbooks/_template.md` - operational runbook template
- `.arkos/gates/research.yml` - Research gate criteria
- `.arkos/gates/plan.yml` - Plan gate criteria
- `.arkos/gates/build.yml` - Build gate criteria
- `.arkos/gates/ship.yml` - Ship gate criteria
- `.arkos/gates/run.yml` - Run gate criteria
- `.arkos/scripts/check-spec-link.sh` - CI script: spec reference check
- `.arkos/scripts/check-changelog.sh` - CI script: CHANGELOG update check
- `.arkos/scripts/check-banned-patterns.sh` - CI script: banned pattern check
- `.arkos/proof-sheet.md` - procurement evidence template
- `.github/workflows/arkos.yml` - CI workflow enforcing all governance gates
- `.github/ISSUE_TEMPLATE/feature.md` - feature issue template
- `.github/ISSUE_TEMPLATE/bug.md` - bug report template
- `.github/pull_request_template.md` - PR checklist template
- `docs/framework.md` - ArkOS v0.1 framework document
- `docs/standards.md` - standards reference with versions and rationale
- `docs/adr/` - framework-level ADRs mirrored for documentation
- `README.md` - repository overview and quickstart
- `QUICKSTART.md` - standalone onboarding guide
- `CONTRIBUTING.md` - contribution guidelines
- `SECURITY.md` - coordinated disclosure policy
- `LICENSE` - Apache 2.0

### Changed

- `AGENTS.md` repositioned as an active-enforcer instruction set, not a passive reference
- IDE adapters (`CLAUDE.md`, `.cursor/rules/arkos.mdc`, `.github/copilot-instructions.md`) reinforce the active-enforcer framing
- `check-changelog.sh` moved from `ship-gate` to `build-gate` so PRs enforce CHANGELOG updates pre-merge instead of post-merge
- Banned-pattern checks on `console.log` and `: any` scoped to `.js/.jsx/.ts/.tsx/.mjs/.cjs` source files only
- SAST and dependency scan workflow steps converted to commented placeholders (CodeQL requires an init step; users now have explicit guidance)
- `proof-sheet.md` evidence column annotated "after wiring" for placeholder CI steps
- `.arkos/arkos.yml` `created` field is now a `<YYYY-MM-DD>` placeholder rather than the hard-coded build date
- `CHANGELOG.md` 0.1.0 link converted to `<your-org>/<your-repo>` placeholder
- PR template banned-pattern checklist item now references `AGENTS.md` instead of hardcoding JS-specific patterns
- `QUICKSTART.md` first-commit budget reconciled from 15 minutes to 30 to 60 minutes; full CI wiring stated as "an afternoon"
- `QUICKSTART.md` first-commit flow switched from `push origin main` to feature branch + pull request, consistent with branch-protected repositories
- Em dashes removed from all files except `docs/framework.md` (preserved verbatim per REQ-049)

### Fixed

- YAML parse failure in `arkos.yml` workflow caused by inline comments between step keys and by unquoted colons in placeholder echo strings
- `check-banned-patterns.sh` silently passing in CI because it scanned the (always-empty) staging area instead of the diff
- `check-changelog.sh` crashing on the first commit of a fresh repository (no `HEAD~1`)
- `check-spec-link.sh` missing merge commits where the spec reference appears in an earlier commit on the branch
- `proof-sheet.md` referencing a non-existent `.arkos/proof-essential-eight.md` file

---

[Unreleased]: https://github.com/ark360-hq/arkos/compare/v0.2.1...HEAD
[0.2.1]: https://github.com/ark360-hq/arkos/compare/0583d0d7f2d70ecefef1fd1b856cf8e1f13315bd...v0.2.1
[0.1.0]: https://github.com/ark360-hq/arkos/compare/349f709efc9af22b38d969eb33370d4620630c19...0583d0d7f2d70ecefef1fd1b856cf8e1f13315bd
