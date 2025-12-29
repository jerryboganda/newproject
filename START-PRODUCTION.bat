@echo off
title StreamVault Production Setup
color 0A
mode con: cols=80 lines=40

:: Check administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Please run as Administrator!
    echo Right-click this file and select "Run as administrator"
    pause
    exit /b 1
)

cls
echo ==========================================
echo    STREAMVAULT PROFESSIONAL SETUP
echo ==========================================
echo.
echo This will set up StreamVault with SQLite database
echo for persistent data storage.
echo.

:: Create necessary directories
echo Creating directories...
if not exist "logs" mkdir logs
if not exist "streamvault-backend\logs" mkdir streamvault-backend\logs
if not exist "streamvault-frontend\.next" mkdir streamvault-backend\.next

:: Setup Backend
echo.
echo [1/3] Setting up Backend...
cd /d "%~dp0streamvault-backend\src\StreamVault.Api"

:: Backup original Program.cs
if exist "Program.cs" (
    copy "Program.cs" "Program.cs.backup" > nul
    echo Original Program.cs backed up
)

:: Use production version
copy "Program.Production.cs" "Program.cs" > nul
echo Using production configuration with SQLite

:: Install required packages
echo.
echo Installing required packages...
dotnet add package Microsoft.EntityFrameworkCore.Sqlite > nul 2>&1
dotnet add package Microsoft.EntityFrameworkCore.Design > nul 2>&1
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks > nul 2>&1
dotnet add package Serilog.Extensions.Hosting > nul 2>&1
dotnet add package Serilog.Sinks.File > nul 2>&1
dotnet add package BCrypt.Net-Next > nul 2>&1

:: Create database migrations
echo.
echo Creating database migrations...
dotnet ef migrations add InitialCreate --project ../StreamVault.Infrastructure > nul 2>&1

:: Build the project
echo.
echo Building backend project...
dotnet build --configuration Release --no-restore
if %ERRORLEVEL% neq 0 (
    echo.
    echo ❌ BUILD FAILED!
    echo Check the error messages above.
    echo.
    echo Common fixes:
    echo 1. Make sure .NET 7 SDK is installed
    echo 2. Run 'dotnet restore' manually
    echo 3. Check for missing packages
    pause
    exit /b 1
)

:: Start Backend
echo.
echo [2/3] Starting Backend Server...
echo Backend will start with SQLite database
echo Database file: streamvault.db
echo.
start "StreamVault Backend - Production" cmd /k ^
    "title StreamVault Backend - Production && ^
     cd /d \"%~dp0streamvault-backend\src\StreamVault.Api\" && ^
     echo ======================================== && ^
     echo    STREAMVAULT BACKEND SERVER && ^
     echo    Database: SQLite (Persistent) && ^
     echo ======================================== && ^
     dotnet run --configuration Release"

:: Wait for backend to initialize
echo.
echo Waiting for backend to initialize...
timeout /t 20 /nobreak > nul

:: Setup Frontend
echo.
echo [3/3] Setting up Frontend...
cd /d "%~dp0streamvault-frontend"

:: Create .env.local
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local
echo NEXT_PUBLIC_WS_URL=ws://localhost:5000 >> .env.local
echo NEXT_PUBLIC_ENVIRONMENT=development >> .env.local

:: Install dependencies if needed
if not exist "node_modules" (
    echo Installing frontend dependencies...
    call npm install
    if %ERRORLEVEL% neq 0 (
        echo.
        echo ❌ Frontend dependency installation failed!
        echo Try running 'npm install' manually in the frontend folder
        pause
        exit /b 1
    )
)

:: Start Frontend
echo.
echo Starting frontend server...
start "StreamVault Frontend - Production" cmd /k ^
    "title StreamVault Frontend - Production && ^
     cd /d \"%~dp0streamvault-frontend\" && ^
     echo ======================================== && ^
     echo    STREAMVAULT FRONTEND SERVER && ^
     echo ======================================== && ^
     npm run dev"

:: Wait for services to fully start
echo.
echo Waiting for services to fully start...
timeout /t 30 /nobreak > nul

:: Test connections
echo.
echo Testing service connections...
curl -s http://localhost:5000/health > nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo ✅ Backend is running and healthy!
) else (
    echo ⚠️  Backend may still be starting...
)

curl -s http://localhost:3000 > nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo ✅ Frontend is running!
) else (
    echo ⚠️  Frontend may still be starting...
)

:: Display final information
cls
echo ==========================================
echo    SETUP COMPLETE!
echo ==========================================
echo.
echo 🎉 StreamVault is now running with persistent storage!
echo.
echo 📊 DATABASE: SQLite (streamvault.db)
echo    - Your data will be saved permanently
echo    - Database file created in backend folder
echo    - Automatic migrations applied
echo.
echo 🌐 ACCESS URLS:
echo ---------------
echo   Frontend:     http://localhost:3000
echo   Backend API:  http://localhost:5000
echo   Swagger UI:   http://localhost:5000/swagger
echo   Health Check: http://localhost:5000/health
echo.
echo 🔑 LOGIN CREDENTIALS:
echo --------------------
echo   Email:    admin@streamvault.com
echo   Password: Admin123!
echo.
echo 📁 IMPORTANT FILES:
echo ------------------
echo   Database:     streamvault-backend\streamvault.db
echo   Logs:         streamvault-backend\logs\
echo   Config:       streamvault-backend\src\StreamVault.Api\appsettings.json
echo.
echo 🛠️  MANAGEMENT:
echo ---------------
echo   - Backend and Frontend are running in separate windows
echo   - Close those windows to stop the services
echo   - Database persists between restarts
echo   - All data is saved locally
echo.
echo ==========================================
echo.
echo Press any key to exit...
pause > nul
