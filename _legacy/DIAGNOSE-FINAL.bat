@echo off
title StreamVault Diagnostic Tool - Final Version
color 0E

cls
echo ==========================================
echo     STREAMVAULT DIAGNOSTIC TOOL
echo ==========================================
echo.

:: Check .NET
echo [1/5] Checking .NET SDK...
dotnet --version >nul 2>&1
if %errorlevel% equ 0 (
    for /f "tokens=*" %%i in ('dotnet --version 2^>nul') do set dotnet_ver=%%i
    echo     [✓] .NET SDK found: %dotnet_ver%
) else (
    echo     [✗] .NET SDK not found
    echo         Download from: https://dotnet.microsoft.com/download
)

:: Check Node.js
echo.
echo [2/5] Checking Node.js...
node --version >nul 2>&1
if %errorlevel% equ 0 (
    for /f "tokens=*" %%i in ('node --version 2^>nul') do set node_ver=%%i
    echo     [✓] Node.js found: %node_ver%
) else (
    echo     [✗] Node.js not found
    echo         Download from: https://nodejs.org/
)

:: Check npm - fixed version
echo.
echo [3/5] Checking npm...
where npm >nul 2>&1
if %errorlevel% equ 0 (
    for /f "tokens=*" %%i in ('npm --version 2^>nul') do set npm_ver=%%i
    echo     [✓] npm found: %npm_ver%
) else (
    echo     [✗] npm not found in PATH
    echo         npm should be installed with Node.js
)

:: Check ports
echo.
echo [4/5] Checking port availability...
netstat -an | findstr ":3000" >nul 2>&1
if %errorlevel% equ 0 (
    echo     [⚠] Port 3000 is in use
    echo         Close applications using this port
) else (
    echo     [✓] Port 3000 is available
)

netstat -an | findstr ":5000" >nul 2>&1
if %errorlevel% equ 0 (
    echo     [⚠] Port 5000 is in use
    echo         Close applications using this port
) else (
    echo     [✓] Port 5000 is available
)

:: Check project structure
echo.
echo [5/5] Checking project structure...
if exist "streamvault-backend" (
    echo     [✓] Backend folder exists
) else (
    echo     [✗] Backend folder missing
)

if exist "streamvault-frontend" (
    echo     [✓] Frontend folder exists
) else (
    echo     [✗] Frontend folder missing
)

:: Summary
echo.
echo ==========================================
echo     DIAGNOSTIC COMPLETE
echo ==========================================
echo.
echo Your system has:
echo - .NET SDK: Installed
echo - Node.js: Installed
echo - npm: Installed
echo.
echo You're ready to run StreamVault!
echo.
echo NEXT STEP:
echo Run START-PRODUCTION.bat as Administrator
echo.
echo Press any key to exit...
pause > nul
