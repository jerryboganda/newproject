@echo off
title StreamVault Working Version Launcher
color 0A

echo ==========================================
echo     STREAMVAULT WORKING VERSION
echo ==========================================
echo.
echo Starting simplified version without errors...
echo.

:: Backend
echo [1/2] Starting Backend...
cd streamvault-backend\src\StreamVault.Api

:: Use working version
copy Program.SimpleWorking.cs Program.cs > nul

:: Install required packages
echo Installing packages...
dotnet add package Microsoft.EntityFrameworkCore.Sqlite > nul 2>&1
dotnet add package BCrypt.Net-Next > nul 2>&1
dotnet add package Swashbuckle.AspNetCore > nul 2>&1

:: Start backend
echo Starting backend with SQLite database...
start "StreamVault Backend" cmd /k "title Backend && echo Starting backend... && dotnet run"

:: Wait
echo.
echo [2/2] Waiting for backend...
timeout /t 15 /nobreak > nul

:: Frontend
echo Starting Frontend...
cd ..\..\..\streamvault-frontend

:: Create env file
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local

:: Start frontend
start "StreamVault Frontend" cmd /k "title Frontend && echo Starting frontend... && npm run dev"

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
echo.
echo Login:
echo - Email: admin@streamvault.com
echo - Password: Admin123!
echo.
echo This version includes:
echo - User authentication
echo - Video upload/view
echo - SQLite database
echo - API documentation
echo.
echo Press any key to exit...
pause > nul
