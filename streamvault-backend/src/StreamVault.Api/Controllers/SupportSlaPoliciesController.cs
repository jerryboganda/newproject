using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/support/sla-policies")]
[Authorize]
public class SupportSlaPoliciesController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;

    public SupportSlaPoliciesController(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupportSlaPolicyDto>>> List(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var query = _dbContext.SupportSlaPolicies
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId);

        if (!isSuperAdmin)
            query = query.Where(s => s.IsActive);

        var items = await query
            .OrderBy(s => s.Name)
            .Select(s => new SupportSlaPolicyDto(s.Id, s.Name, s.FirstResponseMinutes, s.ResolutionMinutes, s.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<SupportSlaPolicyDto>> Create([FromBody] CreateSupportSlaPolicyRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required" });

        if (request.FirstResponseMinutes <= 0 || request.ResolutionMinutes <= 0)
            return BadRequest(new { error = "Minutes must be greater than 0" });

        var entity = new SupportSlaPolicy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            FirstResponseMinutes = request.FirstResponseMinutes,
            ResolutionMinutes = request.ResolutionMinutes,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.SupportSlaPolicies.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new SupportSlaPolicyDto(entity.Id, entity.Name, entity.FirstResponseMinutes, entity.ResolutionMinutes, entity.IsActive));
    }

    [HttpPut("{policyId:guid}")]
    public async Task<ActionResult<SupportSlaPolicyDto>> Update(Guid policyId, [FromBody] UpdateSupportSlaPolicyRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required" });

        if (request.FirstResponseMinutes <= 0 || request.ResolutionMinutes <= 0)
            return BadRequest(new { error = "Minutes must be greater than 0" });

        var entity = await _dbContext.SupportSlaPolicies
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == policyId, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "SLA policy not found" });

        entity.Name = request.Name.Trim();
        entity.FirstResponseMinutes = request.FirstResponseMinutes;
        entity.ResolutionMinutes = request.ResolutionMinutes;
        entity.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new SupportSlaPolicyDto(entity.Id, entity.Name, entity.FirstResponseMinutes, entity.ResolutionMinutes, entity.IsActive));
    }

    [HttpPut("{policyId:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid policyId, [FromBody] SetSupportSlaPolicyStatusRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        var entity = await _dbContext.SupportSlaPolicies
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == policyId, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "SLA policy not found" });

        entity.IsActive = request.IsActive;
        entity.UpdatedAt = now;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { success = true });
    }

    [HttpDelete("{policyId:guid}")]
    public async Task<IActionResult> Delete(Guid policyId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();

        var entity = await _dbContext.SupportSlaPolicies
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == policyId, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "SLA policy not found" });

        var isReferenced = await _dbContext.SupportDepartments
            .AsNoTracking()
            .AnyAsync(d => d.TenantId == tenantId && d.DefaultSlaPolicyId == policyId, cancellationToken);

        if (isReferenced)
            return Conflict(new { error = "SLA policy is in use by a department. Disable it instead." });

        _dbContext.SupportSlaPolicies.Remove(entity);
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
}

public record SupportSlaPolicyDto(Guid Id, string Name, int FirstResponseMinutes, int ResolutionMinutes, bool IsActive);

public record CreateSupportSlaPolicyRequest(string Name, int FirstResponseMinutes, int ResolutionMinutes);

public record UpdateSupportSlaPolicyRequest(string Name, int FirstResponseMinutes, int ResolutionMinutes);

public record SetSupportSlaPolicyStatusRequest(bool IsActive);
