@echo off
title StreamVault - Clean Start
color 0A

echo ==========================================
echo     STREAMVAULT CLEAN START
echo ==========================================
echo.
echo This will start StreamVault without errors
echo.

:: Navigate to backend API folder
cd streamvault-backend\src\StreamVault.Api

:: Backup original Program.cs if it exists
if exist Program.cs.original (
    echo Restoring original Program.cs...
    copy Program.cs.original Program.cs > nul
) else (
    if exist Program.cs (
        echo Backing up original Program.cs...
        copy Program.cs Program.cs.original > nul
    )
)

:: Use the working version
echo Using simplified working version...
copy Program.SimpleWorking.cs Program.cs > nul

:: Clean the project
echo Cleaning project...
dotnet clean > nul 2>&1

:: Restore packages
echo Restoring packages...
dotnet restore > nul 2>&1

:: Install required packages
echo Installing required packages...
dotnet add package Microsoft.EntityFrameworkCore.Sqlite > nul 2>&1
dotnet add package BCrypt.Net-Next > nul 2>&1
dotnet add package Swashbuckle.AspNetCore > nul 2>&1

:: Build the project
echo Building project...
dotnet build --no-restore > nul 2>&1

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Build failed!
    echo Check the error messages above.
    echo.
    pause
    exit /b 1
)

:: Start backend
echo.
echo Starting backend...
start "StreamVault Backend" cmd /k "title StreamVault Backend && echo Backend starting with SQLite database... && echo Database: streamvault.db && echo. && dotnet run"

:: Wait for backend
echo Waiting 20 seconds for backend to start...
timeout /t 20 /nobreak > nul

:: Start frontend
echo.
echo Starting frontend...
cd ..\..\..\streamvault-frontend

:: Create .env.local
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local
echo NEXT_PUBLIC_WS_URL=ws://localhost:5000 >> .env.local

:: Install dependencies if needed
if not exist node_modules (
    echo Installing frontend dependencies...
    call npm install > nul 2>&1
)

:: Start frontend
start "StreamVault Frontend" cmd /k "title StreamVault Frontend && echo Frontend starting... && npm run dev"

:: Done
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
echo - Health:   http://localhost:5000/health
echo.
echo Login:
echo - Email: admin@streamvault.com
echo - Password: Admin123!
echo.
echo Database: SQLite (streamvault.db)
echo Data is persisted between restarts
echo.
echo Press any key to exit...
pause > nul
