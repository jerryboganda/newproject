@echo off
echo Starting StreamVault Backend...
echo.

REM Add .NET to PATH for this session
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to API directory
cd streamvault-backend\src\StreamVault.API

REM Build and run
echo Building project...
dotnet build StreamVault.Api.csproj --no-restore --warnaserror:false

echo.
echo Starting API server...
dotnet run --project StreamVault.Api.csproj

pause
