@echo off
title StreamVault Launcher
color 0A

echo ==========================================
echo     STREAMVAULT LAUNCHER
echo ==========================================
echo.
echo This will start StreamVault with SQLite database
echo.

:: Check if we're in the right directory
if not exist "streamvault-backend" (
    echo ERROR: Please run this from the newproject folder!
    echo.
    echo Current directory: %CD%
    echo.
    echo Make sure you can see the streamvault-backend folder
    echo.
    pause
    exit /b 1
)

:: Create logs directory
if not exist "logs" mkdir logs

:: STEP 1: Backend
echo.
echo [1/3] Starting Backend...
cd streamvault-backend\src\StreamVault.Api

:: Check if Program.Production.cs exists
if not exist "Program.Production.cs" (
    echo ERROR: Program.Production.cs not found!
    echo Please ensure all files are present.
    pause
    exit /b 1
)

:: Copy production program
copy Program.Production.cs Program.cs > nul

:: Install packages (quiet mode)
echo Installing packages...
dotnet add package Microsoft.EntityFrameworkCore.Sqlite > nul 2>&1
dotnet add package BCrypt.Net-Next > nul 2>&1
dotnet add package Swashbuckle.AspNetCore > nul 2>&1

:: Build project
echo Building backend...
dotnet build --no-restore > ..\..\..\logs\backend-build.log 2>&1

:: Start backend in new window
echo Starting backend server...
start "StreamVault Backend" cmd /k "cd /d \"%CD%\" && echo Backend starting... && dotnet run"

:: Go back to root
cd ..\..\..

:: Wait for backend
echo.
echo [2/3] Waiting for backend to start...
echo Please wait 20 seconds...
timeout /t 20 /nobreak > nul

:: STEP 2: Frontend
echo.
echo [3/3] Starting Frontend...
cd streamvault-frontend

:: Create env file
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local

:: Install dependencies if needed
if not exist "node_modules" (
    echo Installing frontend dependencies...
    call npm install > ..\logs\frontend-install.log 2>&1
)

:: Start frontend in new window
echo Starting frontend server...
start "StreamVault Frontend" cmd /k "cd /d \"%CD%\" && echo Frontend starting... && npm run dev"

:: Go back to root
cd ..

:: Final message
echo.
echo ==========================================
echo     STREAMVAULT IS STARTING!
echo ==========================================
echo.
echo Please wait 30 seconds for full startup
echo.
echo URLs:
echo - Frontend: http://localhost:3000
echo - Backend:  http://localhost:5000
echo - API Docs: http://localhost:5000/swagger
echo.
echo Login:
echo - Email: admin@streamvault.com
echo - Password: Admin123!
echo.
echo ==========================================
echo.
echo Backend and Frontend are opening in new windows.
echo Keep those windows open to keep StreamVault running.
echo.
echo Press any key to exit this launcher...
pause > nul
