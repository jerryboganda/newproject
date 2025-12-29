@echo off
echo ========================================
echo Creating Clean StreamVault API
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to project
cd /d "%~dp0"

echo Creating new clean API project...
dotnet new webapi -n StreamVaultClean --force
cd StreamVaultClean

echo.
echo Configuring CORS...
dotnet add package Microsoft.AspNetCore.Cors

echo.
echo Building and running...
dotnet run --urls="http://localhost:5000"

cd ..
pause
