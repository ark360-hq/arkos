#!/usr/bin/env bash
# Fixture tests for check-product-ai.sh (SPEC-0013).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../../../.." && pwd)"
SCRIPT="$ROOT/.arkos/scripts/check-product-ai.sh"
FIXTURES="$(cd "$(dirname "$0")" && pwd)/fixtures"
SCANNER_TMP="$(mktemp)"
FAILED=false

chmod +x "$SCRIPT"

ok() { echo "OK: $1"; }
fail() { echo "ERROR: $1"; FAILED=true; }

require_file() {
  if [[ -f "$ROOT/$1" ]]; then
    ok "file $1"
  else
    fail "missing file $1"
  fi
}

require_contains() {
  local file="$1"
  local pattern="$2"
  local label="$3"
  if [[ -f "$ROOT/$file" ]] && grep -qE -- "$pattern" "$ROOT/$file"; then
    ok "$label"
  else
    fail "$label ($file)"
  fi
}

run_scan() {
  local tree="$1"
  local allow="${2:-}"
  local env_allow=()
  if [[ -n "$allow" ]]; then
    env_allow=(ALLOWLIST_FILE="$allow")
  fi
  set +e
  env SCAN_ROOT="$tree" "${env_allow[@]}" bash "$SCRIPT" >"$SCANNER_TMP" 2>&1
  local rc=$?
  set -e
  return "$rc"
}

expect_pass() {
  local label="$1"
  local tree="$2"
  local allow="${3:-}"
  if run_scan "$tree" "$allow"; then
    ok "$label"
  else
    fail "$label (expected pass)"
    cat "$SCANNER_TMP"
  fi
}

expect_fail() {
  local label="$1"
  local tree="$2"
  local needle="$3"
  local allow="${4:-}"
  if run_scan "$tree" "$allow"; then
    fail "$label (expected fail)"
    cat "$SCANNER_TMP"
    return
  fi
  if grep -qE -- "$needle" "$SCANNER_TMP"; then
    ok "$label"
  else
    fail "$label (missing expected error: $needle)"
    cat "$SCANNER_TMP"
  fi
}

# REQ-001 / REQ-002 / REQ-003 / REQ-004
require_file ".arkos/specs/0013-product-ai-gates.md"
require_file ".arkos/adr/0007-product-ai-static-gate.md"
require_file ".arkos/scripts/check-product-ai.sh"
require_file ".arkos/product-ai-allowlist.yml"
require_contains ".arkos/specs/0013-product-ai-gates.md" "status: Approved" "SPEC-0013 is Approved"
require_contains ".arkos/specs/0013-product-ai-gates.md" "AzureOpenAIGuard" "spec names AzureOpenAIGuard"
require_contains ".arkos/specs/0013-product-ai-gates.md" "AzureAiSearchGuard" "spec names AzureAiSearchGuard"
require_contains ".github/workflows/arkos.yml" "check-product-ai.sh" "workflow runs product AI scan"
require_contains ".arkos/gates/build.yml" "CRT-018" "build gate lists CRT-018"
require_contains ".arkos/product-ai-allowlist.yml" "marketing" "allow-list documents the marketing site"
require_contains "CHANGELOG.md" "SPEC-0013" "CHANGELOG references SPEC-0013"

# REQ-012: scanner must not demand the retired gateway/Bicep/audit design.
if grep -qE '/v1/complete|accept/edit/reject audit is required|template Bicep is required' "$SCRIPT"; then
  fail "scanner requires retired HTTP gateway, Bicep, or adapter audit"
else
  ok "scanner does not require HTTP /v1 gateway, Bicep, or adapter audit"
fi

# This repository (template) has no product AI.
expect_pass "template tree has no product AI" "$ROOT"

# REQ-008 / REQ-011
expect_pass "btros-like Intelligence extract" "$FIXTURES/btros-like"

# REQ-009
expect_pass "access-os-like Entra audit tree" "$FIXTURES/access-os-like"

# REQ-005
expect_fail "non-Azure OpenAI fails" "$FIXTURES/non-azure-openai" "non-Azure product AI"

# REQ-006
expect_fail "Azure OpenAI without guard fails" "$FIXTURES/azure-openai-without-guard" "AzureOpenAIGuard.EnsureCanSend"

# REQ-007
expect_fail "Azure AI Search without guard fails" "$FIXTURES/azure-search-without-guard" "AzureAiSearchGuard.EnsureCanSend"

# REQ-010
expect_pass "allow-listed marketing site" "$FIXTURES/marketing-site" "$FIXTURES/marketing-site/.arkos/product-ai-allowlist.yml"
expect_fail "marketing widget without allow-list fails" "$FIXTURES/marketing-site" "non-Azure product AI" "$ROOT/.arkos/product-ai-allowlist.yml"

rm -f "$SCANNER_TMP"

if [[ "$FAILED" == "true" ]]; then
  echo ""
  echo "Product AI policy tests failed."
  exit 1
fi

echo ""
echo "OK: Product AI policy tests passed."
exit 0
