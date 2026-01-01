using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize(Roles = "SuperAdmin")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly StreamVaultDbContext _db;

    public AdminAuditLogsController(StreamVaultDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<AuditLogListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? action = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.AuditLogs.AsNoTracking();

        if (tenantId.HasValue) query = query.Where(a => a.TenantId == tenantId.Value);
        if (userId.HasValue) query = query.Where(a => a.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(action))
        {
            var term = action.Trim();
            query = query.Where(a => EF.Functions.ILike(a.Action, $"%{term}%"));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogListItem(
                a.Id,
                a.TenantId,
                a.UserId,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.IpAddress,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(new AuditLogListResponse(items, total, page, pageSize));
    }

    public sealed record AuditLogListItem(
        Guid Id,
        Guid TenantId,
        Guid UserId,
        string Action,
        string EntityType,
        Guid? EntityId,
        string? IpAddress,
        DateTime CreatedAt);

    public sealed record AuditLogListResponse(
        IReadOnlyList<AuditLogListItem> Items,
        int Total,
        int Page,
        int PageSize);
}
