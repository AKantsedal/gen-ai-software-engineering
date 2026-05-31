# Codebase Research — BankingApi

**Researcher:** Bug Researcher Agent
**Date:** 2026-05-31
**Source directory:** `src/BankingApi/`

---

## Summary

Three issues were found in the BankingApi codebase: two logic bugs and one security vulnerability.

---

## Finding 1 — Withdrawal sign error in balance calculation

**File:** `src/BankingApi/Services/TransactionService.cs`
**Line:** 57

**Description:**
In `GetBalance()`, the withdrawal branch uses `+=` instead of `-=`, causing withdrawals to *increase* the account balance instead of decreasing it. A user who withdraws funds will see their balance go up.

**Code snippet (line 57):**
```csharp
                balance += t.Amount; // BUG: should be -= (withdrawals incorrectly increase balance)
```

**Root cause:**
Wrong arithmetic operator. The deposit branch on line 53 correctly uses `+=`; the withdrawal branch should use `-=` but uses `+=` instead.

**Impact:** CRITICAL — incorrect balance is returned for any account with withdrawals.

---

## Finding 2 — Zero-amount transactions bypass validation

**File:** `src/BankingApi/Validators/TransactionValidator.cs`
**Line:** 42

**Description:**
In `ValidateAmount()`, the guard condition is `amount < 0` instead of `amount <= 0`. This allows a transaction with `amount = 0` to pass validation and be persisted, which violates the business rule "Amount must be a positive number."

**Code snippet (line 42):**
```csharp
        if (amount < 0) // BUG: should be <= 0 (allows zero-amount transactions)
```

**Root cause:**
Off-by-one in the comparison operator — `<` excludes zero from the rejection set.

**Impact:** MEDIUM — zero-value transactions pollute transaction history and skew account summaries.

---

## Finding 3 — Hardcoded debug bypass key in AccountsController

**File:** `src/BankingApi/Controllers/AccountsController.cs`
**Lines:** 13–22

**Description:**
A `GET /accounts/debug` endpoint is present with a hardcoded secret key `"debug-bypass-2024"`. Any caller who knows the key receives the full list of all transactions without authentication. The key is embedded in plain text in the source code.

**Code snippet (lines 13–22):**
```csharp
    // SECURITY: hardcoded debug key — exposes all transaction data without authentication
    private const string DebugBypassKey = "debug-bypass-2024";

    [HttpGet("debug")]
    public IActionResult Debug([FromQuery] string key)
    {
        if (key == DebugBypassKey)
            return Ok(transactionService.GetAll());
        return Unauthorized();
    }
```

**Root cause:**
Debug endpoint left in production code with a hardcoded secret. Hardcoded secrets are trivially discoverable via source code access or decompilation.

**Impact:** HIGH — full data exposure; secret cannot be rotated without a code deploy.

---

## Files Inspected

- `src/BankingApi/Services/TransactionService.cs`
- `src/BankingApi/Validators/TransactionValidator.cs`
- `src/BankingApi/Controllers/AccountsController.cs`
- `src/BankingApi/Controllers/TransactionsController.cs`
- `src/BankingApi/Repositories/TransactionRepository.cs`
- `src/BankingApi/Models/Transaction.cs`
