using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StreamVault.Application.Interfaces;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Services
{
    /// <summary>
    /// Implementation of ITenantResolver with multiple resolution strategies
    /// </summary>
    public class TenantResolver : ITenantResolver
    {
        private readonly StreamVaultDbContext _dbContext;
        private readonly ILogger<TenantResolver> _logger;
        private readonly IOptions<TenantResolutionOptions> _options;

        public TenantResolver(
            StreamVaultDbContext dbContext,
            ILogger<TenantResolver> logger,
            IOptions<TenantResolutionOptions> options)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<Tenant?> ResolveTenantAsync(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Tenant? tenant = null;

            // Strategy 1: Resolve from header (highest priority for API calls)
            tenant = await ResolveFromHeaderAsync(request);
            if (tenant != null)
            {
                _logger.LogDebug("Tenant resolved from header: {TenantId}", tenant.Id);
                return tenant;
            }

            // Strategy 2: Resolve from custom domain
            if (_options.Value.EnableCustomDomains)
            {
                tenant = await ResolveFromCustomDomainAsync(request);
                if (tenant != null)
                {
                    _logger.LogDebug("Tenant resolved from custom domain: {TenantId}", tenant.Id);
                    return tenant;
                }
            }

            // Strategy 3: Resolve from subdomain
            if (_options.Value.EnableSubdomains)
            {
                tenant = await ResolveFromSubdomainAsync(request);
                if (tenant != null)
                {
                    _logger.LogDebug("Tenant resolved from subdomain: {TenantId}", tenant.Id);
                    return tenant;
                }
            }

            _logger.LogDebug("No tenant could be resolved from request");
            return null;
        }

        private async Task<Tenant?> ResolveFromHeaderAsync(HttpRequest request)
        {
            if (request.Headers.TryGetValue("X-Tenant-Slug", out var headerValue))
            {
                var slug = headerValue.ToString();
                if (!string.IsNullOrWhiteSpace(slug))
                {
                    return await _dbContext.Tenants
                        .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive());
                }
            }

            return null;
        }

        private async Task<Tenant?> ResolveFromCustomDomainAsync(HttpRequest request)
        {
            var host = request.Host.Host;
            if (string.IsNullOrWhiteSpace(host))
                return null;

            return await _dbContext.Tenants
                .FirstOrDefaultAsync(t => 
                    t.CustomDomain != null && 
                    t.CustomDomain.ToLowerInvariant() == host.ToLowerInvariant() && 
                    t.IsActive());
        }

        private async Task<Tenant?> ResolveFromSubdomainAsync(HttpRequest request)
        {
            var host = request.Host.Host;
            if (string.IsNullOrWhiteSpace(host))
                return null;

            var subdomain = ExtractSubdomain(host);
            if (string.IsNullOrWhiteSpace(subdomain))
                return null;

            return await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Slug == subdomain && t.IsActive());
        }

        private string? ExtractSubdomain(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return null;

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
                return subdomain.ToLowerInvariant();
            }

            return null;
        }
    }
}
