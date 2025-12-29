@echo off
echo ========================================
echo Creating Fresh StreamVault API
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to project
cd /d "%~dp0"

REM Create new API project
echo Creating new API project...
dotnet new webapi -n StreamVaultAPI -o StreamVaultAPI --force

cd StreamVaultAPI

echo.
echo Adding packages...
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Swashbuckle.AspNetCore

echo.
echo Creating appsettings.json...
(
echo {
echo   "ConnectionStrings": {
echo     "DefaultConnection": "Host=localhost;Database=streamvault_dev;Username=postgres;Password=password"
echo   },
echo   "Logging": {
echo     "LogLevel": {
echo       "Default": "Information",
echo       "Microsoft.AspNetCore": "Warning"
echo     }
echo   },
echo   "AllowedHosts": "*"
echo }
) > appsettings.json

echo.
echo Creating controllers...
copy "..\streamvault-backend\src\StreamVault.API\Controllers\ApiController.cs" "Controllers\ApiController.cs" >nul 2>&1

echo.
echo Building and running...
echo.
echo ========================================
echo StreamVault API is starting...
echo ========================================
echo.
echo API will be available at:
echo - http://localhost:5000
echo - http://localhost:5000/api/health
echo - http://localhost:5000/swagger
echo ========================================
echo.

dotnet run --urls="http://localhost:5000"

cd ..
pause
