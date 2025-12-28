using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Subscriptions;
using StreamVault.Application.Subscriptions.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetAvailablePlans()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var plans = await _subscriptionService.GetAvailablePlansAsync(tenantId);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("current")]
    public async Task<ActionResult<SubscriptionDto>> GetCurrentSubscription()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(tenantId);
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("subscribe")]
    public async Task<ActionResult<SubscriptionDto>> Subscribe([FromBody] SubscribeRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var subscription = await _subscriptionService.SubscribeToPlanAsync(tenantId, request.PlanId, request);
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelSubscription()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            await _subscriptionService.CancelSubscriptionAsync(tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("update")]
    public async Task<ActionResult<SubscriptionDto>> UpdateSubscription([FromBody] UpdateSubscriptionRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var subscription = await _subscriptionService.UpdateSubscriptionAsync(tenantId, request.PlanId);
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<SubscriptionDto>>> GetSubscriptionHistory()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var history = await _subscriptionService.GetSubscriptionHistoryAsync(tenantId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class UpdateSubscriptionRequest
{
    public Guid PlanId { get; set; }
}
