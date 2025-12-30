@echo off
title StreamVault Fixed Startup
color 0A

echo ==========================================
echo    STREAMVAULT LOCAL DEVELOPMENT SETUP
echo ==========================================
echo.

:: Check if we're in the right directory
if not exist "streamvault-backend" (
    echo ERROR: Please run this script from the newproject folder!
    pause
    exit /b 1
)

echo Step 1: Setting up Backend...
cd /d "%~dp0streamvault-backend\src\StreamVault.Api"

:: Backup original if exists
if exist "Program.cs" (
    echo Backing up original Program.cs...
    copy "Program.cs" "Program.cs.original" > nul 2>&1
)

:: Use fixed version
echo Using fixed Program.cs...
copy "Program.Fixed.cs" "Program.cs" > nul

echo.
echo Step 2: Installing Backend Dependencies...
echo Installing required packages...
dotnet add package Microsoft.EntityFrameworkCore.InMemory > nul 2>&1
dotnet add package Microsoft.EntityFrameworkCore.Sqlite > nul 2>&1
dotnet add package BCrypt.Net-Next > nul 2>&1
dotnet add package Swashbuckle.AspNetCore > nul 2>&1

echo.
echo Step 3: Building Backend...
dotnet build --no-restore
if %ERRORLEVEL% neq 0 (
    echo ERROR: Backend build failed!
    pause
    exit /b 1
)

echo.
echo Step 4: Starting Backend Server...
echo This will open in a new window...
start "StreamVault Backend" cmd /k "cd /d \"%~dp0streamvault-backend\src\StreamVault.Api\" && echo ======================================== && echo    STREAMVAULT BACKEND SERVER && echo ======================================== && dotnet run"

echo.
echo Step 5: Waiting for Backend to start...
echo Waiting 15 seconds for backend initialization...
timeout /t 15 /nobreak > nul

echo.
echo Step 6: Starting Frontend...
cd /d "%~dp0streamvault-frontend"

:: Check if node_modules exists
if not exist "node_modules" (
    echo Installing frontend dependencies...
    call npm install
    if %ERRORLEVEL% neq 0 (
        echo ERROR: Frontend dependency installation failed!
        pause
        exit /b 1
    )
)

echo Starting frontend server...
start "StreamVault Frontend" cmd /k "cd /d \"%~dp0streamvault-frontend\" && echo ======================================== && echo    STREAMVAULT FRONTEND SERVER && echo ======================================== && npm run dev"

echo.
echo ==========================================
echo    STREAMVAULT IS STARTING UP!
echo ==========================================
echo.
echo Please wait 30 seconds for full initialization...
echo.
echo ACCESS URLS:
echo ------------
echo   Frontend:     http://localhost:3000
echo   Backend API:  http://localhost:5000
echo   Swagger UI:   http://localhost:5000
echo   Health Check: http://localhost:5000/health
echo.
echo LOGIN CREDENTIALS:
echo ------------------
echo   Email:    admin@streamvault.com
echo   Password: Admin123!
echo.
echo ==========================================
echo.
echo TROUBLESHOOTING:
echo - If you see "connection refused", wait 30 more seconds
echo - Check the Backend and Frontend windows for errors
echo - Make sure ports 3000 and 5000 are not blocked
echo.
echo To STOP: Close the Backend and Frontend windows
echo.
pause
