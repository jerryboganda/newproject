using StreamVault.Application.Notifications.DTOs;

namespace StreamVault.Application.Notifications;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    Task<NotificationDto?> GetNotificationAsync(Guid notificationId, Guid userId, Guid tenantId);
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request, Guid tenantId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, Guid tenantId);
    Task MarkAllAsReadAsync(Guid userId, Guid tenantId);
    Task DeleteNotificationAsync(Guid notificationId, Guid userId, Guid tenantId);
    Task<int> GetUnreadCountAsync(Guid userId, Guid tenantId);
    
    // Real-time notification methods
    Task<bool> SendNotificationAsync(Guid userId, Guid tenantId, CreateNotificationRequest request);
    Task<bool> SendBulkNotificationAsync(List<Guid> userIds, Guid tenantId, CreateNotificationRequest request);
    Task<bool> SendRealTimeNotificationAsync(Guid userId, Guid tenantId, RealTimeNotificationRequest request);
    Task<bool> SubscribeToNotificationsAsync(Guid userId, string connectionId);
    Task<bool> UnsubscribeFromNotificationsAsync(Guid userId, string connectionId);
    Task<List<string>> GetUserConnectionsAsync(Guid userId);
    
    // Notification preferences and templates
    Task<bool> UpdateNotificationPreferencesAsync(Guid userId, Guid tenantId, NotificationPreferencesDto preferences);
    Task<NotificationPreferencesDto> GetNotificationPreferencesAsync(Guid userId, Guid tenantId);
    Task<List<NotificationTemplateDto>> GetNotificationTemplatesAsync(Guid tenantId);
    Task<bool> CreateNotificationTemplateAsync(Guid tenantId, CreateNotificationTemplateRequest request);
    
    // Multi-channel notifications
    Task<bool> SendPushNotificationAsync(Guid userId, Guid tenantId, PushNotificationRequest request);
    Task<bool> SendEmailNotificationAsync(Guid userId, Guid tenantId, EmailNotificationRequest request);
    Task<bool> SendSMSNotificationAsync(Guid userId, Guid tenantId, SMSNotificationRequest request);
    
    // Analytics and reporting
    Task<List<NotificationAnalyticsDto>> GetNotificationAnalyticsAsync(Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<List<NotificationDto>> GetNotificationsByTypeAsync(Guid userId, Guid tenantId, string type, int page = 1, int pageSize = 20);
}
