# Transaction Processing Pipeline — Technical Specification

> Ingest the information from this file, implement the Low-Level Tasks, and generate the code that will satisfy the High and Mid-Level Objectives.

---

## 1. High-Level Objective

Process all incoming banking transactions through a sequential multi-agent pipeline (validator → fraud detector → settlement reporter), writing a result file per transaction and a summary report to `shared/results/`, with full audit logging and no plaintext PII in any output.

---

## 2. Mid-Level Objectives

1. **All 8 transactions from `sample-transactions.json` produce a result file** in `shared/results/` after a pipeline run — no transaction is silently dropped.
2. **Transactions with invalid currency codes or invalid amounts are rejected** before reaching the fraud detector, with a `rejection_reason` field in their result file (covers TXN006 currency `XYZ`, TXN007 negative amount `-100.00`).
3. **Transactions above $10,000 USD-equivalent are flagged** with `status: "flagged"` and a `risk_score` between 50–100; transactions at or below $10,000 that pass all checks receive `status: "approved"` with a `risk_score` of 0–49 (covers TXN002 $25,000, TXN005 $75,000).
4. **Near-threshold structuring patterns ($9,000–$9,999.99) and off-hours transactions (00:00–06:00 UTC) receive elevated risk scores** of at least 40, even if below the $10,000 flag threshold (covers TXN003 $9,999.99, TXN004 02:47 UTC cross-border).
5. **All agent operations produce a structured audit log entry** containing ISO 8601 timestamp, agent name, transaction ID, and outcome — account numbers never appear in logs in plaintext.

---

## 3. Implementation Notes

### Monetary Values
- Use `decimal` for all amount parsing and arithmetic — never `double` or `float`, even temporarily
- Parse `amount` from the JSON string field using `decimal.Parse(value, CultureInfo.InvariantCulture)`
- All risk score thresholds and comparisons operate on `decimal`

### Currency Validation
- Valid currency codes (ISO 4217): `USD`, `EUR`, `GBP`, `JPY`, `CAD`, `AUD`, `CHF`, `CNY`, `SEK`, `NOK`, `DKK`, `SGD`, `HKD`, `NZD`, `MXN`, `BRL`, `INR`, `ZAR`, `KRW`
- Any currency code not in this list → reject with `rejection_reason: "invalid_currency_code"`
- Currency comparison is case-sensitive; codes must be uppercase

### Logging
- Use `ILogger<T>` exclusively — no `Console.WriteLine` in agent code
- Every log entry must include: `transaction_id`, `agent`, `outcome`, `timestamp` (ISO 8601)
- Log level: `Information` for normal outcomes, `Warning` for rejections and flags, `Error` for unexpected failures

### PII Handling
- `source_account` and `destination_account` are PII — mask as `ACC-****` before any log call, file write, or output
- `transaction_id` (e.g. `TXN001`) is not PII and may appear anywhere
- Result files written to `shared/results/` must also contain masked account values

### File-Based Message Passing
- Each transaction in transit is a single JSON file named `<transaction_id>.json`
- Agents must move files atomically: write to a `.tmp` file then rename, to avoid partial reads
- An agent must delete or move the source file after processing — no file should remain in `shared/processing/` after the pipeline completes

### Message Envelope
All inter-agent files conform to this schema:
```json
{
  "message_id": "<UUID v4>",
  "timestamp": "<ISO 8601>",
  "source_agent": "<snake_case agent name>",
  "target_agent": "<snake_case agent name>",
  "message_type": "transaction",
  "data": {
    "transaction_id": "<string>",
    "amount": "<decimal as string>",
    "currency": "<ISO 4217>",
    "transaction_type": "<string>",
    "status": "<validated | flagged | approved | rejected | settled>",
    "risk_score": "<integer 0–100, present from fraud_detector onward>",
    "rejection_reason": "<string, present only when status = rejected>"
  }
}
```

---

## 4. Context

### Beginning State
- `sample-transactions.json` exists at the project root with 8 raw transaction records
- Known validation failures in the input: TXN006 (`currency: "XYZ"`), TXN007 (`amount: "-100.00"`, type `refund` — negative allowed but this must still be validated for field completeness)
- Known fraud signals in the input: TXN002 ($25,000), TXN003 ($9,999.99 structuring), TXN004 (02:47 UTC off-hours + cross-border DE), TXN005 ($75,000)
- `shared/` directory tree exists with subdirectories: `input/`, `processing/`, `output/`, `results/`
- No agent source files exist yet

### Ending State
- 8 result files exist in `shared/results/`, one per transaction (`TXN001.json` … `TXN008.json`)
- `shared/results/summary.json` exists with pipeline run totals: total, validated, rejected, flagged, approved, settled counts
- All `shared/input/`, `shared/processing/`, `shared/output/` directories are empty (files moved through)
- Test coverage ≥ 90% as reported by coverlet
- All agent source files and the integrator are present and compilable

---

## 5. Low-Level Tasks

---

### Task: Transaction Validator

**Prompt:**
"You are implementing the TransactionValidator agent for a .NET 10 multi-agent banking pipeline. Read `task-1-specification/specification.md` and `task-1-specification/agents.md` for full context. The validator reads JSON envelope files from `shared/processing/`, checks required fields, validates the amount is a parseable decimal, rejects negative amounts (except transaction_type = 'refund'), and validates the currency code against the ISO 4217 allowlist in agents.md. Valid transactions are written to `shared/output/` with status 'validated'. Rejected transactions are written directly to `shared/results/` with status 'rejected' and a rejection_reason. All account numbers must be masked as ACC-**** in any log or output. Use ILogger<T> for all logging. Use decimal for all amount handling."

**File to CREATE:** `agents/TransactionValidator.cs`

**Function to CREATE:**
```csharp
Task<ValidationResult> ProcessAsync(string envelopeFilePath, CancellationToken ct = default)
```

**Details:**
- Reads one `<transaction_id>.json` envelope from `shared/processing/`
- Checks all required fields are present and non-empty: `transaction_id`, `amount`, `currency`, `transaction_type`, `source_account`, `destination_account`, `timestamp`
- Parses `amount` as `decimal`; rejects if not parseable
- Rejects negative amounts unless `transaction_type == "refund"`
- Validates `currency` against the 19-code ISO 4217 allowlist (case-sensitive, uppercase)
- Writes approved envelope (status: `"validated"`) to `shared/output/<transaction_id>.json`
- Writes rejected envelope (status: `"rejected"`, includes `rejection_reason`) to `shared/results/<transaction_id>.json`
- Moves source file out of `shared/processing/` after processing

**Acceptance Criteria:**
- [ ] TXN001–TXN005 and TXN008 produce files in `shared/output/` with `status: "validated"`
- [ ] TXN006 produces a file in `shared/results/` with `status: "rejected"` and `rejection_reason: "invalid_currency_code"`
- [ ] TXN007 (negative amount, type `refund`) is treated as valid and passes to `shared/output/` — refunds are allowed to be negative

---

### Task: Fraud Detector

**Prompt:**
"You are implementing the FraudDetector agent for a .NET 10 multi-agent banking pipeline. Read `task-1-specification/specification.md` and `task-1-specification/agents.md` for full context. The fraud detector reads validated transaction envelopes from `shared/output/`, computes a risk_score (0–100) based on multiple signals, and writes the scored envelope to `shared/processing/` for the settlement reporter. Scoring rules: amount > 10000 → base score 60, amount 9000–9999.99 (structuring) → base score 40, transaction timestamp between 00:00–06:00 UTC → add 20, country != 'US' (cross-border) → add 15. Cap score at 100. Transactions with score >= 50 get status 'flagged'; others get status 'approved'. Use decimal for all amount comparisons. Use ILogger<T> for logging. Mask account numbers as ACC-**** in all output."

**File to CREATE:** `agents/FraudDetector.cs`

**Function to CREATE:**
```csharp
Task<FraudScoringResult> ProcessAsync(string envelopeFilePath, CancellationToken ct = default)
```

**Details:**
- Reads one `<transaction_id>.json` from `shared/output/`
- Computes `risk_score` using additive signal rules (amount threshold, structuring window, off-hours, cross-border)
- Sets `status` to `"flagged"` if score ≥ 50, `"approved"` if score < 50
- Writes scored envelope to `shared/processing/<transaction_id>.json` for the settlement reporter
- Deletes source file from `shared/output/` after processing

**Acceptance Criteria:**
- [ ] TXN002 ($25,000 USD) → `risk_score` ≥ 60, `status: "flagged"`
- [ ] TXN003 ($9,999.99 USD) → `risk_score` ≥ 40 (structuring signal)
- [ ] TXN004 (€500, 02:47 UTC, country DE) → `risk_score` ≥ 35 (off-hours + cross-border)
- [ ] TXN005 ($75,000 USD) → `risk_score` = 100 (capped), `status: "flagged"`
- [ ] TXN001 ($1,500 USD, US, daytime) → `risk_score` = 0, `status: "approved"`

---

### Task: Settlement Reporter

**Prompt:**
"You are implementing the SettlementReporter agent for a .NET 10 multi-agent banking pipeline. Read `task-1-specification/specification.md` and `task-1-specification/agents.md` for full context. The settlement reporter reads fraud-scored envelopes from `shared/processing/`, assigns a settlement_status field ('settled' for approved, 'held_for_review' for flagged), and writes the final result envelope to `shared/results/<transaction_id>.json`. This is the last agent before results. Use ILogger<T> for logging. Use decimal for amount fields. Mask account numbers as ACC-**** in all output."

**File to CREATE:** `agents/SettlementReporter.cs`

**Function to CREATE:**
```csharp
Task<SettlementResult> ProcessAsync(string envelopeFilePath, CancellationToken ct = default)
```

**Details:**
- Reads one `<transaction_id>.json` from `shared/processing/` (post-fraud-scoring)
- Assigns `settlement_status`: `"settled"` if `status == "approved"`, `"held_for_review"` if `status == "flagged"`
- Writes final envelope to `shared/results/<transaction_id>.json`
- Deletes source file from `shared/processing/` after writing result

**Acceptance Criteria:**
- [ ] TXN001, TXN003, TXN004, TXN008 → `settlement_status: "settled"` in `shared/results/`
- [ ] TXN002, TXN005 → `settlement_status: "held_for_review"` in `shared/results/`
- [ ] TXN007 (refund, negative, approved) → `settlement_status: "settled"` in `shared/results/`

---

### Task: Integrator / Orchestrator

**Prompt:**
"You are implementing the Integrator for a .NET 10 multi-agent banking pipeline. Read `task-1-specification/specification.md` and `task-1-specification/agents.md` for full context. The integrator is the entry point: it reads `sample-transactions.json`, wraps each record in a JSON envelope (adding message_id as UUID v4, timestamp, source_agent 'integrator', target_agent 'transaction_validator', message_type 'transaction'), writes one envelope per transaction to `shared/input/`, then moves each to `shared/processing/` and starts the agent sequence: TransactionValidator → FraudDetector → SettlementReporter. After all transactions are processed, it reads all result files from `shared/results/`, counts totals (total, validated, rejected, flagged, approved, settled, held_for_review), and writes `shared/results/summary.json`. Use ILogger<T>. Use decimal. Mask account numbers in all output."

**File to CREATE:** `Integrator.cs` (project root or `src/` depending on solution structure)

**Function to CREATE:**
```csharp
Task RunAsync(string transactionsFilePath, CancellationToken ct = default)
```

**Details:**
- Reads `sample-transactions.json` and deserializes to a list of raw transaction objects
- Wraps each in a message envelope (generates `message_id` via `Guid.NewGuid()`)
- Writes envelopes to `shared/input/` then moves to `shared/processing/`
- Calls `TransactionValidator.ProcessAsync` for each file in `shared/processing/`
- Calls `FraudDetector.ProcessAsync` for each file in `shared/output/`
- Calls `SettlementReporter.ProcessAsync` for each file in `shared/processing/` (post-fraud)
- After all results land in `shared/results/`, reads them and writes `summary.json`
- Logs pipeline start, per-transaction outcomes, and pipeline completion at `Information` level

**Acceptance Criteria:**
- [ ] All 8 transactions from `sample-transactions.json` produce a result file in `shared/results/`
- [ ] `shared/results/summary.json` exists and counts sum to 8
- [ ] `shared/input/`, `shared/processing/`, `shared/output/` are empty after a run
- [ ] Pipeline runs to completion with `dotnet run` (or equivalent) and exits with code 0
