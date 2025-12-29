@echo off
echo StreamVault Local Setup with In-Memory Database
echo ===============================================

echo.
echo NOTE: Since Docker/PostgreSQL is not available, we'll use in-memory database
echo This means data will be lost when you restart the backend
echo.

echo Step 1: Creating temporary appsettings.json with in-memory database...
cd /d "%~dp0streamvault-backend\src\StreamVault.Api"

echo { > appsettings.Development.json
echo   "ConnectionStrings": { >> appsettings.Development.json
echo     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StreamVault_LocalDB;Trusted_Connection=true;MultipleActiveResultSets=true" >> appsettings.Development.json
echo   }, >> appsettings.Development.json
echo   "JwtSettings": { >> appsettings.Development.json
echo     "Issuer": "streamvault", >> appsettings.Development.json
echo     "Audience": "streamvault", >> appsettings.Development.json
echo     "SigningKey": "your-super-secret-jwt-key-change-this-in-production-123456789" >> appsettings.Development.json
echo   }, >> appsettings.Development.json
echo   "RedisConnectionString": "localhost:6379", >> appsettings.Development.json
echo   "Logging": { >> appsettings.Development.json
echo     "LogLevel": { >> appsettings.Development.json
echo       "Default": "Information", >> appsettings.Development.json
echo       "Microsoft.AspNetCore": "Warning" >> appsettings.Development.json
echo     } >> appsettings.Development.json
echo   }, >> appsettings.Development.json
echo   "AllowedHosts": "*" >> appsettings.Development.json
echo } >> appsettings.Development.json

echo.
echo Step 2: Starting Backend API...
start "StreamVault Backend" cmd /k "dotnet run"

echo.
echo Step 3: Waiting for backend to start...
timeout /t 10 /nobreak > nul

echo.
echo Step 4: Starting Frontend...
cd /d "%~dp0streamvault-frontend"
start "StreamVault Frontend" cmd /k "npm run dev"

echo.
echo ========================================
echo Services should be available at:
echo Frontend: http://localhost:3000
echo Backend: http://localhost:5000
echo.
echo Default Login:
echo Email: admin@streamvault.com
echo Password: Admin123!
echo ========================================
echo.
echo Press any key to exit...
pause > nul
