# Agent Guidelines — Card Data Lookup Service

## Tech Stack
- .NET 8, ASP.NET Core Web API (controller-based, no minimal API)
- PostgreSQL via Entity Framework Core
- xUnit + WebApplicationFactory for tests
- Swashbuckle for OpenAPI/Swagger

## AI Model
- AI model to use must be explicitly specified per each task

## Domain Rules
- This is a **read-only** BIN lookup service — never generate write, update, or delete operations
- Treat card numbers as sensitive PAN data at all times
- Never log or store a full card number; use masked form only (`4111 **** **** 1111`)
- Strip spaces and dashes from input before validation — do not reject formatted card numbers outright

## Code Style
- Async/await throughout; no blocking calls
- Repository pattern for all database access
- Inject dependencies via constructor; register services in `Program.cs`
- Use `ILogger<T>` for all logging

## Security and Compliance
- Never expose raw card numbers in logs, responses, or error messages
- Return `{"error": "unknown_bin"}` with 404 for unrecognised BINs — do not reveal lookup internals
- Validate input with FluentValidation or Data Annotations before any processing

## Testing Expectations
- Every endpoint and validator must have a corresponding xUnit test
- Cover all five reference card numbers (four valid, one Luhn-fail)
- Integration tests use `WebApplicationFactory<T>` with an in-memory or test PostgreSQL database

## Edge Case Handling
- Unknown BIN → 404, log masked number only
- Luhn failure → 422, do not query the database
- Input outside 13–19 digits → 400 before any further processing
