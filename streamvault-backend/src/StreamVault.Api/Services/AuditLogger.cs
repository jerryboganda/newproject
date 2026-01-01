using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Services;

public sealed class AuditLogger
{
    private readonly StreamVaultDbContext _db;

    public AuditLogger(StreamVaultDbContext db)
    {
        _db = db;
    }

    public async Task TryLogAsync(
        HttpContext httpContext,
        string action,
        string entityType,
        Guid? entityId,
        Dictionary<string, object>? oldValues,
        Dictionary<string, object>? newValues,
        Guid? tenantIdOverride = null,
        CancellationToken cancellationToken = default)
    {
        if (httpContext.User?.Identity?.IsAuthenticated != true)
            return;

        var userId = GetUserId(httpContext.User);
        if (userId == null)
            return;

        var tenantId = tenantIdOverride ?? GetTenantId(httpContext.User);
        if (tenantId == null)
            return;

        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            UserId = userId.Value,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(audit);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    private static Guid? GetTenantId(ClaimsPrincipal user)
    {
        var tid = user.FindFirstValue("tenant_id");
        return Guid.TryParse(tid, out var guid) ? guid : null;
    }
}
