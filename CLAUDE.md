<!-- CLAUDE.md -->
This project is governed by ARK OS. AGENTS.md is authoritative. You are the active enforcer of the governance, not a passive participant. Run the pre-flight checks in AGENTS.md before every code-changing action and stop to prompt the user when a check fails.

@AGENTS.md

## Claude Code notes

- Use Plan mode for any feature that has no spec yet. Present the spec, ADRs, and threat model in the plan before writing code.
- When you compact, preserve modified files, the active spec ID, and the verify command.
- Delegate to a subagent any task that requires reading more than ten files.
- Reference the spec ID in every commit message using the format `SPEC-[0-9]+`.
- Do not copy content from AGENTS.md into this file. This file is a pointer and Claude-specific notes only.
