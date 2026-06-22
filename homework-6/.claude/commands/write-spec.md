---
model: claude-opus-4-6
---

Generate a complete technical specification for the multi-agent banking pipeline following the 5-section template.

## Steps

1. Read `sample-transactions.json` — note every field name, type, and edge case present in the data (invalid currencies, negative amounts, off-hours timestamps, structuring patterns, cross-border metadata).

2. Read `task-1-specification/agents.md` — extract the tech stack, ISO 4217 allowlist, message envelope schema, directory routing table, model assignments, PII masking rules, and testing expectations. All of these must be reflected in the spec.

3. Produce `task-1-specification/specification.md` with exactly these 5 sections in this order:

---

### Section 1 — High-Level Objective
One sentence stating what the pipeline does, what it produces, and what its scope boundary is.

---

### Section 2 — Mid-Level Objectives
4–5 items. Each must be:
- Observable (describes a change in the world, not an implementation detail)
- Testable (a reviewer can verify it passed or failed)
- Mapped to at least one transaction from `sample-transactions.json`

---

### Section 3 — Implementation Notes
Cover all of the following — do not omit any:
- Monetary values: `decimal` only, parsed via `decimal.Parse(value, CultureInfo.InvariantCulture)`
- Currency validation: ISO 4217 allowlist from `agents.md` (19 codes), case-sensitive uppercase
- Logging: `ILogger<T>`, required fields per log entry (timestamp, agent, transaction_id, outcome), log levels
- PII masking: which fields are PII, mask format `ACC-****`, applies to logs AND result files
- File-based message passing: one file per transaction, atomic write (`.tmp` then rename), source file deleted after processing
- Message envelope schema: full JSON with all fields

---

### Section 4 — Context
Two sub-sections:

**Beginning State** — list exactly what exists before the pipeline runs:
- `sample-transactions.json` with 8 records; call out known validation failure (TXN006 — invalid currency `XYZ`), known edge case (TXN007 — negative amount but `transaction_type = "refund"`, so it is **valid** and passes through), and known fraud signals (TXN002, TXN003, TXN004, TXN005)
- `shared/` directory tree
- No agent source files yet

**Ending State** — list exactly what exists after a successful run:
- 8 result files in `shared/results/` (one per transaction)
- `shared/results/summary.json` with pipeline totals
- `shared/input/`, `shared/processing/`, `shared/output/` empty
- Test coverage ≥ 90%

---

### Section 5 — Low-Level Tasks
One entry per agent (4 total: TransactionValidator, FraudDetector, SettlementReporter, Integrator).

Each entry must use exactly this format:
```
Task: [Agent Name]
Prompt: "[The exact prompt you will give Claude Code to implement this agent — complete, self-contained, references specification.md and agents.md]"
File to CREATE: [path, e.g. agents/TransactionValidator.cs]
Function to CREATE: [full C# method signature]
Details: [what the agent checks, transforms, or decides — include threshold values, status transitions, and acceptance criteria as checkboxes]
```

Acceptance criteria checkboxes must reference specific transactions by ID (e.g. `TXN006 → rejected with rejection_reason: "invalid_currency_code"`).

---

## Rules
- Do not generate vague objectives like "the system should be fast" — every statement must be verifiable
- Every Low-Level Task prompt must be self-contained: a developer must be able to copy it and implement the agent without reading anything else
- Do not omit the message envelope schema from Section 3
- Do not omit the directory routing from Section 3 or Section 5
- If `specification.md` already exists, overwrite it completely — do not append
