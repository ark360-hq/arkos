# Claude Code extension layer (optional)

This directory is **optional**. ARK OS governance lives in `AGENTS.md` at the repository root. Every IDE reads that file; Claude Code users may also use this folder for team-shared settings, slash commands, and hooks.

## When to use what

| Layer | File | Purpose |
|-------|------|---------|
| Universal | `AGENTS.md` | Constitution, pre-flight, specs, gates, banned patterns |
| Claude adapter | `CLAUDE.md` | Imports `AGENTS.md`; session notes (Plan mode, compaction) |
| Claude extension | `.claude/*` | Settings, rules, commands, hooks (this directory) |
| MCP template | `.mcp.json.example` | Copy to `.mcp.json` when your team configures MCP |

Claude Code works without `.claude/`. Copilot, Cursor, and other IDEs do not need this directory.

## Settings

- **Team defaults:** `settings.json` (committed). Hooks are **disabled** by default.
- **Personal overrides:** copy `settings.local.json.example` to `settings.local.json` (gitignored).

See [Claude Code settings](https://code.claude.com/docs/en/settings) for precedence order.

## Slash commands

| Command | Purpose |
|---------|---------|
| `/governance-check` | Walk `AGENTS.md` pre-flight checks for the current change |
| `/scaffold-spec` | Start a new spec from `.arkos/specs/_template.md` |

## Sample hook (opt-in)

To block file edits on `main`, see `.claude/hooks/README.md` and enable the `PreToolUse` block in `settings.json`.

## More detail

- [docs/v0.2-enhancement-proposal.md](../docs/v0.2-enhancement-proposal.md)
- [ADR-0004](../.arkos/adr/0004-claude-code-extension-layer.md)
