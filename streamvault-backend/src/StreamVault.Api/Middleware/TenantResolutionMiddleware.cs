using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StreamVault.Application.Interfaces;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Middleware;

/// <summary>
/// Middleware responsible for resolving the current tenant from the request
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private readonly IOptions<TenantResolutionOptions> _options;
    private const string TenantKey = "Tenant";

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger,
        IOptions<TenantResolutionOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InvokeAsync(
        HttpContext context, 
        StreamVaultDbContext dbContext,
        ITenantContext tenantContext)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (dbContext == null)
            throw new ArgumentNullException(nameof(dbContext));
        if (tenantContext == null)
            throw new ArgumentNullException(nameof(tenantContext));

        try
        {
            // Skip tenant resolution for certain paths
            if (ShouldSkipTenantResolution(context.Request.Path))
            {
                _logger.LogDebug("Skipping tenant resolution for path: {Path}", context.Request.Path);
                await _next(context);
                return;
            }

            // Resolve tenant from the request
            var tenant = await ResolveTenantAsync(context, dbContext);

            if (tenant != null)
            {
                // Set tenant in context
                tenantContext.SetCurrentTenant(tenant);
                context.Items[TenantKey] = tenant;

                // Add tenant headers for debugging
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["X-Tenant-Id"] = tenant.Id.ToString();
                    context.Response.Headers["X-Tenant-Name"] = tenant.Name;
                    context.Response.Headers["X-Tenant-Slug"] = tenant.Slug;
                    return Task.CompletedTask;
                });

                _logger.LogDebug("Tenant resolved: {TenantId} - {TenantName}", tenant.Id, tenant.Name);
            }
            else
            {
                // Handle unresolved tenant
                if (!IsPublicEndpoint(context.Request.Path))
                {
                    _logger.LogWarning("Unable to resolve tenant for host: {Host}", context.Request.Host);
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new { error = "Tenant not found" });
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during tenant resolution");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        await _next(context);
    }

    private async Task<Tenant?> ResolveTenantAsync(HttpContext context, StreamVaultDbContext dbContext)
    {
        string? tenantIdentifier = null;

        // Priority 1: Check header first (for API calls)
        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var headerValue))
        {
            tenantIdentifier = headerValue.ToString();
            _logger.LogDebug("Resolving tenant from header: {TenantSlug}", tenantIdentifier);
        }
        // Priority 2: Check custom domain
        else if (_options.Value.EnableCustomDomains)
        {
            var host = context.Request.Host.Host;
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.CustomDomain == host && t.IsActive());

            if (tenant != null)
            {
                _logger.LogDebug("Resolving tenant from custom domain: {Domain}", host);
                return tenant;
            }
        }
        // Priority 3: Check subdomain
        else if (_options.Value.EnableSubdomains)
        {
            var host = context.Request.Host.Host;
            var subdomain = GetSubdomain(host);
            if (!string.IsNullOrEmpty(subdomain))
            {
                tenantIdentifier = subdomain;
                _logger.LogDebug("Resolving tenant from subdomain: {Subdomain}", subdomain);
            }
        }

        if (!string.IsNullOrEmpty(tenantIdentifier))
        {
            // Find tenant by slug
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Slug == tenantIdentifier && t.IsActive());

            if (tenant != null)
            {
                return tenant;
            }
        }

        return null;
    }

    private bool ShouldSkipTenantResolution(PathString path)
    {
        var skipPaths = _options.Value.SkipTenantResolutionPaths ?? new string[0];
        
        foreach (var skipPath in skipPaths)
        {
            if (path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Skip for static files and health checks
        return path.StartsWithSegments("/static") ||
               path.StartsWithSegments("/_next") ||
               path.StartsWithSegments("/favicon.ico") ||
               path.StartsWithSegments("/health") ||
               path.StartsWithSegments("/metrics") ||
               path.StartsWithSegments("/swagger") ||
               path.StartsWithSegments("/api/docs");
    }

    private bool IsPublicEndpoint(PathString path)
    {
        var publicPaths = _options.Value.PublicPaths ?? new string[0];
        
        foreach (var publicPath in publicPaths)
        {
            if (path.StartsWithSegments(publicPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Public API endpoints
        return path.StartsWithSegments("/api/public") ||
               path.StartsWithSegments("/embed") ||
               path.StartsWithSegments("/webhooks") ||
               path.StartsWithSegments("/api/v1/auth/register") ||
               path.StartsWithSegments("/api/v1/auth/login") ||
               path.StartsWithSegments("/api/v1/auth/forgot-password");
    }

    private string? GetSubdomain(string host)
    {
        // Skip if it's the main domain or localhost
        if (host.Contains("localhost") || 
            host.Equals(_options.Value.BaseDomain, StringComparison.OrdinalIgnoreCase) || 
            !host.Contains("."))
        {
            return null;
        }

        var parts = host.Split('.');
        if (parts.Length >= 2)
        {
            // Extract subdomain, excluding www
            var subdomain = parts[0];
            if (subdomain.Equals("www", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return subdomain;
        }

        return null;
    }
}

/// <summary>
/// Configuration options for tenant resolution
/// </summary>
public class TenantResolutionOptions
{
    /// <summary>
    /// Paths to skip tenant resolution
    /// </summary>
    public string[]? SkipTenantResolutionPaths { get; set; }

    /// <summary>
    /// Paths that are public and don't require tenant resolution
    /// </summary>
    public string[]? PublicPaths { get; set; }

    /// <summary>
    /// Base domain for subdomain-based tenant resolution
    /// </summary>
    public string? BaseDomain { get; set; } = "streamvault.com";

    /// <summary>
    /// Enable custom domain support
    /// </summary>
    public bool EnableCustomDomains { get; set; } = true;

    /// <summary>
    /// Enable subdomain support
    /// </summary>
    public bool EnableSubdomains { get; set; } = true;
}
