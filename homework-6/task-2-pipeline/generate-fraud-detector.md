---
model: claude-opus-4-6
---

Generate the FraudDetector agent for the multi-agent banking pipeline.

## Pre-flight

1. Read `task-1-specification/specification.md` — Section 5, "Task: Fraud Detector" for exact scoring rules, thresholds, and acceptance criteria.
2. Read `task-1-specification/agents.md` — directory routing, PII masking, code style.
3. Read `sample-transactions.json` — understand the fraud signals: TXN002 ($25K high-value), TXN003 ($9,999.99 structuring), TXN004 (02:47 UTC off-hours + country DE cross-border), TXN005 ($75K high-value).
4. Read existing `task-2-pipeline/pipeline-code/Models/MessageEnvelope.cs` and `task-2-pipeline/pipeline-code/Helpers/FileHelper.cs` — use shared models and utilities.
5. Use **MCP context7** to look up `decimal` handling in C# / .NET — parsing, comparison, `CultureInfo.InvariantCulture`. Document the query and key insight for `research-notes.md`.

## Output location

All generated code goes inside `task-2-pipeline/pipeline-code/`.

## File to create

### `task-2-pipeline/pipeline-code/agents/FraudDetector.cs`

**Constructor:** Takes `ILoggerFactory` and `JsonSerializerOptions`. Creates `ILogger<FraudDetector>` from the factory.

**Method:** `async Task<string> ProcessAsync(string envelopeFilePath, CancellationToken ct = default)`

Returns the status string: `"flagged"` or `"approved"`.

### Risk scoring logic

Read validated envelope from `shared/output/<transaction_id>.json`.

Compute `risk_score` (integer 0–100) using **additive signals** on `decimal` amounts:

| Signal | Condition | Points |
|--------|-----------|--------|
| High-value | `amount > 10_000m` | Base score 60 |
| Structuring | `amount >= 9_000m && amount <= 9_999.99m` | Base score 40 |
| Off-hours | Transaction timestamp hour is 00–05 UTC (i.e. `hour >= 0 && hour < 6`) | +20 |
| Cross-border | `metadata.country != "US"` | +15 |

Rules:
- High-value and Structuring are mutually exclusive (a transaction matches at most one)
- Off-hours and Cross-border are additive on top of the base score
- Cap total `risk_score` at 100
- Parse the original transaction `timestamp` from `envelope.Data.Timestamp` to extract the UTC hour
- Parse `amount` from `envelope.Data.Amount` using `decimal.Parse(value, CultureInfo.InvariantCulture)`

### Status assignment

- `risk_score >= 50` → `status: "flagged"`
- `risk_score < 50` → `status: "approved"`

### Output routing

- Update envelope: `source_agent = "fraud_detector"`, `target_agent = "settlement_reporter"`, set `status` and `risk_score`, new `timestamp`
- Write to `shared/processing/<transaction_id>.json` using `FileHelper.WriteJsonAtomicAsync`
- Delete source file from `shared/output/`
- Log at `Information` for approved, `Warning` for flagged
- Log message: `"Transaction {TransactionId} scored {RiskScore} — {Status}"` (mask accounts)

### Acceptance criteria

- [ ] TXN001 ($1,500 USD, US, 09:00 UTC) → `risk_score: 0`, `status: "approved"`
- [ ] TXN002 ($25,000 USD, US, 09:15 UTC) → `risk_score: 60`, `status: "flagged"`
- [ ] TXN003 ($9,999.99 USD, US, 09:30 UTC) → `risk_score: 40`, `status: "approved"` (40 < 50)
- [ ] TXN004 (€500 EUR, DE, 02:47 UTC) → `risk_score: 35` (off-hours 20 + cross-border 15), `status: "approved"` (35 < 50)
- [ ] TXN005 ($75,000 USD, US, 10:00 UTC) → `risk_score: 60`, `status: "flagged"`
- [ ] TXN007 (£-100 GBP, GB, 10:10 UTC) → `risk_score: 15` (cross-border), `status: "approved"`
- [ ] TXN008 ($3,200 USD, US, 10:15 UTC) → `risk_score: 0`, `status: "approved"`

Note: TXN006 was rejected by the validator and never reaches the fraud detector.

### Rules
- Use `decimal` for all amount comparisons — never `float` or `double`
- Use `ILogger<FraudDetector>` — never `Console.WriteLine`
- Mask account numbers as `ACC-****` in ALL log output
- Atomic file writes via `FileHelper.WriteJsonAtomicAsync`
- Delete source file from `shared/output/` after writing
- async/await throughout
