# ADR-0004: Claude Code extension layer (optional `.claude/`)

**Status:** Accepted

**Date:** 2026-05-22

**Supersedes:** (none)

**Superseded by:** (none)

---

## Context

Claude Code reads project configuration from `CLAUDE.md`, `.claude/settings.json`, `.claude/rules/`, `.claude/commands/`, hooks, and `.mcp.json`. ARK OS v0.1 ships only `CLAUDE.md` as a thin `@AGENTS.md` pointer (ADR-0001). Teams want Claude-native ergonomics without moving governance out of `AGENTS.md` or disadvantaging Copilot, Cursor, and AGENTS.md-native IDEs.

## Decision

ARK OS v0.2 adds an **optional** `.claude/` extension layer and `.mcp.json.example` for Claude Code only. Universal governance remains in `AGENTS.md`. `CLAUDE.md` remains a thin adapter. Files under `.claude/rules/` are pointers, not copies of `AGENTS.md`. Hooks and MCP configuration ship disabled or example-only by default. Framework version bumps to 0.2 with `claude-extension: true` in `.arkos/arkos.yml`.

## Consequences

Plus: Claude Code users get repeatable slash commands (`/governance-check`, `/scaffold-spec`) and a documented settings layering model.

Plus: Non-Claude users are unaffected; no parallel `.cursor/` governance tree is required.

Plus: ADR-0001 remains valid; extension layer is explicitly subordinate to `AGENTS.md`.

Minus: Template file count increases by roughly twelve stub files.

Minus: Teams that enable the sample hook must use feature branches consistently or fork the hook.

Minus: If Anthropic ships native `AGENTS.md` support, `CLAUDE.md` and parts of `.claude/rules/` may become redundant (acceptable; documented in v0.3 deferrals).
