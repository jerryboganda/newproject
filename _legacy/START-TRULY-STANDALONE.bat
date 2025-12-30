@echo off
title StreamVault Truly Standalone - No References
color 0A

echo ==========================================
echo     STREAMVAULT TRULY STANDALONE
echo ==========================================
echo.
echo Starting without ANY project references...
echo.

:: Navigate to backend
cd streamvault-backend\src\StreamVault.Api

:: Backup original project file
if exist StreamVault.Api.csproj.original (
    echo Restoring original project file...
    copy StreamVault.Api.csproj.original StreamVault.Api.csproj > nul
) else (
    if exist StreamVault.Api.csproj (
        echo Backing up original project file...
        copy StreamVault.Api.csproj StreamVault.Api.csproj.original > nul
    )
)

:: Use standalone project file
echo Using standalone project file...
copy StreamVault.Api.Standalone.csproj StreamVault.Api.csproj > nul

:: Use standalone program
copy Program.Standalone.Fixed.cs Program.cs > nul

:: Clean everything
echo Cleaning project...
if exist bin rmdir /s /q bin > nul 2>&1
if exist obj rmdir /s /q obj > nul 2>&1

:: Restore packages
echo Restoring packages...
dotnet restore StreamVault.Api.csproj

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Package restore failed!
    pause
    exit /b 1
)

:: Build
echo Building...
dotnet build StreamVault.Api.csproj --no-restore

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Build failed!
    echo.
    echo Try running these commands manually:
    echo.
    echo cd streamvault-backend\src\StreamVault.Api
    echo copy StreamVault.Api.Standalone.csproj StreamVault.Api.csproj
    echo copy Program.Standalone.Fixed.cs Program.cs
    echo rmdir /s /q bin
    echo rmdir /s /q obj
    echo dotnet restore
    echo dotnet build
    echo.
    pause
    exit /b 1
)

:: Start backend
echo.
echo Starting backend...
start "StreamVault Backend" cmd /k "title Backend && echo Backend running on http://localhost:5000 && echo Database: SQLite (streamvault.db) && echo. && dotnet run"

:: Wait
echo Waiting 20 seconds...
timeout /t 20 /nobreak > nul

:: Start frontend
echo.
echo Starting frontend...
cd ..\..\..\streamvault-frontend

:: Create env file
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local
echo NEXT_PUBLIC_WS_URL=ws://localhost:5000 >> .env.local

:: Install dependencies if needed
if not exist node_modules (
    echo Installing frontend dependencies...
    call npm install
)

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
echo Data is persisted between restarts
echo.
echo Press any key to exit...
pause > nul
