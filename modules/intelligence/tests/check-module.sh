#!/usr/bin/env bash
# Verifies SPEC-0012 Intelligence module extract markers (REQ-009 to REQ-017).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
FAILED=false

fail() {
  echo "ERROR: $1"
  FAILED=true
}

ok() {
  echo "OK: $1"
}

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

forbid_contains() {
  local file="$1"
  local pattern="$2"
  local label="$3"
  if [[ -f "$ROOT/$file" ]] && grep -qE -- "$pattern" "$ROOT/$file"; then
    fail "$label ($file)"
  else
    ok "$label"
  fi
}

require_file "modules/intelligence/README.md"
require_file "modules/intelligence/SOURCE.md"
require_file "modules/intelligence/bicep/openai.bicep"
require_file "modules/intelligence/bicep/search.bicep"
require_file "modules/intelligence/examples/worked-example/README.md"
require_file "modules/intelligence/examples/worked-example/main.bicep"
require_file "modules/intelligence/examples/worked-example/gateway-calls.md"
require_file ".arkos/contracts/intelligence-gateway.md"
require_file ".arkos/contracts/intelligence-gateway.openapi.yaml"
require_file ".arkos/specs/0012-intelligence-module.md"

require_contains "modules/intelligence/README.md" "extracted" "README states extract-from-btros"
require_contains "modules/intelligence/README.md" "not a fork" "README forbids a diverging fork"
require_contains "modules/intelligence/SOURCE.md" "btros#472" "SOURCE.md cites btros#472"
require_contains "modules/intelligence/SOURCE.md" "btros#473" "SOURCE.md cites btros#473"

require_contains "modules/intelligence/bicep/openai.bicep" "Microsoft.CognitiveServices" "openai.bicep declares Cognitive Services"
require_contains "modules/intelligence/bicep/search.bicep" "Microsoft.Search" "search.bicep declares Search"

require_contains ".arkos/contracts/intelligence-gateway.md" "audit" "human contract requires audit"
require_contains ".arkos/contracts/intelligence-gateway.openapi.yaml" "AuditRecord" "OpenAPI defines AuditRecord"
require_contains ".arkos/contracts/intelligence-gateway.openapi.yaml" "required:" "OpenAPI has required fields"
require_contains ".arkos/contracts/intelligence-gateway.openapi.yaml" "[[:space:]]audit:" "OpenAPI requires audit on responses"

require_contains "modules/intelligence/examples/worked-example/gateway-calls.md" "POST /v1/complete" "example uses gateway complete"
require_contains "modules/intelligence/examples/worked-example/gateway-calls.md" "Do not call a raw Azure OpenAI SDK" "example forbids raw OpenAI SDK"
require_contains "modules/intelligence/examples/worked-example/gateway-calls.md" "raw Azure AI Search SDK" "example forbids raw Search SDK"

require_contains "CHANGELOG.md" "SPEC-0012" "CHANGELOG references SPEC-0012"

if ls "$ROOT/.arkos/specs"/0013-*.md >/dev/null 2>&1; then
  fail "SPEC-0013 file is present (issue #19 is out of scope)"
else
  ok "no SPEC-0013 file"
fi

if grep -R --include='*.yml' -nE 'non-Azure product AI|missing gateway audit' \
  "$ROOT/.arkos/gates" >/dev/null 2>&1; then
  fail "issue #19 gate checks appear in .arkos/gates"
else
  ok "no issue #19 gate checks in gates"
fi

if grep -R --include='*.bicep' -nE 'Microsoft.Stripe|Microsoft.AzureActiveDirectory|Microsoft.Authorization/roleAssignments' \
  "$ROOT/modules/intelligence" >/dev/null 2>&1; then
  fail "Bicep declares Entra, Stripe, or role assignment resources"
else
  ok "Bicep creates no Entra, Stripe, or role assignment resources"
fi

if [[ "$FAILED" == "true" ]]; then
  echo ""
  echo "Intelligence module check failed."
  exit 1
fi

echo ""
echo "OK: Intelligence module checks passed."
exit 0
