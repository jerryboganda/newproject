using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using StreamVault.Infrastructure.Data;
using StreamVault.Application.Interfaces;
using StreamVault.Application.Auth;
using StreamVault.Application.Services;
using StreamVault.Application.Repositories;
using StreamVault.Api.Middleware;
using StreamVault.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure tenant resolution options
builder.Services.Configure<TenantResolutionOptions>(options =>
{
    options.BaseDomain = builder.Configuration["Tenant:BaseDomain"] ?? "streamvault.com";
    options.EnableCustomDomains = true;
    options.EnableSubdomains = true;
    options.SkipTenantResolutionPaths = new[] { "/health", "/metrics", "/swagger", "/api/docs" };
    options.PublicPaths = new[] { "/api/public", "/embed", "/webhooks" };
});

// Register multi-tenancy services
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ITenantResolver, TenantResolver>();

// Register authentication services
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, StreamVault.Infrastructure.Services.AuthService>();

// Add database with multi-tenancy support
builder.Services.AddDbContext<StreamVaultDbContext>((serviceProvider, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Host=localhost;Database=streamvault;Username=postgres;Password=password";
    
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("StreamVault.Infrastructure");
    });

    // Get tenant context from service provider
    var tenantContext = serviceProvider.GetService<ITenantContext>();
    if (tenantContext != null)
    {
        options.AddInterceptors(new TenantInterceptor(tenantContext));
    }
});

// Register repositories
builder.Services.AddScoped(typeof(ITenantRepository<>), typeof(TenantRepository<>));
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Register core infrastructure services
builder.Services.AddHttpClient<BunnyCDNService>();
builder.Services.AddSingleton<StripeService>();
builder.Services.AddSingleton<StreamVault.Application.Services.IEmailService, StreamVault.Application.Services.EmailService>();
builder.Services.AddSingleton<IFileStorageService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<BunnyStorageService>>();
    
    // Choose storage provider based on configuration
    var provider = configuration["FileStorage:Provider"]?.ToLowerInvariant();
    
    return provider switch
    {
        "s3" => new S3StorageService(configuration, logger),
        "local" => new LocalFileStorageService(configuration, logger),
        _ => new BunnyStorageService(configuration, logger)
    };
});
builder.Services.AddSingleton<EmailTemplateService>();

// Register seeding service
builder.Services.AddScoped<TenantSeedingService>();

// Add authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Add authorization
builder.Services.AddAuthorization();

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

// Add Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "StreamVault API", 
        Version = "v1",
        Description = "Multi-tenant Video Hosting Platform API"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StreamVault API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Use tenant resolution middleware
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var seedingService = scope.ServiceProvider.GetRequiredService<TenantSeedingService>();
    await seedingService.SeedAsync();
}

app.Run();
