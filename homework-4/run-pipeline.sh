#!/usr/bin/env bash
# run-pipeline.sh — Runs the full 4-agent bug-fix pipeline for BankingApi
#
# Prerequisites:
#   - .NET 8 SDK installed
#   - Claude Code CLI installed (`claude`)
#   - ANTHROPIC_API_KEY set in environment
#
# Usage:
#   chmod +x run-pipeline.sh
#   ./run-pipeline.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONTEXT="$SCRIPT_DIR/context/bugs/001"

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

log() { echo; echo "======================================================"; echo "  $1"; echo "======================================================"; }
fail() { echo "[ERROR] $1" >&2; exit 1; }

# Strip YAML frontmatter (--- ... ---) from an agent file and return the body
agent_prompt() {
  local file="$1"
  awk '/^---/{found++; next} found>=2{print}' "$file"
}

# ---------------------------------------------------------------------------
# Pre-flight checks
# ---------------------------------------------------------------------------

command -v claude >/dev/null 2>&1 || fail "Claude Code CLI not found. Install from https://claude.ai/code"
command -v dotnet >/dev/null 2>&1 || fail ".NET SDK not found. Install .NET 8 from https://dotnet.microsoft.com/download/dotnet/8.0"
# Auth check — accept either API key env var or Claude Code's own keychain auth
if [[ -z "${ANTHROPIC_API_KEY:-}" ]]; then
  claude --version >/dev/null 2>&1 || fail "No ANTHROPIC_API_KEY and Claude Code auth not available."
  echo "[INFO] Using Claude Code keychain authentication."
fi

[[ -f "$CONTEXT/research/codebase-research.md" ]] || fail "codebase-research.md not found. Run the Bug Researcher step first."
[[ -f "$CONTEXT/implementation-plan.md" ]] || fail "implementation-plan.md not found. Run the Bug Planner step first."

# Check that input files are not empty placeholders
grep -q "To be populated" "$CONTEXT/research/codebase-research.md" && \
  fail "codebase-research.md is still a placeholder — populate it before running the pipeline."
grep -q "To be populated" "$CONTEXT/implementation-plan.md" && \
  fail "implementation-plan.md is still a placeholder — populate it before running the pipeline."

# ---------------------------------------------------------------------------
# Step 1 — Research Verifier
# ---------------------------------------------------------------------------

log "Step 1/4 — Research Verifier (claude-opus-4-6)"

claude -p \
  "$(agent_prompt "$SCRIPT_DIR/agents/research-verifier.agent.md")" \
  --model claude-opus-4-6 \
  --add-dir "$SCRIPT_DIR" \
  --dangerously-skip-permissions \
  --output-format text

echo "[OK] verified-research.md written."

# ---------------------------------------------------------------------------
# Step 2 — Bug Fixer
# ---------------------------------------------------------------------------

log "Step 2/4 — Bug Fixer (claude-sonnet-4-6)"

claude -p \
  "$(agent_prompt "$SCRIPT_DIR/agents/bug-fixer.agent.md")" \
  --model claude-sonnet-4-6 \
  --add-dir "$SCRIPT_DIR" \
  --dangerously-skip-permissions \
  --output-format text

echo "[OK] fix-summary.md written."

# ---------------------------------------------------------------------------
# Step 3 — Security Verifier  (runs after bug-fixer)
# ---------------------------------------------------------------------------

log "Step 3/4 — Security Verifier (claude-opus-4-6)"

claude -p \
  "$(agent_prompt "$SCRIPT_DIR/agents/security-verifier.agent.md")" \
  --model claude-opus-4-6 \
  --add-dir "$SCRIPT_DIR" \
  --dangerously-skip-permissions \
  --output-format text

echo "[OK] security-report.md written."

# ---------------------------------------------------------------------------
# Step 4 — Unit Test Generator  (runs after bug-fixer)
# ---------------------------------------------------------------------------

log "Step 4/4 — Unit Test Generator (claude-haiku-4-5-20251001)"

claude -p \
  "$(agent_prompt "$SCRIPT_DIR/agents/unit-test-generator.agent.md")" \
  --model claude-haiku-4-5-20251001 \
  --add-dir "$SCRIPT_DIR" \
  --dangerously-skip-permissions \
  --output-format text

echo "[OK] test-report.md written."

# ---------------------------------------------------------------------------
# Done
# ---------------------------------------------------------------------------

log "Pipeline complete"
echo "Outputs written to $CONTEXT/"
echo ""
echo "  research/verified-research.md"
echo "  fix-summary.md"
echo "  security-report.md"
echo "  test-report.md"
echo ""
echo "Run tests to confirm fixes:"
echo "  dotnet test tests/BankingApi.Tests/"
