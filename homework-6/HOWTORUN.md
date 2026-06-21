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
   dotnet restore
   ```

## Run the Pipeline

3. Execute the pipeline:
   ```bash
   dotnet run
   ```

4. Check results:
   ```bash
   ls shared/results/
   ```
   Expected: `TXN001.json` through `TXN008.json` + `summary.json`

## Run Tests

5. Run tests with coverage:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

## Using Claude Code Skills

6. Run the full pipeline via skill:
   ```
   /run-pipeline
   ```

7. Validate transactions without running the pipeline:
   ```
   /validate-transactions
   ```

## MCP Server

8. The custom MCP server starts automatically when configured in `.claude/mcp.json`. Available tools:
   - `get_transaction_status` — query status of a specific transaction
   - `list_pipeline_results` — summary of all processed transactions
   - `pipeline://summary` — latest pipeline run summary
