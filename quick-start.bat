@echo off
echo StreamVault Quick Start (SQLite)
echo ===============================

echo.
echo Setting up backend with SQLite database...
cd /d "%~dp0streamvault-backend"

echo.
echo Installing SQLite packages...
dotnet add src/StreamVault.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/StreamVault.Infrastructure package Microsoft.EntityFrameworkCore.Design

echo.
echo Creating database context...
cd /d "%~dp0streamvault-backend\src\StreamVault.Infrastructure\Data"

echo Creating temporary DbContext for SQLite...
(
echo using Microsoft.EntityFrameworkCore;
echo using StreamVault.Domain.Entities;
echo.
echo namespace StreamVault.Infrastructure.Data;
echo.
echo public class SqliteDbContext : DbContext
echo {
echo     public SqliteDbContext^(DbContextOptions<SqliteDbContext> options^) : base^(options^) { }
echo.
echo     public DbSet<User> Users { get; set; }
echo     public DbSet<Video> Videos { get; set; }
echo     public DbSet<Tenant> Tenants { get; set; }
echo     public DbSet<Comment> Comments { get; set; }
echo     public DbSet<Playlist> Playlists { get; set; }
echo     public DbSet<Notification> Notifications { get; set; }
echo     public DbSet<SubscriptionTier> SubscriptionTiers { get; set; }
echo     public DbSet<LiveStream> LiveStreams { get; set; }
echo.
echo     protected override void OnModelCreating^(ModelBuilder modelBuilder^)
echo     {
echo         // Add any required configurations here
echo         base.OnModelCreating^(modelBuilder^);
echo     }
echo }
echo ) > SqliteDbContext.cs

echo.
echo Updating Program.cs to use SQLite...
cd /d "%~dp0streamvault-backend\src\StreamVault.Api"

echo Backing up original Program.cs...
copy Program.cs Program.cs.bak

echo.
echo Starting backend with SQLite...
start "StreamVault Backend" cmd /k "dotnet run --project StreamVault.Api.csproj"

echo.
echo Waiting 10 seconds for backend to start...
timeout /t 10 /nobreak > nul

echo.
echo Starting frontend...
cd /d "%~dp0streamvault-frontend"
start "StreamVault Frontend" cmd /k "npm run dev"

echo.
echo =====================================
echo Services starting...
echo Frontend: http://localhost:3000
echo Backend: http://localhost:5000
echo.
echo Please wait 30 seconds for full startup
echo =====================================
echo.
echo Press any key to exit...
pause > nul
