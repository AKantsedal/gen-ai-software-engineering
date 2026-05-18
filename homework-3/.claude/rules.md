# Claude Code Rules — Card Data Lookup Service

## General
- Do not add features beyond what the spec asks for
- Prefer editing existing files over creating new ones

## Naming Conventions
- Controllers: `<Entity>Controller.cs` (e.g., `CardLookupController.cs`)
- Repositories: `I<Entity>Repository.cs` + `<Entity>Repository.cs`
- DTOs: `<Action>Request.cs` / `<Action>Response.cs`
- Tests: `<ClassUnderTest>Tests.cs`

## .NET Patterns
- Use constructor injection; never `ServiceLocator` or `new` for dependencies
- All database calls must be `async`/`await`; no `.Result` or `.Wait()`
- Use `ILogger<T>` for logging; never `Console.WriteLine`
- Money and numeric BIN ranges: use `long` or `decimal`, never `float`/`double`

## FinTech Defaults
- Never include a full card number in any string, log, exception message, or test assertion
- Always mask card numbers as `{first4} **** **** {last4}` before logging or returning in error responses
- Validate card number length (13–19 digits) and Luhn check before any database call
- Return `400` for malformed input, `422` for Luhn failure, `404` for unknown BIN — never leak internal details in error bodies

## What to Avoid
- Do not use `var` where the type is not immediately obvious
- Do not swallow exceptions with empty `catch` blocks
- Do not add EF migrations or seed data — database population is out of scope
- Do not use minimal API style; use controller-based Web API only
