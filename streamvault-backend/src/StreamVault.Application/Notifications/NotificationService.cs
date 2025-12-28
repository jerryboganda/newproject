using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Notifications.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using StreamVault.Api.Hubs;
using System.Text.Json;

namespace StreamVault.Application.Notifications;

public class NotificationService : INotificationService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(StreamVaultDbContext dbContext, IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var notifications = await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            ActionUrl = n.ActionUrl,
            Type = n.Type,
            IsRead = n.IsRead,
            ReadAt = n.ReadAt,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task<NotificationDto?> GetNotificationAsync(Guid notificationId, Guid userId, Guid tenantId)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
            return null;

        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            ActionUrl = notification.ActionUrl,
            Type = notification.Type,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt
        };
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request, Guid tenantId)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            ActionUrl = request.ActionUrl,
            Type = request.Type,
            IsRead = false,
            ReadAt = DateTimeOffset.MinValue,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            ActionUrl = notification.ActionUrl,
            Type = notification.Type,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt
        };
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, Guid tenantId)
    {
        var notification = await _dbContext.Notifications
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null || notification.User.TenantId != tenantId)
            throw new Exception("Notification not found");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId, Guid tenantId)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var unreadNotifications = await _dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteNotificationAsync(Guid notificationId, Guid userId, Guid tenantId)
    {
        var notification = await _dbContext.Notifications
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null || notification.User.TenantId != tenantId)
            throw new Exception("Notification not found");

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, Guid tenantId)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        return await _dbContext.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    // Real-time notification methods
    public async Task<bool> SendNotificationAsync(Guid userId, Guid tenantId, CreateNotificationRequest request)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // Create notification
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            Message = request.Message,
            ActionUrl = request.ActionUrl,
            Type = request.Type,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        // Send real-time notification if requested
        if (request.SendRealTime)
        {
            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                ActionUrl = notification.ActionUrl,
                Type = notification.Type,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                CreatedAt = notification.CreatedAt,
                Channel = request.Channel,
                Metadata = request.Metadata
            };

            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notificationDto);
        }

        // Send push notification if enabled
        if (request.Channel == NotificationChannel.Push || request.Channel == NotificationChannel.Email)
        {
            // TODO: Implement push/email sending
            _logger.LogInformation($"Sending {request.Channel} notification to user {userId}");
        }

        return true;
    }

    public async Task<bool> SendBulkNotificationAsync(List<Guid> userIds, Guid tenantId, CreateNotificationRequest request)
    {
        var results = new List<bool>();
        
        foreach (var userId in userIds)
        {
            try
            {
                var userRequest = new CreateNotificationRequest
                {
                    UserId = userId,
                    Title = request.Title,
                    Message = request.Message,
                    ActionUrl = request.ActionUrl,
                    Type = request.Type,
                    Channel = request.Channel,
                    SendRealTime = request.SendRealTime,
                    Metadata = request.Metadata
                };
                
                results.Add(await SendNotificationAsync(userId, tenantId, userRequest));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to user {userId}");
                results.Add(false);
            }
        }

        return results.All(r => r);
    }

    public async Task<bool> SendRealTimeNotificationAsync(Guid userId, Guid tenantId, RealTimeNotificationRequest request)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        if (request.Broadcast)
        {
            await _hubContext.Clients.All.SendAsync(request.Event, request.Data);
        }
        else
        {
            await _hubContext.Clients.Group($"User_{userId}").SendAsync(request.Event, request.Data);
        }

        return true;
    }

    public async Task<bool> SubscribeToNotificationsAsync(Guid userId, string connectionId)
    {
        // This is handled by the SignalR hub
        return true;
    }

    public async Task<bool> UnsubscribeFromNotificationsAsync(Guid userId, string connectionId)
    {
        // This is handled by the SignalR hub
        return true;
    }

    public async Task<List<string>> GetUserConnectionsAsync(Guid userId)
    {
        return NotificationHub.GetUserConnections(userId);
    }

    // Notification preferences and templates
    public async Task<bool> UpdateNotificationPreferencesAsync(Guid userId, Guid tenantId, NotificationPreferencesDto preferences)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // Save preferences (would need to create a UserNotificationPreferences entity)
        // For now, just log it
        _logger.LogInformation($"Updated notification preferences for user {userId}");
        return true;
    }

    public async Task<NotificationPreferencesDto> GetNotificationPreferencesAsync(Guid userId, Guid tenantId)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // Return default preferences (would load from database)
        return new NotificationPreferencesDto();
    }

    public async Task<List<NotificationTemplateDto>> GetNotificationTemplatesAsync(Guid tenantId)
    {
        // TODO: Implement template retrieval from database
        return new List<NotificationTemplateDto>();
    }

    public async Task<bool> CreateNotificationTemplateAsync(Guid tenantId, CreateNotificationTemplateRequest request)
    {
        // TODO: Implement template creation
        _logger.LogInformation($"Created notification template '{request.Name}' for tenant {tenantId}");
        return true;
    }

    // Multi-channel notifications
    public async Task<bool> SendPushNotificationAsync(Guid userId, Guid tenantId, PushNotificationRequest request)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // TODO: Implement push notification sending (Firebase, Apple Push, etc.)
        _logger.LogInformation($"Sending push notification to user {userId}: {request.Title}");
        return true;
    }

    public async Task<bool> SendEmailNotificationAsync(Guid userId, Guid tenantId, EmailNotificationRequest request)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // TODO: Implement email sending (SendGrid, SMTP, etc.)
        _logger.LogInformation($"Sending email to user {userId}: {request.Subject}");
        return true;
    }

    public async Task<bool> SendSMSNotificationAsync(Guid userId, Guid tenantId, SMSNotificationRequest request)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // TODO: Implement SMS sending (Twilio, etc.)
        _logger.LogInformation($"Sending SMS to user {userId}: {request.Message}");
        return true;
    }

    // Analytics and reporting
    public async Task<List<NotificationAnalyticsDto>> GetNotificationAnalyticsAsync(Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // TODO: Implement analytics calculation
        return new List<NotificationAnalyticsDto>();
    }

    public async Task<List<NotificationDto>> GetNotificationsByTypeAsync(Guid userId, Guid tenantId, string type, int page = 1, int pageSize = 20)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var notifications = await _dbContext.Notifications
            .Where(n => n.UserId == userId && n.Type.ToString() == type)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            ActionUrl = n.ActionUrl,
            Type = n.Type,
            IsRead = n.IsRead,
            ReadAt = n.ReadAt,
            CreatedAt = n.CreatedAt,
            Channel = NotificationChannel.InApp
        }).ToList();
    }
}
