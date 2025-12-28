using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Monetization;
using StreamVault.Application.Monetization.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VideoMonetizationController : ControllerBase
{
    private readonly IVideoMonetizationService _monetizationService;

    public VideoMonetizationController(IVideoMonetizationService monetizationService)
    {
        _monetizationService = monetizationService;
    }

    [HttpGet("video/{videoId}")]
    public async Task<ActionResult<VideoMonetizationDto>> GetVideoMonetization(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var monetization = await _monetizationService.GetVideoMonetizationAsync(videoId, userId, tenantId);
            return Ok(monetization);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("video/{videoId}")]
    public async Task<ActionResult<VideoMonetizationDto>> UpdateVideoMonetization(Guid videoId, [FromBody] UpdateVideoMonetizationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var monetization = await _monetizationService.UpdateVideoMonetizationAsync(videoId, request, userId, tenantId);
            return Ok(monetization);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/enable")]
    public async Task<ActionResult<VideoMonetizationDto>> EnableMonetization(Guid videoId, [FromBody] EnableMonetizationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var monetization = await _monetizationService.EnableMonetizationAsync(videoId, request, userId, tenantId);
            return Ok(monetization);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/disable")]
    public async Task<ActionResult<bool>> DisableMonetization(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _monetizationService.DisableMonetizationAsync(videoId, userId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/purchase")]
    public async Task<ActionResult<VideoPurchaseDto>> PurchaseVideo(Guid videoId, [FromBody] PurchaseVideoRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var purchase = await _monetizationService.PurchaseVideoAsync(videoId, request, userId, tenantId);
            return Ok(purchase);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/rent")]
    public async Task<ActionResult<VideoRentalDto>> RentVideo(Guid videoId, [FromBody] RentVideoRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var rental = await _monetizationService.RentVideoAsync(videoId, request, userId, tenantId);
            return Ok(rental);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/access")]
    public async Task<ActionResult<bool>> CheckVideoAccess(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var hasAccess = await _monetizationService.CheckVideoAccessAsync(videoId, userId, tenantId);
            return Ok(hasAccess);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("purchases")]
    public async Task<ActionResult<List<VideoPurchaseDto>>> GetUserPurchases([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var purchases = await _monetizationService.GetUserPurchasesAsync(userId, tenantId, page, pageSize);
            return Ok(purchases);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("rentals")]
    public async Task<ActionResult<List<VideoRentalDto>>> GetUserRentals([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var rentals = await _monetizationService.GetUserRentalsAsync(userId, tenantId, page, pageSize);
            return Ok(rentals);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/revenue")]
    public async Task<ActionResult<RevenueDto>> GetVideoRevenue(Guid videoId, [FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var revenue = await _monetizationService.GetVideoRevenueAsync(videoId, userId, tenantId, startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<List<RevenueDto>>> GetCreatorRevenue([FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var revenue = await _monetizationService.GetCreatorRevenueAsync(userId, tenantId, startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("ad-revenue")]
    public async Task<ActionResult<AdRevenueDto>> RecordAdRevenue([FromBody] RecordAdRevenueRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var adRevenue = await _monetizationService.RecordAdRevenueAsync(request, userId, tenantId);
            return Ok(adRevenue);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/sponsorship")]
    public async Task<ActionResult<SponsorshipDto>> CreateSponsorship(Guid videoId, [FromBody] CreateSponsorshipRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var sponsorship = await _monetizationService.CreateSponsorshipAsync(videoId, request, userId, tenantId);
            return Ok(sponsorship);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/can-watch")]
    public async Task<ActionResult<bool>> CanUserWatchVideo(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var canWatch = await _monetizationService.CanUserWatchVideoAsync(videoId, userId, tenantId);
            return Ok(canWatch);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
