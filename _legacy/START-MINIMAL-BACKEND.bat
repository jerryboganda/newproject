@echo off
echo ========================================
echo Creating Minimal Working Backend
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to project
cd /d "%~dp0streamvault-backend\src\StreamVault.API"

REM Backup controllers
echo Temporarily moving controllers...
if exist "Controllers" move "Controllers" "Controllers.bak" >nul 2>&1

REM Create a minimal controller
echo Creating minimal controllers...
mkdir Controllers 2>nul

echo import System; > Controllers\HealthController.cs
echo import Microsoft.AspNetCore.Mvc; >> Controllers\HealthController.cs
echo. >> Controllers\HealthController.cs
echo [ApiController] >> Controllers\HealthController.cs
echo [Route("api/[controller]")] >> Controllers\HealthController.cs
echo public class HealthController : ControllerBase >> Controllers\HealthController.cs
echo { >> Controllers\HealthController.cs
echo     [HttpGet] >> Controllers\HealthController.cs
echo     public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow }); >> Controllers\HealthController.cs
echo } >> Controllers\HealthController.cs

echo.
echo Restoring and building...
dotnet restore StreamVault.Api.csproj
dotnet build StreamVault.Api.csproj --verbosity quiet

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
    dotnet run --project StreamVault.Api.csproj --urls="http://localhost:5000"
) else (
    echo.
    echo Build failed.
)

REM Restore
echo.
echo Restoring original controllers...
rmdir /S /Q Controllers 2>nul
move "Controllers.bak" "Controllers" >nul 2>&1

pause
