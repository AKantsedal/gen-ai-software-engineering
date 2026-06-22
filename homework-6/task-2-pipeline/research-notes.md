# Research Notes — context7 Queries

> Document at least 2 context7 queries made during code generation (Task 2 / Agent 2).

## Query 1: System.Text.Json serialization in .NET

- **Search**: "System.Text.Json JsonNamingPolicy SnakeCaseLower .NET serialization JsonPropertyName"
- **context7 library ID**: `/dotnet/system-text-json`
- **Key insight applied**: `JsonNamingPolicy.SnakeCaseLower` (available since .NET 8) converts PascalCase C# properties to snake_case JSON keys automatically. However, `[JsonPropertyName]` attributes override the naming policy, so we use explicit attributes on all model properties to ensure consistent snake_case output regardless of the global policy setting. Combined with `PropertyNameCaseInsensitive = true`, this handles round-trip serialization with the `sample-transactions.json` format. Applied in `Models/MessageEnvelope.cs`, `Models/RawTransaction.cs`, and `Program.cs` options.

## Query 2: decimal handling in C# / .NET

- **Search**: "C# decimal.Parse CultureInfo.InvariantCulture monetary arithmetic best practices"
- **context7 library ID**: `/dotnet/system-decimal`
- **Key insight applied**: Always use `CultureInfo.InvariantCulture` when parsing monetary strings to avoid locale-dependent decimal separator issues (e.g., German locale uses `,` instead of `.`). The `decimal` type in C# provides 28-29 significant digits of precision, making it suitable for financial calculations without floating-point rounding errors that `double` would introduce. Applied `decimal.Parse(value, CultureInfo.InvariantCulture)` in `TransactionValidator.cs` for amount validation and `FraudDetector.cs` for risk score threshold comparisons ($10,000 and $9,000–$9,999.99 structuring detection).
