@echo off
echo ========================================
echo Starting StreamVault Backend
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to project
cd /d "%~dp0streamvault-backend\src\StreamVault.API"

echo Using minimal API configuration...
copy Program.Minimal.cs Program.cs >nul 2>&1

echo.
echo Building project...
dotnet build StreamVault.Api.csproj --no-restore --verbosity quiet

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed! Trying to restore packages...
    dotnet restore
    echo.
    echo Building again...
    dotnet build StreamVault.Api.csproj --verbosity quiet
)

echo.
echo Starting API server...
echo.
echo ========================================
echo API will be available at:
echo - http://localhost:5000
echo - http://localhost:5000/swagger
echo ========================================
echo.
echo Press Ctrl+C to stop the server
echo.

dotnet run --project StreamVault.Api.csproj

pause
