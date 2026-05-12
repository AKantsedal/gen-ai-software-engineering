# Specification Templates for AI-Assisted Development

## Basic Specification Template

```markdown
# Card details detection service

> Ingest the information from this file, implement the Low-Level Tasks, and generate the code that will satisfy the High and Mid-Level Objectives.

## High-Level Objective
- Service which accepts a card number and returns detailed card information(debit/credit, issuing bank,  country of issuance)

## Mid-Level Objectives
- Accept cart number as input
- Based on the card number, identify whether it is a credit or debit card
- Provide the bank name and country of issuance
- Return the information in JSON format

## Implementation Notes
- Card number is 13 to 19 digits long(inclusive)
- Use regex to validate the card number is digits only
- Use Luhn algorithm to validate the card number is valid
- Implement range based card number lookup
- Accept only the card number, do not accept any other information(like client name, CVC/CVV, PIN, expiry date etc.)
- Return whether the card is credit or debit, the bank name and country of issuance in JSON format,
- Use the following card numbers for testing:
  4111 1111 1111 1111 (valid)
  5555 5555 5555 4444 (valid)
  3782 822463 10005 (valid)
  6011 1111 1111 1117 (valid)
  4111 1111 1111 1112 (invalid due to Luhn algorithm check failure)
- Accept card number via REST API
- Do not store and do not log card numbers in the unobscured form, store only the first four and the last four digits (e.g., 4111 **** **** 1111)
- 99% of the lookup should take under 1 second
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

What prompt would you run to complete this task?
- Read existing solution and make sure the scaffolding is correct
- Create REST API endpoints for card number lookup
- Return the card data in JSON format
- Do not call the database yet, return mocked data and verify that logic is correct

What file do you want to CREATE or UPDATE?
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

### 2. Implement database lookup

What prompt would you run to complete this task?
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

What file do you want to CREATE or UPDATE?
- `Data/CardLookupDbContext.cs` — EF Core `DbContext` with `DbSet<BinEntry>`
- `Data/BinRepository.cs` — repository implementing BIN range lookup
- `Models/BinEntry.cs` — entity mapping to the BIN lookup table

### 3. Implement automated tests

What prompt would you run to complete this task?
- Add xUnit test project to the solution
- Write unit tests for Luhn validation and BIN lookup logic using the five reference card numbers
- Write integration tests for the REST endpoint using `WebApplicationFactory<T>`

What file do you want to CREATE or UPDATE?
- `Tests/CardLookupControllerTests.cs` — integration tests
- `Tests/CardValidatorTests.cs` — unit tests for validation logic


What are details you want to add to drive the code changes?
- Cover all five reference card numbers from the Implementation Notes


#### Testing

```
Create comprehensive tests for the CardLookupController and CardValidator:

Test cases should include:
- Valid card numbers: 4111111111111111, 5555555555554444, 378282246310005, 6011111111111117
- Invalid card number: 4111111111111112 (Luhn check failure)
- Edge cases: 12-digit input (too short), 20-digit input (too long), non-digit characters, empty input
- Error conditions: unknown BIN (404), malformed JSON body (400)
```

#### Documentation

```
Generate documentation for CardLookupController and BinRepository:

Include:
- OpenAPI/Swagger annotations on each endpoint (summary, request/response schema, status codes)
- XML doc comments on public methods describing parameters and return values
- Examples: sample request `{"card_number": "4111111111111111"}` and sample response with masked number
- Usage instructions: how to run the service locally and access Swagger UI at `/swagger`
```