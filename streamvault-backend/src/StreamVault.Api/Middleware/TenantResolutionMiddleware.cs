using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, StreamVaultDbContext dbContext)
    {
        // Resolve tenant from header or subdomain
        string? tenantIdentifier = null;

        // Check header first (for API calls)
        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var headerValue))
        {
            tenantIdentifier = headerValue.ToString();
        }
        else
        {
            // Check subdomain (for web app calls)
            var host = context.Request.Host.Host;
            var subdomain = GetSubdomain(host);
            if (!string.IsNullOrEmpty(subdomain))
            {
                tenantIdentifier = subdomain;
            }
        }

        if (!string.IsNullOrEmpty(tenantIdentifier))
        {
            // Find tenant by slug
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Slug == tenantIdentifier && t.Status == TenantStatus.Active);

            if (tenant != null)
            {
                // Store tenant in HttpContext
                context.Items["Tenant"] = tenant;
            }
            else
            {
                // Tenant not found or inactive
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { error = "Tenant not found" });
                return;
            }
        }

        await _next(context);
    }

    private string? GetSubdomain(string host)
    {
        // Assuming format: tenant.domain.com
        // Skip if it's the main domain or localhost
        if (host.Contains("localhost") || host == "streamvault.com" || !host.Contains("."))
        {
            return null;
        }

        var parts = host.Split('.');
        if (parts.Length >= 2)
        {
            return parts[0];
        }

        return null;
    }
}
