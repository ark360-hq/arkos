# ARK OS Quickstart

A developer with no prior ARK OS knowledge can follow this guide and make their first governed commit in 30 to 60 minutes. Wiring CI commands for a full stack typically takes an afternoon. The 15-minute path is "click template, clone, commit the placeholders", useful for kicking the tyres but not for real work.

`npx create-arkos` and `npx arkos upgrade` are **planned and not available**. Use the GitHub template steps below.

---

## Before you start

You need:
- A GitHub account
- Git installed locally
- Your preferred agentic IDE (Claude Code, Cursor, Copilot, Codex, Windsurf, or similar)

You do not need to install anything else before adopting the template. Stack-specific tooling (linters, test runners, SBOM generators) is wired in later.

---

## Step 1 - Create your repository from the template

1. Go to the ARK OS template repository on GitHub.
2. Click **"Use this template"** then **"Create a new repository"**.
3. Name your repository, set the owner and visibility, then click **"Create repository"**.

Clone your new repository:

```
git clone https://github.com/<your-org>/<your-repo>.git
cd <your-repo>
```

Your repository now contains the full ARK OS file tree. Nothing is running yet. The CI workflow exists but uses placeholder commands that print instructions rather than running real tools.

---

## Step 2 - Enable branch protection on `main`

ARK OS enforces governance through CI gates that run on pull requests. None of those gates are mandatory unless `main` is protected. Without branch protection, any contributor (including the agent) can push directly to `main` and bypass every check.

**Required settings on the `main` branch:**

- Require a pull request before merging
- Require status checks to pass before merging: `plan-gate`, `build-gate`
- Do not allow bypassing the above

**Via the GitHub UI:**

1. Go to your repository's **Settings** then **Branches**.
2. Click **Add branch protection rule**.
3. Branch name pattern: `main`.
4. Tick: **Require a pull request before merging**.
5. Tick: **Require status checks to pass before merging**. Search for and add `plan-gate` and `build-gate`. (These status checks appear in the list only after the workflow has run at least once.)
6. Tick: **Do not allow bypassing the above settings**.
7. Click **Create**.

**Via the `gh` CLI (run from your local clone):**

```
gh api -X PUT "repos/:owner/:repo/branches/main/protection" \
  -F required_status_checks.strict=true \
  -F 'required_status_checks.contexts[]=plan-gate' \
  -F 'required_status_checks.contexts[]=build-gate' \
  -F required_pull_request_reviews.required_approving_review_count=0 \
  -F enforce_admins=true \
  -F restrictions=
```

Adjust `required_approving_review_count` upward once you have collaborators.

If branch protection is not enabled, ARK OS becomes opt-in rather than enforced. Adopt this step before your first commit.

---

## Step 3 - Read the constitution

Open `.arkos/constitution.md`. Read it in full before touching any other file.

It contains:
- Five non-negotiable principles (do not edit these without an ADR)
- The conformance level your project claims
- Four process rules (edit these if your team's conventions differ)

---

## Step 4 - Personalise AGENTS.md

Open `AGENTS.md`. This is the single source of truth for every AI coding agent that works in this repo. Replace every placeholder section before your first commit.

**Project section:** Write one paragraph - what this project is, who uses it, why it exists.

**Tech stack section:** Replace each placeholder line with your actual stack:
```
- Runtime: Node.js 22 LTS
- Framework: Next.js 15 (App Router)
- Language: TypeScript strict
- Database: Postgres 16 via Drizzle ORM
- Package manager: pnpm 9
- Test runner: Vitest + Playwright
```

**Commands section:** Replace each `<placeholder>` with your actual commands:
```
- `pnpm install` - install dependencies
- `pnpm dev` - start local dev server
- `pnpm verify` - lint, typecheck, and test
- `pnpm sbom` - generate CycloneDX SBOM
- `pnpm test:a11y` - accessibility checks in isolation
```

**Banned patterns section:** The template includes two JavaScript/TypeScript-specific examples (`console.log`, `: any`). Replace or remove these for your stack, and add any project-specific patterns.

Keep `AGENTS.md` under 150 lines. If your stack requires more detail, use nested `AGENTS.md` files closer to the relevant code.

**Also edit `.arkos/arkos.yml`:** set `project` to your project name and `created` to today's date in YYYY-MM-DD format.

---

## Step 5 - Configure your IDE adapter

Your IDE adapter is already wired. You only need to open and review it.

**Claude Code**

Open `CLAUDE.md`. It imports `AGENTS.md` via `@AGENTS.md` and adds five Claude-specific notes. Add or remove notes as needed. Do not copy content from `AGENTS.md` into `CLAUDE.md` - it is a pointer, not a copy.

**GitHub Copilot**

Open `.github/copilot-instructions.md`. No changes needed for basic use.

**Cursor**

Open `.cursor/rules/arkos.mdc`. No changes needed for basic use.

**Codex, Windsurf, Amp, Gemini CLI, or any other AGENTS.md-native IDE**

`AGENTS.md` is read natively. No adapter file is needed.

---

## Step 6 - Wire your CI commands

Open `.github/workflows/arkos.yml`.

The workflow contains placeholder steps that print `REPLACE:` messages. These placeholders mean the build gate passes immediately (so the template does not fail CI out of the box) but does not run real checks yet. Replace each placeholder before relying on the gates.

Search for lines containing `echo "REPLACE:` and replace them with your stack's actual commands.

### Build gate replacements (required)

| Step | What to put here |
|---|---|
| Install dependencies | Your install command (e.g. `pnpm install --frozen-lockfile`) |
| Lint | Your lint command (e.g. `pnpm lint`, `ruff check .`) |
| Typecheck | Your typecheck command (e.g. `pnpm typecheck`, `mypy .`) |
| Unit tests | Your unit test command (e.g. `pnpm test:unit`, `pytest tests/unit`) |
| Integration tests | Your integration test command |
| Accessibility tests | Your WCAG 2.2 AA test command (e.g. `pnpm test:a11y`, `pa11y-ci`) |
| Contract tests | Your contract test command, or remove the step if not applicable |

### Ship gate replacements (required before first release)

| Step | What to put here |
|---|---|
| Generate SBOM | Your CycloneDX command - see note below |
| SAST | A static analysis tool - see note below |
| Dependency scan | A vulnerability scanner - see note below |

**SBOM generation:** Use the CycloneDX tool for your stack:
- Node.js: `pnpm exec cyclonedx-npm --output-file sbom.cdx.json`
- Python: `cyclonedx-py --output sbom.cdx.json`
- Other stacks: see https://cyclonedx.org/tool-center/

**SAST:** CodeQL is the recommended option for GitHub-hosted repos. It requires two steps - an `init` step (declaring your language) and then the `analyze` step. The comment in the workflow shows the exact syntax. For multi-language or non-GitHub-Advanced-Security repos, Semgrep is a good alternative.

**Dependency scan:** Trivy and OSV-Scanner both work without tokens and cover most stacks. The comments in the workflow show the exact syntax for each.

### What is not in this workflow

Deployment is deliberately absent. Add a separate `.github/workflows/deploy.yml` for your hosting target (Vercel, AWS, GCP, fly.io, Docker, etc.) once the governance gates are green.

---

## Step 7 - Create the product PRD (greenfield)

If this repository is a **new product** (not a framework-only repo like ARK OS itself), create the PRD before your foundation spec.

1. Describe your product idea to the agent in one or two paragraphs.
2. The agent runs the interview in `.arkos/prompts/prd-discovery.md` and asks clarifying questions (users, MVP, non-goals, stack, privacy, trust boundaries).
3. The agent writes `.arkos/prd/product.md` from `.arkos/prd/_template.md`.
4. Review the PRD. Tell the agent when to set `status: Approved`.

Do not scaffold application code until the PRD is Approved.

---

## Step 8 - Write your foundation spec (SPEC-0001)

Copy the spec template:

```
cp .arkos/specs/_template.md .arkos/specs/0001-<feature-slug>.md
```

Open the new file and complete every section:

1. Set `id: SPEC-0001` and `slug: <foundation-slug>` in the frontmatter (foundation spec: architecture and scaffold from the PRD).
2. Set `status: Draft` to start, then change to `Approved` when ready.
3. Write `## Problem` - one paragraph, who benefits, why this exists.
4. Write `## Out of scope` - at least three explicit exclusions.
5. Write requirements using EARS patterns. Include at least one `WHEN`, `IF`, `WHILE`, or `WHERE` pattern.
6. Fill the acceptance criteria table - one row per requirement, linked to a test file and type.
7. If the feature touches personal data, complete the Privacy notes section (APP 1, 5, 11).
8. If the feature has a UI, complete the Accessibility notes section (WCAG 2.2 AA).
9. File an ADR in `.arkos/adr/` for any significant architectural decision the spec implies.

Set `status: Approved` before raising a PR.

---

## Step 9 - Make your first commit on a feature branch

Create a feature branch, stage your personalised files, and commit using Conventional Commits format. The commit message must reference the spec.

```
git checkout -b feat/initialise
git add AGENTS.md .arkos/constitution.md .arkos/arkos.yml .arkos/specs/0001-<feature-slug>.md
git commit -m "feat: initialise project with ARK OS governance (SPEC-0001)"
```

The plan gate checks that `SPEC-[0-9]+` appears in the commit message, PR title, or PR body. If it is missing, the gate fails with a clear error.

Also update `CHANGELOG.md` with an entry for this change. The build gate checks that `CHANGELOG.md` is modified in the PR.

---

## Step 10 - Push the branch and open a PR

```
git push -u origin feat/initialise
```

Open a pull request from `feat/initialise` to `main` in GitHub. The PR template will prompt you through the checklist (spec reference, tests, CHANGELOG, banned patterns, accessibility, ADR).

Go to your repository's **Actions** tab. You will see the `arkos` workflow running on the PR.

### Which jobs run when

| Job | Runs on PRs | Runs on push to main |
|---|---|---|
| `plan-gate` | Yes | Yes |
| `build-gate` | Yes | Yes |
| `ship-gate` | No | Yes |
| `run-readiness` | No | Yes |

On your first push, `plan-gate` and `build-gate` run immediately. `ship-gate` and `run-readiness` run only on pushes to `main`.

### What to expect on first push

With placeholder build steps in place, `plan-gate` and `build-gate` will pass as long as your commit message references a spec. The placeholder steps print `REPLACE:` messages and exit 0.

Once you replace the placeholder steps with real commands, each gate will start enforcing real checks.

### If ship-gate fails

The ship gate runs on pushes to `main`. Common failure reasons after you have replaced the placeholders:

- **SBOM step fails:** the CycloneDX tool is not installed or not in `PATH`. Add an install step for it before the generate step.
- **SAST step fails:** CodeQL requires the `init` step with a `languages:` declaration before `analyze`. See the comment in the workflow for the exact syntax.
- **CHANGELOG check fails:** `CHANGELOG.md` must include a new entry in the commit being pushed. Add or update an entry before committing.

---

## Step 11 - Before production

Before promoting any release to production, complete the run readiness checklist in `.arkos/proof-sheet.md`:

- SLOs defined in the runbook with numeric targets
- Runbook covers health checks, common alerts, and rollback procedure
- Structured logging wired; one metric per SLO; one alert per failure mode
- On-call rotation recorded

The `run-readiness` CI job prints this checklist as a reminder but does not block the merge. It requires human sign-off.

---

## Step 12 - For procurement

Fill in `.arkos/proof-sheet.md` with your project name, owner name, and review date. Share the completed file with the procurement reviewer alongside a link to the repository. The evidence trail (specs, ADRs, SBOM artefacts, SAST reports) is all in the repo.

---

## Reference

| File | Purpose |
|---|---|
| `AGENTS.md` | Agent instructions - edit this first |
| `.arkos/constitution.md` | Project non-negotiables |
| `.arkos/prd/_template.md` | PRD template - agent fills `product.md` after discovery interview |
| `.arkos/prompts/prd-discovery.md` | Required questions when starting a new product |
| `.arkos/specs/_template.md` | Spec template - copy for each feature |
| `.arkos/adr/_template.md` | ADR template - copy for each decision |
| `.arkos/threat-models/_template.md` | Threat model template - copy for trust-boundary features |
| `.arkos/runbooks/_template.md` | Runbook template - copy for each service |
| `.arkos/proof-sheet.md` | Procurement evidence - fill before first procurement review |
| `.github/workflows/arkos.yml` | CI workflow - replace placeholder steps with real commands |
| `.arkos/scripts/check-banned-patterns.sh` | Banned pattern script - replace stack-specific examples |
| `.arkos/scripts/check-product-ai.sh` | Product AI policy scan - non-Azure providers and missing Intelligence guards |
| `.arkos/product-ai-allowlist.yml` | Marketing-site path prefixes that are not product AI |
| `docs/framework.md` | Full ARK OS v0.2.1 framework document |
| `docs/standards.md` | Standards reference with versions and links |
| `CONTRIBUTING.md` | How to contribute to ARK OS itself |
