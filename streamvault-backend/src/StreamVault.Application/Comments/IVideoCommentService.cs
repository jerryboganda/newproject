using StreamVault.Application.Comments.DTOs;

namespace StreamVault.Application.Comments;

public interface IVideoCommentService
{
    // Comment CRUD
    Task<VideoCommentDto> AddCommentAsync(Guid videoId, Guid userId, Guid tenantId, CreateCommentRequest request);
    Task<VideoCommentDto> UpdateCommentAsync(Guid commentId, Guid userId, Guid tenantId, UpdateCommentRequest request);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<VideoCommentDto?> GetCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<List<VideoCommentDto>> GetVideoCommentsAsync(Guid videoId, Guid userId, Guid tenantId, CommentSortOption sortBy = CommentSortOption.Newest, int page = 1, int pageSize = 20);
    
    // Comment Replies
    Task<VideoCommentDto> ReplyToCommentAsync(Guid commentId, Guid userId, Guid tenantId, CreateCommentRequest request);
    Task<List<VideoCommentDto>> GetCommentRepliesAsync(Guid commentId, Guid userId, Guid tenantId, int page = 1, int pageSize = 10);
    
    // Comment Interactions
    Task<bool> LikeCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<bool> UnlikeCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<bool> DislikeCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<List<CommentReactionDto>> GetCommentReactionsAsync(Guid commentId, Guid userId, Guid tenantId);
    
    // Comment Moderation
    Task<bool> ReportCommentAsync(Guid commentId, Guid userId, Guid tenantId, ReportCommentRequest request);
    Task<bool> ApproveCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<bool> HideCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<bool> PinCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<bool> UnpinCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<List<ReportedCommentDto>> GetReportedCommentsAsync(Guid tenantId, int page = 1, int pageSize = 20);
    
    // Comment Search and Filtering
    Task<List<VideoCommentDto>> SearchCommentsAsync(Guid videoId, Guid userId, Guid tenantId, SearchCommentsRequest request);
    Task<List<VideoCommentDto>> GetUserCommentsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    Task<List<VideoCommentDto>> GetMentionedCommentsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    
    // Comment Analytics
    Task<CommentAnalyticsDto> GetCommentAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<CommentEngagementDto> GetCommentEngagementAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<List<CommentSentimentDto>> GetCommentSentimentAsync(Guid videoId, Guid userId, Guid tenantId);
    
    // Comment Settings
    Task<CommentSettingsDto> GetCommentSettingsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<bool> UpdateCommentSettingsAsync(Guid videoId, Guid userId, Guid tenantId, UpdateCommentSettingsRequest request);
    
    // Comment Threads
    Task<CommentThreadDto> GetCommentThreadAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<bool> CollapseThreadAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<bool> ExpandThreadAsync(Guid commentId, Guid userId, Guid tenantId);
    
    // Comment Notifications
    Task<bool> SubscribeToCommentsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<bool> UnsubscribeFromCommentsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<List<CommentNotificationDto>> GetCommentNotificationsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    
    // Comment Highlights
    Task<bool> HighlightCommentAsync(Guid commentId, Guid userId, Guid tenantId, string reason);
    Task<bool> UnhighlightCommentAsync(Guid commentId, Guid userId, Guid tenantId);
    Task<List<VideoCommentDto>> GetHighlightedCommentsAsync(Guid videoId, Guid userId, Guid tenantId);
    
    // Comment Exports
    Task<string> ExportCommentsAsync(Guid videoId, Guid userId, Guid tenantId, CommentExportFormat format);
    Task<bool> ImportCommentsAsync(Guid videoId, Guid userId, Guid tenantId, ImportCommentsRequest request);
}
