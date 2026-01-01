using System.Text;
using System.Text.Json.Serialization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.PostgreSql;
using StreamVault.Application.Auth;
using StreamVault.Application.Interfaces;
using StreamVault.Application.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using StreamVault.Infrastructure.Services;
using StreamVault.Api.Hubs;
using StreamVault.Api.Jobs;
using StreamVault.Api.Auth;
using StreamVault.Api.Services;
using StreamVault.Api.Middleware;
using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<ITenantContext, TenantContext>();

builder.Services.AddDbContext<StreamVaultDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("StreamVault.Infrastructure")));

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<StreamVault.Application.Services.IEmailService, StreamVault.Infrastructure.Services.TemplatedEmailService>();
builder.Services.AddSingleton<IPlaybackTokenService, PlaybackTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditLogger>();

builder.Services.AddHttpClient("bunny-api");
builder.Services.AddHttpClient("bunny-upload");
builder.Services.AddHttpClient("webhooks");
builder.Services.AddScoped<IBunnyNetService, BunnyNetService>();

builder.Services.AddScoped<IWebhookPublisher, WebhookPublisher>();

// Hangfire (Postgres)
builder.Services.AddHangfire(config =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("DefaultConnection is required for Hangfire storage");

    config.UseSimpleAssemblyNameTypeSerializer();
    config.UseRecommendedSerializerSettings();
    config.UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions
    {
        SchemaName = builder.Configuration["Hangfire:Schema"] ?? "hangfire"
    });
});
builder.Services.AddHangfireServer();

builder.Services.AddTransient<UsageSyncJob>();
builder.Services.AddTransient<MonthlyOverageInvoiceJob>();
builder.Services.AddTransient<AnalyticsRollupJob>();
builder.Services.AddTransient<SupportSlaEscalationJob>();
builder.Services.AddTransient<WebhookDeliveryJob>();

var jwtSecretKey =
    builder.Configuration["JwtSettings:SecretKey"]
    ?? builder.Configuration["JwtSettings:SigningKey"]
    ?? builder.Configuration["Jwt:SecretKey"]
    ?? builder.Configuration["Jwt:SigningKey"];

if (string.IsNullOrWhiteSpace(jwtSecretKey))
    throw new InvalidOperationException("JWT signing key not configured (JwtSettings:SecretKey/JwtSettings:SigningKey or Jwt:SecretKey/Jwt:SigningKey)");

var keyBytes = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "BearerOrApiKey";
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddPolicyScheme("BearerOrApiKey", "JWT or API key", options =>
    {
        options.ForwardDefaultSelector = context =>
            context.Request.Headers.ContainsKey(ApiKeyAuthenticationHandler.HeaderName)
                ? ApiKeyAuthenticationHandler.SchemeName
                : JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, _ => { });

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var apiKeyId = httpContext.User?.FindFirst("api_key_id")?.Value;
        if (!string.IsNullOrWhiteSpace(apiKeyId))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"api:{apiKeyId}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
        }

        var userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"user:{userId}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"ip:{ip}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
});
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StreamVault API", Version = "v1" });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        var configuredOrigins = builder.Configuration["Cors:AllowedOrigins"];
        var origins = (configuredOrigins ?? string.Empty)
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (origins.Length == 0 && builder.Environment.IsDevelopment())
        {
            origins = new[]
            {
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:5173",
                "http://localhost:8080"
            };
        }

        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            // Locked down by default when not configured.
            policy.SetIsOriginAllowed(_ => false);
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCors");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<ApiKeyScopeEnforcementMiddleware>();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapHub<LiveAnalyticsHub>("/hubs/live-analytics");
app.MapHub<StreamVault.Api.Hubs.SupportHub>("/hubs/support");

// Recurring jobs
RecurringJob.AddOrUpdate<UsageSyncJob>(
    "usage-sync-hourly",
    job => job.RunAsync(CancellationToken.None),
    Cron.Hourly);

RecurringJob.AddOrUpdate<MonthlyOverageInvoiceJob>(
    "overage-invoice-monthly",
    job => job.RunAsync(CancellationToken.None),
    Cron.Daily);

RecurringJob.AddOrUpdate<AnalyticsRollupJob>(
    "analytics-rollup",
    job => job.RunAsync(CancellationToken.None),
    Cron.Hourly);

RecurringJob.AddOrUpdate<SupportSlaEscalationJob>(
    "support-sla-escalation",
    job => job.RunAsync(CancellationToken.None),
    Cron.Minutely);

RecurringJob.AddOrUpdate<WebhookDeliveryJob>(
    "webhook-delivery",
    job => job.RunOnceAsync(CancellationToken.None),
    Cron.Minutely);

var tusStorePath = builder.Configuration["Tus:StorePath"]
    ?? Path.Combine(app.Environment.ContentRootPath, "App_Data", "tus");

Directory.CreateDirectory(tusStorePath);
var tusStore = new TusDiskStore(tusStorePath);

app.UseTus(httpContext =>
{
    return new DefaultTusConfiguration
    {
        Store = tusStore,
        UrlPath = "/api/v1/uploads/tus",
        MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
        Events = new Events
        {
            OnAuthorizeAsync = ctx =>
            {
                if (ctx.HttpContext.User?.Identity?.IsAuthenticated != true)
                {
                    ctx.FailRequest("Unauthorized");
                }
                return Task.CompletedTask;
            },
            OnFileCompleteAsync = async ctx =>
            {
                await using var scope = app.Services.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
                var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
                var bunnyNet = scope.ServiceProvider.GetRequiredService<IBunnyNetService>();

                var tusFile = await tusStore.GetFileAsync(ctx.FileId, CancellationToken.None);
                var metadata = await tusFile.GetMetadataAsync(CancellationToken.None);

                if (!metadata.TryGetValue("videoId", out var videoIdMeta))
                    return;

                var videoIdString = videoIdMeta.GetString(Encoding.UTF8);
                if (!Guid.TryParse(videoIdString, out var videoId))
                    return;

                var video = await dbContext.Videos.FirstOrDefaultAsync(v => v.Id == videoId);
                if (video == null) return;

                var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == video.TenantId);
                if (tenant == null) return;
                tenantContext.SetCurrentTenant(tenant);

                try
                {
                    video.Status = VideoStatus.Processing;
                    video.UpdatedAt = DateTimeOffset.UtcNow;
                    await dbContext.SaveChangesAsync();

                    await using var stream = await tusFile.GetContentAsync(CancellationToken.None);

                    var fileName = metadata.TryGetValue("fileName", out var fn) ? fn.GetString(Encoding.UTF8) : (video.OriginalFileName ?? "video");
                    var contentType = metadata.TryGetValue("contentType", out var ct) ? ct.GetString(Encoding.UTF8) : (video.MimeType ?? "application/octet-stream");

                    var result = await bunnyNet.UploadVideoToStreamAsync(stream, fileName ?? "video", contentType ?? "application/octet-stream");

                    video.StoragePath = result.VideoId;
                    video.Status = VideoStatus.Uploaded;
                    video.ThumbnailPath = result.ThumbnailUrl ?? video.ThumbnailPath;
                    video.UpdatedAt = DateTimeOffset.UtcNow;
                    await dbContext.SaveChangesAsync();
                }
                catch
                {
                    video.Status = VideoStatus.Failed;
                    video.UpdatedAt = DateTimeOffset.UtcNow;
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    };
});

// Apply migrations + seed demo data
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
