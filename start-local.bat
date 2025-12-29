@echo off
echo Starting StreamVault Local Development Environment
echo ================================================

echo.
echo Step 1: Starting Backend API...
cd /d "%~dp0streamvault-backend\src\StreamVault.Api"
start "StreamVault Backend" cmd /k "dotnet run"

echo.
echo Step 2: Starting Frontend...
cd /d "%~dp0streamvault-frontend"
start "StreamVault Frontend" cmd /k "npm run dev"

echo.
echo Waiting for services to start...
timeout /t 15 /nobreak > nul

echo.
echo Services should be available at:
echo Frontend: http://localhost:3000
echo Backend: http://localhost:5000
echo.
echo Press any key to exit...
pause > nul
