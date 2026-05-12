# Homework 3 — Specification-Driven Design

**Student:** Artur Kantsedal
**Task:** Design a specification package for a card data lookup service (read-only BIN lookup) in a regulated FinTech environment. No implementation — documents only.

---

## Rationale

The spec is structured in layers (high-level → mid-level → implementation notes → low-level tasks) so an AI agent or engineering team can execute top-down without guessing intent.

**Performance targets:** P99 ≤ 1 s and P95 ≤ 500 ms were chosen because card-present and digital wallet flows expect sub-second BIN resolution. These are labeled as assumed targets since actual SLAs depend on infrastructure and load.

**Verification depth:** Each low-level task has 2 acceptance criteria checkboxes — enough to confirm the task is done without over-specifying implementation details. Five reference card numbers (four valid, one Luhn-fail) serve as a shared test fixture across unit and integration tests.

**Edge cases as a first-class section:** Rather than scattering failure modes across notes, a dedicated `Edge Cases and Failure Modes` section makes them reviewable and auditable in one place.

---

## Industry Best Practices

| Practice | Where it appears |
|----------|-----------------|
| PAN masking (never log full card numbers) | `card-data-lookup-specification.md` → Implementation Notes; `agents.md` → Domain Rules; `.claude/rules.md` → FinTech Defaults |
| Input validation before processing | `card-data-lookup-specification.md` → Implementation Notes (Luhn + regex); `.claude/rules.md` → FinTech Defaults (validate before DB call) |
| Explicit error codes per failure type (400/422/404) | `.claude/rules.md` → FinTech Defaults; `card-data-lookup-specification.md` → Edge Cases |
| Read-only service boundary (no writes) | `agents.md` → Domain Rules; `.claude/rules.md` → General |
| Acceptance criteria on every task | `card-data-lookup-specification.md` → Low-Level Tasks (Tasks 1–3) |
| Latency budgets with percentile targets | `card-data-lookup-specification.md` → Implementation Notes (P99/P95) |
| Async-only data access | `agents.md` → Code Style; `.claude/rules.md` → .NET Patterns |
