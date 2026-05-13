# Card Details Detection Service

> Ingest the information from this file, implement the Low-Level Tasks, and generate the code that will satisfy the High and Mid-Level Objectives.

## High-Level Objective
- Service which accepts a card number and returns detailed card information (debit/credit, issuing bank, country of issuance)

## Mid-Level Objectives
- Accept card number as input
- Based on the card number, identify whether it is a credit or debit card
- Provide the bank name and country of issuance
- Return the information in JSON format

## Non-Functional Requirements & Policy

### Security
- All endpoints must be served over TLS only (HTTPS); reject or redirect plain HTTP requests
- Never log, store, or return full card numbers — mask as `{first4} **** **** {last4}` at the point of entry, before any further processing
- Accept only the card number field; reject requests that include additional cardholder data (name, CVC/CVV, PIN, expiry) to minimise PCI-DSS scope
- Sanitise all input before use — no raw user input in SQL, logs, or error responses 

### Audit & Logging
- Every request must produce a structured log entry containing: timestamp, masked card number (if available), response status code, and latency in ms
- Failed lookups (404, 422, 400) must be logged at `Warning` level; 5xx errors at `Error` level
- Logs must not contain full card numbers, stack traces exposed to clients, or any PII beyond masked PAN
- Log retention: assume 90-day minimum for compliance review (configurable per environment)

### Reliability & Availability
- **Uptime target**: 99.9 % monthly
- Database connection pool: configure with a max pool size and a connection timeout (≤ 3 s) to prevent cascading failures
- If the database is unreachable, return 503 within 2 s — do not let requests hang
- The service must start and pass health checks even if the BIN table is empty (graceful degradation: lookups return 404 until data is loaded)

### Performance & Latency
- **P99 lookup latency ≤ 1 s; P95 ≤ 500 ms** 

### Data Handling & Compliance
- This is a **read-only** service — no endpoints may create, update, or delete BIN data
- BIN lookup data is owned by the third-party vendor; the service must not re-expose raw vendor data beyond the defined response schema (`type`, `bank`, `country`, `masked`)
- All monetary and BIN range values use `long` or `decimal` — never `float`/`double`
- Idempotency: lookups are inherently idempotent (GET-semantics over POST); identical requests always return the same result for the same BIN data version

## Implementation Notes
- Card number is 13 to 19 digits long (inclusive)
- Use regex to validate the card number is digits only
- Use Luhn algorithm to validate the card number is valid
- Implement range based card number lookup
- Accept only the card number, do not accept any other information (like client name, CVC/CVV, PIN, expiry date etc.)
- Return whether the card is credit or debit, the bank name and country of issuance in JSON format
- Use the following card numbers for testing:
  - 4111 1111 1111 1111 (valid)
  - 5555 5555 5555 4444 (valid)
  - 3782 822463 10005 (valid)
  - 6011 1111 1111 1117 (valid)
  - 4111 1111 1111 1112 (invalid due to Luhn algorithm check failure)
- Accept card number via REST API
- Do not store and do not log card numbers in the unobscured form, store only the first four and the last four digits (e.g., 4111 **** **** 1111)
- P99 lookup latency ≤ 1 s; P95 ≤ 500 ms
- Implement unit tests for the service, with both valid and invalid card numbers

## Prerequisites
- BIN lookup data should be provided by the third party data vendor

## Context

### Beginning context
- BIN lookup data is provided by the third party data vendor
- Empty solution is already scaffolded
- Database is available and ready to use
- Populating the database with BIN lookup data is not in the scope of this project

### Ending context
- REST endpoints accepting card numbers and returning card data are implemented
- Database lookup is implemented
- Automated tests suite is implemented

## Low-Level Tasks

### 1. Implement REST API endpoints

**Prompt:**
- Read existing solution and make sure the scaffolding is correct
- Create REST API endpoints for card number lookup
- Return the card data in JSON format
- Do not call the database yet, return mocked data and verify that logic is correct

**Files to create or update:**
- Create REST API endpoints in `Controllers/CardLookupController.cs`
- Use ASP.NET Core Web API (controller-based, do not use minimal API)
- Use Swashbuckle for OpenAPI/Swagger documentation
- Use ASP.NET Core's built-in dependency injection container
- Use ASP.NET Core middleware pipeline for request logging
- Use ASP.NET Core exception handling middleware (`UseExceptionHandler`) for centralized error handling
- Use ASP.NET Core model validation with Data Annotations or FluentValidation for input validation
- Use `IActionResult` / `Results<T>` for typed response generation
- Use `xUnit` with `WebApplicationFactory<T>` for unit and integration testing
- Use Swashbuckle's `AddEndpointsApiExplorer` + `AddSwaggerGen` for API documentation

**Acceptance Criteria:**
- [ ] `POST /api/v1/card/lookup` returns 200 with `type`, `bank`, `country`, and `masked` fields for all four valid test cards
- [ ] Response for an invalid card returns 422; no unmasked card number appears in logs

### 2. Implement database lookup

**Prompt:**
- Add data access layer (DAL) to the project
- Use Entity Framework Core for database access
- Use `DbContext` for database context
- Use `DbSet<T>` for entity set
- Use `OnModelCreating` for model configuration
- Use `ILogger<T>` for logging
- Use `IConfiguration` for configuration
- Use `IServiceProvider` for dependency injection
- Use `async`/`await` for asynchronous operations
- Use `TryAddSingleton` for singleton service registration
- Implement BIN data lookup using the provided BIN lookup data

**Files to create or update:**
- `Data/CardLookupDbContext.cs` — EF Core `DbContext` with `DbSet<BinEntry>`
- `Data/BinRepository.cs` — repository implementing BIN range lookup
- `Models/BinEntry.cs` — entity mapping to the BIN lookup table

**Acceptance Criteria:**
- [ ] BIN range query returns correct `type`, `bank`, and `country` for all four valid test card prefixes
- [ ] Unknown BIN returns `null`; caller receives 404

### 3. Implement automated tests

**Prompt:**
- Add xUnit test project to the solution
- Write unit tests for Luhn validation and BIN lookup logic using the five reference card numbers
- Write integration tests for the REST endpoint using `WebApplicationFactory<T>`

**Files to create or update:**
- `Tests/CardLookupControllerTests.cs` — integration tests
- `Tests/CardValidatorTests.cs` — unit tests for validation logic

**Details:**
- Cover all five reference card numbers from the Implementation Notes

**Acceptance Criteria:**
- [ ] All unit and integration tests pass; test run reports 0 failures
- [ ] Test suite covers Luhn validation, BIN lookup, masked output, and the unknown-BIN 404 case

**Test cases:**

| # | Scenario | Input / Setup | Expected Result | HTTP Status |
|---|----------|---------------|-----------------|-------------|
| 1 | Valid Visa card | `4111111111111111` | Returns `type`, `bank`, `country`, `masked` | 200 |
| 2 | Valid Mastercard | `5555555555554444` | Returns `type`, `bank`, `country`, `masked` | 200 |
| 3 | Valid Amex | `378282246310005` | Returns `type`, `bank`, `country`, `masked` | 200 |
| 4 | Valid Discover | `6011111111111117` | Returns `type`, `bank`, `country`, `masked` | 200 |
| 5 | Invalid Luhn | `4111111111111112` | `{"error": "invalid_card_number"}` | 422 |
| 6 | Too short (12 digits) | `411111111111` | `{"error": "invalid_length"}` | 400 |
| 7 | Too long (20 digits) | `41111111111111111111` | `{"error": "invalid_length"}` | 400 |
| 8 | Non-digit characters | `4111-ABCD-1111-1111` | `{"error": "invalid_characters"}` | 400 |
| 9 | Empty input | `""` or missing field | `{"error": "card_number_required"}` | 400 |
| 10 | Unknown BIN (valid Luhn, BIN not in DB) | Valid card number with unrecognised BIN | `{"error": "unknown_bin"}` | 404 |
| 11 | Malformed JSON body | `{card_number: }` | `{"error": "malformed_request"}` | 400 |
| 12 | Card number with spaces | `4111 1111 1111 1111` | Strips spaces, processes normally | 200 |
| 13 | Card number with dashes | `4111-1111-1111-1111` | Strips dashes, processes normally | 200 |
| 14 | Oversized request body (> 1 KB) | Body exceeding 1 KB | Reject before parsing | 413 / 400 |
| 15 | Oversized card_number string (> 50 chars) | `card_number` field > 50 characters | `{"error": "invalid_length"}` | 400 |
| 16 | Database unavailable / query timeout | DB down or query > 2 s | `{"error": "service_unavailable"}` | 503 |
| 17 | Stale BIN data (empty BIN table) | Service started with zero BIN rows | Health check passes; lookups return 404 | 404 |
| 18 | Concurrent identical requests | Two simultaneous requests with same card | Both served independently, same result | 200 / 404 |
| 19 | Rate limit exceeded (> 100 req/s) | Single client sends > 100 req/s | `{"error": "rate_limit_exceeded"}` | 429 |
| 20 | BIN at range boundary | Card number at edge of a BIN range | Range-based lookup matches correctly | 200 |
| 21 | Overlapping BIN ranges in vendor data | Duplicate/overlapping ranges loaded | Most specific (narrowest) range wins | 200 |
| 22 | Request over plain HTTP | Non-TLS request | Reject or redirect to HTTPS | 301 / 403 |

**Documentation:**
- OpenAPI/Swagger annotations on each endpoint (summary, request/response schema, status codes)
- XML doc comments on public methods describing parameters and return values
- Examples: sample request `{"card_number": "4111111111111111"}` and sample response with masked number
- Usage instructions: how to run the service locally and access Swagger UI at `/swagger`
