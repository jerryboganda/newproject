@echo off
echo ========================================
echo Starting StreamVault Mock API Server
echo ========================================
echo.

REM Check if Node.js is in PATH
where node >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Adding Node.js to PATH...
    set PATH=%PATH%;C:\Program Files\nodejs
)

REM Install express if not already installed
echo Checking dependencies...
npm list express >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Installing Express.js...
    npm install express cors
)

echo.
echo Starting Mock API Server...
echo.
node mock-api-server.js

pause
