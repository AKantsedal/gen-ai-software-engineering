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
│   ├── commands/          Slash commands (meta-agents)
│   └── mcp.json           MCP server configuration
├── task-1-specification/  Specification (Agent 1 output)
├── task-2-pipeline/       Code generation agents (Agent 2)
├── agents/                Pipeline agent C# modules
├── Models/                Shared data models
├── Helpers/               Utility classes (atomic writes, PII masking)
├── shared/                File-based communication directories
│   ├── input/
│   ├── processing/
│   ├── output/
│   └── results/
├── mcp/                   Custom FastMCP server
├── tests/                 xUnit test suite
└── docs/screenshots/      Evidence screenshots
```
