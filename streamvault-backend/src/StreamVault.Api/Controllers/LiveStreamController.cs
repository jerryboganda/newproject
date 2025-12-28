using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.LiveStreaming;
using StreamVault.Application.LiveStreaming.DTOs;
using StreamVault.Domain.Entities;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LiveStreamController : ControllerBase
{
    private readonly ILiveStreamingService _liveStreamingService;

    public LiveStreamController(ILiveStreamingService liveStreamingService)
    {
        _liveStreamingService = liveStreamingService;
    }

    [HttpPost]
    public async Task<ActionResult<LiveStreamDto>> CreateStream([FromBody] CreateLiveStreamRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var stream = await _liveStreamingService.CreateStreamAsync(request, userId, tenantId);
            return Ok(stream);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{streamId}")]
    public async Task<ActionResult<LiveStreamDto>> GetStream(Guid streamId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var stream = await _liveStreamingService.GetStreamAsync(streamId, userId, tenantId);
            return Ok(stream);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("my-streams")]
    public async Task<ActionResult<List<LiveStreamDto>>> GetUserStreams([FromQuery] LiveStreamStatus? status = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var streams = await _liveStreamingService.GetUserStreamsAsync(userId, tenantId, status);
            return Ok(streams);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<List<LiveStreamDto>>> GetActiveStreams([FromQuery] int? limit = null, [FromQuery] Guid? tenantId = null)
    {
        try
        {
            // If no tenantId provided, use the authenticated user's tenant
            var currentTenantId = tenantId;
            if (!currentTenantId.HasValue && User.Identity.IsAuthenticated)
            {
                currentTenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            }

            if (!currentTenantId.HasValue)
                return BadRequest(new { error = "Tenant ID is required" });

            var streams = await _liveStreamingService.GetActiveStreamsAsync(currentTenantId.Value, limit);
            return Ok(streams);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{streamId}/start")]
    public async Task<ActionResult<LiveStreamDto>> StartStream(Guid streamId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var stream = await _liveStreamingService.StartStreamAsync(streamId, userId, tenantId);
            return Ok(stream);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{streamId}/end")]
    public async Task<ActionResult<LiveStreamDto>> EndStream(Guid streamId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var stream = await _liveStreamingService.EndStreamAsync(streamId, userId, tenantId);
            return Ok(stream);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{streamId}")]
    public async Task<ActionResult<LiveStreamDto>> UpdateStream(Guid streamId, [FromBody] UpdateLiveStreamRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var stream = await _liveStreamingService.UpdateStreamAsync(streamId, request, userId, tenantId);
            return Ok(stream);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{streamId}")]
    public async Task<IActionResult> DeleteStream(Guid streamId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _liveStreamingService.DeleteStreamAsync(streamId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{streamId}/access")]
    public async Task<ActionResult<StreamAccessDto>> GetStreamAccess(Guid streamId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var access = await _liveStreamingService.GetStreamAccessAsync(streamId, userId, tenantId);
            return Ok(access);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{streamId}/join")]
    public async Task<IActionResult> JoinStream(Guid streamId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _liveStreamingService.JoinStreamAsync(streamId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{streamId}/leave")]
    public async Task<IActionResult> LeaveStream(Guid streamId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _liveStreamingService.LeaveStreamAsync(streamId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{streamId}/chat")]
    public async Task<ActionResult<LiveStreamChatMessageDto>> SendChatMessage(Guid streamId, [FromBody] SendChatMessageRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var message = await _liveStreamingService.SendChatMessageAsync(streamId, request, userId, tenantId);
            return Ok(message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{streamId}/chat")]
    public async Task<ActionResult<List<LiveStreamChatMessageDto>>> GetChatMessages(Guid streamId, [FromQuery] int? limit = null, [FromQuery] DateTimeOffset? before = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var messages = await _liveStreamingService.GetChatMessagesAsync(streamId, userId, tenantId, limit, before);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{streamId}/stats")]
    public async Task<ActionResult<LiveStreamStatsDto>> GetStreamStats(Guid streamId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var stats = await _liveStreamingService.GetStreamStatsAsync(streamId, userId, tenantId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
