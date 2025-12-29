@echo off
echo ========================================
echo Starting StreamVault Backend (Simple)
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to project
cd /d "%~dp0streamvault-backend\src\StreamVault.API"

echo Using simple API configuration...
copy Program.Simple.cs Program.cs >nul 2>&1
copy StreamVault.Api.Simple.csproj StreamVault.Api.csproj >nul 2>&1

echo.
echo Building project...
dotnet build StreamVault.Api.csproj --verbosity quiet

echo.
echo Starting API server...
echo.
echo ========================================
echo API is available at:
echo - http://localhost:5000
echo - http://localhost:5000/swagger
echo ========================================
echo.
echo Press Ctrl+C to stop the server
echo.

dotnet run --project StreamVault.Api.csproj --urls="http://localhost:5000"

pause
