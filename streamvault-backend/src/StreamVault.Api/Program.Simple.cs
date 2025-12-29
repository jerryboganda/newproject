var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();

// Health check
app.MapGet("/api/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Auth endpoints (mock for now)
app.MapPost("/api/auth/login", (object login) => {
    return Results.Ok(new {
        token = "mock-jwt-token",
        user = new { email = "superadmin@streamvault.app", role = "SuperAdmin" }
    });
});

// Dashboard data (mock)
app.MapGet("/api/dashboard/stats", () => {
    return Results.Ok(new {
        totalVideos = 0,
        totalViews = 0,
        totalUsers = 1,
        totalStorage = 0
    });
});

// Videos endpoints (mock)
app.MapGet("/api/videos", () => {
    return Results.Ok(new List<object>());
});

app.MapGet("/api/videos/{id}", (string id) => {
    return Results.Ok(new { id = id, title = "Sample Video" });
});

app.Run();