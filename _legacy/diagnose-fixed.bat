@echo off
title StreamVault Diagnostic Tool - Fixed Version
color 0E
mode con: cols=80 lines=50

:menu
cls
echo ==========================================
echo     STREAMVAULT DIAGNOSTIC TOOL
echo ==========================================
echo.
echo This tool will check your system for all
echo requirements to run StreamVault locally
echo.
echo Press any key to start diagnosis...
pause > nul

:diagnose
cls
echo ==========================================
echo     RUNNING SYSTEM DIAGNOSTICS
echo ==========================================
echo.

:: Create results array
set "all_good=true"

:: Check .NET
echo [1/7] Checking .NET SDK...
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo    ❌ .NET SDK not found
    echo    → Please install .NET 7 SDK from: https://dotnet.microsoft.com/download
    set "all_good=false"
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
    echo    ✅ .NET SDK found: %DOTNET_VERSION%
)

:: Check Node.js
echo.
echo [2/7] Checking Node.js...
node --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo    ❌ Node.js not found
    echo    → Please install Node.js from: https://nodejs.org/
    set "all_good=false"
) else (
    for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
    echo    ✅ Node.js found: %NODE_VERSION%
)

:: Check npm
echo.
echo [3/7] Checking npm...
npm --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo    ❌ npm not found
    echo    → npm should come with Node.js, please reinstall Node.js
    set "all_good=false"
) else (
    for /f "tokens=*" %%i in ('npm --version') do set NPM_VERSION=%%i
    echo    ✅ npm found: %NPM_VERSION%
)

:: Check ports
echo.
echo [4/7] Checking port availability...
netstat -an | findstr ":3000" >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo    ⚠️  Port 3000 is in use
    echo    → Close applications using port 3000 or restart your computer
    set "all_good=false"
) else (
    echo    ✅ Port 3000 is available
)

netstat -an | findstr ":5000" >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo    ⚠️  Port 5000 is in use
    echo    → Close applications using port 5000 or restart your computer
    set "all_good=false"
) else (
    echo    ✅ Port 5000 is available
)

:: Check project structure
echo.
echo [5/7] Checking project structure...
if exist "streamvault-backend" (
    echo    ✅ Backend folder found
) else (
    echo    ❌ Backend folder not found
    echo    → Make sure you're in the correct directory
    set "all_good=false"
)

if exist "streamvault-frontend" (
    echo    ✅ Frontend folder found
) else (
    echo    ❌ Frontend folder not found
    echo    → Make sure you're in the correct directory
    set "all_good=false"
)

:: Check backend project file
echo.
echo [6/7] Checking backend project...
if exist "streamvault-backend\StreamVault.sln" (
    echo    ✅ Backend solution file found
) else (
    echo    ❌ Backend solution file not found
    set "all_good=false"
)

:: Check frontend dependencies
echo.
echo [7/7] Checking frontend dependencies...
if exist "streamvault-frontend\package.json" (
    echo    ✅ Frontend package.json found
    if exist "streamvault-frontend\node_modules" (
        echo    ✅ Frontend dependencies installed
    ) else (
        echo    ⚠️  Frontend dependencies not installed
        echo    → Run 'npm install' in streamvault-backend folder
    )
) else (
    echo    ❌ Frontend package.json not found
    set "all_good=false"
)

:: Results
echo.
echo ==========================================
echo     DIAGNOSTIC RESULTS
echo ==========================================
echo.

if "%all_good%"=="true" (
    echo    ✅ All checks passed! Your system is ready.
    echo.
    echo    You can now run: START-HERE.bat
) else (
    echo    ❌ Some issues found. Please fix them above.
    echo.
    echo    Common fixes:
    echo    1. Install missing dependencies from the links above
    echo    2. Restart your computer if ports are in use
    echo    3. Run Command Prompt as Administrator if needed
)

echo.
echo ==========================================
echo     OPTIONS
echo ==========================================
echo.
echo [1] Run diagnosis again
echo [2] Install missing dependencies automatically
echo [3] View detailed logs
echo [4] Exit
echo.
set /p choice="Select an option (1-4): "

if "%choice%"=="1" goto diagnose
if "%choice%"=="2" goto auto_install
if "%choice%"=="3" goto view_logs
if "%choice%"=="4" goto end
goto menu

:auto_install
cls
echo ==========================================
echo     AUTO-INSTALL DEPENDENCIES
echo ==========================================
echo.
echo This will attempt to install missing dependencies.
echo Note: You may still need to install some manually.
echo.
pause

:: Try to detect and offer downloads
echo Checking for .NET...
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo.
    echo .NET SDK not found.
    echo Opening download page in your browser...
    start https://dotnet.microsoft.com/download/dotnet/7.0
    pause
)

echo.
echo Checking for Node.js...
node --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo.
    echo Node.js not found.
    echo Opening download page in your browser...
    start https://nodejs.org/en/download/
    pause
)

echo.
echo Installing frontend dependencies...
if exist "streamvault-frontend\package.json" (
    cd streamvault-frontend
    call npm install
    cd ..
)

echo.
echo Auto-install complete!
echo Please run diagnosis again to verify.
pause
goto menu

:view_logs
cls
echo ==========================================
echo     DETAILED SYSTEM INFORMATION
echo ==========================================
echo.

echo System Information:
systeminfo | findstr /B /C:"OS Name" /C:"OS Version" /C:"System Type"

echo.
echo Environment Variables:
echo ------------------------
echo PATH=%PATH%

echo.
echo Installed Programs:
echo -------------------
echo Checking for Visual Studio...
where code >nul 2>&1
if %ERRORLEVEL% equ 0 echo ✅ Visual Studio Code found

echo Checking for Git...
git --version >nul 2>&1
if %ERRORLEVEL% equ 0 (
    for /f "tokens=*" %%i in ('git --version') do echo ✅ %%i
) else (
    echo ❌ Git not found
)

echo.
echo Port Usage:
echo ------------
netstat -an | findstr ":3000"
netstat -an | findstr ":5000"

pause
goto menu

:end
exit
