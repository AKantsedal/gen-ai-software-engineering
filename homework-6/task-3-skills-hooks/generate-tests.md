---
model: claude-opus-4-6
---

Generate a comprehensive xUnit test suite for the multi-agent banking pipeline.

You are Agent 3 — the unit-test generation agent. Your job is to produce tests that achieve ≥ 80% coverage (target ≥ 90%).

## Pre-flight

1. Read `task-1-specification/specification.md` — acceptance criteria for each agent define what to test.
2. Read `task-1-specification/agents.md` — testing expectations, FIRST principles, isolation requirements.
3. Read all source files in `task-2-pipeline/pipeline-code/` — understand every class, method, and branch to cover.
4. Read `sample-transactions.json` — use real transaction shapes as test data.

## Output location

All generated test files go inside `task-2-pipeline/tests/`.

## Files to create

### 1. `task-2-pipeline/tests/BankingPipeline.Tests.csproj`

xUnit test project targeting `net10.0`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector" Version="6.*">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../pipeline-code/BankingPipeline.csproj" />
  </ItemGroup>
</Project>
```

Adjust package versions to whatever `dotnet` resolves.

### 2. `task-2-pipeline/tests/TransactionValidatorTests.cs`

Test class for TransactionValidator. Use a temporary directory for all shared/ paths — never read/write the real `shared/` directories.

**Tests to include:**
- Valid transaction (TXN001-like) → status "validated", file written to output dir
- Invalid currency (TXN006-like, currency "XYZ") → status "rejected", rejection_reason "invalid_currency_code", file in results dir
- Negative amount non-refund → status "rejected", rejection_reason "negative_amount"
- Negative amount refund (TXN007-like) → status "validated" (negative allowed for refunds)
- Missing required field (empty transaction_id) → status "rejected", rejection_reason "missing_required_field"
- Invalid amount (non-numeric string) → status "rejected", rejection_reason "invalid_amount"
- All 19 valid ISO 4217 currencies accepted
- Source file deleted from processing dir after processing

### 3. `task-2-pipeline/tests/FraudDetectorTests.cs`

Test class for FraudDetector.

**Tests to include:**
- Normal transaction ($1,500 USD, US, daytime) → risk_score 0, status "approved"
- High-value ($25,000) → risk_score 60, status "flagged"
- Structuring pattern ($9,999.99) → risk_score 40, status "approved"
- Off-hours (02:47 UTC) → adds 20 to score
- Cross-border (country "DE") → adds 15 to score
- Combined signals: off-hours + cross-border ($500) → risk_score 35, status "approved"
- High-value + off-hours → risk_score 80, status "flagged"
- Score capped at 100 (high-value + off-hours + cross-border = 95, not over 100)
- Source file deleted from output dir after processing

### 4. `task-2-pipeline/tests/SettlementReporterTests.cs`

Test class for SettlementReporter.

**Tests to include:**
- Approved transaction → settlement_status "settled"
- Flagged transaction → settlement_status "held_for_review"
- Result file written to results dir
- Source file deleted from processing dir after processing
- Unexpected status throws InvalidOperationException

### 5. `task-2-pipeline/tests/IntegrationTests.cs`

Full pipeline integration test.

**Tests to include:**
- Load all 8 transactions from a copy of sample-transactions.json in a temp directory
- Run full pipeline end-to-end via Integrator.RunAsync
- Assert 8 result files exist in results dir
- Assert summary.json exists with correct totals (total=8, rejected=1, flagged=2, approved=5, settled=5, held=2)
- Assert shared/input, shared/processing, shared/output are empty after run
- Assert TXN006 is rejected with "invalid_currency_code"
- Assert TXN002 and TXN005 are "held_for_review"

### 6. `task-2-pipeline/tests/Helpers/FileHelperTests.cs`

**Tests to include:**
- WriteJsonAtomicAsync creates the file with valid JSON content
- WriteJsonAtomicAsync does not leave .tmp file behind
- ReadJsonAsync deserializes correctly
- ReadJsonAsync throws on missing file

### 7. `task-2-pipeline/tests/Helpers/PiiMaskerTests.cs`

**Tests to include:**
- MaskAccount returns "ACC-****" for any input
- MaskAccount handles empty string
- MaskAccount handles null-like edge cases

## Test isolation rules

- Every test must create its own temp directory: `Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())`
- Create `shared/{input,processing,output,results}` subdirs inside the temp directory
- Clean up temp directories in test disposal/teardown
- Tests must NOT touch the real `shared/` directory at the homework-6 root
- For integration tests, copy `sample-transactions.json` into the temp directory

## Rules

- All tests must pass with `dotnet test`
- Target ≥ 90% line coverage
- Use `ILoggerFactory` from `LoggerFactory.Create(b => b.AddConsole())` or a NullLoggerFactory for quiet tests
- Use the same `JsonSerializerOptions` as production code (SnakeCaseLower, case-insensitive, indented)
- Follow FIRST principles: Fast, Independent, Repeatable, Self-validating, Timely
