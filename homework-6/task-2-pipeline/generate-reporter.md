Generate the SettlementReporter (Reporting Agent) for the multi-agent banking pipeline.

## Pre-flight

1. Read `task-1-specification/specification.md` — Section 5, "Task: Settlement Reporter" for exact logic and acceptance criteria.
2. Read `task-1-specification/agents.md` — directory routing, PII masking, code style.
3. Read existing `task-2-pipeline/pipeline-code/Models/MessageEnvelope.cs` and `task-2-pipeline/pipeline-code/Helpers/FileHelper.cs` — use shared models and utilities.

## Output location

All generated code goes inside `task-2-pipeline/pipeline-code/`.

## File to create

### `task-2-pipeline/pipeline-code/agents/SettlementReporter.cs`

**Constructor:** Takes `ILoggerFactory` and `JsonSerializerOptions`. Creates `ILogger<SettlementReporter>` from the factory.

**Method:** `async Task<string> ProcessAsync(string envelopeFilePath, CancellationToken ct = default)`

Returns the settlement status string: `"settled"` or `"held_for_review"`.

### Settlement logic

Read fraud-scored envelope from `shared/processing/<transaction_id>.json`.

Assign `settlement_status` based on the fraud detector's `status` field:

| Input status | Settlement status |
|-------------|-------------------|
| `"approved"` | `"settled"` |
| `"flagged"` | `"held_for_review"` |

### Output routing

- Update envelope: `source_agent = "settlement_reporter"`, `target_agent = "none"`, set `settlement_status`, new `timestamp`
- Write final result envelope to `shared/results/<transaction_id>.json` using `FileHelper.WriteJsonAtomicAsync`
- Delete source file from `shared/processing/`
- Log at `Information` for settled, `Warning` for held_for_review
- Log message: `"Transaction {TransactionId} — {SettlementStatus}"` (mask accounts)

### Acceptance criteria

- [ ] TXN001 (approved, risk 0) → `shared/results/TXN001.json` with `settlement_status: "settled"`
- [ ] TXN002 (flagged, risk 60) → `shared/results/TXN002.json` with `settlement_status: "held_for_review"`
- [ ] TXN003 (approved, risk 40) → `shared/results/TXN003.json` with `settlement_status: "settled"`
- [ ] TXN004 (approved, risk 35) → `shared/results/TXN004.json` with `settlement_status: "settled"`
- [ ] TXN005 (flagged, risk 60) → `shared/results/TXN005.json` with `settlement_status: "held_for_review"`
- [ ] TXN006 — already in `shared/results/` (rejected by validator) — reporter does NOT process it
- [ ] TXN007 (approved, risk 15) → `shared/results/TXN007.json` with `settlement_status: "settled"`
- [ ] TXN008 (approved, risk 0) → `shared/results/TXN008.json` with `settlement_status: "settled"`

### Final state after full pipeline

After all 3 agents run, `shared/results/` should contain 8 files:
- `TXN001.json` through `TXN008.json` (one per transaction)
- `summary.json` (written by the Integrator after all agents complete)

`shared/input/`, `shared/processing/`, and `shared/output/` should all be empty.

### Rules
- Use `ILogger<SettlementReporter>` — never `Console.WriteLine`
- Mask account numbers as `ACC-****` in ALL log output
- Atomic file writes via `FileHelper.WriteJsonAtomicAsync`
- Delete source file from `shared/processing/` after writing
- async/await throughout
