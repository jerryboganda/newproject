using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/support/escalation-rules")]
[Authorize]
public class SupportEscalationRulesController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;

    public SupportEscalationRulesController(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupportEscalationRuleDto>>> List(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var query = _dbContext.SupportEscalationRules
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId);

        if (!isSuperAdmin)
            query = query.Where(r => r.IsActive);

        var items = await query
            .OrderBy(r => r.Name)
            .Select(r => new SupportEscalationRuleDto(
                r.Id,
                r.Name,
                r.Trigger.ToString(),
                r.ThresholdMinutes,
                r.EscalateToPriority.ToString(),
                r.SetStatusToEscalated,
                r.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<SupportEscalationRuleDto>> Create([FromBody] CreateSupportEscalationRuleRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required" });

        if (request.ThresholdMinutes <= 0)
            return BadRequest(new { error = "ThresholdMinutes must be greater than 0" });

        var entity = new SupportEscalationRule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Trigger = request.Trigger,
            ThresholdMinutes = request.ThresholdMinutes,
            EscalateToPriority = request.EscalateToPriority,
            SetStatusToEscalated = request.SetStatusToEscalated,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.SupportEscalationRules.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new SupportEscalationRuleDto(
            entity.Id,
            entity.Name,
            entity.Trigger.ToString(),
            entity.ThresholdMinutes,
            entity.EscalateToPriority.ToString(),
            entity.SetStatusToEscalated,
            entity.IsActive));
    }

    [HttpPut("{ruleId:guid}")]
    public async Task<ActionResult<SupportEscalationRuleDto>> Update(Guid ruleId, [FromBody] UpdateSupportEscalationRuleRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required" });

        if (request.ThresholdMinutes <= 0)
            return BadRequest(new { error = "ThresholdMinutes must be greater than 0" });

        var entity = await _dbContext.SupportEscalationRules
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == ruleId, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "Escalation rule not found" });

        entity.Name = request.Name.Trim();
        entity.Trigger = request.Trigger;
        entity.ThresholdMinutes = request.ThresholdMinutes;
        entity.EscalateToPriority = request.EscalateToPriority;
        entity.SetStatusToEscalated = request.SetStatusToEscalated;
        entity.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new SupportEscalationRuleDto(
            entity.Id,
            entity.Name,
            entity.Trigger.ToString(),
            entity.ThresholdMinutes,
            entity.EscalateToPriority.ToString(),
            entity.SetStatusToEscalated,
            entity.IsActive));
    }

    [HttpPut("{ruleId:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid ruleId, [FromBody] SetSupportEscalationRuleStatusRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        var entity = await _dbContext.SupportEscalationRules
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == ruleId, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "Escalation rule not found" });

        entity.IsActive = request.IsActive;
        entity.UpdatedAt = now;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { success = true });
    }

    [HttpDelete("{ruleId:guid}")]
    public async Task<IActionResult> Delete(Guid ruleId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();

        var entity = await _dbContext.SupportEscalationRules
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == ruleId, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "Escalation rule not found" });

        _dbContext.SupportEscalationRules.Remove(entity);
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

public record SupportEscalationRuleDto(
    Guid Id,
    string Name,
    string Trigger,
    int ThresholdMinutes,
    string EscalateToPriority,
    bool SetStatusToEscalated,
    bool IsActive);

public record CreateSupportEscalationRuleRequest(
    string Name,
    SupportEscalationTrigger Trigger,
    int ThresholdMinutes,
    TicketPriority EscalateToPriority,
    bool SetStatusToEscalated);

public record UpdateSupportEscalationRuleRequest(
    string Name,
    SupportEscalationTrigger Trigger,
    int ThresholdMinutes,
    TicketPriority EscalateToPriority,
    bool SetStatusToEscalated);

public record SetSupportEscalationRuleStatusRequest(bool IsActive);
