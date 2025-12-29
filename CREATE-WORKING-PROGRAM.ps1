# Navigate to the fresh project
Set-Location "c:\Users\Admin\CascadeProjects\newproject\streamvault-fresh\StreamVault.Api"

# Remove the problematic Program.cs
Remove-Item "Program.cs" -Force

# Create a simple working Program.cs
$programContent = @"
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StreamVaultDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=streamvault.db");
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<StreamVaultDbContext>();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins("http://localhost:3000");
    });
});

var app = builder.Build();
app.UseCors();

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

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
"@

# Write the file with UTF-8 encoding
[System.IO.File]::WriteAllText("Program.cs", $programContent, [System.Text.Encoding]::UTF8)

# Build
Write-Host "Building..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! Starting server..." -ForegroundColor Green
    dotnet run
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}
