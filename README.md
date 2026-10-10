# ARK OS

**The governed operating system for agentic software development.**

ARK OS is a GitHub template repository. Use it as the starting point for any project where you need an AI coding agent to behave like an enterprise engineer: spec-driven, auditable, and compliant with Australian security, privacy, and accessibility baselines.

It is a files-and-conventions framework. No runtime. No inference proxy. No vendor lock-in. The governance travels with the codebase.

`npx create-arkos` and `npx arkos upgrade` are **planned and not available**. The current way to use ARK OS is to click **Use this template** on GitHub (or clone this repository and copy the file tree). See Quickstart below.

---

## What ARK OS gives you

| What | File | Purpose |
|---|---|---|
| Agent instructions | `AGENTS.md` | Single source of truth for every agentic IDE |
| IDE adapters | `CLAUDE.md`, `.cursor/rules/arkos.mdc`, `.github/copilot-instructions.md` | Thin pointers to `AGENTS.md`; no duplicated content |
| Framework config | `.arkos/arkos.yml` | Framework version, conformance level, IDE adapters, modules |
| Project constitution | `.arkos/constitution.md` | Five non-negotiable principles that survive across sessions and agents |
| PRD template | `.arkos/prd/_template.md` | Product intent before foundation; filled via PRD discovery interview |
| PRD discovery prompt | `.arkos/prompts/prd-discovery.md` | Required questions when the developer describes a new product idea |
| Spec template | `.arkos/specs/_template.md` | EARS-based requirements before any agent touches a file |
| ADR template | `.arkos/adr/_template.md` | Nygard-format architectural decisions; immutable once accepted |
| Threat model template | `.arkos/threat-models/_template.md` | STRIDE-lite, one per trust-boundary feature |
| Runbook template | `.arkos/runbooks/_template.md` | On-call runbook covering health, alerts, and rollback |
| Contract template | `.arkos/contracts/_template.md` | OpenAPI / JSON Schema / Protobuf stub; one per external interface |
| Gate definitions | `.arkos/gates/` | Machine-readable pass/fail criteria for all five gates |
| Product AI allow-list | `.arkos/product-ai-allowlist.yml` | Marketing-site paths that may name third-party models; product AI is not allow-listed |
| CI workflow | `.github/workflows/arkos.yml` | Governance gates enforced on every PR and push to main |
| Proof Sheet | `.arkos/proof-sheet.md` | Fillable procurement evidence document |
| Intelligence module | `modules/intelligence/` | Extracted in-process Azure OpenAI and Azure AI Search adapters (SPEC-0012) |

---

## Compliance baselines (v1)

| Domain | Standard |
|---|---|
| Application security | OWASP ASVS v5.0.0 Level 1 |
| Cyber hygiene | ASD Essential Eight ML1 |
| Privacy | Privacy Act 1988 (Cth) - APP 1, 5, 11 |
| Accessibility | WCAG 2.2 AA (ISO/IEC 40500:2025) |
| Supply chain | CycloneDX SBOM 1.6+ |

See `docs/standards.md` for versions, rationale, and source links.

---

## IDE support

ARK OS works with any IDE that reads `AGENTS.md` natively. No configuration required for those IDEs. Thin adapter files are included for IDEs that need a pointer.

| IDE | Works how |
|---|---|
| GitHub Copilot | `.github/copilot-instructions.md` points to `AGENTS.md` |
| Cursor | `.cursor/rules/arkos.mdc` points to `AGENTS.md` |
| Claude Code | `CLAUDE.md` imports `AGENTS.md` via `@AGENTS.md` |
| Codex, Windsurf, Amp, Gemini CLI | Read `AGENTS.md` natively; no adapter needed |

---

## Tech stack support

ARK OS is stack-agnostic. The base template contains no stack-specific code, commands, or dependencies. You wire your own install, lint, typecheck, test, and SBOM commands into the CI workflow after adopting the template.

---

## The five governance gates

These gates are the core of ARK OS. Each gate has a named question, machine-readable criteria in `.arkos/gates/`, and CI enforcement where automation is possible.

| Gate | Question | Runs on |
|---|---|---|
| Research | Are we solving the right problem? | Spec review (not automated) |
| Plan | Is the design coherent and contracted? | Every PR and push to main |
| Build | Is what we built correct, safe and accessible? | Every PR and push to main |
| Ship | Is this safe to release and traceable? | Pushes to main only |
| Run | Can we operate this without surprise? | Human checklist in `proof-sheet.md` |

### What the CI workflow automates

The workflow runs four jobs. Two run on every PR and push; two run on pushes to main only.

**Every PR and push to main:**
- `plan-gate` - checks that the PR title, body, or latest commit references a spec (`SPEC-[0-9]+`)
- `build-gate` - runs install, lint, typecheck, unit tests, integration tests, accessibility tests, contract tests, the banned-pattern check, the Intelligence module check, and the product AI policy check

**Pushes to main only:**
- `ship-gate` - generates a CycloneDX SBOM, verifies `CHANGELOG.md` was updated, runs SAST, runs dependency vulnerability scanning
- `run-readiness` - prints the human sign-off checklist; does not block the merge

### What the CI workflow does not automate

- Deployment. This is intentional. Add a separate `.github/workflows/deploy.yml` for your hosting target.
- The Research gate. Problem statements and prior art are reviewed by a human reading the spec.
- The Run gate in full. SLOs, runbooks, and on-call rotation require human sign-off in `proof-sheet.md`.

---

## Quickstart

First governed commit: 30 to 60 minutes. Full CI wiring for a real stack: an afternoon. See `QUICKSTART.md` for the full step-by-step guide. Summary:

1. Click **"Use this template"** on GitHub.
2. Clone your new repo.
3. **Enable branch protection on `main`** with required status checks `plan-gate` and `build-gate`. Without this, every gate is opt-in.
4. Read `.arkos/constitution.md`. Confirm or adjust the process rules.
5. Edit `AGENTS.md` - replace placeholder stack, commands, and banned patterns.
6. Edit `.arkos/arkos.yml` - set `project` name and `created` date (today, YYYY-MM-DD format).
7. Edit `.github/workflows/arkos.yml` - replace placeholder build steps with your stack's commands.
8. Write your first spec in `.arkos/specs/0001-<slug>.md`.
9. Create a feature branch, commit with a spec reference (`SPEC-0001`), and open a PR.
10. CI runs the plan gate and build gate on the PR. Merge when green.

---

## What to configure after adopting the template

The following steps in the CI workflow are placeholders. Replace them before relying on the ship gate:

| Step | What to replace it with |
|---|---|
| Install dependencies | Your stack's install command |
| Lint, Typecheck, Tests | Your stack's commands |
| Generate SBOM | `cyclonedx-npm`, `cyclonedx-py`, or equivalent for your stack |
| SAST | CodeQL (with init step + language) or Semgrep |
| Dependency scan | Trivy or OSV-Scanner |

The banned-pattern checks in `.arkos/scripts/check-banned-patterns.sh` include two JavaScript/TypeScript-specific examples. Replace or remove them for your stack.

The product AI check in `.arkos/scripts/check-product-ai.sh` fails non-Azure product AI and Azure OpenAI or Azure AI Search send sites that do not call `AzureOpenAIGuard.EnsureCanSend` or `AzureAiSearchGuard.EnsureCanSend`. It matches the in-process Intelligence guards. It does not require an HTTP `/v1` gateway. Add marketing-site prefixes to `.arkos/product-ai-allowlist.yml`.

---

## Framework documentation

- `QUICKSTART.md` - step-by-step onboarding guide
- `docs/framework.md` - the ARK OS v0.2.1 framework document
- `docs/standards.md` - standards reference with versions and rationale
- `docs/adr/` - framework-level architectural decisions
- `CONTRIBUTING.md` - how to propose changes to ARK OS itself

---

## Licence

Apache 2.0. Copyright 2026 ARK360. See `LICENSE` and `NOTICE`.

ARK OS is an open-source release from ARK360 - building intelligent enterprise systems to amplify human excellence.
