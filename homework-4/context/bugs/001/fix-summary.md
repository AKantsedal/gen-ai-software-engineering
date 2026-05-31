# Fix Summary — BankingApi Bug Fixes

**Agent:** Bug Fixer
**Date:** 2026-05-31
**Plan:** `context/bugs/001/implementation-plan.md`

---

## Changes Made

| Fix # | File | Line | Before (summary) | After (summary) | Test Result |
|-------|------|------|------------------|-----------------|-------------|
| 1 | `src/BankingApi/Services/TransactionService.cs` | 57 | `balance += t.Amount;` (withdrawal branch) | `balance -= t.Amount;` | PASSED (30/30) |
| 2 | `src/BankingApi/Validators/TransactionValidator.cs` | 42 | `if (amount < 0)` | `if (amount <= 0)` | PASSED (30/30) |
| 3 | `src/BankingApi/Controllers/AccountsController.cs` | 13–21 | `DebugBypassKey` constant + `Debug` action method | *(removed entirely)* | PASSED (30/30) |

> **Note on Fix 1 test run:** After applying Fix 1, 2 tests were still failing
> (`ValidateAmount_WithZeroAmount_HasError`, `Validate_WithZeroAmountInWithdrawal_HasError`).
> These are pre-existing failures caused by the unfixed zero-amount bug (Fix 2), not by Fix 1.
> Fix 2 resolved them; all 30 tests pass after Fix 2 and continue passing after Fix 3.

---

## Overall Status

`COMPLETE` — All 3 fixes applied and all 30 tests pass.

---

## Manual Verification

**Fix 1 — Withdrawal balance calculation:**
1. `POST /transactions`  body: `{"type":"deposit","toAccount":"ACC-AAAAA","amount":100,"currency":"USD"}` → 201
2. `POST /transactions`  body: `{"type":"withdrawal","fromAccount":"ACC-AAAAA","amount":30,"currency":"USD"}` → 201
3. `GET  /accounts/ACC-AAAAA/balance`  → expect `{"balance": 70.00, ...}`
   - Before fix: returned `{"balance": 130.00}` because withdrawals were added instead of subtracted.

**Fix 2 — Zero-amount transaction rejection:**
1. `POST /transactions`  body: `{"type":"deposit","toAccount":"ACC-AAAAA","amount":0,"currency":"USD"}`
2. Expect `400 Bad Request` with `{"details":[{"field":"amount","message":"Amount must be a positive number."}]}`
   - Before fix: a zero-amount transaction was accepted (201) because `< 0` did not catch zero.

**Fix 3 — Debug bypass endpoint removed:**
1. `GET  /accounts/debug?key=debug-bypass-2024`
2. Expect `404 Not Found` (route no longer exists)
   - Before fix: returned all transactions without authentication using the hardcoded key.

---

## References

- **Files modified:**
  - `src/BankingApi/Services/TransactionService.cs`
  - `src/BankingApi/Validators/TransactionValidator.cs`
  - `src/BankingApi/Controllers/AccountsController.cs`
- **Implementation plan:** `context/bugs/001/implementation-plan.md`
