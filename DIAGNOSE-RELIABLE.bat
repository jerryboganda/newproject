@echo off
setlocal enabledelayedexpansion
title StreamVault Diagnostic Tool - Reliable Version
color 0E

:: Prevent window from closing on error
if "%1"=="" (
    cmd /k "%~f0" RUN
    exit /b
)

:: Main diagnostic routine
:RUN
cls
echo ==========================================
echo     STREAMVAULT DIAGNOSTIC TOOL
echo ==========================================
echo.
echo Checking system requirements for StreamVault
echo.
echo [Press Ctrl+C to exit at any time]
echo.

:: Initialize counters
set /a dotnet_ok=0
set /a node_ok=0
set /a npm_ok=0
set /a port3000_ok=0
set /a port5000_ok=0
set /a backend_ok=0
set /a frontend_ok=0

:: Check .NET
echo Checking .NET SDK...
dotnet --version >nul 2>&1
if !errorlevel! equ 0 (
    for /f "tokens=*" %%i in ('dotnet --version 2^>nul') do set dotnet_version=%%i
    echo [✓] .NET SDK found: !dotnet_version!
    set /a dotnet_ok=1
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
    set /a node_ok=1
) else (
    echo [✗] Node.js not found
    echo     Download from: https://nodejs.org/
)

:: Check npm
echo.
echo Checking npm...
npm --version >nul 2>&1
if !errorlevel! equ 0 (
    for /f "tokens=*" %%i in ('npm --version 2^>nul') do set npm_version=%%i
    echo [✓] npm found: !npm_version!
    set /a npm_ok=1
) else (
    echo [✗] npm not found
    echo     Should be installed with Node.js
)

:: Check ports
echo.
echo Checking port availability...
netstat -an | findstr ":3000" >nul 2>&1
if !errorlevel! equ 0 (
    echo [✗] Port 3000 is in use
    echo     Close applications using this port
) else (
    echo [✓] Port 3000 is available
    set /a port3000_ok=1
)

netstat -an | findstr ":5000" >nul 2>&1
if !errorlevel! equ 0 (
    echo [✗] Port 5000 is in use
    echo     Close applications using this port
) else (
    echo [✓] Port 5000 is available
    set /a port5000_ok=1
)

:: Check project structure
echo.
echo Checking project structure...
if exist "streamvault-backend" (
    echo [✓] Backend folder exists
    set /a backend_ok=1
) else (
    echo [✗] Backend folder missing
)

if exist "streamvault-frontend" (
    echo [✓] Frontend folder exists
    set /a frontend_ok=1
) else (
    echo [✗] Frontend folder missing
)

:: Calculate results
set /a total_checks=7
set /a passed_count=dotnet_ok+node_ok+npm_ok+port3000_ok+port5000_ok+backend_ok+frontend_ok

:: Display results
echo.
echo ==========================================
echo     DIAGNOSTIC RESULTS
echo ==========================================
echo.
echo Checks passed: !passed_count! / !total_checks!
echo.

if !passed_count! equ !total_checks! (
    echo [SUCCESS] All checks passed!
    echo.
    echo Your system is ready to run StreamVault.
    echo.
    echo Next steps:
    echo 1. Run START-PRODUCTION.bat as Administrator
    echo 2. Wait for services to start
    echo 3. Open http://localhost:3000 in your browser
) else (
    echo [WARNING] Some issues found
    echo.
    echo Please fix the issues marked with [✗] above
    echo.
    
    :: Specific fixes
    if !dotnet_ok! equ 0 (
        echo • Install .NET 7 SDK: https://dotnet.microsoft.com/download
    )
    if !node_ok! equ 0 (
        echo • Install Node.js: https://nodejs.org/
    )
    if !port3000_ok! equ 0 (
        echo • Close application using port 3000 or restart PC
    )
    if !port5000_ok! equ 0 (
        echo • Close application using port 5000 or restart PC
    )
)

echo.
echo ==========================================
echo     OPTIONS
echo ==========================================
echo.
echo [1] Run diagnosis again
echo [2] Test individual components
echo [3] View system information
echo [4] Exit
echo.
set /p choice="Select option (1-4): "

if "!choice!"=="1" goto RUN
if "!choice!"=="2" goto test_components
if "!choice!"=="3" goto system_info
if "!choice!"=="4" goto end

goto RUN

:test_components
cls
echo ==========================================
echo     COMPONENT TESTING
echo ==========================================
echo.

echo Testing .NET...
dotnet --version
if !errorlevel! equ 0 (
    echo [✓] .NET working correctly
) else (
    echo [✗] .NET not working
)

echo.
echo Testing Node.js...
node --version
if !errorlevel! equ 0 (
    echo [✓] Node.js working correctly
) else (
    echo [✗] Node.js not working
)

echo.
echo Testing npm...
npm --version
if !errorlevel! equ 0 (
    echo [✓] npm working correctly
) else (
    echo [✗] npm not working
)

echo.
echo Press any key to return to main menu...
pause > nul
goto RUN

:system_info
cls
echo ==========================================
echo     SYSTEM INFORMATION
echo ==========================================
echo.

echo Operating System:
systeminfo | findstr /B /C:"OS Name" /C:"OS Version" /C:"System Type"

echo.
echo Environment Variables:
echo ------------------------
echo PATH=%PATH:~0,200%...

echo.
echo Installed Development Tools:
echo -----------------------------
echo Checking for Git...
git --version >nul 2>&1
if !errorlevel! equ 0 (
    git --version
) else (
    echo Git not installed
)

echo.
echo Checking for Visual Studio Code...
where code >nul 2>&1
if !errorlevel! equ 0 (
    echo Visual Studio Code installed
) else (
    echo Visual Studio Code not installed
)

echo.
echo Checking for Docker...
docker --version >nul 2>&1
if !errorlevel! equ 0 (
    docker --version
) else (
    echo Docker not installed
)

echo.
echo Press any key to return to main menu...
pause > nul
goto RUN

:end
echo.
echo Diagnostic complete. Goodbye!
timeout /t 3 >nul
exit /b 0
