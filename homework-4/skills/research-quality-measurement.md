# Skill: Research Quality Measurement

## Purpose

This skill defines a standard rubric for measuring the quality of codebase bug research.
The **Research Verifier** agent must apply this skill when producing `verified-research.md`.

---

## Quality Levels

| Score | Label | Symbol |
|-------|-------|--------|
| 4 | **Gold** | 🥇 |
| 3 | **Silver** | 🥈 |
| 2 | **Bronze** | 🥉 |
| 1 | **Insufficient** | ❌ |

---

## Level Criteria

### 🥇 Gold (4)
- **All** file:line references resolve to real files and correct line numbers
- Every quoted code snippet matches the source exactly (character-for-character, ignoring only trailing newlines)
- Root cause is clearly explained for every reported bug
- Zero unresolved discrepancies
- Output is immediately actionable by the Bug Planner

### 🥈 Silver (3)
- ≥ 80% of file:line references verified
- Minor snippet drift allowed (whitespace differences, comment-only changes) but no logic differences
- Root cause present for all bugs, may lack detail
- Any discrepancies are minor and documented
- Output is usable by the Bug Planner with low risk

### 🥉 Bronze (2)
- 50–79% of file:line references verified
- Some snippets have logic-level differences from source, or line numbers are off by more than 2
- Root cause present for at least half the bugs
- Notable discrepancies documented
- Bug Planner should re-verify flagged items before acting

### ❌ Insufficient (1)
- Fewer than 50% of references verified, OR
- One or more critical claims are completely unverifiable (file not found, wrong file), OR
- Root cause missing for the majority of bugs
- Bug Planner **must not act** on this research without a full re-run

---

## Scoring Method

1. Count total claims (each file:line reference = 1 claim).
2. Count verified claims (status = `VERIFIED`).
3. Compute pass rate: `verified / total * 100`.
4. Apply the level whose threshold the pass rate meets or exceeds.
5. If any critical claim is `FILE_NOT_FOUND`, cap the level at **Insufficient** regardless of pass rate.

---

## Claim Verification Statuses

| Status | Meaning |
|--------|---------|
| `VERIFIED` | File exists, line number correct, snippet matches source |
| `LINE_MISMATCH` | File exists but the code at the stated line differs from the snippet |
| `SNIPPET_MISMATCH` | Line exists but quoted snippet has logic-level differences |
| `FILE_NOT_FOUND` | Referenced file does not exist in the repository |

---

## Required Output Sections in `verified-research.md`

The verifier **must** produce a file with exactly these top-level sections, in this order:

1. `## Verification Summary` — total claims, verified count, pass rate %, overall quality label
2. `## Verified Claims` — table of all `VERIFIED` references with file:line and snippet confirmation
3. `## Discrepancies Found` — table of all non-`VERIFIED` statuses with expected vs actual
4. `## Research Quality Assessment` — level label, numeric score (1–4), pass rate, reasoning paragraph
5. `## References` — bulleted list of every source file opened during verification
