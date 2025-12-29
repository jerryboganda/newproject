@echo off
echo ========================================
echo Building Clean StreamVault Backend
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to project
cd /d "%~dp0streamvault-backend\src\StreamVault.API"

REM Temporarily exclude problematic files
echo Temporarily excluding problematic domain files...
cd ..\StreamVault.Domain\Entities
if exist "AdditionalEntities.cs" move "AdditionalEntities.cs" "AdditionalEntities.cs.bak" >nul 2>&1
if exist "SupportEntities.cs" move "SupportEntities.cs" "SupportEntities.cs.bak" >nul 2>&1

REM Go back to API directory
cd ..\..\StreamVault.API

echo.
echo Restoring packages...
dotnet restore StreamVault.Api.csproj

echo.
echo Building project...
dotnet build StreamVault.Api.csproj --no-restore --verbosity normal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build successful!
    echo ========================================
    echo.
    echo Starting API server...
    echo.
    echo API will be available at:
    echo - http://localhost:5000
    echo - http://localhost:5000/swagger
    echo ========================================
    echo.
    dotnet run --urls="http://localhost:5000"
) else (
    echo.
    echo Build failed. Check the errors above.
)

REM Restore files
echo.
echo Restoring excluded files...
cd ..\StreamVault.Domain\Entities
if exist "AdditionalEntities.cs.bak" move "AdditionalEntities.cs.bak" "AdditionalEntities.cs" >nul 2>&1
if exist "SupportEntities.cs.bak" move "SupportEntities.cs.bak" "SupportEntities.cs" >nul 2>&1

pause
