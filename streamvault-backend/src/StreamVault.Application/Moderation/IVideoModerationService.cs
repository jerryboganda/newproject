using StreamVault.Application.Moderation.DTOs;

namespace StreamVault.Application.Moderation;

public interface IVideoModerationService
{
    Task<ModerationResultDto> ModerateVideoAsync(Guid videoId, Guid tenantId);
    Task<List<ModerationFlagDto>> GetVideoFlagsAsync(Guid videoId, Guid tenantId);
    Task<bool> ApproveVideoAsync(Guid videoId, Guid tenantId, Guid moderatorId);
    Task<bool> RejectVideoAsync(Guid videoId, Guid tenantId, Guid moderatorId, string reason);
    Task<bool> ReportVideoAsync(Guid videoId, Guid userId, Guid tenantId, ReportVideoRequest request);
    Task<List<ReportedVideoDto>> GetReportedVideosAsync(Guid tenantId, int page = 1, int pageSize = 20);
    Task<List<PendingModerationDto>> GetPendingModerationAsync(Guid tenantId, int page = 1, int pageSize = 20);
    Task<ModerationStatsDto> GetModerationStatsAsync(Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<bool> UpdateModerationSettingsAsync(Guid tenantId, ModerationSettingsDto settings);
    Task<ModerationSettingsDto> GetModerationSettingsAsync(Guid tenantId);
    Task<List<ModerationActionDto>> GetModerationHistoryAsync(Guid videoId, Guid tenantId);
    Task<bool> AppealModerationDecisionAsync(Guid videoId, Guid userId, Guid tenantId, string appealReason);
    Task<List<AutoModeratedContentDto>> GetAutoModeratedContentAsync(Guid tenantId, int page = 1, int pageSize = 20);
}
