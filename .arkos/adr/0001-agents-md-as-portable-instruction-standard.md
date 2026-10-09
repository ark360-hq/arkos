# ADR-0001: AGENTS.md as the Portable Instruction Standard

**Status:** Accepted

**Date:** 2026-05-13

**Supersedes:** (none)

**Superseded by:** (none)

---

## Context

Multiple agentic IDEs ship their own instruction file conventions. CLAUDE.md, `.cursorrules`, `.github/copilot-instructions.md` and others address the same need through incompatible formats. A single framework cannot maintain separate canonical files per IDE without introducing drift and duplication. AGENTS.md, stewarded by the Linux Foundation's Agentic AI Foundation (AAIF) since December 2025, has achieved majority adoption across the major agentic IDEs.

## Decision

AGENTS.md is the single source of truth for agent instructions in any ARK OS-governed repository. IDE-specific files (CLAUDE.md, `.cursor/rules/arkos.mdc`, `.github/copilot-instructions.md`) are thin pointers to AGENTS.md only. No governance content is duplicated into adapter files.

## Consequences

Plus: One file to maintain. AGENTS.md works natively with GitHub Copilot, Cursor, Codex, Windsurf, and Amp without an adapter.

Plus: Future IDEs that adopt the agents.md specification work without modification to the framework.

Minus: Claude Code requires a CLAUDE.md pointer file (open issue #34235 as of May 2026). If Anthropic ships native AGENTS.md support, the CLAUDE.md adapter collapses to nothing and can be removed.
