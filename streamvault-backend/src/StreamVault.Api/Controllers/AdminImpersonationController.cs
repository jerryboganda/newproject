using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Services;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "SuperAdmin")]
public class AdminImpersonationController : ControllerBase
{
    private readonly StreamVaultDbContext _db;
    private readonly ITokenService _tokenService;

    public AdminImpersonationController(StreamVaultDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("impersonate")]
    public async Task<ActionResult<ImpersonationResponse>> Impersonate([FromBody] ImpersonationRequest request, CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
            return BadRequest(new { error = "tenantId is required" });

        var tenant = await _db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);
        if (tenant == null)
            return NotFound();

        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.TenantId == request.TenantId)
            .OrderByDescending(u => u.Status == StreamVault.Domain.Entities.UserStatus.Active)
            .ThenBy(u => u.CreatedAt)
            .FirstOrDefaultAsync(u => u.UserRoles.Any(r => r.Role.Name == "BusinessAdmin"), cancellationToken)
            ?? await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.TenantId == request.TenantId)
                .OrderBy(u => u.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return BadRequest(new { error = "Tenant has no users to impersonate" });

        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();

        var accessToken = _tokenService.GenerateAccessToken(
            userId: user.Id,
            email: user.Email,
            firstName: user.FirstName,
            lastName: user.LastName,
            tenantId: user.TenantId,
            roles: roles);

        var targetHost = !string.IsNullOrWhiteSpace(tenant.CustomDomain)
            ? tenant.CustomDomain
            : $"{tenant.Slug}.streamvault.app";

        return Ok(new ImpersonationResponse
        {
            Token = accessToken,
            TenantSlug = tenant.Slug,
            TargetUrl = $"https://{targetHost}"
        });
    }

    public sealed class ImpersonationRequest
    {
        public Guid TenantId { get; set; }
    }

    public sealed class ImpersonationResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TenantSlug { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;
    }
}
