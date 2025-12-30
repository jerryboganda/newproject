# Make sure we're in the right directory
Set-Location "c:\Users\Admin\CascadeProjects\newproject\streamvault-backend\src\StreamVault.Api"

Write-Host "Current directory: $PWD" -ForegroundColor Yellow

# List all Program files
Write-Host "Files to delete:" -ForegroundColor Red
Get-ChildItem Program*.cs -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "  $($_.Name)" }

# Delete ALL Program.cs files
Write-Host "`nDeleting all Program.cs files..." -ForegroundColor Yellow
Remove-Item Program*.cs -Force -ErrorAction SilentlyContinue

# Delete standalone project files
Write-Host "Deleting standalone project files..." -ForegroundColor Yellow
Remove-Item StreamVault.Api.Standalone.csproj -Force -ErrorAction SilentlyContinue

# Create minimal working Program.cs
Write-Host "Creating minimal Program.cs..." -ForegroundColor Green
@"
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

// DbContext
public class StreamVaultDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Video> Videos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=streamvault.db");
    }
}

// Entities
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
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
}

// Main application
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<StreamVaultDbContext>();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins("http://localhost:3000");
    });
});

var app = builder.Build();

app.UseCors();

// Endpoints
app.MapGet("/health", () => new { status = "ok" });

app.MapPost("/api/auth/login", async (LoginRequest request, StreamVaultDbContext context) => {
    var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (user == null || !BCrypt.Verify(request.Password, user.PasswordHash))
        return Results.Unauthorized();
    return Results.Ok(new { token = "dummy-token" });
});

app.MapGet("/api/videos", async (StreamVaultDbContext context) => {
    var videos = await context.Videos.ToListAsync();
    return Results.Ok(videos);
});

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Users.AnyAsync())
    {
        db.Users.Add(new User {
            Id = Guid.NewGuid(),
            Email = "admin@streamvault.com",
            PasswordHash = BCrypt.HashPassword("Admin123!"),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

app.Run();

// DTOs
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
"@ | Out-File -FilePath "Program.cs" -Encoding UTF8

# Verify files
Write-Host "`nCurrent files:" -ForegroundColor Cyan
Get-ChildItem *.cs | ForEach-Object { Write-Host "  $($_.Name)" }

# Build
Write-Host "`nBuilding..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful! Starting server..." -ForegroundColor Green
    dotnet run
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
}
