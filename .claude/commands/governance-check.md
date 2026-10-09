---
description: Review ARK OS governance readiness for the current change
---

Run a governance readiness review for this repository. Do not edit files unless the user asks you to fix gaps.

1. Read `AGENTS.md` in full, especially the STOP block and **Pre-flight checks** table.
2. Report pass/fail for each row that applies to the user's stated or inferred change:
   - Feature branch (not `main`)
   - Approved spec in `.arkos/specs/`
   - PRD status if greenfield (`.arkos/prd/product.md`)
   - Privacy notes if personal data
   - Threat model if trust boundary crossed
   - Contract if new external interface
   - ADR if new dependency or architectural decision
   - `CHANGELOG.md` planned if required
3. If any check fails, stop and tell the user what to do next, citing the relevant `AGENTS.md` section.
4. Remind the user that CI enforces spec references and CHANGELOG on PRs; hooks in `.claude/` are optional backstops only.

Keep the summary concise. Reference spec IDs when known.
