---
model: claude-sonnet-4-6
---

Run the multi-agent banking pipeline end-to-end.

## Steps

1. Check that `sample-transactions.json` exists at the homework-6 root. If missing, stop and report the error.

2. Clear all files from `shared/` directories:
   ```bash
   rm -f shared/input/*.json shared/processing/*.json shared/output/*.json shared/results/*.json
   ```

3. Run the pipeline:
   ```bash
   dotnet run --project task-2-pipeline/pipeline-code/BankingPipeline.csproj
   ```

4. Show a summary of results by reading `shared/results/summary.json` and displaying:
   - Total transactions processed
   - Validated / Rejected / Flagged / Approved / Settled / Held for review counts

5. Report any transactions that were rejected and why:
   - Read each `shared/results/TXN*.json` file
   - For any with `status: "rejected"`, show the `transaction_id` and `rejection_reason`
   - Show a table of all transaction outcomes (transaction_id, status, risk_score, settlement_status)
