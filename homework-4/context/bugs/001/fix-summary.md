# Fix Summary — BankingApi Bug Fixes

**Agent:** Bug Fixer
**Date:** 2026-05-31
**Plan:** `context/bugs/001/implementation-plan.md`

---

## Changes Made

| Fix # | File | Line | Before (summary) | After (summary) | Test Result |
|-------|------|------|------------------|-----------------|-------------|
| 1 | `src/BankingApi/Services/TransactionService.cs` | 57 | `balance += t.Amount; // BUG: should be -=` | `balance -= t.Amount;` | PASSED |
| 2 | `src/BankingApi/Validators/TransactionValidator.cs` | 42 | `if (amount < 0) // BUG: should be <= 0` | `if (amount <= 0)` | PASSED |
| 3 | `src/BankingApi/Controllers/AccountsController.cs` | 13–22 | `DebugBypassKey` constant + `Debug` action method | *(removed entirely)* | PASSED |

> **Note on test results:** The test project (`BankingApi.Tests`) compiled successfully after each fix but contains no test methods yet. `dotnet test` reported "No test is available" — i.e., build passed, zero tests failed. Each fix is recorded as PASSED on that basis.

---

## Overall Status

`COMPLETE` — All 3 fixes applied and the project builds without errors after each change.

---

## Manual Verification

**Fix 1 — Withdrawal sign error:**
1. `POST /transactions`  body: `{"type":"deposit","toAccount":"ACC-AAAAA","amount":100,"currency":"USD"}`
2. `POST /transactions`  body: `{"type":"withdrawal","fromAccount":"ACC-AAAAA","amount":30,"currency":"USD"}`
3. `GET  /accounts/ACC-AAAAA/balance`  → expect `{"balance": 70.00, ...}`
   - Before fix: balance would have been 130.00 (withdrawal was adding instead of subtracting)

**Fix 2 — Zero-amount validation:**
1. `POST /transactions`  body: `{"type":"deposit","toAccount":"ACC-AAAAA","amount":0,"currency":"USD"}`
2. Expect `400 Bad Request` with `{"details":[{"field":"amount","message":"Amount must be a positive number."}]}`
   - Before fix: a zero-amount deposit was accepted (200 OK)

**Fix 3 — Debug bypass endpoint removed:**
1. `GET  /accounts/debug?key=debug-bypass-2024`
2. Expect `404 Not Found` (route no longer exists)
   - Before fix: this returned all transactions without any authentication

---

## References

- **Files modified:**
  - `src/BankingApi/Services/TransactionService.cs`
  - `src/BankingApi/Validators/TransactionValidator.cs`
  - `src/BankingApi/Controllers/AccountsController.cs`
- **Implementation plan:** `context/bugs/001/implementation-plan.md`
