#!/usr/bin/env bash
# Verifies SPEC-0012 Intelligence module extract markers.
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

forbid_file() {
  if [[ -e "$ROOT/$1" ]]; then
    fail "forbidden path still present: $1"
  else
    ok "absent $1"
  fi
}

require_file "modules/intelligence/README.md"
require_file "modules/intelligence/SOURCE.md"
require_file "modules/intelligence/src/Arkos.Intelligence/ILanguageModelClient.cs"
require_file "modules/intelligence/src/Arkos.Intelligence/IRetrievalClient.cs"
require_file "modules/intelligence/src/Arkos.Intelligence/AzureOpenAIGuard.cs"
require_file "modules/intelligence/src/Arkos.Intelligence/AzureAiSearchGuard.cs"
require_file "modules/intelligence/src/Arkos.Intelligence/RagGroundingService.cs"
require_file "modules/intelligence/src/Arkos.Intelligence/RagPromptComposer.cs"
require_file "modules/intelligence/src/Arkos.Intelligence.Infrastructure/AzureOpenAILanguageModelClient.cs"
require_file "modules/intelligence/src/Arkos.Intelligence.Infrastructure/AzureAiSearchRetrievalClient.cs"
require_file "modules/intelligence/src/Arkos.Intelligence.Infrastructure/IntelligenceServiceCollectionExtensions.cs"
require_file "modules/intelligence/examples/worked-example/README.md"
require_file ".arkos/contracts/intelligence-gateway.md"
require_file ".arkos/contracts/azure-openai-chat.md"
require_file ".arkos/contracts/azure-ai-search.md"
require_file ".arkos/adr/0006-intelligence-in-process-extract.md"
require_file ".arkos/specs/0012-intelligence-module.md"

require_contains "modules/intelligence/README.md" "extracted" "README states extract-from-btros"
require_contains "modules/intelligence/README.md" "not a fork" "README forbids a diverging fork"
require_contains "modules/intelligence/SOURCE.md" "a04a31a" "SOURCE.md cites btros commit a04a31a"
require_contains "modules/intelligence/SOURCE.md" "btros#472" "SOURCE.md cites btros#472"
require_contains "modules/intelligence/SOURCE.md" "btros#473" "SOURCE.md cites btros#473"
require_contains "modules/intelligence/examples/worked-example/README.md" "AddArkosIntelligence" "worked example shows registration"
require_contains "CHANGELOG.md" "SPEC-0012" "CHANGELOG references SPEC-0012"

forbid_file "modules/intelligence/bicep/openai.bicep"
forbid_file "modules/intelligence/bicep/search.bicep"
forbid_file ".arkos/contracts/intelligence-gateway.openapi.yaml"
forbid_file ".arkos/reference/btros-intelligence"

if find "$ROOT/modules/intelligence" -name '*.bicep' | grep -q .; then
  fail "Bicep files exist under modules/intelligence"
else
  ok "no Bicep under modules/intelligence"
fi

if grep -R --include='*.md' --include='*.cs' --include='*.yaml' --include='*.yml' -nE -- '/v1/complete|/v1/retrieve' \
  "$ROOT/modules/intelligence" \
  "$ROOT/.arkos/contracts/intelligence-gateway.md" \
  >/dev/null 2>&1; then
  fail "product HTTP /v1 gateway paths are still present"
else
  ok "no product HTTP /v1 gateway paths"
fi

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

if [[ "$FAILED" == "true" ]]; then
  echo ""
  echo "Intelligence module check failed."
  exit 1
fi

echo ""
echo "OK: Intelligence module checks passed."
exit 0
