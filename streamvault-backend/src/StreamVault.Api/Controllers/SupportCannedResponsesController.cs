using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/support/canned-responses")]
[Authorize]
public class SupportCannedResponsesController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;

    public SupportCannedResponsesController(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CannedResponseDto>>> List(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var query = _dbContext.CannedResponses
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId);

        if (!isSuperAdmin)
            query = query.Where(r => r.IsActive);

        var items = await query
            .OrderByDescending(r => r.UpdatedAt)
            .Take(200)
            .Select(r => new CannedResponseDto(
                r.Id,
                r.Name,
                r.Content,
                r.Category,
                r.Shortcuts,
                r.IsActive,
                r.UsageCount,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<CannedResponseDto>> Create([FromBody] CreateCannedResponseRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Name and content are required" });

        var entity = new CannedResponse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Content = request.Content.Trim(),
            Category = (request.Category ?? "general").Trim(),
            Shortcuts = request.Shortcuts?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct().ToList() ?? new(),
            IsActive = true,
            UsageCount = 0,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = userId
        };

        _dbContext.CannedResponses.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CannedResponseDto(entity.Id, entity.Name, entity.Content, entity.Category, entity.Shortcuts, entity.IsActive, entity.UsageCount, entity.CreatedAt, entity.UpdatedAt));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCannedResponseRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var entity = await _dbContext.CannedResponses
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "Canned response not found" });

        if (request.Name != null) entity.Name = request.Name.Trim();
        if (request.Content != null) entity.Content = request.Content.Trim();
        if (request.Category != null) entity.Category = request.Category.Trim();
        if (request.Shortcuts != null) entity.Shortcuts = request.Shortcuts.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct().ToList();
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { success = true });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var entity = await _dbContext.CannedResponses
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "Canned response not found" });

        _dbContext.CannedResponses.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { success = true });
    }

    private Guid RequireTenantId()
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        if (tenantClaim == null || !Guid.TryParse(tenantClaim, out var tenantId))
            throw new UnauthorizedAccessException("Missing tenant_id claim");
        return tenantId;
    }

    private Guid RequireUserId()
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userClaim == null || !Guid.TryParse(userClaim, out var userId))
            throw new UnauthorizedAccessException("Missing user id claim");
        return userId;
    }
}

public record CannedResponseDto(
    Guid Id,
    string Name,
    string Content,
    string Category,
    IReadOnlyList<string> Shortcuts,
    bool IsActive,
    int UsageCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateCannedResponseRequest(
    string Name,
    string Content,
    string? Category,
    IReadOnlyList<string>? Shortcuts);

public record UpdateCannedResponseRequest(
    string? Name,
    string? Content,
    string? Category,
    IReadOnlyList<string>? Shortcuts,
    bool? IsActive);
