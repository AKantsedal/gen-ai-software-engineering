# Implementation Plan — BankingApi Bug Fixes

**Planner:** Bug Planner Agent
**Date:** 2026-05-31
**Source:** `context/bugs/001/research/verified-research.md`

---

## Fix 1: Withdrawal sign error in balance calculation

**File:** `src/BankingApi/Services/TransactionService.cs`
**Line:** 57
**Test command:** `dotnet test tests/BankingApi.Tests/`

**Before:**
```csharp
                balance += t.Amount; // BUG: should be -= (withdrawals incorrectly increase balance)
```

**After:**
```csharp
                balance -= t.Amount;
```

**Rationale:** Withdrawals reduce an account's balance. The `+=` operator was incrementing the balance instead of decrementing it. Replacing with `-=` restores correct behaviour.

---

## Fix 2: Zero-amount transactions bypass validation

**File:** `src/BankingApi/Validators/TransactionValidator.cs`
**Line:** 42
**Test command:** `dotnet test tests/BankingApi.Tests/`

**Before:**
```csharp
        if (amount < 0) // BUG: should be <= 0 (allows zero-amount transactions)
```

**After:**
```csharp
        if (amount <= 0)
```

**Rationale:** The business rule requires a strictly positive amount. The condition `< 0` excluded zero from rejection, allowing `amount = 0` to pass. Changing to `<= 0` correctly rejects zero and negative values.

---

## Fix 3: Remove hardcoded debug bypass endpoint

**File:** `src/BankingApi/Controllers/AccountsController.cs`
**Lines:** 13–22
**Test command:** `dotnet test tests/BankingApi.Tests/`

**Before:**
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

**After:**
```csharp
```
*(remove the constant and the entire Debug action method)*

**Rationale:** Debug endpoints with hardcoded secrets must not exist in production code. Removing the endpoint eliminates the data exposure risk and the hardcoded secret.
