"""Custom FastMCP server for querying the banking pipeline results."""

import json
import os
from pathlib import Path

from fastmcp import FastMCP

mcp = FastMCP("Banking Pipeline Status")

RESULTS_DIR = Path(__file__).parent.parent / "shared" / "results"


@mcp.tool()
def get_transaction_status(transaction_id: str) -> str:
    """Get the current status of a specific transaction from shared/results/.

    Args:
        transaction_id: The transaction ID to look up (e.g. "TXN001").

    Returns:
        JSON string with the transaction's status, risk score, and settlement status.
    """
    result_file = RESULTS_DIR / f"{transaction_id}.json"

    if not result_file.exists():
        return json.dumps({"error": f"Transaction {transaction_id} not found in results."})

    with open(result_file) as f:
        envelope = json.load(f)

    data = envelope.get("data", {})
    return json.dumps(
        {
            "transaction_id": data.get("transaction_id"),
            "status": data.get("status"),
            "risk_score": data.get("risk_score"),
            "rejection_reason": data.get("rejection_reason"),
            "settlement_status": data.get("settlement_status"),
            "amount": data.get("amount"),
            "currency": data.get("currency"),
        },
        indent=2,
    )


@mcp.tool()
def list_pipeline_results() -> str:
    """Return a summary of all processed transactions from shared/results/.

    Returns:
        JSON string with a list of all transaction outcomes.
    """
    if not RESULTS_DIR.exists():
        return json.dumps({"error": "Results directory not found. Run the pipeline first."})

    results = []
    for file in sorted(RESULTS_DIR.glob("TXN*.json")):
        with open(file) as f:
            envelope = json.load(f)
        data = envelope.get("data", {})
        results.append(
            {
                "transaction_id": data.get("transaction_id"),
                "status": data.get("status"),
                "risk_score": data.get("risk_score"),
                "rejection_reason": data.get("rejection_reason"),
                "settlement_status": data.get("settlement_status"),
                "amount": data.get("amount"),
                "currency": data.get("currency"),
            }
        )

    return json.dumps({"total": len(results), "transactions": results}, indent=2)


@mcp.resource("pipeline://summary")
def pipeline_summary() -> str:
    """Return the latest pipeline run summary as text."""
    summary_file = RESULTS_DIR / "summary.json"

    if not summary_file.exists():
        return "No pipeline summary found. Run the pipeline first."

    with open(summary_file) as f:
        summary = json.load(f)

    return (
        f"Pipeline Run Summary ({summary.get('pipeline_run_timestamp', 'unknown')})\n"
        f"{'=' * 50}\n"
        f"Total processed:  {summary.get('total', 0)}\n"
        f"Validated:        {summary.get('validated', 0)}\n"
        f"Rejected:         {summary.get('rejected', 0)}\n"
        f"Flagged:          {summary.get('flagged', 0)}\n"
        f"Approved:         {summary.get('approved', 0)}\n"
        f"Settled:          {summary.get('settled', 0)}\n"
        f"Held for review:  {summary.get('held_for_review', 0)}\n"
    )


if __name__ == "__main__":
    mcp.run()
