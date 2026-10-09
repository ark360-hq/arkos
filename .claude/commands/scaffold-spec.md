---
description: Scaffold a new ARK OS spec from the template
---

Help the user start a new governed feature per `AGENTS.md` **Starting a new feature**.

1. List existing specs in `.arkos/specs/` and propose the next four-digit ID (`SPEC-NNNN`).
2. Ask for a short slug (kebab-case) if not provided.
3. Copy `.arkos/specs/_template.md` to `.arkos/specs/NNNN-<slug>.md` using a shell command (`cp`). Do not implement code yet.
4. Guide the user to fill: Problem, Out of scope (at least three items), EARS requirements, acceptance criteria table.
5. Ask about personal data and trust boundaries; note if privacy or threat model sections are needed.
6. Remind them to file ADRs and contracts before implementation when applicable.
7. Set `status: Draft` until the user explicitly approves; then create a GitHub issue and branch `feat/SPEC-NNNN-slug` per `AGENTS.md`.

Do not skip the spec or branch steps. Do not copy governance text from `AGENTS.md` into the new spec file beyond the template structure.
