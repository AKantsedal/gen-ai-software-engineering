# Agent Guidelines — AI-Powered Multi-Agent Banking Pipeline

## Tech Stack
- .NET 10, C# 13
- `System.Text.Json` for all JSON serialization/deserialization
- `Microsoft.Extensions.Logging` (`ILogger<T>`) for structured logging
- `xUnit` for unit and integration tests
- `coverlet` for code coverage collection (`dotnet test --collect:"XPlat Code Coverage"`)
- Python + FastMCP for the MCP server (`mcp/server.py`)

## AI Model
Each agent must declare an explicit model. Model choice must match task complexity:

| Agent | Model | Justification |
|-------|-------|---------------|
| Transaction Validator | `claude-haiku-4-5-20251001` | Mechanical field checks — fast, deterministic, no reasoning required |
| Fraud Detector | `claude-opus-4-8` | Risk scoring requires deep multi-signal reasoning; missing a fraud flag has high cost — Opus provides strongest analytical accuracy |
| Settlement Reporter | `claude-haiku-4-5-20251001` | Structured output generation — deterministic, no open-ended reasoning |
| Integrator / Orchestrator | `claude-opus-4-8` | Orchestration decisions, error recovery, and pipeline sequencing require strong reasoning to handle edge cases correctly |

## Domain Rules
- All monetary amounts must use `decimal` — never `double` or `float`
- Valid currencies are ISO 4217 codes only. Accepted set: `USD`, `EUR`, `GBP`, `JPY`, `CAD`, `AUD`, `CHF`, `CNY`, `SEK`, `NOK`, `DKK`, `SGD`, `HKD`, `NZD`, `MXN`, `BRL`, `INR`, `ZAR`, `KRW`
- Negative amounts are only permitted for `transaction_type = "refund"`; all other types must have `amount > 0`
- Account numbers are PII — mask as `ACC-****` in all logs, result files, and console output; never write the raw account ID anywhere
- `transaction_id` is safe to log and include in all output

## Message Protocol

### JSON Envelope Schema
All inter-agent messages must conform to this schema:
```json
{
  "message_id": "<UUID v4>",
  "timestamp": "<ISO 8601, e.g. 2026-03-16T10:00:00Z>",
  "source_agent": "<agent name, snake_case>",
  "target_agent": "<agent name, snake_case>",
  "message_type": "transaction",
  "data": {
    "transaction_id": "<string>",
    "amount": "<decimal as string, e.g. \"1500.00\">",
    "currency": "<ISO 4217>",
    "transaction_type": "<string>",
    "status": "<validated | flagged | approved | rejected | settled>",
    "risk_score": "<integer 0–100, present from fraud_detector onward>",
    "rejection_reason": "<string, present only when status = rejected>"
  }
}
```

### Directory Routing
| Stage | Agent reads from | Agent writes to |
|-------|-----------------|-----------------|
| Input loading | `shared/input/` | `shared/processing/` |
| Validation | `shared/processing/` | `shared/output/` (valid) or `shared/results/` (rejected) |
| Fraud detection | `shared/output/` | `shared/processing/` (scored) |
| Settlement reporting | `shared/processing/` (scored) | `shared/results/` |
| Integrator | `shared/results/` | `shared/results/summary.json` |

Each file in transit is a single JSON envelope named `<transaction_id>.json`.

## Code Style
- `async`/`await` throughout — no `.Result`, `.Wait()`, or blocking calls
- Constructor injection for all dependencies; never `new` a dependency inside a class
- Use `ILogger<T>` for all logging — never `Console.WriteLine` in agent code
- Money values: always `decimal`, never cast to `double`/`float` even temporarily
- Use `JsonSerializerOptions` with `PropertyNameCaseInsensitive = true` for envelope parsing
- Prefer `ReadOnlySpan<char>` / `string.Equals(OrdinalIgnoreCase)` for currency code comparison

## Security & PII
- Account numbers (`source_account`, `destination_account`) must be masked to `ACC-****` **before** any log call, file write, or output
- The raw account ID must never appear in: log lines, result JSON files, the summary report, or any exception message
- `transaction_id` is not PII and may appear anywhere
- Result files written to `shared/results/` must also use masked account values
- No secrets, API keys, or credentials may be hardcoded; use environment variables or configuration

## Testing Expectations
- One unit test class per agent covering: happy path, validation failures, edge cases (boundary amounts, off-hours timestamps, unknown currencies)
- One integration test that runs the full pipeline end-to-end: loads all 8 transactions from `sample-transactions.json` and asserts all 8 appear in `shared/results/`
- Tests must **not** read from or write to the real `shared/` directories — use `Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())` for isolation
- Coverage gate: push is **blocked** if line coverage falls below **80%**; target ≥ **90%**
- Test framework: xUnit; coverage tool: coverlet via `dotnet test --collect:"XPlat Code Coverage"`
- Every test must satisfy FIRST principles: Fast, Independent, Repeatable, Self-validating, Timely
