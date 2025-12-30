@echo off
echo ========================================
echo Restarting StreamVault API
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Kill existing process
echo Stopping existing API...
taskkill /F /IM StreamVaultAPI.exe 2>nul
timeout /t 2 >nul

REM Navigate to project
cd /d "%~dp0StreamVaultAPI"

echo.
echo Starting API...
echo.
echo ========================================
echo StreamVault API is starting...
echo ========================================
echo.
echo API will be available at:
echo - http://localhost:5000
echo - http://localhost:5000/api/health
echo - http://localhost:5000/swagger
echo ========================================
echo.

dotnet run --urls="http://localhost:5000"

pause
