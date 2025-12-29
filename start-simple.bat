@echo off
title StreamVault Simple Startup

echo ========================================
echo Starting StreamVault (Simple Mode)
echo ========================================
echo.

echo Step 1: Preparing backend...
cd /d "%~dp0streamvault-backend\src\StreamVault.Api"

echo Backing up original Program.cs...
if exist Program.cs (
    copy Program.cs Program.cs.backup > nul
)

echo Using simplified Program.cs...
copy Program.Simple.cs Program.cs > nul

echo.
echo Step 2: Installing required packages...
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package BCrypt.Net-Next
dotnet add package Swashbuckle.AspNetCore

echo.
echo Step 3: Starting Backend API...
echo This will take 10-15 seconds...
start "StreamVault Backend" cmd /k "echo Starting Backend... && dotnet run"

echo.
echo Step 4: Waiting for backend to start...
timeout /t 15 /nobreak > nul

echo.
echo Step 5: Starting Frontend...
cd /d "%~dp0streamvault-frontend"
start "StreamVault Frontend" cmd /k "echo Starting Frontend... && npm run dev"

echo.
echo ========================================
echo READY! Services are starting...
echo.
echo URLs:
echo - Frontend: http://localhost:3000
echo - Backend API: http://localhost:5000
echo - Swagger Docs: http://localhost:5000/swagger
echo.
echo Login Credentials:
echo Email: admin@streamvault.com
echo Password: Admin123!
echo ========================================
echo.
echo Please wait 30 seconds for everything to load
echo.
echo To stop: Close the Backend and Frontend windows
echo.
pause
