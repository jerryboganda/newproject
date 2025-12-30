@echo off
cls
echo ========================================
echo     STREAMVAULT - FINAL SOLUTION
echo ========================================
echo.
echo 1. Backend API: http://localhost:5001
echo 2. Frontend:   http://localhost:3000
echo 3. Login: superadmin@streamvault.app / SuperAdmin123!
echo.
echo Starting both services...
echo.

REM Start Backend
start "Backend" cmd /k "cd /d %~dp0 && node mock-api-5001.js"

REM Wait 2 seconds
timeout /t 2 >nul

REM Start Frontend
start "Frontend" cmd /k "cd /d %~dp0\streamvault-admin-dashboard && npm run dev"

REM Wait for frontend to start
timeout /t 5 >nul

REM Open browser
start http://localhost:3000/auth/login

echo.
echo System is running! Check the two terminal windows.
pause
