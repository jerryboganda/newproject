@echo off
title StreamVault System Check
color 0B

echo ==========================================
echo     STREAMVAULT SYSTEM CHECK
echo ==========================================
echo.

echo [1] Checking .NET...
dotnet --version 2>nul && echo     OK: .NET installed || echo     MISSING: Install .NET from dotnet.microsoft.com

echo.
echo [2] Checking Node.js...
node --version 2>nul && echo     OK: Node.js installed || echo     MISSING: Install Node.js from nodejs.org

echo.
echo [3] Checking npm...
npm.cmd --version 2>nul && echo     OK: npm installed || echo     MISSING: npm not found

echo.
echo [4] Checking ports...
netstat -an | findstr :3000 >nul 2>&1 && echo     BUSY: Port 3000 in use || echo     FREE: Port 3000 available
netstat -an | findstr :5000 >nul 2>&1 && echo     BUSY: Port 5000 in use || echo     FREE: Port 5000 available

echo.
echo [5] Checking folders...
if exist streamvault-backend (echo     FOUND: Backend folder) else (echo     MISSING: Backend folder)
if exist streamvault-frontend (echo     FOUND: Frontend folder) else (echo     MISSING: Frontend folder)

echo.
echo ==========================================
echo.
echo If everything is OK, run START-PRODUCTION.bat
echo.
pause
