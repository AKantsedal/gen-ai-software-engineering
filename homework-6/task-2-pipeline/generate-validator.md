Generate the TransactionValidator agent for the multi-agent banking pipeline.

## Pre-flight

1. Read `task-1-specification/specification.md` — Section 5, "Task: Transaction Validator" for exact logic, acceptance criteria, and expected outcomes.
2. Read `task-1-specification/agents.md` — ISO 4217 allowlist, directory routing, PII masking, code style.
3. Read `sample-transactions.json` — understand all edge cases: TXN006 (invalid currency XYZ), TXN007 (negative refund — valid).
4. Read existing `Models/MessageEnvelope.cs` and `Helpers/FileHelper.cs` — use the shared models and utilities already created by the scaffolding step.

## File to create

### `agents/TransactionValidator.cs`

**Constructor:** Takes `ILoggerFactory` and `JsonSerializerOptions`. Creates `ILogger<TransactionValidator>` from the factory.

**Method:** `async Task<string> ProcessAsync(string envelopeFilePath, CancellationToken ct = default)`

Returns the status string: `"validated"` or `"rejected"`.

### Validation logic

1. Read the `MessageEnvelope` from the given file path using `FileHelper.ReadJsonAsync`
2. Check all required fields in `envelope.Data` are present and non-empty:
   - `transaction_id`, `amount`, `currency`, `transaction_type`, `source_account`, `destination_account`, `timestamp`
   - If any missing → reject with `rejection_reason: "missing_required_field"`
3. Parse `amount` as `decimal` using `decimal.Parse(value, CultureInfo.InvariantCulture)`
   - If parse fails → reject with `rejection_reason: "invalid_amount"`
4. Check amount sign:
   - If `amount < 0` AND `transaction_type != "refund"` → reject with `rejection_reason: "negative_amount"`
   - If `amount < 0` AND `transaction_type == "refund"` → **valid** (TXN007 passes)
5. Validate `currency` against the 19-code ISO 4217 allowlist (case-sensitive, uppercase):
   `USD, EUR, GBP, JPY, CAD, AUD, CHF, CNY, SEK, NOK, DKK, SGD, HKD, NZD, MXN, BRL, INR, ZAR, KRW`
   - If not in list → reject with `rejection_reason: "invalid_currency_code"` (TXN006)

### Output routing

- **Valid transaction:**
  - Update envelope: `source_agent = "transaction_validator"`, `target_agent = "fraud_detector"`, `status = "validated"`, new `timestamp`
  - Write to `shared/output/<transaction_id>.json` using `FileHelper.WriteJsonAtomicAsync`
  - Delete source file from `shared/processing/`
  - Log at `Information` level: `"Transaction {TransactionId} validated"` (mask accounts)

- **Rejected transaction:**
  - Update envelope: `source_agent = "transaction_validator"`, `target_agent = "none"`, `status = "rejected"`, set `rejection_reason`, new `timestamp`
  - Write to `shared/results/<transaction_id>.json` using `FileHelper.WriteJsonAtomicAsync`
  - Delete source file from `shared/processing/`
  - Log at `Warning` level: `"Transaction {TransactionId} rejected: {RejectionReason}"` (mask accounts)

### Acceptance criteria

- [ ] TXN001 ($1,500 USD) → `shared/output/` with `status: "validated"`
- [ ] TXN002 ($25,000 USD) → `shared/output/` with `status: "validated"`
- [ ] TXN003 ($9,999.99 USD) → `shared/output/` with `status: "validated"`
- [ ] TXN004 (€500 EUR) → `shared/output/` with `status: "validated"`
- [ ] TXN005 ($75,000 USD) → `shared/output/` with `status: "validated"`
- [ ] TXN006 ($200 XYZ) → `shared/results/` with `status: "rejected"`, `rejection_reason: "invalid_currency_code"`
- [ ] TXN007 (£-100 GBP, refund) → `shared/output/` with `status: "validated"` — negative allowed for refunds
- [ ] TXN008 ($3,200 USD) → `shared/output/` with `status: "validated"`

### Rules
- Use `decimal` for all amount parsing and comparison — never `float` or `double`
- Use `ILogger<TransactionValidator>` — never `Console.WriteLine`
- Mask account numbers as `ACC-****` in ALL log output using `PiiMasker.MaskAccount()`
- Atomic file writes via `FileHelper.WriteJsonAtomicAsync`
- Delete source file from `shared/processing/` after writing output
- async/await throughout — no `.Result` or `.Wait()`
