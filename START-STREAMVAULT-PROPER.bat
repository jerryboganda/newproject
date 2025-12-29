@echo off
echo ========================================
echo Starting StreamVault Backend
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to project
cd /d "%~dp0streamvault-backend\src\StreamVault.API"

REM Use minimal configuration
echo Setting up minimal backend...
copy Program.MinimalBackend.cs Program.cs >nul 2>&1

REM Move full controllers temporarily
if exist "Controllers.bak" rmdir /S /Q "Controllers.bak" >nul 2>&1
if exist "Controllers" move "Controllers" "Controllers.bak" >nul 2>&1

REM Create new Controllers folder
mkdir Controllers >nul 2>&1
copy "..\..\..\MinimalControllers.cs" "Controllers\MinimalControllers.cs" >nul 2>&1

echo.
echo Building project...
dotnet build StreamVault.Api.csproj --verbosity quiet

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build successful!
    echo ========================================
    echo.
    echo Starting API server...
    echo.
    echo API will be available at:
    echo - http://localhost:5000
    echo - http://localhost:5000/api/health
    echo - http://localhost:5000/swagger
    echo ========================================
    echo.
    echo Press Ctrl+C to stop the server
    echo.
    dotnet run --project StreamVault.Api.csproj --urls="http://localhost:5000"
) else (
    echo.
    echo Build failed. Check errors above.
    pause
)

pause
