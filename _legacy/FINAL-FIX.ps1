# Navigate to the fresh project
Set-Location "c:\Users\Admin\CascadeProjects\newproject\streamvault-fresh\StreamVault.Api"

# Clean up
Remove-Item "Program.cs" -Force -ErrorAction SilentlyContinue
Remove-Item "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "obj" -Recurse -Force -ErrorAction SilentlyContinue

# Create a new project with top-level statements enabled
dotnet new webapi --force --name StreamVault.Api

# Remove all default controllers and files
Remove-Item "Controllers\WeatherForecastController.cs" -Force
Remove-Item "WeatherForecast.cs" -Force

# Create a new Program.cs with minimal content
$program = @"
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => new { status = "ok", timestamp = DateTime.UtcNow });

app.Run();
"@

# Set the content to Program.cs
Set-Content -Path "Program.cs" -Value $program -NoNewline

# Build it
Write-Host "Building minimal version..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "SUCCESS! The minimal version builds!" -ForegroundColor Green
    Write-Host "Now adding database and authentication..." -ForegroundColor Yellow
    
    # Now add the full content
    $fullProgram = @"
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
    return Results.Ok(new { token = "dummy-token", email = user.Email });
});

app.MapGet("/api/videos", async (StreamVaultDbContext context) => {
    return Results.Ok(new[] { 
        new { id = 1, title = "Sample Video", description = "This is a sample video" }
    });
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
"@
    
    # Update the project file to include EF Core
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
    
    Set-Content -Path "StreamVault.Api.csproj" -Value $projectFile -NoNewline
    Set-Content -Path "Program.cs" -Value $fullProgram -NoNewline
    
    # Build again
    Write-Host "Building full version..." -ForegroundColor Yellow
    dotnet build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "SUCCESS! Full version builds!" -ForegroundColor Green
        Write-Host "Starting the server..." -ForegroundColor Green
        dotnet run
    } else {
        Write-Host "Full version failed to build." -ForegroundColor Red
    }
} else {
    Write-Host "FAILED! Even the minimal version doesn't build." -ForegroundColor Red
}
