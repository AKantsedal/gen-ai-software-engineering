@echo off
cd /d "%~dp0..\src\BankingApi"
echo Building BankingApi...
dotnet build --nologo
if errorlevel 1 (
    echo Build failed.
    pause
    exit /b 1
)
echo Starting BankingApi on http://localhost:3000
echo Swagger UI: http://localhost:3000/swagger
echo Press Ctrl+C to stop.
dotnet run --no-build --project . --urls http://localhost:3000