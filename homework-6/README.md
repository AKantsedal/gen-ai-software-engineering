# AI-Powered Multi-Agent Banking Pipeline

**Created by: Oleksii Kantsedal**

## Overview

A multi-agent transaction processing system built with .NET 10 / C# 13. Four AI meta-agents (specification, code generation, testing, documentation) produce and maintain a banking pipeline that validates, scores, and settles transactions through file-based inter-agent communication.

The pipeline processes transactions from `sample-transactions.json` through three cooperating agents, writing final results to `shared/results/`.

## Agent Responsibilities

- **Transaction Validator** — checks required fields, validates amounts (decimal only), verifies ISO 4217 currency codes, rejects invalid transactions
- **Fraud Detector** — scores transactions for risk (high-value, structuring patterns, off-hours timing, cross-border signals), flags score >= 50
- **Settlement Reporter** — assigns final settlement status: approved transactions are settled, flagged transactions are held for review

## Architecture

```
sample-transactions.json
        |
        v
  +-----------+
  | Integrator |  Wraps raw transactions in JSON envelopes
  +-----------+
        |
        v
  shared/input/  -->  shared/processing/
                          |
                          v
                 +--------------------+
                 | TransactionValidator|  Validates fields, amounts, currency
                 +--------------------+
                    |             |
               (valid)       (rejected)
                    |             |
                    v             v
             shared/output/   shared/results/
                    |
                    v
              +--------------+
              | FraudDetector |  Scores risk 0-100
              +--------------+
                    |
                    v
             shared/processing/
                    |
                    v
            +-------------------+
            | SettlementReporter |  Settles or holds for review
            +-------------------+
                    |
                    v
             shared/results/
                    |
                    v
  +-----------+
  | Integrator |  Writes summary.json
  +-----------+
```

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Language | C# 13 / .NET 10 |
| Serialization | System.Text.Json |
| Logging | Microsoft.Extensions.Logging (ILogger<T>) |
| Testing | xUnit + coverlet |
| MCP Server | Python + FastMCP |
| AI Agents | Claude Code slash commands |

## Project Structure

```
homework-6/
├── .claude/
│   ├── commands/                  Slash commands (run-pipeline, validate-transactions)
│   ├── settings.json              Hooks (coverage gate on git push)
│   └── mcp.json                   MCP server configuration
├── task-1-specification/          Specification (Agent 1 output)
├── task-2-pipeline/
│   ├── pipeline-code/             .NET 10 C# pipeline source
│   │   ├── agents/                TransactionValidator, FraudDetector, SettlementReporter
│   │   ├── Models/                MessageEnvelope, RawTransaction, PipelineSummary
│   │   ├── Helpers/               FileHelper (atomic writes), PiiMasker
│   │   └── Integrator.cs          Orchestrator — runs all agents end-to-end
│   └── tests/                     xUnit test suite (53 tests, ~94% coverage)
├── task-3-skills-hooks/           Skills and hooks agent definitions
├── task-4-mcp/                    MCP agent definition
├── mcp/                           FastMCP server (Python)
├── shared/                        File-based inter-agent communication
│   ├── input/
│   ├── processing/
│   ├── output/
│   └── results/
├── docs/screenshots/               Evidence screenshots (5 required)
└── sample-transactions.json       8 test transactions
```
