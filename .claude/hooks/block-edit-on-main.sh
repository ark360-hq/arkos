#!/usr/bin/env bash
# ARK OS sample hook: block Edit/Write on main branch.
# Enable in .claude/settings.json (see hooks/README.md). Requires jq.
set -euo pipefail

branch="$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "")"

if [[ "$branch" != "main" && "$branch" != "master" ]]; then
  exit 0
fi

if ! command -v jq >/dev/null 2>&1; then
  echo "block-edit-on-main: jq required" >&2
  exit 1
fi

jq -n '{
  hookSpecificOutput: {
    hookEventName: "PreToolUse",
    permissionDecision: "deny",
    permissionDecisionReason: "ARK OS: create a feature branch before editing. AGENTS.md requires feat/SPEC-NNNN-slug, not main."
  }
}'
