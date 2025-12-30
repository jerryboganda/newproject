@echo off
title Create Complete StreamVault Backend
color 0A

echo ==========================================
echo     CREATING COMPLETE BACKEND
echo ==========================================
echo.

cd c:\Users\Admin\CascadeProjects\newproject
if exist CompleteBackend rmdir /s /q CompleteBackend > nul 2>&1
mkdir CompleteBackend
cd CompleteBackend

echo Creating web API project...
dotnet new webapi --force --name CompleteBackend

echo.
echo Removing default files...
del Controllers\WeatherForecastController.cs > nul 2>&1
del WeatherForecast.cs > nul 2>&1

echo.
echo Creating complete Program.cs with all endpoints...
(
echo using System.Text.Json;
echo.
echo var builder = WebApplication.CreateBuilder^(args^);
echo builder.Services.AddEndpointsApiExplorer^(^);
echo builder.Services.AddSwaggerGen^(^);
echo builder.Services.AddCors^(options =^> {
echo     options.AddDefaultPolicy^(policy =^> {
echo         policy.WithOrigins^("http://localhost:3000", "http://localhost:3001"^)
echo               .AllowAnyMethod^(^)
echo               .AllowAnyHeader^(^)
echo               .AllowCredentials^(^);
echo     }^);
echo }^);
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
echo app.UseCors^(^);
echo.
echo // Health endpoint
echo app.MapGet^("/health", ^(^) =^> new { status = "ok", timestamp = DateTime.UtcNow }^);
echo.
echo // Videos endpoints
echo app.MapGet^("/api/videos", ^(^) =^> {
echo     var videos = new[] {
echo         new {
echo             id = 1,
echo             title = "Welcome to StreamVault",
echo             description = "Get started with our video hosting platform",
echo             thumbnailUrl = "https://picsum.photos/seed/welcome/640/360.jpg",
echo             videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
echo             duration = 120,
echo             createdAt = DateTime.UtcNow,
echo             viewCount = 150,
echo             isPublic = true,
echo             user = new {
echo                 username = "admin",
echo                 firstName = "Admin",
echo                 lastName = "User"
echo             }
echo         },
echo         new {
echo             id = 2,
echo             title = "Video Upload Tutorial",
echo             description = "Learn how to upload and manage your videos",
echo             thumbnailUrl = "https://picsum.photos/seed/tutorial/640/360.jpg",
echo             videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4",
echo             duration = 300,
echo             createdAt = DateTime.UtcNow.AddDays^(-1^),
echo             viewCount = 89,
echo             isPublic = true,
echo             user = new {
echo                 username = "admin",
echo                 firstName = "Admin",
echo                 lastName = "User"
echo             }
echo         },
echo         new {
echo             id = 3,
echo             title = "Live Streaming Guide",
echo             description = "Complete guide to live streaming",
echo             thumbnailUrl = "https://picsum.photos/seed/streaming/640/360.jpg",
echo             videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4",
echo             duration = 450,
echo             createdAt = DateTime.UtcNow.AddDays^(-2^),
echo             viewCount = 234,
echo             isPublic = true,
echo             user = new {
echo                 username = "admin",
echo                 firstName = "Admin",
echo                 lastName = "User"
echo             }
echo         },
echo         new {
echo             id = 4,
echo             title = "Video Editing Tips",
echo             description = "Professional video editing techniques",
echo             thumbnailUrl = "https://picsum.photos/seed/editing/640/360.jpg",
echo             videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4",
echo             duration = 180,
echo             createdAt = DateTime.UtcNow.AddDays^(-3^),
echo             viewCount = 567,
echo             isPublic = true,
echo             user = new {
echo                 username = "admin",
echo                 firstName = "Admin",
echo                 lastName = "User"
echo             }
echo         },
echo         new {
echo             id = 5,
echo             title = "Monetization Strategies",
echo             description = "How to monetize your video content",
echo             thumbnailUrl = "https://picsum.photos/seed/money/640/360.jpg",
echo             videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerFun.mp4",
echo             duration = 600,
echo             createdAt = DateTime.UtcNow.AddDays^(-4^),
echo             viewCount = 445,
echo             isPublic = true,
echo             user = new {
echo                 username = "admin",
echo                 firstName = "Admin",
echo                 lastName = "User"
echo             }
echo         },
echo         new {
echo             id = 6,
echo             title = "SEO for Videos",
echo             description = "Optimize your videos for search engines",
echo             thumbnailUrl = "https://picsum.photos/seed/seo/640/360.jpg",
echo             videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerJoyrides.mp4",
echo             duration = 240,
echo             createdAt = DateTime.UtcNow.AddDays^(-5^),
echo             viewCount = 123,
echo             isPublic = true,
echo             user = new {
echo                 username = "admin",
echo                 firstName = "Admin",
echo                 lastName = "User"
echo             }
echo         }
echo     };
echo     return Results.Ok^(videos^);
echo }^);
echo.
echo app.MapGet^("/api/videos/{id}", ^(int id^) =^> {
echo     var videos = new[] {
echo         new { id = 1, title = "Welcome to StreamVault", description = "Get started with our video hosting platform" },
echo         new { id = 2, title = "Video Upload Tutorial", description = "Learn how to upload and manage your videos" },
echo         new { id = 3, title = "Live Streaming Guide", description = "Complete guide to live streaming" }
echo     };
echo     var video = videos.FirstOrDefault^(v =^> v.id == id^);
echo     return video == null ? Results.NotFound^(^) : Results.Ok^(video^);
echo }^);
echo.
echo // Auth endpoints
echo app.MapPost^("/api/auth/login", ^(LoginRequest request^) =^> {
echo     if ^(request.Email == "admin@streamvault.com" ^&^& request.Password == "Admin123!"^)
echo     {
echo         return Results.Ok^(new {
echo             token = "dummy-jwt-token-12345",
echo             user = new {
echo                 id = Guid.NewGuid^(^),
echo                 email = "admin@streamvault.com",
echo                 username = "admin",
echo                 firstName = "Admin",
echo                 lastName = "User",
echo                 isAdmin = true
echo             }
echo         }^);
echo     }
echo     return Results.Unauthorized^(^);
echo }^);
echo.
echo app.MapPost^("/api/auth/register", ^(RegisterRequest request^) =^> {
echo     // Mock registration - always succeeds
echo     return Results.Ok^(new {
echo         message = "User registered successfully",
echo         user = new {
echo             id = Guid.NewGuid^(^),
echo             email = request.Email,
echo             username = request.Username
echo         }
echo     }^);
echo }^);
echo.
echo // User endpoints
echo app.MapGet^("/api/user/profile", ^(^) =^> {
echo     return Results.Ok^(new {
echo         id = Guid.NewGuid^(^),
echo         email = "admin@streamvault.com",
echo         username = "admin",
echo         firstName = "Admin",
echo         lastName = "User",
echo         avatar = "https://picsum.photos/seed/avatar/200/200.jpg",
echo         subscribers = 1000,
echo         totalViews = 50000
echo     }^);
echo }^);
echo.
echo app.MapGet^("/api/user/videos", ^(^) =^> {
echo     return Results.Ok^(new[] {
echo         new { id = 1, title = "My Video 1", isPublic = true },
echo         new { id = 2, title = "My Video 2", isPublic = false }
echo     }^);
echo }^);
echo.
echo // Analytics endpoint
echo app.MapGet^("/api/analytics/dashboard", ^(^) =^> {
echo     return Results.Ok^(new {
echo         totalViews = 50000,
echo         totalVideos = 25,
echo         totalSubscribers = 1000,
echo         monthlyViews = new[] { 4000, 4500, 5000, 4800, 5200, 5500, 5800, 6000 },
echo         topVideos = new[] {
echo             new { id = 1, title = "Welcome to StreamVault", views = 1500 },
echo             new { id = 2, title = "Video Upload Tutorial", views = 1200 }
echo         }
echo     }^);
echo }^);
echo.
echo // Categories endpoint
echo app.MapGet^("/api/categories", ^(^) =^> {
echo     return Results.Ok^(new[] {
echo         new { id = 1, name = "Education", videoCount = 10 },
echo         new { id = 2, name = "Entertainment", videoCount = 15 },
echo         new { id = 3, name = "Gaming", videoCount = 8 },
echo         new { id = 4, name = "Music", videoCount = 12 },
echo         new { id = 5, name = "Sports", videoCount = 6 }
echo     }^);
echo }^);
echo.
echo app.Run^(^);
echo.
echo // DTOs
echo public class LoginRequest
echo {
echo     public string Email { get; set; }
echo     public string Password { get; set; }
echo }
echo.
echo public class RegisterRequest
echo {
echo     public string Email { get; set; }
echo     public string Username { get; set; }
echo     public string Password { get; set; }
echo }
) > Program.cs

echo.
echo Building project...
dotnet build

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo.
echo SUCCESS! Complete backend is ready.
echo.
echo Starting backend on port 5000...
start "StreamVault Complete Backend" cmd /k "title Backend && echo Backend running on http://localhost:5000 && echo. && echo Available endpoints: && echo - GET  /health && echo - GET  /api/videos && echo - GET  /api/videos/{id} && echo - POST /api/auth/login && echo - POST /api/auth/register && echo - GET  /api/user/profile && echo - GET  /api/user/videos && echo - GET  /api/analytics/dashboard && echo - GET  /api/categories && echo. && echo Login: admin@streamvault.com / Admin123! && echo. && dotnet run --urls="http://localhost:5000"

echo.
echo Backend is starting on http://localhost:5000
echo.
echo Updating frontend configuration...
cd ..\streamvault-frontend
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > .env.local

echo.
echo ==========================================
echo     SETUP COMPLETE!
echo ==========================================
echo.
echo Backend: http://localhost:5000
echo Frontend: http://localhost:3000
echo.
echo The frontend has been configured to connect to port 5000
echo Please refresh your frontend browser after the backend starts.
echo.
echo Press any key to exit...
pause > nul
