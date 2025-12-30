@echo off
echo ========================================
echo Creating Clean StreamVault Backend
echo ========================================
echo.

REM Set PATH
set PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to parent directory
cd /d "%~dp0"

REM Create new clean API project
echo Creating new clean API project...
dotnet new webapi -n StreamVaultBackend -o StreamVaultBackend --force

cd StreamVaultBackend

echo.
echo Adding required packages...
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Swashbuckle.AspNetCore

echo.
echo Creating controllers...
mkdir Controllers

echo using Microsoft.AspNetCore.Mvc; > Controllers\HealthController.cs
echo using Microsoft.AspNetCore.Cors; >> Controllers\HealthController.cs
echo. >> Controllers\HealthController.cs
echo [ApiController] >> Controllers\HealthController.cs
echo [Route("api/[controller]")] >> Controllers\HealthController.cs
echo public class HealthController : ControllerBase >> Controllers\HealthController.cs
echo { >> Controllers\HealthController.cs
echo     [HttpGet] >> Controllers\HealthController.cs
echo     public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow }); >> Controllers\HealthController.cs
echo } >> Controllers\HealthController.cs

echo.
echo Building and running...
dotnet run --urls="http://localhost:5000"

cd ..
pause
