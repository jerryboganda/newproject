using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/support/departments")]
[Authorize]
public class SupportDepartmentsController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;

    public SupportDepartmentsController(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupportDepartmentDto>>> List(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var query = _dbContext.SupportDepartments
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId);

        if (!isSuperAdmin)
            query = query.Where(d => d.IsActive);

        var items = await query
            .OrderBy(d => d.Name)
            .Select(d => new SupportDepartmentDto(d.Id, d.Name, d.Slug, d.IsActive, d.DefaultSlaPolicyId))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<SupportDepartmentDto>> Create([FromBody] CreateSupportDepartmentRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required" });

        var slug = Slugify(request.Slug ?? request.Name);

        var exists = await _dbContext.SupportDepartments
            .AsNoTracking()
            .AnyAsync(d => d.TenantId == tenantId && d.Slug == slug, cancellationToken);

        if (exists)
            return BadRequest(new { error = "Department slug already exists" });

        var entity = new SupportDepartment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Slug = slug,
            IsActive = true,
            DefaultSlaPolicyId = request.DefaultSlaPolicyId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.SupportDepartments.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new SupportDepartmentDto(entity.Id, entity.Name, entity.Slug, entity.IsActive, entity.DefaultSlaPolicyId));
    }

    [HttpPut("{departmentId:guid}")]
    public async Task<ActionResult<SupportDepartmentDto>> Update(
        Guid departmentId,
        [FromBody] UpdateSupportDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required" });

        var entity = await _dbContext.SupportDepartments
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == departmentId, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "Department not found" });

        var slug = Slugify(request.Slug ?? request.Name);

        var slugExists = await _dbContext.SupportDepartments
            .AsNoTracking()
            .AnyAsync(d => d.TenantId == tenantId && d.Slug == slug && d.Id != departmentId, cancellationToken);

        if (slugExists)
            return BadRequest(new { error = "Department slug already exists" });

        if (request.DefaultSlaPolicyId.HasValue)
        {
            var policyExists = await _dbContext.SupportSlaPolicies
                .AsNoTracking()
                .AnyAsync(s => s.TenantId == tenantId && s.Id == request.DefaultSlaPolicyId.Value, cancellationToken);

            if (!policyExists)
                return BadRequest(new { error = "Invalid default SLA policy" });
        }

        entity.Name = request.Name.Trim();
        entity.Slug = slug;
        entity.DefaultSlaPolicyId = request.DefaultSlaPolicyId;
        entity.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new SupportDepartmentDto(entity.Id, entity.Name, entity.Slug, entity.IsActive, entity.DefaultSlaPolicyId));
    }

    [HttpPut("{departmentId:guid}/status")]
    public async Task<IActionResult> SetStatus(
        Guid departmentId,
        [FromBody] SetSupportDepartmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        var entity = await _dbContext.SupportDepartments
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == departmentId, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "Department not found" });

        entity.IsActive = request.IsActive;
        entity.UpdatedAt = now;
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

    private static string Slugify(string value)
    {
        var raw = value.Trim().ToLowerInvariant();
        var chars = raw
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var collapsed = new string(chars);
        while (collapsed.Contains("--")) collapsed = collapsed.Replace("--", "-");
        return collapsed.Trim('-');
    }
}

public record SupportDepartmentDto(Guid Id, string Name, string Slug, bool IsActive, Guid? DefaultSlaPolicyId);

public record CreateSupportDepartmentRequest(string Name, string? Slug, Guid? DefaultSlaPolicyId);

public record UpdateSupportDepartmentRequest(string Name, string? Slug, Guid? DefaultSlaPolicyId);

public record SetSupportDepartmentStatusRequest(bool IsActive);
