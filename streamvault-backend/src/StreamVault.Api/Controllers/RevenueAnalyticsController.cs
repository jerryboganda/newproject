using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Analytics;
using StreamVault.Application.Analytics.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RevenueAnalyticsController : ControllerBase
{
    private readonly IRevenueAnalyticsService _revenueAnalyticsService;

    public RevenueAnalyticsController(IRevenueAnalyticsService revenueAnalyticsService)
    {
        _revenueAnalyticsService = revenueAnalyticsService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<RevenueOverviewDto>> GetRevenueOverview(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var overview = await _revenueAnalyticsService.GetRevenueOverviewAsync(userId, tenantId, startDate, endDate);
            return Ok(overview);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("trends")]
    public async Task<ActionResult<List<RevenueTrendDto>>> GetRevenueTrends(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? period = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var trends = await _revenueAnalyticsService.GetRevenueTrendsAsync(userId, tenantId, startDate, endDate, period);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("top-videos")]
    public async Task<ActionResult<List<TopEarningVideoDto>>> GetTopEarningVideos(
        [FromQuery] int limit = 10,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var topVideos = await _revenueAnalyticsService.GetTopEarningVideosAsync(userId, tenantId, limit, startDate, endDate);
            return Ok(topVideos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("by-source")]
    public async Task<ActionResult<RevenueBySourceDto>> GetRevenueBySource(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var revenueBySource = await _revenueAnalyticsService.GetRevenueBySourceAsync(userId, tenantId, startDate, endDate);
            return Ok(revenueBySource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("by-country")]
    public async Task<ActionResult<RevenueByCountryDto>> GetRevenueByCountry(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var revenueByCountry = await _revenueAnalyticsService.GetRevenueByCountryAsync(userId, tenantId, startDate, endDate);
            return Ok(revenueByCountry);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("subscribers")]
    public async Task<ActionResult<SubscriberAnalyticsDto>> GetSubscriberAnalytics(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var subscriberAnalytics = await _revenueAnalyticsService.GetSubscriberAnalyticsAsync(userId, tenantId, startDate, endDate);
            return Ok(subscriberAnalytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("forecast")]
    public async Task<ActionResult<RevenueForecastDto>> GetRevenueForecast([FromQuery] int months = 6)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var forecast = await _revenueAnalyticsService.GetRevenueForecastAsync(userId, tenantId, months);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("monthly-report")]
    public async Task<ActionResult<List<MonthlyRevenueReportDto>>> GetMonthlyRevenueReport([FromQuery] int months = 12)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var report = await _revenueAnalyticsService.GetMonthlyRevenueReportAsync(userId, tenantId, months);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<RevenueMetricsDto>> GetRevenueMetrics(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var metrics = await _revenueAnalyticsService.GetRevenueMetricsAsync(userId, tenantId, startDate, endDate);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
