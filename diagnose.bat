@echo off
title StreamVault Diagnostic Tool
color 0E

echo ==========================================
echo     STREAMVAULT DIAGNOSTIC TOOL
echo ==========================================
echo.

echo Checking prerequisites...
echo.

:: Check .NET
echo [1/5] Checking .NET SDK...
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ❌ .NET SDK not found
    echo Please install .NET 7 SDK from: https://dotnet.microsoft.com/download
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
    echo ✅ .NET SDK found: %DOTNET_VERSION%
)

:: Check Node.js
echo.
echo [2/5] Checking Node.js...
node --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ❌ Node.js not found
    echo Please install Node.js from: https://nodejs.org/
) else (
    for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
    echo ✅ Node.js found: %NODE_VERSION%
)

:: Check npm
echo.
echo [3/5] Checking npm...
npm --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ❌ npm not found
) else (
    for /f "tokens=*" %%i in ('npm --version') do set NPM_VERSION=%%i
    echo ✅ npm found: %NPM_VERSION%
)

:: Check ports
echo.
echo [4/5] Checking port availability...
netstat -an | findstr ":3000" >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo ⚠️  Port 3000 is in use
    echo Close any applications using port 3000
) else (
    echo ✅ Port 3000 is available
)

netstat -an | findstr ":5000" >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo ⚠️  Port 5000 is in use
    echo Close any applications using port 5000
) else (
    echo ✅ Port 5000 is available
)

:: Check project structure
echo.
echo [5/5] Checking project structure...
if exist "streamvault-backend" (
    echo ✅ Backend folder found
) else (
    echo ❌ Backend folder not found
)

if exist "streamvault-frontend" (
    echo ✅ Frontend folder found
) else (
    echo ❌ Frontend folder not found
)

echo.
echo ==========================================
echo     RECOMMENDATIONS
echo ==========================================
echo.

:: Check for common issues
if not exist "streamvault-backend\src\StreamVault.Api\Program.Fixed.cs" (
    echo ⚠️  Program.Fixed.cs not found - run start-fixed.bat first
)

if not exist "streamvault-frontend\node_modules" (
    echo ⚠️  Frontend dependencies not installed - run npm install
)

echo.
echo QUICK FIXES:
echo -----------
echo 1. If ports are in use: Restart your computer
echo 2. If .NET not found: Install from https://dotnet.microsoft.com/download
echo 3. If Node.js not found: Install from https://nodejs.org/
echo 4. For permission errors: Run Command Prompt as Administrator
echo.
echo READY TO START? Run start-fixed.bat
echo.
pause
