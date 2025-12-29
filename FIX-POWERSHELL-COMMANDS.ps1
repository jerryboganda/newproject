# Navigate to the correct directory first!
Set-Location "c:\Users\Admin\CascadeProjects\newproject\streamvault-backend\src\StreamVault.Api"

# Verify you're in the right place
Write-Host "Current directory:" $PWD

# Delete all Program files
Write-Host "Deleting all Program files..."
Remove-Item Program.cs* -Force -ErrorAction SilentlyContinue

# Delete all project file variants
Write-Host "Deleting project file variants..."
Remove-Item StreamVault.Api.csproj* -Force -ErrorAction SilentlyContinue

# Create clean Program.cs
Write-Host "Creating clean Program.cs..."
@"
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using BCrypt.Net;

// DTOs
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class CreateVideoRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int? Duration { get; set; }
    public bool IsPublic { get; set; } = true;
}

// Entities
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Video
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Duration { get; set; }
    public string ThumbnailUrl { get; set; }
    public string VideoUrl { get; set; }
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long ViewCount { get; set; }
    public User User { get; set; }
}

// DbContext
public class StreamVaultDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Video> Videos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=streamvault.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });
    }
}

// Main application
var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "StreamVault API", 
        Version = "v1",
        Description = "Video Hosting Platform API"
    });
});

// Add SQLite database
builder.Services.AddDbContext<StreamVaultDbContext>(options =>
    options.UseSqlite("Data Source=streamvault.db"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add JWT authentication
var key = Encoding.UTF8.GetBytes("your-super-secret-jwt-key-change-this-in-production-123456789");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "streamvault",
            ValidAudience = "streamvault",
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StreamVault API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check
app.MapGet("/health", () => new { status = "ok", timestamp = DateTime.UtcNow, database = "SQLite" });

// Basic API endpoints
app.MapPost("/api/auth/login", async (LoginRequest request, StreamVaultDbContext context) =>
{
    var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    
    if (user == null || !BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }
    
    var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}:{DateTime.UtcNow.AddHours(1)}"));
    
    return Results.Ok(new
    {
        token,
        user = new
        {
            user.Id,
            user.Email,
            user.Username,
            user.FirstName,
            user.LastName,
            user.IsAdmin = user.Email == "admin@streamvault.com"
        }
    });
});

app.MapGet("/api/videos", async (StreamVaultDbContext context) =>
{
    var videos = await context.Videos
        .Where(v => v.IsPublic)
        .Include(v => v.User)
        .Select(v => new
        {
            v.Id,
            v.Title,
            v.Description,
            v.Duration,
            v.ThumbnailUrl,
            v.VideoUrl,
            v.CreatedAt,
            v.ViewCount,
            v.IsPublic,
            User = new
            {
                v.User.FirstName,
                v.User.LastName,
                v.User.Username
            }
        })
        .OrderByDescending(v => v.CreatedAt)
        .ToListAsync();
    
    return Results.Ok(videos);
});

app.MapGet("/api/videos/{id}", async (Guid id, StreamVaultDbContext context) =>
{
    var video = await context.Videos
        .Where(v => v.Id == id && v.IsPublic)
        .Include(v => v.User)
        .Select(v => new
        {
            v.Id,
            v.Title,
            v.Description,
            v.Duration,
            v.ThumbnailUrl,
            v.VideoUrl,
            v.CreatedAt,
            v.ViewCount,
            User = new
            {
                v.User.FirstName,
                v.User.LastName,
                v.User.Username
            }
        })
        .FirstOrDefaultAsync();
    
    return video == null ? Results.NotFound() : Results.Ok(video);
});

app.MapPost("/api/videos", async (CreateVideoRequest request, StreamVaultDbContext context) =>
{
    // Get user from token (simplified - in production, validate JWT)
    var userId = Guid.NewGuid(); // Placeholder
    
    var video = new Video
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        Description = request.Description ?? "",
        Duration = request.Duration ?? 0,
        ThumbnailUrl = $"/api/thumbnails/{Guid.NewGuid()}.jpg",
        VideoUrl = $"/api/videos/{Guid.NewGuid()}/stream",
        UserId = userId,
        TenantId = Guid.NewGuid(),
        IsPublic = request.IsPublic,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        ViewCount = 0
    };
    
    context.Videos.Add(video);
    await context.SaveChangesAsync();
    
    return Results.Created($"/api/videos/{video.Id}", new
    {
        video.Id,
        video.Title,
        video.Description,
        video.ThumbnailUrl,
        video.VideoUrl,
        video.CreatedAt
    });
});

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
    
    // Create database
    await dbContext.Database.EnsureCreatedAsync();
    
    // Seed admin user
    if (!await dbContext.Users.AnyAsync())
    {
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@streamvault.com",
            Username = "admin",
            PasswordHash = BCrypt.HashPassword("Admin123!"),
            FirstName = "Admin",
            LastName = "User",
            EmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        dbContext.Users.Add(adminUser);
        
        // Add sample videos
        var sampleVideos = new[]
        {
            new Video
            {
                Id = Guid.NewGuid(),
                Title = "Welcome to StreamVault",
                Description = "Get started with our video hosting platform",
                Duration = 120,
                ThumbnailUrl = "/api/placeholder/welcome.jpg",
                VideoUrl = "/api/videos/welcome/stream.mp4",
                UserId = adminUser.Id,
                TenantId = Guid.NewGuid(),
                IsPublic = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ViewCount = 150
            },
            new Video
            {
                Id = Guid.NewGuid(),
                Title = "Video Upload Tutorial",
                Description = "Learn how to upload and manage your videos",
                Duration = 300,
                ThumbnailUrl = "/api/placeholder/tutorial.jpg",
                VideoUrl = "/api/videos/tutorial/stream.mp4",
                UserId = adminUser.Id,
                TenantId = Guid.NewGuid(),
                IsPublic = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ViewCount = 89
            },
            new Video
            {
                Id = Guid.NewGuid(),
                Title = "Live Streaming Guide",
                Description = "Complete guide to live streaming",
                Duration = 450,
                ThumbnailUrl = "/api/placeholder/livestream.jpg",
                VideoUrl = "/api/videos/livestream/stream.mp4",
                UserId = adminUser.Id,
                TenantId = Guid.NewGuid(),
                IsPublic = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ViewCount = 234
            }
        };
        
        foreach (var video in sampleVideos)
        {
            dbContext.Videos.Add(video);
        }
        
        await dbContext.SaveChangesAsync();
    }
}

app.Run();
"@ | Out-File -FilePath "Program.cs" -Encoding UTF8

# Create clean project file
Write-Host "Creating clean project file..."
@"
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
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="7.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>
</Project>
"@ | Out-File -FilePath "StreamVault.Api.csproj" -Encoding UTF8

# Clean build folders
Write-Host "Cleaning build folders..."
if (Test-Path "bin") { Remove-Item -Path "bin" -Recurse -Force }
if (Test-Path "obj") { Remove-Item -Path "obj" -Recurse -Force }

# Build
Write-Host "Restoring packages..."
dotnet restore

Write-Host "Building..."
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host "Starting backend..." -ForegroundColor Green
    dotnet run
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}
