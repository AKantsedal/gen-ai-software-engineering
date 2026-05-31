# run-pipeline.ps1 — Runs the full 4-agent bug-fix pipeline for BankingApi
#
# Prerequisites:
#   - .NET 8 SDK installed
#   - Claude Code CLI installed (claude)
#   - Anthropic auth: either Claude Code keychain (interactive session) OR ANTHROPIC_API_KEY env var
#
# Usage:
#   .\run-pipeline.ps1           — run all 4 steps
#   .\run-pipeline.ps1 -Step 1  — run only step 1 (research-verifier)
#   .\run-pipeline.ps1 -Step 2  — run only step 2 (bug-fixer)
#   .\run-pipeline.ps1 -Step 3  — run only step 3 (security-verifier)
#   .\run-pipeline.ps1 -Step 4  — run only step 4 (unit-test-generator)
#   .\run-pipeline.ps1 -From 2  — run steps 2 through 4

param(
    [int]$Step = 0,   # run only this step (0 = all)
    [int]$From = 1    # start from this step
)

$ErrorActionPreference = "Stop"

$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ContextDir = Join-Path $ScriptDir "context\bugs\001"

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

function Log($msg) {
    Write-Host ""
    Write-Host "======================================================"
    Write-Host "  $msg"
    Write-Host "======================================================"
}

function Fail($msg) {
    Write-Error "[ERROR] $msg"
    exit 1
}

# Strip YAML frontmatter (--- ... ---) from an agent file and return the body
function Get-AgentPrompt($file) {
    $lines  = Get-Content $file
    $count  = 0
    $result = @()
    foreach ($line in $lines) {
        if ($line -eq "---") { $count++; continue }
        if ($count -ge 2)    { $result += $line }
    }
    return $result -join "`n"
}

function ShouldRun($stepNum) {
    if ($Step -ne 0 -and $Step -ne $stepNum) { return $false }
    if ($stepNum -lt $From)                   { return $false }
    return $true
}

# ---------------------------------------------------------------------------
# Pre-flight checks
# ---------------------------------------------------------------------------

if (-not (Get-Command claude -ErrorAction SilentlyContinue)) {
    Fail "Claude Code CLI not found. Install from https://claude.ai/code"
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Fail ".NET SDK not found. Install .NET 8 from https://dotnet.microsoft.com/download/dotnet/8.0"
}

# Auth check — accept either API key env var or Claude Code's own keychain auth
if (-not $env:ANTHROPIC_API_KEY) {
    Write-Host "[INFO] ANTHROPIC_API_KEY not set — using Claude Code keychain authentication."
}

$researchFile = Join-Path $ContextDir "research\codebase-research.md"
$planFile     = Join-Path $ContextDir "implementation-plan.md"

if (-not (Test-Path $researchFile)) { Fail "codebase-research.md not found. Run the Bug Researcher step first." }
if (-not (Test-Path $planFile))     { Fail "implementation-plan.md not found. Run the Bug Planner step first." }

if ((Get-Content $researchFile) -match "To be populated") { Fail "codebase-research.md is still a placeholder." }
if ((Get-Content $planFile)     -match "To be populated") { Fail "implementation-plan.md is still a placeholder." }

# ---------------------------------------------------------------------------
# Step 1 — Research Verifier
# ---------------------------------------------------------------------------

if (ShouldRun 1) {
    Log "Step 1/4 — Research Verifier (claude-opus-4-6)"
    $prompt = Get-AgentPrompt (Join-Path $ScriptDir "agents\research-verifier.agent.md")
    claude -p $prompt --model claude-opus-4-6 --add-dir $ScriptDir --dangerously-skip-permissions --output-format text
    Write-Host "[OK] verified-research.md written."
}

# ---------------------------------------------------------------------------
# Step 2 — Bug Fixer
# ---------------------------------------------------------------------------

if (ShouldRun 2) {
    Log "Step 2/4 — Bug Fixer (claude-sonnet-4-6)"
    $prompt = Get-AgentPrompt (Join-Path $ScriptDir "agents\bug-fixer.agent.md")
    claude -p $prompt --model claude-sonnet-4-6 --add-dir $ScriptDir --dangerously-skip-permissions --output-format text
    Write-Host "[OK] fix-summary.md written."
}

# ---------------------------------------------------------------------------
# Step 3 — Security Verifier
# ---------------------------------------------------------------------------

if (ShouldRun 3) {
    Log "Step 3/4 — Security Verifier (claude-opus-4-6)"
    $prompt = Get-AgentPrompt (Join-Path $ScriptDir "agents\security-verifier.agent.md")
    claude -p $prompt --model claude-opus-4-6 --add-dir $ScriptDir --dangerously-skip-permissions --output-format text
    Write-Host "[OK] security-report.md written."
}

# ---------------------------------------------------------------------------
# Step 4 — Unit Test Generator
# ---------------------------------------------------------------------------

if (ShouldRun 4) {
    Log "Step 4/4 — Unit Test Generator (claude-haiku-4-5-20251001)"
    $prompt = Get-AgentPrompt (Join-Path $ScriptDir "agents\unit-test-generator.agent.md")
    claude -p $prompt --model claude-haiku-4-5-20251001 --add-dir $ScriptDir --dangerously-skip-permissions --output-format text
    Write-Host "[OK] test-report.md written."
}

# ---------------------------------------------------------------------------
# Done
# ---------------------------------------------------------------------------

Log "Pipeline complete"
Write-Host "Outputs written to $ContextDir"
Write-Host ""
Write-Host "  research\verified-research.md"
Write-Host "  fix-summary.md"
Write-Host "  security-report.md"
Write-Host "  test-report.md"
Write-Host ""
Write-Host "Run tests to confirm fixes:"
Write-Host "  dotnet test tests\BankingApi.Tests\"
