using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Monetization;
using StreamVault.Application.Monetization.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SubscriptionTierController : ControllerBase
{
    private readonly ISubscriptionTierService _subscriptionTierService;

    public SubscriptionTierController(ISubscriptionTierService subscriptionTierService)
    {
        _subscriptionTierService = subscriptionTierService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SubscriptionTierDto>>> GetSubscriptionTiers()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var tiers = await _subscriptionTierService.GetSubscriptionTiersAsync(tenantId);
            return Ok(tiers);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{tierId}")]
    public async Task<ActionResult<SubscriptionTierDto>> GetSubscriptionTier(Guid tierId)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var tier = await _subscriptionTierService.GetSubscriptionTierAsync(tierId, tenantId);
            return Ok(tier);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionTierDto>> CreateSubscriptionTier([FromBody] CreateSubscriptionTierRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var tier = await _subscriptionTierService.CreateSubscriptionTierAsync(request, tenantId);
            return Ok(tier);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{tierId}")]
    public async Task<ActionResult<SubscriptionTierDto>> UpdateSubscriptionTier(Guid tierId, [FromBody] UpdateSubscriptionTierRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var tier = await _subscriptionTierService.UpdateSubscriptionTierAsync(tierId, request, tenantId);
            return Ok(tier);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{tierId}")]
    public async Task<ActionResult<bool>> DeleteSubscriptionTier(Guid tierId)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _subscriptionTierService.DeleteSubscriptionTierAsync(tierId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{tierId}/subscribe")]
    public async Task<ActionResult<UserSubscriptionDto>> SubscribeToTier(Guid tierId, [FromBody] CreateSubscriptionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var subscription = await _subscriptionTierService.SubscribeToTierAsync(tierId, request, userId, tenantId);
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("subscription/{subscriptionId}/cancel")]
    public async Task<ActionResult<bool>> CancelSubscription(Guid subscriptionId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _subscriptionTierService.CancelSubscriptionAsync(subscriptionId, userId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("subscription/current")]
    public async Task<ActionResult<UserSubscriptionDto>> GetUserSubscription()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var subscription = await _subscriptionTierService.GetUserSubscriptionAsync(userId, tenantId);
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("subscriptions")]
    public async Task<ActionResult<List<UserSubscriptionDto>>> GetUserSubscriptions()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var subscriptions = await _subscriptionTierService.GetUserSubscriptionsAsync(userId, tenantId);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("feature/{featureName}/access")]
    public async Task<ActionResult<bool>> CanUserAccessTierFeature(string featureName)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var hasAccess = await _subscriptionTierService.CanUserAccessTierFeatureAsync(userId, featureName, tenantId);
            return Ok(hasAccess);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("limit/{limitType}")]
    public async Task<ActionResult<int>> GetUserTierLimit(string limitType)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var limit = await _subscriptionTierService.GetUserTierLimitAsync(userId, limitType, tenantId);
            return Ok(limit);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("subscriptions/active")]
    public async Task<ActionResult<List<UserSubscriptionDto>>> GetActiveSubscriptions()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var subscriptions = await _subscriptionTierService.GetActiveSubscriptionsAsync(tenantId);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueDto>> GetSubscriptionRevenue([FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var revenue = await _subscriptionTierService.GetSubscriptionRevenueAsync(tenantId, startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
