# How to Run the Application

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Install .NET 8 SDK

**macOS** (using Homebrew):
```bash
brew install dotnet@8
```

**macOS** (manual installer):
Download from https://dotnet.microsoft.com/download/dotnet/8.0 and run the `.pkg` installer.

**Windows** (using winget):
```powershell
winget install Microsoft.DotNet.SDK.8
```

**Windows** (manual installer):
Download from https://dotnet.microsoft.com/download/dotnet/8.0 and run the `.exe` installer.

### Verify installation

```bash
dotnet --version
```

Expected output: `8.x.x`

## Build

```bash
cd homework-1/src/BankingApi
dotnet build
```

## Run

```bash
cd homework-1/src/BankingApi
dotnet run
```

The API starts at **http://localhost:3000**.

Swagger UI is available at **http://localhost:3000/swagger**.

## Test with curl

### Create transactions

```bash
# Create a deposit
curl -X POST http://localhost:3000/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "fromAccount": "",
    "toAccount": "ACC-12345",
    "amount": 500,
    "currency": "USD",
    "type": "deposit"
  }'

# Create a transfer
curl -X POST http://localhost:3000/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "fromAccount": "ACC-12345",
    "toAccount": "ACC-67890",
    "amount": 100.50,
    "currency": "USD",
    "type": "transfer"
  }'

# Create a withdrawal
curl -X POST http://localhost:3000/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "fromAccount": "ACC-12345",
    "toAccount": "",
    "amount": 50,
    "currency": "USD",
    "type": "withdrawal"
  }'
```

### Query transactions

```bash
# List all transactions
curl http://localhost:3000/transactions

# Get a transaction by ID (replace {id} with actual ID)
curl http://localhost:3000/transactions/{id}

# Filter by account
curl "http://localhost:3000/transactions?accountId=ACC-12345"

# Filter by type
curl "http://localhost:3000/transactions?type=transfer"

# Filter by date range
curl "http://localhost:3000/transactions?from=2026-04-01&to=2026-04-30"

# Combine filters
curl "http://localhost:3000/transactions?accountId=ACC-12345&type=deposit"
```

### Account endpoints

```bash
# Get account balance
curl http://localhost:3000/accounts/ACC-12345/balance

# Get account summary
curl http://localhost:3000/accounts/ACC-12345/summary
```

### Validation examples

```bash
# Invalid amount (negative)
curl -X POST http://localhost:3000/transactions \
  -H "Content-Type: application/json" \
  -d '{"fromAccount":"ACC-12345","toAccount":"ACC-67890","amount":-10,"currency":"USD","type":"transfer"}'

# Invalid account format
curl -X POST http://localhost:3000/transactions \
  -H "Content-Type: application/json" \
  -d '{"fromAccount":"12345","toAccount":"ACC-67890","amount":10,"currency":"USD","type":"transfer"}'

# Invalid currency
curl -X POST http://localhost:3000/transactions \
  -H "Content-Type: application/json" \
  -d '{"fromAccount":"ACC-12345","toAccount":"ACC-67890","amount":10,"currency":"XYZ","type":"transfer"}'
```
