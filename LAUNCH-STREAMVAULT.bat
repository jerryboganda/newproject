@echo off
title StreamVault Launcher
color 0A

echo ==========================================
echo     LAUNCHING STREAMVAULT
echo ==========================================
echo.
echo Starting backend and frontend...
echo.

:: Start Backend
echo [1/2] Starting Backend...
cd streamvault-backend\src\StreamVault.Api

:: Use production program
if exist Program.Production.cs (
    copy Program.Production.cs Program.cs > nul
    echo Using production configuration...
)

:: Install packages if needed
echo Installing packages...
dotnet add package Microsoft.EntityFrameworkCore.Sqlite > nul 2>&1
dotnet add package BCrypt.Net-Next > nul 2>&1

:: Start backend
start "StreamVault Backend" cmd /k "title Backend && echo Starting backend with SQLite database... && dotnet run"

:: Wait
echo.
echo Waiting 20 seconds for backend...
timeout /t 20 /nobreak > nul

:: Start Frontend
echo [2/2] Starting Frontend...
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
echo   Frontend: http://localhost:3000
echo   Backend:  http://localhost:5000
echo   API Docs: http://localhost:5000/swagger
echo.
echo Login:
echo   Email: admin@streamvault.com
echo   Password: Admin123!
echo.
echo Keep the Backend and Frontend windows open.
echo.
echo This window will close in 5 seconds...
timeout /t 5 > nul
