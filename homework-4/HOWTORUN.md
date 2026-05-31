# How to Run — Homework 4

## Prerequisites

| Requirement | Version | Check |
|-------------|---------|-------|
| **.NET SDK** | **8.0 (required)** | `dotnet --version` → must show `8.x.x` |
| **Claude Code CLI** | latest | `claude --version` |
| **Anthropic API key** | — | `echo $ANTHROPIC_API_KEY` |

Install .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

---

## 1. Run the Application (BankingApi)

```bash
# From homework-4/
dotnet run --project src/BankingApi/
```

API starts at **http://localhost:3000**
Swagger UI: **http://localhost:3000/swagger**

### Quick smoke test
```bash
# List transactions (empty on fresh start)
curl http://localhost:3000/transactions

# Create a deposit
curl -X POST http://localhost:3000/transactions \
  -H "Content-Type: application/json" \
  -d '{"type":"deposit","toAccount":"ACC-AAAAA","amount":100.00,"currency":"USD"}'

# Check balance
curl http://localhost:3000/accounts/ACC-AAAAA/balance
```

---

## 2. Run Tests

```bash
# From homework-4/
dotnet test tests/BankingApi.Tests/
```

---

## 3. Run the Full 4-Agent Pipeline

> **Note**: This requires the Claude Code CLI and Anthropic authentication.

**Authentication** — the pipeline works with either:
- Claude Code's own keychain auth (already set up if you use `claude` interactively), **or**
- An explicit API key:

```bash
export ANTHROPIC_API_KEY=your-api-key-here
```

Then run:

```bash
# From homework-4/
chmod +x run-pipeline.sh
./run-pipeline.sh
```

The script runs agents in this order:
1. **Research Verifier** — verifies `context/bugs/001/research/codebase-research.md`
2. **Bug Fixer** — applies fixes from `context/bugs/001/implementation-plan.md`
3. **Security Verifier** — scans changed files, writes `security-report.md`
4. **Unit Test Generator** — generates tests for changed code, writes `test-report.md`

### Pipeline outputs (written to `context/bugs/001/`):
| File | Created by |
|------|-----------|
| `research/verified-research.md` | Research Verifier |
| `fix-summary.md` | Bug Fixer |
| `security-report.md` | Security Verifier |
| `test-report.md` | Unit Test Generator |

---

## 4. Pipeline Input Files (required before running)

The pipeline expects these files to exist and be populated:

| File | Purpose |
|------|---------|
| `context/bugs/001/research/codebase-research.md` | Bug Researcher output (file:line references + snippets) |
| `context/bugs/001/implementation-plan.md` | Bug Planner output (before/after code for each fix) |

If running the pipeline from scratch, populate these files first (manually or via a Bug Researcher / Bug Planner agent step).

---

## 5. Build Only

```bash
dotnet build src/BankingApi/
dotnet build tests/BankingApi.Tests/
```

---

## Environment Setup

No environment variables required to run the application.

For the pipeline, set the API key only if you are **not** using Claude Code's interactive session auth:
```bash
export ANTHROPIC_API_KEY=your-key-here
```
