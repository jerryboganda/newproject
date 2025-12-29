@echo off
title StreamVault Clean Start - Remove All Other Versions
color 0A

echo ==========================================
echo     CLEANING ALL PROGRAM VERSIONS
echo ==========================================
echo.

cd streamvault-backend\src\StreamVault.Api

:: Delete all Program.cs variants
echo Deleting all Program.cs variants...
if exist Program.cs.original del Program.cs.original > nul 2>&1
if exist Program.cs del Program.cs > nul 2>&1
if exist Program.Production.cs del Program.Production.cs > nul 2>&1
if exist Program.SimpleWorking.cs del Program.SimpleWorking.cs > nul 2>&1
if exist Program.Standalone.cs del Program.Standalone.cs > nul 2>&1
if exist Program.Standalone.Fixed.cs del Program.Standalone.Fixed.cs > nul 2>&1

:: Delete all project file variants
echo Deleting all project file variants...
if exist StreamVault.Api.csproj.original del StreamVault.Api.csproj.original > nul 2>&1
if exist StreamVault.Api.csproj del StreamVault.Api.csproj > nul 2>&1
if exist StreamVault.Api.Standalone.csproj del StreamVault.Api.Standalone.csproj > nul 2>&1

:: Create clean Program.cs
echo Creating clean Program.cs...
(
echo using Microsoft.AspNetCore.Authentication.JwtBearer;
echo using Microsoft.EntityFrameworkCore;
echo using Microsoft.IdentityModel.Tokens;
echo using Microsoft.OpenApi.Models;
echo using System.Text;
echo using BCrypt.Net;
echo.
echo // DTOs
echo public class LoginRequest
echo {
echo     public string Email { get; set; }
echo     public string Password { get; set; }
echo }
echo.
echo public class CreateVideoRequest
echo {
echo     public string Title { get; set; }
echo     public string Description { get; set; }
echo     public int? Duration { get; set; }
echo     public bool IsPublic { get; set; } = true;
echo }
echo.
echo // Entities
echo public class User
echo {
echo     public Guid Id { get; set; }
echo     public string Email { get; set; }
echo     public string Username { get; set; }
echo     public string PasswordHash { get; set; }
echo     public string FirstName { get; set; }
echo     public string LastName { get; set; }
echo     public bool EmailVerified { get; set; }
echo     public bool IsActive { get; set; }
echo     public DateTime CreatedAt { get; set; }
echo     public DateTime UpdatedAt { get; set; }
echo }
echo.
echo public class Video
echo {
echo     public Guid Id { get; set; }
echo     public string Title { get; set; }
echo     public string Description { get; set; }
echo     public int Duration { get; set; }
echo     public string ThumbnailUrl { get; set; }
echo     public string VideoUrl { get; set; }
echo     public Guid UserId { get; set; }
echo     public Guid TenantId { get; set; }
echo     public bool IsPublic { get; set; }
echo     public bool IsActive { get; set; }
echo     public DateTime CreatedAt { get; set; }
echo     public DateTime UpdatedAt { get; set; }
echo     public long ViewCount { get; set; }
echo     public User User { get; set; }
echo }
echo.
echo // DbContext
echo public class StreamVaultDbContext : DbContext
echo {
echo     public DbSet^<User^> Users { get; set; }
echo     public DbSet^<Video^> Videos { get; set; }
echo.
echo     protected override void OnConfiguring^(DbContextOptionsBuilder optionsBuilder^)
echo     {
echo         optionsBuilder.UseSqlite^("Data Source=streamvault.db"^);
echo     }
echo.
echo     protected override void OnModelCreating^(ModelBuilder modelBuilder^)
echo     {
echo         base.OnModelCreating^(modelBuilder^);
echo         modelBuilder.Entity^<User^>^(entity =^>
echo         {
echo             entity.HasKey^(e =^> e.Id^);
echo             entity.HasIndex^(e =^> e.Email^).IsUnique^(^);
echo         }^);
echo         modelBuilder.Entity^<Video^>^(entity =^>
echo         {
echo             entity.HasKey^(e =^> e.Id^);
echo             entity.HasOne^(e =^> e.User^).WithMany^(^).HasForeignKey^(e =^> e.UserId^);
echo         }^);
echo     }
echo }
) > Program.cs

:: Create clean project file
echo Creating clean project file...
(
echo ^<Project Sdk="Microsoft.NET.Sdk.Web"^>
echo   ^<PropertyGroup^>
echo     ^<TargetFramework^>net7.0^</TargetFramework^>
echo     ^<Nullable^>enable^</Nullable^>
echo     ^<ImplicitUsings^>enable^</ImplicitUsings^>
echo   ^</PropertyGroup^>
echo   ^<ItemGroup^>
echo     ^<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="7.0.0" /^>
echo     ^<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="7.0.0"^>
echo       ^<PrivateAssets^>all^</PrivateAssets^>
echo       ^<IncludeAssets^>runtime; build; native; contentfiles; analyzers; buildtransitive^</IncludeAssets^>
echo     ^</PackageReference^>
echo     ^<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" /^>
echo     ^<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="7.0.0" /^>
echo     ^<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" /^>
echo   ^</ItemGroup^>
echo ^</Project^>
) > StreamVault.Api.csproj

:: Now add the main application code
echo Adding main application code...
(
echo.
echo var builder = WebApplication.CreateBuilder^(args^);
echo.
echo // Services
echo builder.Services.AddControllers^(^);
echo builder.Services.AddDbContext^<StreamVaultDbContext^>^(options =^>
echo     options.UseSqlite^("Data Source=streamvault.db"^)^);
echo builder.Services.AddCors^(options =^>
echo {
echo     options.AddDefaultPolicy^(policy =^>
echo     {
echo         policy.WithOrigins^("http://localhost:3000"^)
echo               .AllowAnyMethod^(^)
echo               .AllowAnyHeader^(^);
echo     }^);
echo }^);
echo.
echo var app = builder.Build^(^);
echo.
echo // Pipeline
echo app.UseHttpsRedirection^(^);
echo app.UseCors^(^);
echo.
echo // Endpoints
echo app.MapGet^("/health", ^(^) =^> new { status = "ok", timestamp = DateTime.UtcNow }^);
echo.
echo app.MapPost^("/api/auth/login", async ^(LoginRequest request, StreamVaultDbContext context^) =^>
echo {
echo     var user = await context.Users.FirstOrDefaultAsync^(u =^> u.Email == request.Email^);
echo     if ^(user == null ^|^| !BCrypt.Verify^(request.Password, user.PasswordHash^)^)
echo         return Results.Unauthorized^(^);
echo     return Results.Ok^(new { token = "dummy-token", user.Email }^);
echo }^);
echo.
echo app.MapGet^("/api/videos", async ^(StreamVaultDbContext context^) =^>
echo {
echo     var videos = await context.Videos
echo         .Where^(v =^> v.IsPublic^)
echo         .Include^(v =^> v.User^)
echo         .Select^(v =^> new { v.Id, v.Title, v.Description, v.ThumbnailUrl, v.CreatedAt }^)
echo         .ToListAsync^(^);
echo     return Results.Ok^(videos^);
echo }^);
echo.
echo // Seed data
echo using ^(var scope = app.Services.CreateScope^(^)^)
echo {
echo     var dbContext = scope.ServiceProvider.GetRequiredService^<StreamVaultDbContext^>^(^);
echo     await dbContext.Database.EnsureCreatedAsync^(^);
echo     if ^(!await dbContext.Users.AnyAsync^(^)^)
echo     {
echo         var adminUser = new User
echo         {
echo             Id = Guid.NewGuid^(^),
echo             Email = "admin@streamvault.com",
echo             Username = "admin",
echo             PasswordHash = BCrypt.HashPassword^("Admin123!"^),
echo             FirstName = "Admin",
echo             LastName = "User",
echo             EmailVerified = true,
echo             IsActive = true,
echo             CreatedAt = DateTime.UtcNow,
echo             UpdatedAt = DateTime.UtcNow
echo         };
echo         dbContext.Users.Add^(adminUser^);
echo         await dbContext.SaveChangesAsync^(^);
echo     }
echo }
echo.
echo app.Run^(^);
) >> Program.cs

:: Clean build folders
echo Cleaning build folders...
if exist bin rmdir /s /q bin > nul 2>&1
if exist obj rmdir /s /q obj > nul 2>&1

:: Build
echo.
echo Building...
dotnet restore
dotnet build

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

:: Start backend
echo.
echo Starting backend...
start "StreamVault Backend" cmd /k "title Backend && echo Backend running on http://localhost:5000 && echo Database: SQLite && echo. && dotnet run"

:: Wait
echo Waiting 20 seconds...
timeout /t 20 /nobreak > nul

:: Start frontend
echo.
echo Starting frontend...
cd ..\..\..\streamvault-frontend

:: Create env file
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local

:: Start frontend
start "StreamVault Frontend" cmd /k "title Frontend && echo Frontend running on http://localhost:3000 && echo. && npm run dev"

:: Done
echo.
echo ==========================================
echo     STREAMVAULT IS STARTING!
echo ==========================================
echo.
echo URLs:
echo - Frontend: http://localhost:3000
echo - Backend:  http://localhost:5000
echo - Health:   http://localhost:5000/health
echo.
echo Login:
echo - Email: admin@streamvault.com
echo - Password: Admin123!
echo.
echo Press any key to exit...
pause > nul
