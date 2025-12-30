@echo off
title StreamVault Standalone - No Dependencies
color 0A

echo ==========================================
echo     STREAMVAULT STANDALONE START
echo ==========================================
echo.
echo Starting without Application layer...
echo.

:: Navigate to backend
cd streamvault-backend\src\StreamVault.Api

:: Use standalone version
copy Program.Standalone.cs Program.cs > nul

:: Install only essential packages
echo Installing essential packages...
dotnet add package Microsoft.EntityFrameworkCore.Sqlite > nul 2>&1
dotnet add package BCrypt.Net-Next > nul 2>&1
dotnet add package Swashbuckle.AspNetCore > nul 2>&1

:: Build
echo Building...
dotnet build --verbosity quiet

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Build failed!
    echo.
    echo Try running these commands manually:
    echo.
    echo cd streamvault-backend\src\StreamVault.Api
    echo copy Program.Standalone.cs Program.cs
    echo dotnet add package Microsoft.EntityFrameworkCore.Sqlite
    echo dotnet add package BCrypt.Net-Next
    echo dotnet add package Swashbuckle.AspNetCore
    echo dotnet build
    echo.
    pause
    exit /b 1
)

:: Start backend
echo.
echo Starting backend...
start "StreamVault Backend" cmd /k "title Backend && echo Backend running on http://localhost:5000 && echo. && dotnet run"

:: Wait
echo Waiting 20 seconds...
timeout /t 20 /nobreak > nul

:: Start frontend
echo.
echo Starting frontend...
cd ..\..\..\streamvault-frontend

:: Create env file
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local

:: Start frontend
start "StreamVault Frontend" cmd /k "title Frontend && echo Frontend running on http://localhost:3000 && echo. && npm run dev"

:: Done
echo.
echo ==========================================
echo     STREAMVAULT IS STARTING!
echo ==========================================
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
echo.
echo Press any key to exit...
pause > nul
