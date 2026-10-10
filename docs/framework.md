# ARK OS
## The Governed Operating System for Agentic Software Development

**Version 0.2.1** | **Australian English** | **October 2026**

> An open framework from ARK360 — building intelligent enterprise systems to amplify human excellence.

---

## TL;DR

ARK OS is a portable, IDE-agnostic spec and governance framework that ships as files and conventions inside the codebase itself. It turns any AI coding agent (Claude Code, GitHub Copilot, Cursor, Codex, Windsurf) into an enterprise-grade engineer by binding it to a single source of truth (AGENTS.md), a project constitution, EARS-based specs, Nygard ADRs, and five CI-enforced governance gates.

v1 is deliberately lightweight: Essential Eight ML1, OWASP ASVS v5 L1, WCAG 2.2 AA, APP 1/5/11, CycloneDX SBOM, Conventional Commits, Keep a Changelog. Enough to clear most SMB and mid-market procurement reviews. Not so much that a solo developer drowns.

A solo dev adopts ARK OS in an afternoon. An enterprise procurement reviewer can verify the evidence trail from the repo alone.

---

## 1. Positioning

**One line:**

> ARK OS is the governed operating system for agentic software development. Specs, decisions, gates and audit trails ship with the code, so any AI coding agent behaves like an enterprise engineer.

**Where ARK OS comes from:**

ARK OS is the reusable IP asset at the centre of ARK360's practice. ARK360 is a consulting and product engineering practice that builds intelligent enterprise systems to amplify human excellence. ARK OS is how that practice delivers repeatably — the opinionated framework ARK360 brings into every engagement, now shipped openly so the broader community can build on it.

ARK OS is open-source because the best frameworks earn authority through adoption, not through being kept proprietary. If ARK OS makes agentic development more reliable and effective for the industry, that serves ARK360's mission.

**The problem ARK OS solves:**

Every AI coding tool ships its own conventions. CLAUDE.md, `.cursorrules`, `.github/copilot-instructions.md` — useful individually, inconsistent collectively. Agentic development without a framework is fast until something goes wrong, then it is slow in exactly the ways enterprise environments cannot afford: no audit trail, no traceability, no reproducible process, no evidence for procurement.

ARK OS is the portable layer above the tools. The codebase carries the governance. The agent reads it. The gates enforce it. The repo proves it.

**What ARK OS is not:**

- Not a runtime. No policy engine, no telemetry collector, no inference proxy.
- Not a CI provider. Ships a workflow skeleton for GitHub Actions; does not run jobs.
- Not a compliance certification. Produces evidence. Auditors interpret it.
- Not opinionated about cloud or framework. Opinionated about process and artefacts.

---

## 2. The ARK OS Manifesto

Six principles. Non-negotiable for any project claiming ARK OS conformance.

1. **Intent is the artefact. Code is the rendering.** The spec, the ADR and the constitution are version-controlled. The implementation regenerates from them.
2. **Portable over proprietary.** AGENTS.md is the single source of truth. IDE-native files are thin pointers, not copies.
3. **Gates over guardrails.** A failing CI check beats a polite README. Governance happens at the merge boundary, not in a culture deck.
4. **Convention over configuration.** Opinionated defaults that work the moment the repo is cloned. Configuration is the escape hatch, not the entry path.
5. **Lightweight enterprise by default.** Essential Eight ML1, ASVS v5 L1, WCAG 2.2 AA, APP 1/5/11. Enough to clear procurement; not so much that a solo developer drowns.
6. **The repo is the audit trail.** Specs, ADRs, threat models, SBOMs, changelogs and run logs all live in git. If it is not in the repo, it did not happen.

---

## 3. Standards ARK OS binds to

| Layer | Standard | Why |
|---|---|---|
| Agent instructions | **AGENTS.md** | The only cross-vendor convention with majority adoption (60,000+ projects as of Dec 2025 per AAIF). Native in Copilot, Cursor, Codex, Windsurf, Gemini CLI, Amp. |
| Tool integration | **Model Context Protocol (MCP)** | AAIF-stewarded. Native across Claude, Cursor, Copilot, ChatGPT, Gemini. |
| Requirements | **EARS** | Five sentence patterns that compile to testable acceptance criteria. Native in AWS Kiro IDE. |
| Decisions | **ADR (Nygard format)** | Battle-tested since 2011. Fits in markdown. |
| Security | **OWASP ASVS v5.0.0 Level 1** | L1 (released May 2025) is the first layer of defence, not a pen-test floor. |
| AU cyber | **Essential Eight ML1** | ASD's published minimum. The procurement floor. |
| AU privacy | **APP 1, 5, 11** | Open management, collection notification, security of personal information. |
| Accessibility | **WCAG 2.2 AA** | Now ISO/IEC 40500:2025 (approved Oct 2025). What procurement reviewers expect. |
| SBOM | **CycloneDX 1.6+** | OWASP project. Lighter than SPDX. Better tooling for typical web stacks. |
| Commits + Changelog | **Conventional Commits 1.0 + Keep a Changelog 1.1** | Machine-readable history. Auto-generated CHANGELOG. |

**Explicitly out of scope for v1:** IRAP, Essential Eight ML2+, SLSA L3/L4, ISO 27001 full ISMS, SOC 2 Type II.

---

## 4. The ARK OS file tree

Mandatory **[M]**, recommended **[R]**, optional **[O]**.

```
<repo-root>/
├── AGENTS.md                          [M, portable]    Primary agent instruction file
├── CLAUDE.md                          [O, Claude Code] One-line @import pointer to AGENTS.md
├── .cursor/rules/arkos.mdc            [O, Cursor]      Pointer rule that defers to AGENTS.md
├── .github/copilot-instructions.md    [O, Copilot]     Pointer to AGENTS.md
├── .aider.conf.yml                    [O, Aider]       read: AGENTS.md
├── .arkos/
│   ├── arkos.yml                      [M]    Framework version, modules, conformance level
│   ├── constitution.md                [M]    Project non-negotiables
│   ├── specs/
│   │   ├── 0001-<slug>.md             [M]    EARS-based spec, one per feature
│   │   └── _template.md               [M]
│   ├── adr/
│   │   ├── 0001-<slug>.md             [M]    Nygard ADRs, monotonic numbering
│   │   └── _template.md               [M]
│   ├── threat-models/
│   │   ├── <feature>.md               [R]    STRIDE-lite, one per trust-boundary feature
│   │   └── _template.md               [R]
│   ├── runbooks/
│   │   ├── <service>.md               [R]    On-call runbook
│   │   └── _template.md               [R]
│   ├── contracts/                     [R]    OpenAPI / JSON Schema / Protobuf
│   ├── proof-sheet.md                 [R]    The procurement one-pager
│   └── gates/
│       ├── research.yml               [M]    Machine-readable gate criteria
│       ├── plan.yml                   [M]
│       ├── build.yml                  [M]
│       ├── ship.yml                   [M]
│       └── run.yml                    [R]
├── .github/workflows/
│   └── arkos.yml                      [M]    Single CI workflow enforcing all gates
├── CHANGELOG.md                       [M]    Keep-a-Changelog convention
├── SECURITY.md                        [M]    Coordinated disclosure
├── LICENSE                            [M]    Apache 2.0
└── README.md                          [M]    For humans; AGENTS.md is for agents
```

---

## 5. Core templates

### 5.1 AGENTS.md

```markdown
# AGENTS.md

This repository is governed by **ARK OS v0.2.1**. Read `.arkos/constitution.md` before doing anything else.

## Project
A short paragraph: what this is, who uses it, why it exists. No marketing copy.

## Tech stack
- Runtime: Node.js 22 LTS
- Framework: Next.js 15 (App Router, RSC)
- Language: TypeScript strict
- Database: Postgres 16 via Drizzle
- Package manager: pnpm 9
- Test: Vitest + Playwright

## How to work in this repo
1. Read `.arkos/constitution.md`.
2. Read the relevant spec in `.arkos/specs/`.
3. Read open ADRs in `.arkos/adr/`.
4. Run `pnpm verify` after every change. It must pass before you finish.
5. Conventional Commits. Imperative mood. Reference the spec ID.

## Commands
- `pnpm install` — install
- `pnpm dev` — local dev server
- `pnpm verify` — lint, typecheck, unit, integration, a11y (the build gate)
- `pnpm sbom` — generate CycloneDX SBOM
- `pnpm test:a11y` — axe + Playwright accessibility checks

## Banned patterns
- Do not use `any` in TypeScript. Use `unknown` and narrow.
- Do not commit secrets. Use `.env.local` (gitignored) or the configured secret store.
- Do not bypass the repository pattern in `src/server/db/`.
- Do not call an SDK or API not in `package.json` without first proposing it via an ADR.
- No `console.log` in committed code. Use the logger in `src/lib/log.ts`.

## Context reading order
1. `.arkos/constitution.md`
2. The spec for the feature you are working on
3. The nearest README to the file being edited
4. Open ADRs that touch the area

## Nested AGENTS.md
This repo uses nested AGENTS.md per the agents.md spec. The closest file to the one being edited wins.
```

### 5.2 constitution.md

```markdown
# Constitution

These are the rules that survive across sessions, agents and contributors.
If you are about to break one, stop and open an ADR.

## Non-negotiable principles
1. **No silent failures.** Every error is logged, surfaced or handled. Empty catch blocks are a defect.
2. **Tests gate merges.** Red main is a P1 incident.
3. **Contracts before code.** Every external interface is defined in `.arkos/contracts/` before implementation lands.
4. **No personal data without a spec.** If a change collects, stores or transmits personal information, the spec must reference APP 1, 5 and 11 and a threat model must exist.
5. **Accessibility is a build gate.** WCAG 2.2 AA failures fail the build.

## Conformance level
- Security: OWASP ASVS v5.0.0 Level 1
- Cyber baseline: Essential Eight ML1
- Privacy: APP 1, 5, 11 minimum coverage
- Accessibility: WCAG 2.2 AA (ISO/IEC 40500:2025)
- SBOM: CycloneDX 1.6+ on every release

## Process
- Plan before code: every non-trivial change has a spec in `.arkos/specs/`.
- Issue before branch: every branch maps to a tracked issue.
- Zero-error boot: `pnpm verify` is green on `main`, always.
- ADRs are immutable. Supersede; do not edit.
```

---

## 6. IDE adapters

| IDE | AGENTS.md native? | ARK OS adapter |
|---|---|---|
| GitHub Copilot | Yes (since Aug 2025) | Optional `.github/copilot-instructions.md` pointer |
| Cursor | Yes | Optional `.cursor/rules/arkos.mdc` pointer |
| Codex CLI | Yes (canonical) | None needed |
| Windsurf | Yes | None needed |
| Claude Code | No (issue #34235 open) | `CLAUDE.md` with `@AGENTS.md` import |
| Aider | No | `.aider.conf.yml: read: AGENTS.md` |

### Claude Code adapter

```markdown
<!-- CLAUDE.md -->
This project is governed by ARK OS. Read AGENTS.md first.

@AGENTS.md

Claude Code notes:
- When you compact, preserve modified files, the active spec ID, and the test commands.
- Use Plan mode before any change touching more than three files.
- Subagent any task involving reading more than ten files.
```

### GitHub Copilot adapter

```markdown
<!-- .github/copilot-instructions.md -->
This project is governed by ARK OS. Primary instructions live in AGENTS.md at the repo root.
Apply those instructions to all generated code, including Copilot Workspace tasks.
```

### Cursor adapter

```markdown
---
description: ARK OS governance pointer
alwaysApply: true
---
This project is governed by ARK OS. The single source of truth is AGENTS.md at the repo root.
Read it before any change.
```

---

## 7. The five governance gates

### Research gate — "Are we solving the right problem?"

| Criterion | Evidence |
|---|---|
| Problem statement exists | Spec has a non-empty `## Problem` section |
| Prior art noted | Spec lists alternatives or links to ADRs |
| Out of scope explicit | Spec has `## Out of scope` with at least 3 items |

### Plan gate — "Is the design coherent and contracted?"

| Criterion | Evidence |
|---|---|
| Spec status is Approved | YAML frontmatter `status: Approved` |
| EARS requirements present | Spec contains at least one WHEN/IF/WHILE/WHERE pattern |
| Trace table maps every REQ to a test | Parser check |
| ADRs filed | Spec references `.arkos/adr/*` |
| Contracts defined at boundaries | `.arkos/contracts/*` exists per external interface |

### Build gate — "Is what we built correct, safe and accessible?"

| Criterion | Evidence |
|---|---|
| Lint clean | `pnpm lint` exit 0 |
| Typecheck clean | `pnpm typecheck` exit 0 |
| Unit + integration green | Test report uploaded |
| Accessibility passes (WCAG 2.2 AA) | axe-core via Playwright |
| Contracts honoured | Spectral / Pact / JSON Schema green |
| No banned patterns | grep check |
| Spec ID in PR | Regex check |
| Conventional Commits | commitlint |

### Ship gate — "Is this safe to release and traceable?"

| Criterion | Evidence |
|---|---|
| CHANGELOG updated | Diff includes `CHANGELOG.md` |
| Version bumped per SemVer | `package.json` or equivalent |
| SBOM produced (CycloneDX 1.6+) | `sbom.cdx.json` artefact |
| SAST clean (no Critical/High) | CodeQL / Semgrep |
| Dependency scan clean | Trivy / OSV-Scanner |
| Threat model exists where required | Spec references `.arkos/threat-models/*` |

### Run gate — "Can we operate this without surprise?"

| Criterion | Evidence |
|---|---|
| SLOs defined | Runbook lists at least one numeric SLO target |
| Runbook exists | Covers health checks, common alerts, rollback |
| Observability wired | Structured logs; one metric per SLO; one alert per failure mode |
| On-call recorded | Runbook lists rotation |

CI cannot fully enforce the Run gate. ARK OS ships a `run-readiness` checklist in `proof-sheet.md` that humans tick before production promotion.

---

## 8. Packaging and distribution

1. **GitHub template repository** (current). Click "Use this template" on this repository to seed a new repo with the full file tree. You can also clone this repository and copy the tree.
2. **CLI scaffolder** (`npx create-arkos@latest`) — **planned, not available**. Five questions, writes the tree. Updatable with `npx arkos upgrade` (also planned, not available).
3. **Modules registry** (`arkos-modules`) — planned. Stack add-ons: `arkos-module-nextjs`, `arkos-module-fastify`, `arkos-module-llm-feature`.
4. **Intelligence module** (`modules/intelligence/`, SPEC-0012). Extracted from btros#472 and btros#473. Azure OpenAI Bicep, Azure AI Search Bicep, and the governed gateway contract. Not a fork of btros. Adopters follow `modules/intelligence/README.md` without cloning btros.

**Versioning:** `.arkos/arkos.yml` records the framework version and modules adopted. When published, `npx arkos upgrade` will perform a three-way merge — user content in `specs/`, `adr/`, `threat-models/`, `runbooks/` is preserved; framework content is overwritable. Until then, adopt or update ARK OS by using this GitHub template or by copying files from this repository.

**Licensing:** Apache 2.0. Maximises adoption. Grants patent peace. Requires downstream attribution to ARK360.

---

## 9. Adoption story

### Solo dev, one afternoon

1. Click **Use this template** on GitHub, or clone this repository and copy the tree. (`npx create-arkos@latest` is planned and is not available yet.)
2. Edit `.arkos/constitution.md` — five minutes.
3. Edit `AGENTS.md` — replace stack and commands — ten minutes.
4. Write the first spec in `.arkos/specs/0001-mvp.md` — twenty minutes.
5. Wire `pnpm verify` — thirty minutes.
6. Push. CI runs every gate.

By Monday morning the agent is bound, the gates are live, and the audit trail starts accumulating.

### The 10x payoff

Agents that previously drifted, hallucinated APIs and produced phantom completions now operate inside a hard-bounded sandbox of spec, ADRs, contracts and gates. The developer's job moves from reviewing every line to reviewing the spec, then auditing the trail. The enterprise's job moves from rejecting AI-assisted work on governance grounds to accepting it, because the evidence is already there.

---

## 10. The procurement proof sheet

```markdown
# ARK OS Proof Sheet — <Project Name>

This codebase was built under **ARK OS v0.2.1** governance.
The complete evidence trail is in the repository.

## Conformance summary

| Domain | Standard | Level | Evidence |
|---|---|---|---|
| Application security | OWASP ASVS v5.0.0 | Level 1 | `.arkos/specs/`, SAST report in CI |
| Cyber hygiene | ASD Essential Eight | ML1 | Attestation in `.arkos/proof-sheet.md` |
| Privacy | Privacy Act 1988 (Cth), APP 1, 5, 11 | Compliant for scope | Privacy notes in each spec |
| Accessibility | WCAG 2.2 AA (ISO/IEC 40500:2025) | Compliant | axe-core CI; manual audit report |
| Supply chain | CycloneDX SBOM | 1.6+ | `sbom.cdx.json` per release |
| Change management | Conventional Commits + ADR | Mandatory | `CHANGELOG.md`, `.arkos/adr/` |

## What ARK OS governance guarantees
1. Every change has an EARS-based, testable, traceable spec.
2. Every architecturally significant decision has an ADR.
3. Every feature touching personal data has a privacy review (APP 1, 5, 11).
4. Every feature crossing a trust boundary has a STRIDE-lite threat model.
5. Every release ships with a CycloneDX SBOM and clean SAST + dependency report.
6. Every PR is gated by CI for lint, typecheck, tests, accessibility, contracts.
7. Every release updates a Keep-a-Changelog CHANGELOG.md.

## Out of scope for this version
IRAP; Essential Eight ML2+; SLSA L3+; ISO 27001 ISMS; SOC 2 Type II.
Available in later versions or as a separately scoped engagement through ARK360.

## Contact
Coordinated disclosure: `SECURITY.md`. Owner: <name>. Last review: <date>.
```

---

## 11. Brand and visual identity

ARK OS is an open-source release from ARK360 and shares the practice's visual identity.

### Wordmark
**ARK OS** - bold ARK, thin OS. The hexagonal mark is the shared family symbol between ARK OS and ARK360.

### Colour palette

| Role | Token | Hex | Use |
|---|---|---|---|
| Primary | Gateway Teal | `#26B9CB` | CTAs, gate-pass signals, active states |
| Accent | Signal Cyan | `#61D1BE` | Success states, icon fills |
| Tertiary | Depth Blue | `#4D8FD4` | Step numbers, section accents |
| Background | Deep Space | `#080D11` | Page background. Always dark. |
| Surface | Card | `#0C1219` | Panel backgrounds |
| Border | Boundary | `#182230` | Dividers, card outlines |
| Foreground | Near White | `#E7EEF0` | Primary text |
| Muted | Mist | `#7D8E94` | Secondary text, captions |

Brand gradient: Signal Cyan to Gateway Teal to Depth Blue. H1 hero text only.

### Typography
- **Geist Sans** — display headings, hero text
- **Inter** — body text, UI labels
- **Geist Mono** — step numbers, code labels, version strings, gate identifiers

### Voice
Confident. Direct. Specialist. The same voice as ARK360.

Write the way people think in high-stakes meetings. Short sentences. Real rhythm. Earned trust over claimed trust — describe the evidence and let the audit trail speak. No marketing filler.

**Words to avoid:** AI-powered, cutting-edge, seamless, leverage, compliance-ready.

### Approved product terms

| Term | Rule |
|---|---|
| ARK OS | The framework. Always two words, all caps. Never "Arkos" in body copy. The wordmark is ARK OS. |
| ARK OS Gate | One of the five governance gates. |
| Build / Ship / Run / Plan / Research gate | Lowercase "gate" after the proper noun. |
| AGENTS.md | Always all-caps with the `.md`. |
| Proof Sheet | The procurement one-pager. Capitalised as a proper noun. |
| ARK360 | The practice. Always one word, all caps. |

---

## 12. Caveats and known limits

- **Vendor velocity.** AGENTS.md is native in Copilot, Cursor, Windsurf, Codex. Claude Code still requires a pointer (May 2026). If Anthropic ships native support, the Claude Code adapter collapses to nothing.
- **AGENTS.md spec is unversioned.** Zero tagged releases on the agents.md GitHub repo as of May 2026. Do not depend on speculative v1.1 semantics.
- **Gates are necessary, not sufficient.** A green build gate means the obvious failure modes were caught. Reviewer judgement still matters.
- **Lightweight enterprise is a posture, not a moat.** ARK OS v1 clears most SMB and mid-market procurement reviews. Federal government, finance and health reviews requiring IRAP or APRA CPS 234 need v2 or a separately scoped ARK360 engagement.
- **The framework cannot enforce culture.** Gates catch the worst. The rest depends on the people in the room.

---

## 13. Roadmap

**Released:** v0.2.1 (9 October 2026). The GitHub template is published under the `ark360-hq` organisation. Apache 2.0.

**v0.3 (plan stage only):** Microsoft agentic module covering Foundry Agent Service, Copilot Studio, Entra Agent ID, approvals, evals and tracing. Nothing is built until a spec (and any required ADR, contract, or threat model) is approved.

**Later (planned, not available):** `npx create-arkos@latest` CLI with `arkos upgrade` and IDE adapter selection. Modules registry and an `arkos proof` command that generates a populated proof sheet from the repo.

**Triggers for v2:** A client requires Essential Eight ML2 or IRAP. Anthropic ships native AGENTS.md in Claude Code. AAIF ratifies AGENTS.md v1.1. The practice wants a commercial conformance attestation service alongside the open framework.

---

*ARK OS is an open-source release from ARK360 — building intelligent enterprise systems to amplify human excellence.*
