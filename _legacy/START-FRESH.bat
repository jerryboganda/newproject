@echo off
title StreamVault Fresh Start
color 0A

echo ==========================================
echo     CREATING FRESH PROJECT
echo ==========================================
echo.

:: Create new directory
cd c:\Users\Admin\CascadeProjects\newproject
if exist streamvault-fresh rmdir /s /q streamvault-fresh > nul 2>&1
mkdir streamvault-fresh
cd streamvault-fresh

:: Create new project
echo Creating new web API project...
dotnet new webapi -n StreamVault.Api --no-https
cd StreamVault.Api

:: Create project file with required packages
echo Creating project file...
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

:: Remove default files
echo Cleaning default files...
del Program.cs > nul 2>&1
del Controllers\WeatherForecastController.cs > nul 2>&1
del WeatherForecast.cs > nul 2>&1

:: Create new Program.cs
echo Creating Program.cs...
(
echo using Microsoft.EntityFrameworkCore;
echo using BCrypt.Net;
echo using Microsoft.AspNetCore.Authentication.JwtBearer;
echo using Microsoft.IdentityModel.Tokens;
echo using System.Text;
echo using Microsoft.OpenApi.Models;
echo.
echo // DTOs
echo public class LoginRequest
echo {
echo     public string Email { get; set; }
echo     public string Password { get; set; }
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
echo }
echo.
echo // Main application
echo var builder = WebApplication.CreateBuilder^(args^);
echo.
echo // Add services
echo builder.Services.AddControllers^(^);
echo builder.Services.AddEndpointsApiExplorer^(^);
echo builder.Services.AddDbContext^<StreamVaultDbContext^>^(options =^>
echo     options.UseSqlite^("Data Source=streamvault.db"^)^);
echo.
echo // Add CORS
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
echo // Add Swagger
echo builder.Services.AddSwaggerGen^(c =^>
echo {
echo     c.SwaggerDoc^("v1", new OpenApiInfo { Title = "StreamVault API", Version = "v1" }^);
echo }^);
echo.
echo var app = builder.Build^(^);
echo.
echo // Configure pipeline
echo if ^(app.Environment.IsDevelopment^(^)^)
echo {
echo     app.UseSwagger^(^);
echo     app.UseSwaggerUI^(^);
echo }
echo.
echo app.UseHttpsRedirection^(^);
echo app.UseCors^(^);
echo app.UseAuthorization^(^);
echo.
echo app.MapControllers^(^);
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
echo     var videos = await context.Videos.ToListAsync^(^);
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
echo         dbContext.Users.Add^(new User
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
echo         }^);
echo         await dbContext.SaveChangesAsync^(^);
echo     }
echo }
echo.
echo app.Run^(^);
) > Program.cs

:: Build and run
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

echo.
echo Starting backend...
start "StreamVault Backend" cmd /k "title Backend && echo Backend running on http://localhost:5000 && echo Database: SQLite && echo. && dotnet run

:: Wait for backend
echo Waiting 20 seconds...
timeout /t 20 /nobreak > nul

:: Start frontend
echo.
echo Starting frontend...
cd ..\..\streamvault-frontend

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
