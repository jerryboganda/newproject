using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Admin;
using StreamVault.Application.Admin.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var dashboard = await _adminService.GetDashboardAsync(tenantId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var users = await _adminService.GetUsersAsync(tenantId, page, pageSize);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid userId)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var user = await _adminService.GetUserAsync(userId, tenantId);
            
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("users/{userId}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UserStatusUpdateRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            await _adminService.UpdateUserStatusAsync(userId, request, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("tenants")]
    public async Task<ActionResult<List<TenantDto>>> GetTenants([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var tenants = await _adminService.GetTenantsAsync(page, pageSize);
            return Ok(tenants);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("tenants/{tenantId}")]
    public async Task<ActionResult<TenantDto>> GetTenant(Guid tenantId)
    {
        try
        {
            var tenant = await _adminService.GetTenantAsync(tenantId);
            
            if (tenant == null)
                return NotFound();

            return Ok(tenant);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("tenants/{tenantId}/status")]
    public async Task<IActionResult> UpdateTenantStatus(Guid tenantId, [FromBody] TenantStatusUpdateRequest request)
    {
        try
        {
            await _adminService.UpdateTenantStatusAsync(tenantId, request);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("system/stats")]
    public async Task<ActionResult<SystemStatsDto>> GetSystemStats()
    {
        try
        {
            var stats = await _adminService.GetSystemStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
