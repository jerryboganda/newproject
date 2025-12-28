using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Notifications;
using StreamVault.Application.Notifications.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var notifications = await _notificationService.GetNotificationsAsync(userId, tenantId, page, pageSize);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{notificationId}")]
    public async Task<ActionResult<NotificationDto>> GetNotification(Guid notificationId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var notification = await _notificationService.GetNotificationAsync(notificationId, userId, tenantId);
            
            if (notification == null)
                return NotFound();

            return Ok(notification);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var notification = await _notificationService.CreateNotificationAsync(request, tenantId);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _notificationService.MarkAsReadAsync(notificationId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _notificationService.MarkAllAsReadAsync(userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var count = await _notificationService.GetUnreadCountAsync(userId, tenantId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<bool>> SendNotification([FromBody] CreateNotificationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _notificationService.SendNotificationAsync(userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<bool>> SendBulkNotification([FromBody] BulkNotificationRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var notificationRequest = new CreateNotificationRequest
            {
                UserId = Guid.Empty, // Will be set for each user
                Title = request.Title,
                Message = request.Message,
                ActionUrl = request.ActionUrl,
                Type = request.Type,
                Channel = request.Channel,
                SendRealTime = request.SendRealTime,
                Metadata = request.Metadata
            };
            
            var result = await _notificationService.SendBulkNotificationAsync(request.UserIds, tenantId, notificationRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("real-time")]
    public async Task<ActionResult<bool>> SendRealTimeNotification([FromBody] RealTimeNotificationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _notificationService.SendRealTimeNotificationAsync(userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferencesDto>> GetNotificationPreferences()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var preferences = await _notificationService.GetNotificationPreferencesAsync(userId, tenantId);
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<bool>> UpdateNotificationPreferences([FromBody] NotificationPreferencesDto preferences)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _notificationService.UpdateNotificationPreferencesAsync(userId, tenantId, preferences);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("templates")]
    public async Task<ActionResult<List<NotificationTemplateDto>>> GetNotificationTemplates()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var templates = await _notificationService.GetNotificationTemplatesAsync(tenantId);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("templates")]
    public async Task<ActionResult<bool>> CreateNotificationTemplate([FromBody] CreateNotificationTemplateRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _notificationService.CreateNotificationTemplateAsync(tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("push")]
    public async Task<ActionResult<bool>> SendPushNotification([FromBody] PushNotificationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _notificationService.SendPushNotificationAsync(userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("email")]
    public async Task<ActionResult<bool>> SendEmailNotification([FromBody] EmailNotificationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _notificationService.SendEmailNotificationAsync(userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("sms")]
    public async Task<ActionResult<bool>> SendSMSNotification([FromBody] SMSNotificationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _notificationService.SendSMSNotificationAsync(userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<List<NotificationAnalyticsDto>>> GetNotificationAnalytics(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var analytics = await _notificationService.GetNotificationAnalyticsAsync(tenantId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<List<NotificationDto>>> GetNotificationsByType(
        string type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var notifications = await _notificationService.GetNotificationsByTypeAsync(userId, tenantId, type, page, pageSize);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(Guid notificationId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _notificationService.DeleteNotificationAsync(notificationId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
