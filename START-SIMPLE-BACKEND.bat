@echo off
title StreamVault Simple Backend
color 0A

echo ==========================================
echo     CREATING SIMPLE BACKEND
echo ==========================================
echo.

cd c:\Users\Admin\CascadeProjects\newproject
if exist SimpleBackend rmdir /s /q SimpleBackend > nul 2>&1
mkdir SimpleBackend
cd SimpleBackend

echo Creating minimal web API...
dotnet new webapi --force --name SimpleBackend

echo.
echo Creating Program.cs...
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
echo app.MapGet^("/api/videos", ^(^) =^> Results.Ok^(new[] {
echo     new {
echo         id = 1,
echo         title = "Welcome to StreamVault",
echo         description = "Get started with our video hosting platform",
echo         thumbnailUrl = "https://picsum.photos/seed/welcome/640/360.jpg",
echo         videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
echo         duration = 120,
echo         createdAt = DateTime.UtcNow,
echo         viewCount = 150,
echo         isPublic = true,
echo         user = new {
echo             username = "admin",
echo             firstName = "Admin",
echo             lastName = "User"
echo         }
echo     },
echo     new {
echo         id = 2,
echo         title = "Video Upload Tutorial",
echo         description = "Learn how to upload and manage your videos",
echo         thumbnailUrl = "https://picsum.photos/seed/tutorial/640/360.jpg",
echo         videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4",
echo         duration = 300,
echo         createdAt = DateTime.UtcNow.AddDays^(-1^),
echo         viewCount = 89,
echo         isPublic = true,
echo         user = new {
echo             username = "admin",
echo             firstName = "Admin",
echo             lastName = "User"
echo         }
echo     }
echo }^)^);
echo.
echo app.MapPost^("/api/auth/login", ^(^) =^> Results.Ok^(new {
echo     token = "dummy-jwt-token",
echo     user = new {
echo         id = Guid.NewGuid^(^),
echo         email = "admin@streamvault.com",
echo         username = "admin",
echo         firstName = "Admin",
echo         lastName = "User",
echo         isAdmin = true
echo     }
echo }^)^);
echo.
echo app.Run^(^);
) > Program.cs

echo.
echo Cleaning default files...
del Controllers\WeatherForecastController.cs > nul 2>&1
del WeatherForecast.cs > nul 2>&1

echo.
echo Building...
dotnet build

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo.
echo SUCCESS! Backend is ready.
echo.
echo Starting backend...
start "StreamVault Backend" cmd /k "title Backend && echo Backend running on http://localhost:5000 && echo. && echo API Endpoints: && echo - Health: http://localhost:5000/health && echo - Videos: http://localhost:5000/api/videos && echo - Login: http://localhost:5000/api/auth/login && echo. && dotnet run"

echo.
echo Backend is starting...
echo.
echo URLs:
echo - Backend: http://localhost:5000
echo - API Docs: http://localhost:5000/swagger
echo.
echo Login credentials:
echo - Email: admin@streamvault.com
echo - Password: (any password works for demo)
echo.
echo Press any key to exit...
pause > nul
