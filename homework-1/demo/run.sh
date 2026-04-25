#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/../src/BankingApi"

cd "$PROJECT_DIR"

echo "Building BankingApi..."
dotnet build --nologo -q

echo "Starting BankingApi on http://localhost:3000"
echo "Swagger UI: http://localhost:3000/swagger"
echo "Press Ctrl+C to stop."
dotnet run --no-build
