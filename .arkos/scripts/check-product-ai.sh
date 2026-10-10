#!/usr/bin/env bash
# Product AI policy scan (SPEC-0013 / ADR-0007).
#
# Fails when application or manifest source:
#   1. Calls a non-Azure product AI provider, or
#   2. Sends to Azure OpenAI or Azure AI Search without the matching
#      in-process guard (AzureOpenAIGuard.EnsureCanSend /
#      AzureAiSearchGuard.EnsureCanSend) in the same file.
#
# This is not an HTTP /v1 gateway audit and it does not require Bicep.
# Intelligence adapters do not emit an accept/edit/reject audit.
#
# Environment:
#   SCAN_ROOT       Tree to scan (default: current working directory).
#   ALLOWLIST_FILE  Allow-list YAML (default: $SCAN_ROOT/.arkos/product-ai-allowlist.yml
#                   then the script repository copy).

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
SCAN_ROOT="$(cd "${SCAN_ROOT:-.}" && pwd)"

if [[ -n "${ALLOWLIST_FILE:-}" ]]; then
  ALLOWLIST_PATH="$ALLOWLIST_FILE"
elif [[ -f "$SCAN_ROOT/.arkos/product-ai-allowlist.yml" ]]; then
  ALLOWLIST_PATH="$SCAN_ROOT/.arkos/product-ai-allowlist.yml"
else
  ALLOWLIST_PATH="$REPO_DIR/.arkos/product-ai-allowlist.yml"
fi

FAILED=false

rel_path() {
  local file="$1"
  local prefix="${SCAN_ROOT}/"
  local rel="${file#"$prefix"}"
  printf '%s\n' "$rel"
}

is_skipped_path() {
  local rel="$1"
  case "$rel" in
    .git/*|*/.git/*) return 0 ;;
    node_modules/*|*/node_modules/*) return 0 ;;
    bin/*|*/bin/*|obj/*|*/obj/*) return 0 ;;
    vendor/*|*/vendor/*|dist/*|*/dist/*) return 0 ;;
    .arkos/scripts/check-product-ai.sh) return 0 ;;
    .arkos/scripts/tests/check-product-ai/*) return 0 ;;
    .arkos/specs/*|.arkos/adr/*|.arkos/contracts/*|.arkos/threat-models/*|.arkos/prompts/*|.arkos/gates/*) return 0 ;;
  esac
  return 1
}

load_allowlist() {
  ALLOW_PREFIXES=()
  if [[ ! -f "$ALLOWLIST_PATH" ]]; then
    return 0
  fi
  local line path
  while IFS= read -r line || [[ -n "$line" ]]; do
    [[ "$line" =~ ^[[:space:]]*# ]] && continue
    if [[ "$line" =~ ^[[:space:]]*-[[:space:]]*[\"\']?([^\"\'#[:space:]]+)[\"\']? ]]; then
      path="${BASH_REMATCH[1]}"
      path="${path%/}"
      [[ -n "$path" ]] && ALLOW_PREFIXES+=("$path")
    fi
  done < "$ALLOWLIST_PATH"
}

is_allowlisted() {
  local rel="$1"
  local prefix
  for prefix in "${ALLOW_PREFIXES[@]+"${ALLOW_PREFIXES[@]}"}"; do
    if [[ "$rel" == "$prefix" || "$rel" == "$prefix"/* ]]; then
      return 0
    fi
  done
  return 1
}

is_test_file() {
  local rel="$1"
  local base
  base="$(basename "$rel")"
  case "$rel" in
    */tests/*|*/test/*|*/__tests__/*) return 0 ;;
  esac
  case "$base" in
    *Test.cs|*Tests.cs|*Test.fs|*Tests.fs|*.test.js|*.test.ts|*.test.tsx|*.spec.js|*.spec.ts|*.spec.tsx) return 0 ;;
  esac
  return 1
}

is_guard_definition() {
  local base
  base="$(basename "$1")"
  case "$base" in
    AzureOpenAIGuard.cs|AzureAiSearchGuard.cs) return 0 ;;
  esac
  return 1
}

has_openai_guard() {
  grep -qE 'AzureOpenAIGuard[[:space:]]*\.[[:space:]]*EnsureCanSend' "$1"
}

has_search_guard() {
  grep -qE 'AzureAiSearchGuard[[:space:]]*\.[[:space:]]*EnsureCanSend' "$1"
}

is_guard_backed_test() {
  local file="$1"
  local rel="$2"
  is_test_file "$rel" || return 1
  if has_openai_guard "$file" || has_search_guard "$file"; then
    return 0
  fi
  grep -qE 'AzureOpenAIGuard|AzureAiSearchGuard' "$file"
}

fail_file() {
  local file="$1"
  local message="$2"
  echo "ERROR: $message"
  echo "  file: $(rel_path "$file")"
  FAILED=true
}

# Non-Azure product AI hosts and public SDK entry points.
NON_AZURE_CODE_REGEX='api\.openai\.com|openai\.com/v1|api\.anthropic\.com|generativelanguage\.googleapis\.com|api\.cohere\.(ai|com)|api\.mistral\.ai|api\.groq\.com|bedrock-runtime|api\.together\.xyz|api\.perplexity\.ai|openrouter\.ai|from openai import|from anthropic import|GoogleGenerativeAI|AmazonBedrockRuntime|Anthropic\(|new OpenAI\('

NON_AZURE_MANIFEST_REGEX='PackageReference Include="OpenAI"|PackageReference Include="Anthropic|PackageReference Include="Azure\.AI\.OpenAI"|PackageReference Include="Azure\.Search\.Documents"|"@anthropic-ai/|"@google/generative-ai"|google-generativeai|"@azure/openai"|"@azure/search-documents"'

# Azure send sites that must sit behind the matching guard.
AZURE_OPENAI_SEND_REGEX='openai/deployments/|/chat/completions|AzureOpenAIClient|new OpenAIClient\('
AZURE_SEARCH_SEND_REGEX='/docs/search|SearchClient\(|Azure\.Search\.Documents'

scan_file() {
  local file="$1"
  local rel
  rel="$(rel_path "$file")"

  is_skipped_path "$rel" && return 0
  is_allowlisted "$rel" && return 0

  local ext="${rel##*.}"
  case "$ext" in
    csproj|fsproj|vbproj|json|txt|toml)
      if grep -nE "$NON_AZURE_MANIFEST_REGEX" "$file" >/dev/null 2>&1; then
        if is_guard_backed_test "$file" "$rel"; then
          return 0
        fi
        fail_file "$file" "non-Azure or raw Azure SDK product AI dependency"
        grep -nE "$NON_AZURE_MANIFEST_REGEX" "$file" || true
      fi
      return 0
      ;;
  esac

  if grep -nE "$NON_AZURE_CODE_REGEX" "$file" >/dev/null 2>&1; then
    if is_guard_backed_test "$file" "$rel"; then
      : # rejected-provider fixtures in Intelligence tests
    else
      fail_file "$file" "non-Azure product AI provider"
      grep -nE "$NON_AZURE_CODE_REGEX" "$file" || true
    fi
  fi

  if is_guard_definition "$rel"; then
    return 0
  fi

  if grep -nE "$AZURE_OPENAI_SEND_REGEX" "$file" >/dev/null 2>&1; then
    if ! has_openai_guard "$file"; then
      fail_file "$file" "Azure OpenAI send site missing AzureOpenAIGuard.EnsureCanSend"
      grep -nE "$AZURE_OPENAI_SEND_REGEX" "$file" || true
    fi
  fi

  if grep -nE "$AZURE_SEARCH_SEND_REGEX" "$file" >/dev/null 2>&1; then
    if ! has_search_guard "$file"; then
      fail_file "$file" "Azure AI Search send site missing AzureAiSearchGuard.EnsureCanSend"
      grep -nE "$AZURE_SEARCH_SEND_REGEX" "$file" || true
    fi
  fi
}

load_allowlist

mapfile -t FILES < <(find "$SCAN_ROOT" -type f \
  \( -name '*.cs' -o -name '*.fs' -o -name '*.vb' \
     -o -name '*.js' -o -name '*.jsx' -o -name '*.ts' -o -name '*.tsx' \
     -o -name '*.mjs' -o -name '*.cjs' -o -name '*.py' -o -name '*.go' \
     -o -name '*.java' -o -name '*.kt' -o -name '*.rb' -o -name '*.php' \
     -o -name '*.csproj' -o -name '*.fsproj' -o -name '*.vbproj' \
     -o -name 'package.json' -o -name 'requirements.txt' -o -name 'pyproject.toml' \
     -o -name 'Pipfile' -o -name 'go.mod' \) \
  ! -path '*/.git/*' \
  ! -path '*/node_modules/*' \
  ! -path '*/bin/*' \
  ! -path '*/obj/*' \
  | sort)

for file in "${FILES[@]+"${FILES[@]}"}"; do
  scan_file "$file"
done

if [[ "$FAILED" == "true" ]]; then
  echo ""
  echo "Product AI policy check failed (SPEC-0013)."
  echo "Product AI must use the in-process AzureOpenAIGuard and AzureAiSearchGuard."
  echo "This scan does not require an HTTP /v1 gateway, Bicep, or an adapter audit call."
  echo "Marketing-site paths belong in .arkos/product-ai-allowlist.yml."
  exit 1
fi

echo "OK: Product AI policy check passed (SPEC-0013)."
exit 0
