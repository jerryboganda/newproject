@echo off
cls
echo ========================================
echo     StreamVault System Startup
echo ========================================
echo.
echo This will start:
echo 1. Mock API Server (Backend)
echo 2. Frontend should already be running
echo.
echo Press any key to continue...
pause >nul
cls

echo [1/3] Starting Mock API Server...
cd /d "%~dp0"
start "StreamVault API" cmd /k "echo StreamVault API Server & node mock-api-5001.js"

echo.
echo [2/3] Waiting for API to start...
timeout /t 3 >nul

echo.
echo [3/3] Checking services...
echo.

echo Checking API...
curl -s http://localhost:5001/api/v1/health >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] API Server is running on http://localhost:5001
) else (
    echo [ERROR] API Server failed to start
)

echo.
echo ========================================
echo         StreamVault is Ready!
echo ========================================
echo.
echo URLs:
echo - Frontend: http://localhost:3000
echo - API: http://localhost:5001
echo - API Health: http://localhost:5001/api/v1/health
echo.
echo Login Credentials:
echo - Email: superadmin@streamvault.app
echo - Password: SuperAdmin123!
echo.
echo Press any key to open the frontend...
pause >nul

start http://localhost:3000

echo.
echo System is running! Keep this window open.
echo Press Ctrl+C in API window to stop the server.
echo.
pause
