---
model: claude-opus-4-6
---

Generate the .NET 10 project scaffolding, shared models, helper utilities, and the Integrator orchestrator for the multi-agent banking pipeline.

## Pre-flight

1. Read `task-1-specification/specification.md` — authoritative source for all rules, thresholds, envelope schema, directory routing.
2. Read `task-1-specification/agents.md` — tech stack, code style, PII masking, testing expectations.
3. Read `sample-transactions.json` — understand all 8 transactions and their fields.
4. Use **MCP context7** to look up `System.Text.Json` serialization patterns for .NET (JsonSerializer, JsonSerializerOptions, snake_case naming, PropertyNameCaseInsensitive). Document the query and key insight for `research-notes.md`.

## Output location

All generated files go inside `task-2-pipeline/pipeline-code/`. Use this as the project root for all paths below.

## Files to create

### 1. `task-2-pipeline/pipeline-code/BankingPipeline.csproj`

.NET 10 console app:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.0-preview.*" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="10.0.0-preview.*" />
  </ItemGroup>
</Project>
```

Adjust package versions to whatever `dotnet` resolves. The key is: target `net10.0`, add logging packages.

### 2. `task-2-pipeline/pipeline-code/Program.cs`

Minimal entry point:
- Create `ILoggerFactory` using `LoggerFactory.Create(builder => builder.AddConsole())`
- Create shared `JsonSerializerOptions` with `PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower`, `PropertyNameCaseInsensitive = true`, `WriteIndented = true`
- Instantiate `Integrator` with the logger factory and JSON options
- Call `await integrator.RunAsync("sample-transactions.json")`
- Return exit code 0 on success, 1 on failure
- Wrap in try/catch, log any unhandled exception

### 3. `task-2-pipeline/pipeline-code/Models/RawTransaction.cs`

POCO matching `sample-transactions.json` shape. Use `[JsonPropertyName("snake_case")]` on every property:
- `TransactionId` (string)
- `Timestamp` (string — keep as raw ISO 8601 string)
- `SourceAccount` (string)
- `DestinationAccount` (string)
- `Amount` (string — keep as string, parse to decimal in agents)
- `Currency` (string)
- `TransactionType` (string)
- `Description` (string)
- `Metadata` — nested object with `Channel` (string) and `Country` (string)

### 4. `task-2-pipeline/pipeline-code/Models/MessageEnvelope.cs`

The inter-agent JSON envelope defined in the specification:
- `MessageId` (string) — UUID v4
- `Timestamp` (string) — ISO 8601 of when envelope was created
- `SourceAgent` (string)
- `TargetAgent` (string)
- `MessageType` (string) — always "transaction"
- `Data` — nested object containing:
  - `TransactionId`, `Amount` (string), `Currency`, `TransactionType`, `Status`
  - `RiskScore` (int?) — present from fraud_detector onward
  - `RejectionReason` (string?) — present only when rejected
  - `SettlementStatus` (string?) — present only from settlement reporter
  - `SourceAccount`, `DestinationAccount`, `Description`
  - `Timestamp` (string) — original transaction timestamp
  - `Metadata` — nested with `Channel`, `Country`

All properties must have `[JsonPropertyName("snake_case")]` attributes.

### 5. `task-2-pipeline/pipeline-code/Models/PipelineSummary.cs`

For `shared/results/summary.json`:
- `Total` (int)
- `Validated` (int)
- `Rejected` (int)
- `Flagged` (int)
- `Approved` (int)
- `Settled` (int)
- `HeldForReview` (int)
- `PipelineRunTimestamp` (string)

### 6. `task-2-pipeline/pipeline-code/Helpers/FileHelper.cs`

Static utility class:
- `WriteJsonAtomicAsync<T>(string filePath, T data, JsonSerializerOptions options, CancellationToken ct)` — writes to `filePath + ".tmp"` then `File.Move(tmpPath, filePath, overwrite: true)`
- `ReadJsonAsync<T>(string filePath, JsonSerializerOptions options, CancellationToken ct)` — reads and deserializes a JSON file

### 7. `task-2-pipeline/pipeline-code/Helpers/PiiMasker.cs`

Static utility class:
- `MaskAccount(string account)` — returns `"ACC-****"` regardless of input
- Every agent must call this before any log call or file write that includes an account number

### 8. `task-2-pipeline/pipeline-code/Integrator.cs`

Orchestrator class. Constructor takes `ILoggerFactory` and `JsonSerializerOptions`.

**Method:** `async Task RunAsync(string transactionsFilePath, CancellationToken ct = default)`

Logic:
1. Ensure `shared/{input,processing,output,results}` directories exist at the homework-6 root (NOT inside pipeline-code)
2. Clear any leftover files from previous runs in all four directories
3. Read and deserialize `sample-transactions.json` (at homework-6 root) into `List<RawTransaction>`
4. For each transaction, create a `MessageEnvelope`:
   - `message_id` = `Guid.NewGuid().ToString()`
   - `timestamp` = `DateTime.UtcNow.ToString("o")`
   - `source_agent` = `"integrator"`, `target_agent` = `"transaction_validator"`
   - `message_type` = `"transaction"`
   - Copy all transaction fields into the `Data` block
5. Write each envelope to `shared/input/<transaction_id>.json` using atomic write
6. Move each file from `shared/input/` to `shared/processing/`
7. Instantiate `TransactionValidator` (from `agents/`), run `ProcessAsync` on each file in `shared/processing/`
8. Instantiate `FraudDetector`, run `ProcessAsync` on each file in `shared/output/`
9. Instantiate `SettlementReporter`, run `ProcessAsync` on each file in `shared/processing/`
10. Read all result files from `shared/results/` (exclude `summary.json`), count statuses, write `shared/results/summary.json`
11. Log pipeline start, per-transaction outcomes, and completion using `ILogger<Integrator>`
12. Mask all account numbers in log output

The Integrator creates agent instances by passing them the `ILoggerFactory` and `JsonSerializerOptions`.

## Rules
- All code must compile with `dotnet build`
- Use `decimal` for money — never `float` or `double`
- Use `ILogger<T>` — never `Console.WriteLine` in production code
- Mask accounts as `ACC-****` in all logs and output
- async/await throughout — no `.Result` or `.Wait()`
- Constructor injection for all dependencies
- Do not create interfaces for agents yet (keep it simple for now)
