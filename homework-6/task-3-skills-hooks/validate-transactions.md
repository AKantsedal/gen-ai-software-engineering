---
model: claude-haiku-4-5-20251001
---

Validate all transactions in sample-transactions.json without running the full pipeline.

## Steps

1. Read `sample-transactions.json` from the homework-6 root.

2. For each transaction, check:
   - All required fields are present and non-empty: `transaction_id`, `amount`, `currency`, `transaction_type`, `source_account`, `destination_account`, `timestamp`
   - `amount` is a valid decimal number
   - Negative amounts are only allowed when `transaction_type` is `"refund"`
   - `currency` is in the ISO 4217 allowlist: USD, EUR, GBP, JPY, CAD, AUD, CHF, CNY, SEK, NOK, DKK, SGD, HKD, NZD, MXN, BRL, INR, ZAR, KRW

3. Report results:
   - Total count of transactions
   - Valid count
   - Invalid count
   - For each invalid transaction: the `transaction_id` and the reason for rejection

4. Show a table of all results:

   | Transaction ID | Amount | Currency | Type | Valid | Rejection Reason |
   |----------------|--------|----------|------|-------|-----------------|
   | TXN001         | 1500.00| USD      | transfer | Yes | — |
   | ...            | ...    | ...      | ...  | ...   | ... |

Do NOT run the pipeline, do NOT write any files to `shared/` directories. This is a read-only dry-run validation.
