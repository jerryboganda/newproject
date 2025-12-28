using StreamVault.Application.LiveStreaming.DTOs;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.LiveStreaming;

public interface ILiveStreamingService
{
    Task<LiveStreamDto> CreateStreamAsync(CreateLiveStreamRequest request, Guid userId, Guid tenantId);
    Task<LiveStreamDto> GetStreamAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<List<LiveStreamDto>> GetUserStreamsAsync(Guid userId, Guid tenantId, LiveStreamStatus? status = null);
    Task<List<LiveStreamDto>> GetActiveStreamsAsync(Guid tenantId, int? limit = null);
    Task<LiveStreamDto> StartStreamAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<LiveStreamDto> EndStreamAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<LiveStreamDto> UpdateStreamAsync(Guid streamId, UpdateLiveStreamRequest request, Guid userId, Guid tenantId);
    Task DeleteStreamAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<StreamAccessDto> GetStreamAccessAsync(Guid streamId, Guid userId, Guid tenantId);
    Task JoinStreamAsync(Guid streamId, Guid userId, Guid tenantId);
    Task LeaveStreamAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<LiveStreamChatMessageDto> SendChatMessageAsync(Guid streamId, SendChatMessageRequest request, Guid userId, Guid tenantId);
    Task<List<LiveStreamChatMessageDto>> GetChatMessagesAsync(Guid streamId, Guid userId, Guid tenantId, int? limit = null, DateTimeOffset? before = null);
    Task<LiveStreamStatsDto> GetStreamStatsAsync(Guid streamId, Guid userId, Guid tenantId);
    
    // Enhanced live streaming features
    Task<bool> PauseStreamAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<bool> ResumeStreamAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<List<LiveStreamDto>> GetScheduledStreamsAsync(Guid tenantId, int page = 1, int pageSize = 20);
    Task<string> GetStreamKeyAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<string> GetPlaybackUrlAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<List<LiveStreamViewerDto>> GetStreamViewersAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<LiveStreamAnalyticsDto> GetStreamAnalyticsAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<bool> BanUserAsync(Guid streamId, Guid userId, Guid tenantId, Guid userToBanId);
    Task<bool> UnbanUserAsync(Guid streamId, Guid userId, Guid tenantId, Guid userToUnbanId);
    Task<List<Guid>> GetBannedUsersAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<bool> RecordStreamAsync(Guid streamId, Guid userId, Guid tenantId, bool enableRecording);
    Task<LiveStreamRecordingDto> GetRecordingAsync(Guid recordingId, Guid userId, Guid tenantId);
    Task<List<LiveStreamRecordingDto>> GetRecordingsAsync(Guid streamId, Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    Task<bool> SendStreamNotificationAsync(Guid streamId, Guid userId, Guid tenantId, string message);
    Task<bool> UpdateStreamSettingsAsync(Guid streamId, Guid userId, Guid tenantId, StreamSettingsDto settings);
    Task<StreamSettingsDto> GetStreamSettingsAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<bool> StartPollAsync(Guid streamId, Guid userId, Guid tenantId, CreatePollRequest request);
    Task<bool> VotePollAsync(Guid streamId, Guid userId, Guid tenantId, Guid pollId, int optionIndex);
    Task<List<StreamPollDto>> GetActivePollsAsync(Guid streamId, Guid userId, Guid tenantId);
    Task<bool> EndPollAsync(Guid streamId, Guid userId, Guid tenantId, Guid pollId);
}
