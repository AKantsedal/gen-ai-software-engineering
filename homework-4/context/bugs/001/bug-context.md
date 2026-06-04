# Bug Context — BankingApi

## Application Overview

**Project**: BankingApi — a simple in-memory banking REST API built with .NET 8 / ASP.NET Core.

**Source**: `src/BankingApi/`

**Endpoints**:
- `GET  /accounts/{accountId}/balance` — returns current balance for an account
- `GET  /accounts/{accountId}/summary` — returns deposit/withdrawal totals and transaction count
- `POST /transactions` — creates a new transaction (deposit, withdrawal, transfer)
- `GET  /transactions` — lists transactions with optional filters (accountId, type, date range)
- `GET  /transactions/{id}` — retrieves a single transaction by ID

**Key source files**:
| File | Responsibility |
|------|---------------|
| `Controllers/AccountsController.cs` | Account balance and summary endpoints |
| `Controllers/TransactionsController.cs` | Transaction CRUD and filtering |
| `Services/TransactionService.cs` | Business logic: balance calculation, transaction creation |
| `Validators/TransactionValidator.cs` | Input validation rules |
| `Repositories/TransactionRepository.cs` | In-memory storage (ConcurrentDictionary) |
| `Models/Transaction.cs` | Transaction entity and enums |

**Business rules**:
- Account IDs must match format `ACC-XXXXX` (5 alphanumeric chars)
- Valid transaction types: `deposit`, `withdrawal`, `transfer`
- `fromAccount` required for withdrawal and transfer
- `toAccount` required for deposit and transfer
- Amount must be a positive number with at most 2 decimal places
- Supported currencies defined in `Models/Currency.cs`

## Pipeline Instructions

The research agent should inspect the source files listed above and identify any bugs or issues present in the codebase. Findings should be written to `research/codebase-research.md`.
