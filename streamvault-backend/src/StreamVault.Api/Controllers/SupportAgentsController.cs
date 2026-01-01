using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/support/agents")]
[Authorize]
public class SupportAgentsController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;

    public SupportAgentsController(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupportAgentDto>>> List(CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();

        var items = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.Status == UserStatus.Active)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new SupportAgentDto(
                u.Id,
                u.FirstName + " " + u.LastName,
                u.Email))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    private Guid RequireTenantId()
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        if (tenantClaim == null || !Guid.TryParse(tenantClaim, out var tenantId))
            throw new UnauthorizedAccessException("Missing tenant_id claim");
        return tenantId;
    }
}

public record SupportAgentDto(Guid Id, string Name, string Email);
