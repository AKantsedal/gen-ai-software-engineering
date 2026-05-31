# Homework 4 — 4-Agent Pipeline

**Student:** Artur Kantsedal
**Task:** Build a 4-agent pipeline that researches, fixes, security-reviews, and tests a real .NET application.

---

## Overview

This homework implements a sequential 4-agent pipeline operating on **BankingApi** — a .NET 8 in-memory banking REST API copied from Homework 1 with intentional bugs and a security issue seeded for the pipeline to find and fix.

```
Bug Researcher → Research Verifier → Bug Planner → Bug Fixer → Security Verifier
                                                              → Unit Test Generator
```

The pipeline is fully automated via a single shell script. Each agent has an explicit model selection appropriate to its responsibility.

---

## Agent Model Choices

| Agent | Model | Justification |
|-------|-------|---------------|
| `research-verifier` | `claude-opus-4-6` | Precision cross-referencing of file:line claims requires the strongest reasoning model |
| `bug-fixer` | `claude-sonnet-4-6` | Executing an explicit before/after plan is well-specified; Sonnet is fast and accurate |
| `security-verifier` | `claude-opus-4-6` | Security analysis has high cost of false negatives; Opus minimises missed findings |
| `unit-test-generator` | `claude-haiku-4-5-20251001` | Test scaffolding for changed code is repetitive and mechanical; Haiku is fast and cost-efficient |

---

## Seeded Issues in BankingApi

| # | File | Line | Issue |
|---|------|------|-------|
| Bug 1 | `Services/TransactionService.cs` | 57 | `+=` instead of `-=` — withdrawals incorrectly inflate balance |
| Bug 2 | `Validators/TransactionValidator.cs` | 42 | `< 0` instead of `<= 0` — zero-amount transactions pass validation |
| Security | `Controllers/AccountsController.cs` | 13 | Hardcoded debug bypass key exposes all data unauthenticated |

---

## Project Structure

```
homework-4/
├── README.md
├── HOWTORUN.md
├── run-pipeline.sh             ← single command to run the full pipeline
├── agents/
│   ├── research-verifier.agent.md
│   ├── bug-fixer.agent.md
│   ├── security-verifier.agent.md
│   └── unit-test-generator.agent.md
├── skills/
│   ├── research-quality-measurement.md
│   └── unit-tests.md
├── context/bugs/001/
│   ├── bug-context.md
│   ├── research/
│   │   ├── codebase-research.md    ← Bug Researcher output
│   │   └── verified-research.md   ← Research Verifier output
│   ├── implementation-plan.md      ← Bug Planner output
│   ├── fix-summary.md              ← Bug Fixer output
│   ├── security-report.md          ← Security Verifier output
│   └── test-report.md              ← Unit Test Generator output
├── src/BankingApi/                 ← application source (pre- and post-fix)
└── tests/BankingApi.Tests/         ← xUnit test project
```

---

## How I Used AI

- Claude Code (Sonnet 4.6 with 1M context) was used to design all agent files, skills, and pipeline structure throughout this session.
- Each agent's system prompt was iteratively refined to precisely match the task requirements.
- The BankingApi source was reused from Homework 1; bugs were seeded with AI assistance to ensure they were realistic and demonstrable.
- Agent outputs (`verified-research.md`, `fix-summary.md`, `security-report.md`, `test-report.md`) were produced by actually running the pipeline via `run-pipeline.sh`.

---

## Prerequisites

See [HOWTORUN.md](HOWTORUN.md) for full setup and run instructions.

- .NET 8 SDK (required)
- Claude Code CLI (`claude`) with a valid Anthropic API key
