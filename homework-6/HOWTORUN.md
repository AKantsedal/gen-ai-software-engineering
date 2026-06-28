# How to Run

## Prerequisites

- .NET 10 SDK
- Node.js (for context7 MCP)
- Python 3.10+ (for FastMCP server)

## Setup

1. Clone the repository and navigate to homework-6:
   ```bash
   cd homework-6
   ```

2. Restore .NET dependencies:
   ```bash
   dotnet restore task-2-pipeline/pipeline-code/BankingPipeline.csproj
   ```

3. Install Python dependencies (for MCP server):
   ```bash
   pip install fastmcp
   ```

## Run the Pipeline

4. Execute the pipeline (from `homework-6/`):
   ```bash
   dotnet run --project task-2-pipeline/pipeline-code/BankingPipeline.csproj
   ```

5. Check results:
   ```bash
   ls shared/results/
   ```
   Expected: `TXN001.json` through `TXN008.json` + `summary.json`

## Run Tests

6. Run tests with coverage (from `homework-6/`):
   ```bash
   dotnet test task-2-pipeline/tests/BankingPipeline.Tests.csproj --collect:"XPlat Code Coverage"
   ```

## Using Claude Code Skills

7. Run the full pipeline via skill:
   ```
   /run-pipeline
   ```

8. Validate transactions without running the pipeline:
   ```
   /validate-transactions
   ```

## MCP Server

9. The custom MCP server starts automatically when configured in `.claude/mcp.json`. Available tools:
   - `get_transaction_status` — query status of a specific transaction
   - `list_pipeline_results` — summary of all processed transactions
   - `pipeline://summary` — latest pipeline run summary
