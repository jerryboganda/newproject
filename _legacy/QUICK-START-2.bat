@echo off
title StreamVault Quick Start

echo Starting StreamVault...
echo.

:: Backend
echo Starting Backend...
cd streamvault-backend\src\StreamVault.Api
copy Program.Production.cs Program.cs
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package BCrypt.Net-Next
start "Backend" cmd /k "dotnet run"

:: Wait
echo Waiting 15 seconds...
timeout /t 15

:: Frontend
echo.
echo Starting Frontend...
cd ..\..\..\streamvault-frontend
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local
start "Frontend" cmd /k "npm run dev"

echo.
echo Done! Check the Backend and Frontend windows.
echo.
echo URLs:
echo - Frontend: http://localhost:3000
echo - Backend:  http://localhost:5000
echo.
echo Login: admin@streamvault.com / Admin123!
pause
