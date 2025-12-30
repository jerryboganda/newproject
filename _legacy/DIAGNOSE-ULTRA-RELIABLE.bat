@echo off
setlocal enabledelayedexpansion
title StreamVault Diagnostic Tool - Ultra Reliable
color 0E

:: Prevent window from closing
if "%1"=="" (
    cmd /k "%~f0" RUN
    exit /b
)

:RUN
cls
echo ==========================================
echo     STREAMVAULT DIAGNOSTIC TOOL
echo ==========================================
echo.
echo Checking system requirements for StreamVault
echo.

:: Check .NET
echo Checking .NET SDK...
dotnet --version >nul 2>&1
if !errorlevel! equ 0 (
    for /f "tokens=*" %%i in ('dotnet --version 2^>nul') do set dotnet_version=%%i
    echo [✓] .NET SDK found: !dotnet_version!
) else (
    echo [✗] .NET SDK not found
    echo     Download from: https://dotnet.microsoft.com/download
)

:: Check Node.js
echo.
echo Checking Node.js...
node --version >nul 2>&1
if !errorlevel! equ 0 (
    for /f "tokens=*" %%i in ('node --version 2^>nul') do set node_version=%%i
    echo [✓] Node.js found: !node_version!
) else (
    echo [✗] Node.js not found
    echo     Download from: https://nodejs.org/
)

:: Check npm with timeout
echo.
echo Checking npm...
echo [This may take a moment...]
(
    timeout /t 5 /nobreak >nul
    npm --version >nul 2>&1
    if !errorlevel! equ 0 (
        for /f "tokens=*" %%i in ('npm --version 2^>nul') do set npm_version=%%i
        echo [✓] npm found: !npm_version!
    ) else (
        echo [✗] npm not found or not responding
        echo     Try: npm --version manually
    )
)

:: Quick port check
echo.
echo Checking ports...
netstat -an | findstr ":3000" >nul 2>&1 && echo [✗] Port 3000 in use || echo [✓] Port 3000 available
netstat -an | findstr ":5000" >nul 2>&1 && echo [✗] Port 5000 in use || echo [✓] Port 5000 available

:: Check folders
echo.
echo Checking project structure...
if exist "streamvault-backend" (echo [✓] Backend folder exists) else (echo [✗] Backend folder missing)
if exist "streamvault-frontend" (echo [✓] Frontend folder exists) else (echo [✗] Frontend folder missing)

:: Results
echo.
echo ==========================================
echo     DIAGNOSTIC COMPLETE
echo ==========================================
echo.
echo If all checks passed, run: START-PRODUCTION.bat
echo.
echo Press any key to exit...
pause > nul
exit /b 0
