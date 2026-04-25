# Banking Transactions API

> **Student Name**: Andrii Kantsedal
> **Date Submitted**: 2026-04-25
> **AI Tools Used**: Claude Code (Claude Sonnet 4.6 / Opus 4.6)

---

## Project Overview

A REST API for banking transactions built with ASP.NET Core 8. The API supports creating transactions, querying them with filters, checking account balances, and viewing account summaries. All data is stored in-memory.

### Features Implemented

**Task 1 -- Core API**
- `POST /transactions` -- create a transaction (deposit, withdrawal, transfer)
- `GET /transactions` -- list all transactions
- `GET /transactions/:id` -- get transaction by ID
- `GET /accounts/:accountId/balance` -- get account balance

**Task 2 -- Validation**
- Amount must be positive with at most 2 decimal places
- Account numbers must match `ACC-XXXXX` format (alphanumeric)
- Currency must be a valid ISO 4217 code (USD, EUR, GBP, etc.)
- Structured error responses with field-level details

**Task 3 -- Filtering**
- Filter by `?accountId=ACC-12345`
- Filter by `?type=transfer`
- Filter by date range `?from=2026-01-01&to=2026-12-31`
- All filters are combinable
- Query parameters are validated before filtering

**Task 4 -- Account Summary (Option A)**
- `GET /accounts/:accountId/summary` -- returns total deposits, total withdrawals, transaction count, and most recent transaction date

### Architecture

```
src/BankingApi/
  Controllers/        -- API endpoints (TransactionsController, AccountsController)
  Models/             -- Domain model (Transaction, enums)
  Models/Dtos/        -- Request/response DTOs
  Validators/         -- Input validation (TransactionValidator)
  Services/           -- Business logic (TransactionService)
  Repositories/       -- In-memory storage (ConcurrentDictionary)
  Extensions/         -- DI and Swagger configuration
  Program.cs          -- Application entry point
```

### AI Tools Usage

All code was generated with **Claude Code** using an iterative approach:
1. Initial project scaffolding and core endpoints
2. Added validation layer with structured error responses
3. Added transaction filtering with input sanitization
4. Added account summary endpoint

Claude Code was used for code generation, testing, and documentation. Each feature was verified with curl commands after implementation.

### Development Log (AI-Assisted Session)

This section documents the step-by-step development process using Claude Code.

**Step 1: Project Review**
- Read existing codebase and TASKS.md requirements
- Identified that Task 1 (core API) was already implemented
- Added .md files to the .sln so they appear in Rider

**Step 2: Task 2 -- Transaction Validation**
- Planned validation rules matching TASKS.md spec
- Created `Validators/TransactionValidator.cs` with static validation
- Created `Models/Dtos/ValidationErrorResponse.cs` for structured errors
- Modified `TransactionsController.Create` to call validator
- Tested: negative amounts, bad decimals, invalid accounts, invalid currency, multiple errors

**Step 3: Task 3 -- Transaction Filtering**
- Planned filtering approach (LINQ in controller)
- Added query params to `GET /transactions`: `accountId`, `type`, `from`, `to`
- Added input validation for all query parameters
- Sanitized `GetById` (trimmed input, removed user input from error messages)
- Tested: each filter individually, combined filters, invalid inputs

**Step 4: Task 4 -- Account Summary (Option A)**
- Created `Models/Dtos/AccountSummaryResponse.cs`
- Added `GetSummary` to service interface and implementation
- Added `GET /accounts/:accountId/summary` endpoint with validation
- Also added validation to existing `GET /accounts/:accountId/balance`
- Tested: account with transactions, empty account, invalid account ID

**Step 5: Documentation**
- Updated `README.md` with project overview, features, architecture
- Updated `HOWTORUN.md` with examples for all endpoints

---

<div align="center">

*This project was completed as part of the AI-Assisted Development course.*

</div>
