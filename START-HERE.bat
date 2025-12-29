@echo off
title StreamVault Setup - Start Here
color 0B

cls
echo ==========================================
echo    STREAMVAULT - AUTOMATIC SETUP
echo ==========================================
echo.
echo This script will automatically fix and start StreamVault
echo.

:: Create logs directory
if not exist "logs" mkdir logs

:: Run diagnostics first
echo Running diagnostics...
call diagnose.bat > logs\diagnostic.log 2>&1

:: Check and fix backend
echo.
echo [STEP 1] Setting up Backend...
cd /d "%~dp0streamvault-backend\src\StreamVault.Api"

:: Create a minimal working Program.cs
echo Creating minimal backend configuration...
(
echo using Microsoft.EntityFrameworkCore;
echo using StreamVault.Infrastructure.Data;
echo using StreamVault.Domain.Entities;
echo using BCrypt.Net;
echo.
echo var builder = WebApplication.CreateBuilder(args);
echo builder.Services.AddControllers();
echo builder.Services.AddEndpointsApiExplorer();
echo builder.Services.AddDbContext<StreamVaultDbContext>(opt => opt.UseInMemoryDatabase("StreamVaultDb"));
echo builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
echo.
echo var app = builder.Build();
echo app.UseCors();
echo app.MapControllers();
echo.
echo // Seed data
echo using var scope = app.Services.CreateScope();
echo var db = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
echo await db.Database.EnsureCreatedAsync();
echo.
echo if (!await db.Users.AnyAsync()) {
echo     var admin = new User {
echo         Id = Guid.NewGuid(),
echo         Email = "admin@streamvault.com",
echo         Username = "admin",
echo         PasswordHash = BCrypt.HashPassword("Admin123!"),
echo         FirstName = "Admin",
echo         LastName = "User",
echo         EmailVerified = true,
echo         IsActive = true,
echo         CreatedAt = DateTime.UtcNow,
echo         UpdatedAt = DateTime.UtcNow
echo     };
echo     db.Users.Add(admin);
echo     await db.SaveChangesAsync();
echo }
echo.
echo app.MapGet("/", () => "StreamVault API is running!");
echo app.MapGet("/health", () => new { status = "ok", timestamp = DateTime.UtcNow });
echo app.MapGet("/api/videos", async (StreamVaultDbContext ctx) => await ctx.Videos.ToListAsync());
echo.
echo app.Run();
) > Program.Minimal.cs

:: Use minimal version
copy Program.Minimal.cs Program.cs > nul 2>&1

:: Install required packages
echo Installing backend packages...
dotnet add package Microsoft.EntityFrameworkCore.InMemory > nul 2>&1
dotnet add package BCrypt.Net-Next > nul 2>&1

:: Build backend
echo Building backend...
dotnet build --no-restore > ..\..\logs\backend-build.log 2>&1

:: Start backend
echo Starting backend...
start "StreamVault Backend" cmd /k "title StreamVault Backend && cd /d \"%~dp0streamvault-backend\src\StreamVault.Api\" && dotnet run"

:: Wait for backend
echo Waiting for backend to start...
timeout /t 10 /nobreak > nul

:: Setup frontend
echo.
echo [STEP 2] Setting up Frontend...
cd /d "%~dp0streamvault-frontend"

:: Create .env.local if not exists
if not exist ".env.local" (
    echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local
    echo NEXT_PUBLIC_WS_URL=ws://localhost:5000 >> .env.local
)

:: Install dependencies if needed
if not exist "node_modules" (
    echo Installing frontend dependencies...
    call npm install > ..\logs\frontend-install.log 2>&1
)

:: Start frontend
echo Starting frontend...
start "StreamVault Frontend" cmd /k "title StreamVault Frontend && cd /d \"%~dp0streamvault-frontend\" && npm run dev"

:: Wait and test
echo.
echo [STEP 3] Testing connections...
timeout /t 20 /nobreak > nul

:: Test backend
echo Testing backend...
curl -s http://localhost:5000/health > nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo ✅ Backend is running!
) else (
    echo ❌ Backend failed to start
    echo Check logs\backend-build.log for errors
)

:: Test frontend
echo Testing frontend...
curl -s http://localhost:3000 > nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo ✅ Frontend is running!
) else (
    echo ❌ Frontend failed to start
    echo Check the frontend window for errors
)

echo.
echo ==========================================
echo    SETUP COMPLETE!
echo ==========================================
echo.
echo OPEN THESE URLS IN YOUR BROWSER:
echo.
echo   🌐 Frontend: http://localhost:3000
echo   🔧 Backend: http://localhost:5000
echo   ❤️  Health: http://localhost:5000/health
echo.
echo LOGIN:
echo   Email: admin@streamvault.com
echo   Password: Admin123!
echo.
echo ==========================================
echo.
echo If you see errors:
echo 1. Check the Backend and Frontend windows
echo 2. Look in the logs folder
echo 3. Make sure ports 3000 and 5000 are free
echo.
echo Press any key to exit...
pause > nul
