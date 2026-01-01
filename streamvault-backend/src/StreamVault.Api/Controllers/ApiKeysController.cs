using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Api.Services;
using StreamVault.Application.Interfaces;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/api-keys")]
[Authorize]
public class ApiKeysController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly AuditLogger _audit;

    public ApiKeysController(StreamVaultDbContext dbContext, ITenantContext tenantContext, AuditLogger audit)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApiKeyListItem>>> List(CancellationToken cancellationToken = default)
    {
        var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
        if (tenantId == null)
            return BadRequest(new { error = "Tenant could not be resolved" });

        var userId = GetRequiredUserId();

        var items = await _dbContext.ApiKeys
            .AsNoTracking()
            .Where(k => k.TenantId == tenantId && k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new ApiKeyListItem(
                k.Id,
                k.Name,
                k.KeyPrefix,
                k.IsActive,
                k.Scopes,
                k.CreatedAt,
                k.ExpiresAt,
                k.LastUsedAt == default ? null : k.LastUsedAt))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<CreateApiKeyResponse>> Create([FromBody] CreateApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
        if (tenantId == null)
            return BadRequest(new { error = "Tenant could not be resolved" });

        var userId = GetRequiredUserId();

        var now = DateTime.UtcNow;
        var expiresAt = request.ExpiresAtUtc ?? now.AddDays(90);
        if (expiresAt <= now.AddMinutes(1))
            return BadRequest(new { error = "ExpiresAtUtc must be in the future" });

        var normalizedScopes = (request.Scopes ?? Array.Empty<string>())
            .Select(s => (s ?? string.Empty).Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedScopes.Length == 0)
            return BadRequest(new { error = "At least one scope is required" });

        var apiKeyRaw = GenerateApiKey(out var prefix);
        var hash = ComputeSha256Hex(apiKeyRaw);

        var entity = new ApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            UserId = userId,
            Name = string.IsNullOrWhiteSpace(request.Name) ? "API Key" : request.Name.Trim(),
            KeyPrefix = prefix,
            KeyHash = hash,
            Scopes = normalizedScopes,
            ExpiresAt = expiresAt,
            LastUsedAt = now,
            IsActive = true,
            CreatedAt = now
        };

        _dbContext.ApiKeys.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "api_key.created",
            entityType: "ApiKey",
            entityId: entity.Id,
            oldValues: null,
            newValues: new Dictionary<string, object>
            {
                ["name"] = entity.Name,
                ["keyPrefix"] = entity.KeyPrefix,
                ["scopes"] = entity.Scopes,
                ["expiresAtUtc"] = entity.ExpiresAt
            },
            tenantIdOverride: tenantId,
            cancellationToken: cancellationToken);

        return Ok(new CreateApiKeyResponse(
            entity.Id,
            entity.Name,
            entity.KeyPrefix,
            apiKeyRaw,
            entity.Scopes,
            entity.CreatedAt,
            entity.ExpiresAt));
    }

    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
        if (tenantId == null)
            return BadRequest(new { error = "Tenant could not be resolved" });

        var userId = GetRequiredUserId();

        var apiKey = await _dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == id && k.TenantId == tenantId && k.UserId == userId, cancellationToken);

        if (apiKey == null)
            return NotFound(new { error = "API key not found" });

        var oldActive = apiKey.IsActive;
        apiKey.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "api_key.revoked",
            entityType: "ApiKey",
            entityId: apiKey.Id,
            oldValues: new Dictionary<string, object>
            {
                ["isActive"] = oldActive
            },
            newValues: new Dictionary<string, object>
            {
                ["isActive"] = apiKey.IsActive,
                ["keyPrefix"] = apiKey.KeyPrefix
            },
            tenantIdOverride: tenantId,
            cancellationToken: cancellationToken);

        return Ok(new { success = true });
    }

    private Guid GetRequiredUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(id, out var userId))
            throw new InvalidOperationException("User id missing from token");
        return userId;
    }

    private async Task<(Guid? tenantId, bool isAuthenticated)> ResolveTenantAsync(CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId.HasValue)
            return (_tenantContext.TenantId, User.Identity?.IsAuthenticated == true);

        var tenantClaim = User.FindFirstValue("tenant_id");
        if (Guid.TryParse(tenantClaim, out var tenantIdFromToken))
        {
            var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantIdFromToken, cancellationToken);
            if (tenant != null)
            {
                _tenantContext.SetCurrentTenant(tenant);
                return (tenant.Id, User.Identity?.IsAuthenticated == true);
            }
        }

        if (Request.Headers.TryGetValue("X-Tenant-Slug", out var slugValues))
        {
            var slug = slugValues.ToString();
            if (!string.IsNullOrWhiteSpace(slug))
            {
                var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
                if (tenant != null)
                {
                    _tenantContext.SetCurrentTenant(tenant);
                    return (tenant.Id, User.Identity?.IsAuthenticated == true);
                }
            }
        }

        return (null, User.Identity?.IsAuthenticated == true);
    }

    private static string GenerateApiKey(out string prefix)
    {
        var secretBytes = RandomNumberGenerator.GetBytes(32);
        var secret = Base64UrlEncode(secretBytes);
        prefix = secret.Length >= 8 ? secret.Substring(0, 8) : secret;
        return $"svk_{prefix}_{secret}";
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string ComputeSha256Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

public record ApiKeyListItem(
    Guid Id,
    string Name,
    string KeyPrefix,
    bool IsActive,
    string[] Scopes,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    DateTime? LastUsedAtUtc);

public record CreateApiKeyRequest(string Name, string[] Scopes, DateTime? ExpiresAtUtc);

public record CreateApiKeyResponse(
    Guid Id,
    string Name,
    string KeyPrefix,
    string ApiKey,
    string[] Scopes,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc);
