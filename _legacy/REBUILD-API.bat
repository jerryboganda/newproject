@echo off
echo ========================================
echo Rebuilding StreamVault API
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to project
cd /d "%~dp0StreamVaultAPI"

REM Kill existing process
echo Stopping API...
taskkill /F /IM StreamVaultAPI.exe 2>nul

REM Clean and rebuild
echo.
echo Cleaning project...
dotnet clean

echo.
echo Restoring packages...
dotnet restore

echo.
echo Building project...
dotnet build

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build successful!
    echo ========================================
    echo.
    echo Starting API...
    echo.
    echo API will be available at:
    echo - http://localhost:5000
    echo - http://localhost:5000/api/v1/health
    echo - http://localhost:5000/swagger
    echo ========================================
    echo.
    dotnet run --urls="http://localhost:5000"
) else (
    echo.
    echo Build failed.
    pause
)

pause
