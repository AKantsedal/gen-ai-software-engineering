# Security Report — BankingApi Bug 001 Fixes

**Agent:** Security Verifier
**Date:** 2026-05-31
**Fix summary reviewed:** `context/bugs/001/fix-summary.md`

---

## Executive Summary

| Severity | Count |
|----------|-------|
| CRITICAL | 0 |
| HIGH | 1 |
| MEDIUM | 3 |
| LOW | 1 |
| INFO | 1 |

**Overall risk rating:** HIGH

The debug bypass endpoint (hardcoded key, unauthenticated data access) was correctly removed. However, the entire API still operates without any authentication or authorization middleware, meaning every endpoint — including the state-changing `POST /transactions` — is publicly accessible. This is the dominant remaining risk.

---

## Findings

### F-1 — No Authentication or Authorization on Any Endpoint

- **Severity:** HIGH
- **File:** `src/BankingApi/Program.cs` (lines 12–19), `src/BankingApi/Controllers/AccountsController.cs` (lines 9–11), `src/BankingApi/Controllers/TransactionsController.cs` (lines 9–11)
- **Line:** Program.cs:12–19, AccountsController.cs:9–11, TransactionsController.cs:9–11
- **Description:** The application does not register authentication or authorization middleware (`UseAuthentication`, `UseAuthorization`), and no controller or action is decorated with `[Authorize]`. Every endpoint is accessible to anonymous callers. For a banking API this constitutes a significant risk: any network-reachable client can create transactions, view any account's balance and summary, and list all transactions. The removal of the debug endpoint (Fix 3) eliminated one unauthenticated access path, but the underlying absence of auth affects all remaining endpoints equally.
- **Evidence:**
  ```csharp
  // Program.cs — no auth middleware
  var app = builder.Build();
  app.UseSwagger();
  app.UseSwaggerUI();
  app.MapControllers();
  app.Run();
  ```
  ```csharp
  // AccountsController.cs — no [Authorize]
  [ApiController]
  [Route("accounts")]
  public class AccountsController(ITransactionService transactionService) : ControllerBase
  ```
- **Remediation:** Add ASP.NET Core authentication (e.g., JWT Bearer) and authorization middleware in `Program.cs` (`builder.Services.AddAuthentication(...); app.UseAuthentication(); app.UseAuthorization();`). Apply `[Authorize]` to both controllers or globally via a filter/policy. Consider role-based access to restrict transaction creation.

---

### F-2 — No Rate Limiting on Financial Endpoints

- **Severity:** MEDIUM
- **File:** `src/BankingApi/Controllers/TransactionsController.cs`
- **Line:** 20–43
- **Description:** The `POST /transactions` endpoint has no rate limiting. An attacker (or compromised client) can submit an unbounded number of transaction requests in rapid succession, potentially flooding the in-memory store, exhausting server resources, or creating fraudulent transactions faster than they can be detected.
- **Evidence:**
  ```csharp
  [HttpPost]
  [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public IActionResult Create([FromBody] CreateTransactionRequest request)
  ```
- **Remediation:** Add the ASP.NET Core rate-limiting middleware (`builder.Services.AddRateLimiter(...)` / `app.UseRateLimiter()`, available in .NET 7+). Apply a fixed-window or token-bucket policy to `POST /transactions` and other state-changing endpoints. Consider per-client limits keyed on authenticated identity.

---

### F-3 — No Maximum Transaction Amount Validation

- **Severity:** MEDIUM
- **File:** `src/BankingApi/Validators/TransactionValidator.cs`
- **Line:** 40–57
- **Description:** The `ValidateAmount` method (correctly, after Fix 2) rejects amounts ≤ 0 and amounts with more than 2 decimal places, but imposes no upper bound. A caller can submit a transaction with `Amount` up to `decimal.MaxValue` (79,228,162,514,264,337,593,543,950,335.00). In a banking context, the absence of a ceiling enables unrealistically large deposits or withdrawals that could distort balances or be used for abuse.
- **Evidence:**
  ```csharp
  private static void ValidateAmount(decimal amount, List<ValidationDetail> errors)
  {
      if (amount <= 0)
      {
          errors.Add(new ValidationDetail
          {
              Field = "amount",
              Message = "Amount must be a positive number."
          });
      }
      else if (decimal.Round(amount, 2) != amount)
      // ... no upper-bound check
  ```
- **Remediation:** Add a maximum amount check (e.g., `amount > 1_000_000`) with a corresponding validation error. The specific ceiling should be determined by business requirements.

---

### F-4 — No Overdraft Protection on Withdrawals

- **Severity:** MEDIUM
- **File:** `src/BankingApi/Services/TransactionService.cs`
- **Line:** 16–32
- **Description:** The `Create` method records a withdrawal or transfer without first verifying that the source account has sufficient balance. Combined with the absence of any transactional locking, concurrent withdrawal requests can each pass independently and drive a balance negative. While Fix 1 corrected the sign error so that withdrawals now correctly reduce the balance, there is still no guard that prevents the balance from going below zero.
- **Evidence:**
  ```csharp
  public Transaction Create(string fromAccount, string toAccount, decimal amount, Currency currency, TransactionType type)
  {
      var transaction = new Transaction
      {
          // ... fields assigned ...
          Status = TransactionStatus.Completed
      };
      _repository.Add(transaction);
      return transaction;
  }
  ```
- **Remediation:** Before persisting a withdrawal or transfer, compute the current balance of `fromAccount` and reject the request if `balance - amount < 0`. Use a lock or concurrency-safe pattern (e.g., optimistic concurrency, or a `lock` on account ID) to prevent race conditions between the balance check and the write.

---

### F-5 — Swagger UI Exposed Unconditionally

- **Severity:** LOW
- **File:** `src/BankingApi/Program.cs`
- **Line:** 14–15
- **Description:** Swagger UI and the OpenAPI spec endpoint are enabled for all environments, not just Development. In a production deployment this exposes full API documentation and an interactive testing interface to any visitor, aiding reconnaissance.
- **Evidence:**
  ```csharp
  app.UseSwagger();
  app.UseSwaggerUI();
  ```
- **Remediation:** Gate Swagger behind an environment check:
  ```csharp
  if (app.Environment.IsDevelopment())
  {
      app.UseSwagger();
      app.UseSwaggerUI();
  }
  ```

---

### F-6 — Transaction ID Path Parameter Not Format-Validated

- **Severity:** INFO
- **File:** `src/BankingApi/Controllers/TransactionsController.cs`
- **Line:** 111–123
- **Description:** The `GET /transactions/{id}` endpoint trims the `id` parameter but does not validate that it conforms to the expected GUID format. Any arbitrary string is passed through to `_transactionService.GetById()`. The practical impact is minimal (a non-matching ID simply returns 404), but it is inconsistent with the strict format validation applied to `accountId` elsewhere and could mask fuzzing attempts in logs.
- **Evidence:**
  ```csharp
  [HttpGet("{id}")]
  public IActionResult GetById(string id)
  {
      id = id.Trim();
      var transaction = _transactionService.GetById(id);
  ```
- **Remediation:** Add a GUID format validation check or a route constraint (`[HttpGet("{id:guid}")]`) to reject malformed IDs at the routing level.

---

## Scope

**Files reviewed (in scope — changed by Bug Fixer):**
- `src/BankingApi/Services/TransactionService.cs`
- `src/BankingApi/Validators/TransactionValidator.cs`
- `src/BankingApi/Controllers/AccountsController.cs`

**Additional files read for context (not in scope for changes, but necessary to assess auth, middleware, and dependency posture):**
- `src/BankingApi/Program.cs`
- `src/BankingApi/Controllers/TransactionsController.cs`
- `src/BankingApi/BankingApi.csproj`
- `src/BankingApi/Repositories/TransactionRepository.cs`
- `src/BankingApi/Repositories/ITransactionRepository.cs`
- `src/BankingApi/Models/Transaction.cs`
- `src/BankingApi/Models/Currency.cs`
- `src/BankingApi/Models/Dtos/CreateTransactionRequest.cs`
- `src/BankingApi/Extensions/ServiceCollectionExtensions.cs`

**Out of scope:** Test projects, CI/CD configuration, infrastructure/deployment files, files not identified in the fix summary.

---

## References

- `context/bugs/001/fix-summary.md`
- `context/bugs/001/research/verified-research.md`
- `src/BankingApi/Services/TransactionService.cs`
- `src/BankingApi/Validators/TransactionValidator.cs`
- `src/BankingApi/Controllers/AccountsController.cs`
- `src/BankingApi/Controllers/TransactionsController.cs`
- `src/BankingApi/Program.cs`
- `src/BankingApi/BankingApi.csproj`
- `src/BankingApi/Repositories/TransactionRepository.cs`
- `src/BankingApi/Repositories/ITransactionRepository.cs`
- `src/BankingApi/Models/Transaction.cs`
- `src/BankingApi/Models/Currency.cs`
- `src/BankingApi/Models/Dtos/CreateTransactionRequest.cs`
- `src/BankingApi/Extensions/ServiceCollectionExtensions.cs`
