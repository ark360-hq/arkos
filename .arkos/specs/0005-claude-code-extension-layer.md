---
id: SPEC-0005
slug: claude-code-extension-layer
title: Claude Code extension layer (ARK OS v0.2)
status: Approved
owner: "@ark360-hq"
created: 2026-05-22
touches-personal-data: false
trust-boundaries-crossed: false
linked-adrs: [ADR-0001, ADR-0004]
linked-threat-model: null
---

# SPEC-0005: Claude Code extension layer (ARK OS v0.2)

## Problem

ARK OS v0.1 provides a thin `CLAUDE.md` adapter but no scaffold for Claude Code's native project structure (`.claude/settings.json`, rules, slash commands, hooks, MCP). Teams adopting Claude Code must invent their own layout, which risks duplicating governance out of `AGENTS.md` or committing personal settings as shared defaults.

Claude Code users need optional, well-documented ergonomics that align with ARK OS gates and workflows without making non-Claude IDEs second-class.

## Out of scope

1. `npx create-arkos` or `npx arkos upgrade` CLI tooling.
2. Stack-specific modules or MCP servers that work out of the box.
3. `.claude/agents/`, `.claude/skills/`, or `output-styles/`.
4. CI enforcement of `.claude/` file contents or MCP connectivity.
5. Cursor, Windsurf, or Copilot parity directories for Claude-only features.

## Requirements

### Ubiquitous

> REQ-001: The repository shall retain `AGENTS.md` as the single source of truth for universal governance. No requirement in `AGENTS.md` shall be duplicated in full into `CLAUDE.md` or `.claude/rules/*`.

> REQ-002: The repository shall add an optional `.claude/` directory containing only stub or example files suitable for team sharing via git.

> REQ-003: Each file in `.claude/rules/` shall contain no more than ten lines of original content and shall point readers to `AGENTS.md` rather than restate governance.

> REQ-004: The repository shall ship `.mcp.json.example` at the repository root as example-only documentation. It shall not be required for ARK OS conformance and shall contain no secrets.

> REQ-005: Personal Claude overrides shall be documented via `.claude/settings.local.json.example` and excluded from git via `.gitignore`.

### Event-driven

> REQ-006: WHEN a Claude Code user runs `/governance-check`, the agent shall walk the pre-flight checks defined in `AGENTS.md` for the current change context.

> REQ-007: WHEN a Claude Code user runs `/scaffold-spec`, the agent shall propose copying `.arkos/specs/_template.md` to the next available `SPEC-NNNN` filename and remind the user of EARS and approval steps per `AGENTS.md`.

> REQ-008: IF a team enables the sample `PreToolUse` hook in `.claude/settings.json`, THEN edits on branch `main` via Edit or Write tools shall be blocked with a clear message directing the user to a feature branch.

### Optional

> REQ-009: WHERE hooks are not enabled in `.claude/settings.json`, Claude Code behaviour shall be unchanged from v0.1 aside from new documentation and slash commands.

> REQ-010: The framework version in `.arkos/arkos.yml` shall be updated to `0.2` and `claude-extension: true` shall be declared under `ide-adapters`.

## Acceptance criteria

| REQ | Test | Type |
|-----|------|------|
| REQ-001 | `CLAUDE.md` contains `@AGENTS.md`; `.claude/rules/arkos.md` has fewer than 11 lines and references `AGENTS.md` | manual |
| REQ-002 | `.claude/README.md`, `settings.json`, `commands/`, `hooks/` exist in template | manual |
| REQ-003 | Line count check on `.claude/rules/arkos.md` | manual |
| REQ-004 | `.mcp.json.example` has header comment; `.mcp.json` in `.gitignore` | manual |
| REQ-005 | `.gitignore` lists `settings.local.json`; example file present | manual |
| REQ-006 | `.claude/commands/governance-check.md` references `AGENTS.md` pre-flight section | manual |
| REQ-007 | `.claude/commands/scaffold-spec.md` references `_template.md` and approval workflow | manual |
| REQ-008 | `block-edit-on-main.sh` exits non-zero on `main`; documented in hooks README | manual |
| REQ-009 | Default `settings.json` has empty `hooks` object | manual |
| REQ-010 | `arkos.yml` shows `arkos-version: "0.2"` and `claude-extension: true` | manual |

## Privacy notes

Not applicable. No personal data collection.

## Accessibility notes

Not applicable. No UI changes.
