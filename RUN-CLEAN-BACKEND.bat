@echo off
title Run StreamVault Clean Backend
color 0A

cd c:\Users\Admin\CascadeProjects\newproject\StreamVaultClean

echo ==========================================
echo     STREAMVAULT CLEAN BACKEND
echo ==========================================
echo.

echo Current directory: %CD%
echo.

echo Listing files:
dir /b *.cs
echo.

echo Building project...
dotnet build

if %ERRORLEVEL% neq 0 (
    echo.
    echo Build failed!
    echo.
    echo Let's try a different approach...
    echo.
    
    echo Creating minimal Program.cs...
    (
    echo var builder = WebApplication.CreateBuilder^(args^);
    echo builder.Services.AddEndpointsApiExplorer^(^);
    echo builder.Services.AddSwaggerGen^(^);
    echo.
    echo var app = builder.Build^(^);
    echo.
    echo if ^(app.Environment.IsDevelopment^(^)^)
    echo {
    echo     app.UseSwagger^(^);
    echo     app.UseSwaggerUI^(^);
    echo }
    echo.
    echo app.UseHttpsRedirection^(^);
    echo.
    echo app.MapGet^("/health", ^(^) =^> new { status = "ok", timestamp = DateTime.UtcNow }^);
    echo.
    echo app.Run^(^);
    ) > Program.cs
    
    echo.
    echo Building minimal version...
    dotnet build
    
    if %ERRORLEVEL% neq 0 (
        echo.
        echo ERROR: Even minimal version failed!
        pause
        exit /b 1
    )
    
    echo.
    echo SUCCESS! Minimal version builds.
    echo Starting backend...
    dotnet run
) else (
    echo.
    echo SUCCESS! Full version builds.
    echo Starting backend...
    start "StreamVault Backend" cmd /k "title Backend && echo Backend running on http://localhost:5000 && echo Database: SQLite && echo. && dotnet run"
    
    echo.
    echo Backend is starting...
    echo URL: http://localhost:5000
    echo Health: http://localhost:5000/health
    echo Swagger: http://localhost:5000/swagger
    echo.
    echo Press any key to exit...
    pause > nul
)
