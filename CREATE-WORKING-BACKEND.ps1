# Create a new working backend
Write-Host "Creating StreamVault Working Backend..." -ForegroundColor Cyan

# Navigate and create new project
Set-Location "c:\Users\Admin\CascadeProjects\newproject"
Remove-Item "StreamVaultWorking" -Recurse -Force -ErrorAction SilentlyContinue
dotnet new webapi -n StreamVaultWorking --no-https

Set-Location "StreamVaultWorking"

# Remove default files
Remove-Item "Controllers\WeatherForecastController.cs" -Force
Remove-Item "WeatherForecast.cs" -Force

# Create Program.cs properly
$program = @"
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Video
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailUrl { get; set; }
    public string VideoUrl { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public long ViewCount { get; set; }
    public bool IsPublic { get; set; }
    public Guid UserId { get; set; }
}

public class StreamVaultDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Video> Videos { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=streamvault.db");
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<StreamVaultDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/health", () => new { status = "ok", timestamp = DateTime.UtcNow, database = "SQLite" });

app.MapPost("/api/auth/login", async (LoginRequest request, StreamVaultDbContext context) => {
    var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (user == null || !BCrypt.Verify(request.Password, user.PasswordHash))
        return Results.Unauthorized();
    return Results.Ok(new { 
        token = "dummy-jwt-token",
        user = new { 
            id = user.Id, 
            email = user.Email,
            username = user.Email.Split('@')[0],
            firstName = "Admin",
            lastName = "User",
            isAdmin = user.Email == "admin@streamvault.com"
        }
    });
});

app.MapGet("/api/videos", async (StreamVaultDbContext context) => {
    var videos = await context.Videos
        .Where(v => v.IsPublic)
        .OrderByDescending(v => v.CreatedAt)
        .Select(v => new {
            v.Id,
            v.Title,
            v.Description,
            v.ThumbnailUrl,
            v.VideoUrl,
            v.Duration,
            v.CreatedAt,
            v.ViewCount,
            v.IsPublic,
            User = new {
                username = "admin",
                firstName = "Admin",
                lastName = "User"
            }
        })
        .ToListAsync();
    
    return Results.Ok(videos);
});

app.MapGet("/api/videos/{id}", async (Guid id, StreamVaultDbContext context) => {
    var video = await context.Videos
        .Where(v => v.Id == id && v.IsPublic)
        .Select(v => new {
            v.Id,
            v.Title,
            v.Description,
            v.ThumbnailUrl,
            v.VideoUrl,
            v.Duration,
            v.CreatedAt,
            v.ViewCount,
            User = new {
                username = "admin",
                firstName = "Admin",
                lastName = "User"
            }
        })
        .FirstOrDefaultAsync();
    
    return video == null ? Results.NotFound() : Results.Ok(video);
});

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
    await db.Database.EnsureCreatedAsync();
    
    if (!await db.Users.AnyAsync())
    {
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@streamvault.com",
            PasswordHash = BCrypt.HashPassword("Admin123!"),
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();
    }
    
    if (!await db.Videos.AnyAsync())
    {
        var sampleVideos = new[]
        {
            new Video
            {
                Id = Guid.NewGuid(),
                Title = "Welcome to StreamVault",
                Description = "Get started with our video hosting platform",
                ThumbnailUrl = "https://picsum.photos/seed/welcome/640/360.jpg",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
                Duration = 120,
                CreatedAt = DateTime.UtcNow,
                ViewCount = 150,
                IsPublic = true,
                UserId = Guid.NewGuid()
            },
            new Video
            {
                Id = Guid.NewGuid(),
                Title = "Video Upload Tutorial",
                Description = "Learn how to upload and manage your videos",
                ThumbnailUrl = "https://picsum.photos/seed/tutorial/640/360.jpg",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4",
                Duration = 300,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ViewCount = 89,
                IsPublic = true,
                UserId = Guid.NewGuid()
            },
            new Video
            {
                Id = Guid.NewGuid(),
                Title = "Live Streaming Guide",
                Description = "Complete guide to live streaming",
                ThumbnailUrl = "https://picsum.photos/seed/streaming/640/360.jpg",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4",
                Duration = 450,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ViewCount = 234,
                IsPublic = true,
                UserId = Guid.NewGuid()
            }
        };
        
        foreach (var video in sampleVideos)
        {
            db.Videos.Add(video);
        }
        await db.SaveChangesAsync();
    }
}

app.Run();
"@

# Write Program.cs with UTF-8 encoding without BOM
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText("$PWD\Program.cs", $program, $utf8NoBom)

# Update project file
$projectFile = @"
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="7.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="7.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>
</Project>
"@

[System.IO.File]::WriteAllText("$PWD\StreamVaultWorking.csproj", $projectFile, $utf8NoBom)

# Build
Write-Host "Building the project..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "SUCCESS! Backend is ready." -ForegroundColor Green
    Write-Host "Starting backend..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host 'Backend running on http://localhost:5000'; Write-Host 'Press Ctrl+C to stop'; dotnet run"
    
    Write-Host ""
    Write-Host "Backend will start on http://localhost:5000" -ForegroundColor Cyan
    Write-Host "API will be available at:" -ForegroundColor Cyan
    Write-Host "  - Health: http://localhost:5000/health" -ForegroundColor White
    Write-Host "  - Videos: http://localhost:5000/api/videos" -ForegroundColor White
    Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor White
    Write-Host ""
    Write-Host "Login credentials:" -ForegroundColor Cyan
    Write-Host "  Email: admin@streamvault.com" -ForegroundColor White
    Write-Host "  Password: Admin123!" -ForegroundColor White
} else {
    Write-Host "FAILED to build!" -ForegroundColor Red
}
