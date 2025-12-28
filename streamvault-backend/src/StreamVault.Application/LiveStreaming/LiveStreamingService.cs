using Microsoft.EntityFrameworkCore;
using StreamVault.Application.LiveStreaming.DTOs;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.LiveStreaming;

public class LiveStreamingService : ILiveStreamingService
{
    private readonly StreamVaultDbContext _dbContext;

    public LiveStreamingService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LiveStreamDto> CreateStreamAsync(CreateLiveStreamRequest request, Guid userId, Guid tenantId)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // Generate stream key and URLs
        var streamKey = GenerateStreamKey();
        var ingestUrl = $"rtmp://live.streamvault.com/live/{streamKey}";
        var playbackUrl = $"https://live.streamvault.com/play/{streamKey}.m3u8";

        var liveStream = new LiveStream
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            StreamKey = streamKey,
            IngestUrl = ingestUrl,
            PlaybackUrl = playbackUrl,
            Status = request.ScheduledAt.HasValue ? LiveStreamStatus.Scheduled : LiveStreamStatus.Live,
            ScheduledAt = request.ScheduledAt,
            IsPublic = request.IsPublic,
            AllowChat = request.AllowChat,
            AllowReactions = request.AllowReactions,
            RecordStream = request.RecordStream,
            Category = request.Category,
            Tags = request.Tags,
            MaxDurationMinutes = request.MaxDurationMinutes,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.LiveStreams.Add(liveStream);
        await _dbContext.SaveChangesAsync();

        // Load user data
        await _dbContext.Entry(liveStream).Reference(ls => ls.User).LoadAsync();

        return MapToDto(liveStream);
    }

    public async Task<LiveStreamDto> GetStreamAsync(Guid streamId, Guid userId, Guid tenantId)
    {
        var liveStream = await _dbContext.LiveStreams
            .Include(ls => ls.User)
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null)
            throw new Exception("Stream not found");

        // Check if user can access this stream
        if (!liveStream.IsPublic && liveStream.UserId != userId)
        {
            // Check if user belongs to the same tenant
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

            if (user == null || user.TenantId != liveStream.User.TenantId)
                throw new Exception("Access denied");
        }

        return MapToDto(liveStream);
    }

    public async Task<List<LiveStreamDto>> GetUserStreamsAsync(Guid userId, Guid tenantId, LiveStreamStatus? status = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var query = _dbContext.LiveStreams
            .Include(ls => ls.User)
            .Where(ls => ls.UserId == userId);

        if (status.HasValue)
            query = query.Where(ls => ls.Status == status.Value);

        var streams = await query
            .OrderByDescending(ls => ls.CreatedAt)
            .ToListAsync();

        return streams.Select(MapToDto).ToList();
    }

    public async Task<List<LiveStreamDto>> GetActiveStreamsAsync(Guid tenantId, int? limit = null)
    {
        IQueryable<LiveStream> query = _dbContext.LiveStreams
            .Include(ls => ls.User)
            .Where(ls => ls.Status == LiveStreamStatus.Live && ls.IsPublic)
            .Where(ls => ls.User.TenantId == tenantId)
            .OrderByDescending(ls => ls.StartedAt ?? DateTimeOffset.MinValue);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var streams = await query.ToListAsync();

        return streams.Select(MapToDto).ToList();
    }

    public async Task<LiveStreamDto> StartStreamAsync(Guid streamId, Guid userId, Guid tenantId)
    {
        var liveStream = await _dbContext.LiveStreams
            .Include(ls => ls.User)
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null || liveStream.UserId != userId || liveStream.User.TenantId != tenantId)
            throw new Exception("Stream not found or access denied");

        if (liveStream.Status != LiveStreamStatus.Scheduled)
            throw new Exception("Stream cannot be started");

        liveStream.Status = LiveStreamStatus.Live;
        liveStream.StartedAt = DateTimeOffset.UtcNow;
        liveStream.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(liveStream);
    }

    public async Task<LiveStreamDto> EndStreamAsync(Guid streamId, Guid userId, Guid tenantId)
    {
        var liveStream = await _dbContext.LiveStreams
            .Include(ls => ls.User)
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null || liveStream.UserId != userId || liveStream.User.TenantId != tenantId)
            throw new Exception("Stream not found or access denied");

        if (liveStream.Status != LiveStreamStatus.Live)
            throw new Exception("Stream is not live");

        liveStream.Status = LiveStreamStatus.Ended;
        liveStream.EndedAt = DateTimeOffset.UtcNow;
        liveStream.UpdatedAt = DateTimeOffset.UtcNow;
        liveStream.ConcurrentViewers = 0;

        // Remove all current viewers
        var currentViewers = await _dbContext.LiveStreamViewers
            .Where(lsv => lsv.LiveStreamId == streamId && lsv.LeftAt == null)
            .ToListAsync();

        foreach (var viewer in currentViewers)
        {
            viewer.LeftAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return MapToDto(liveStream);
    }

    public async Task<LiveStreamDto> UpdateStreamAsync(Guid streamId, UpdateLiveStreamRequest request, Guid userId, Guid tenantId)
    {
        var liveStream = await _dbContext.LiveStreams
            .Include(ls => ls.User)
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null || liveStream.UserId != userId || liveStream.User.TenantId != tenantId)
            throw new Exception("Stream not found or access denied");

        if (liveStream.Status == LiveStreamStatus.Ended || liveStream.Status == LiveStreamStatus.Cancelled)
            throw new Exception("Cannot update ended or cancelled stream");

        if (request.Title != null)
            liveStream.Title = request.Title;

        if (request.Description != null)
            liveStream.Description = request.Description;

        if (request.ThumbnailUrl != null)
            liveStream.ThumbnailUrl = request.ThumbnailUrl;

        if (request.ScheduledAt.HasValue)
            liveStream.ScheduledAt = request.ScheduledAt;

        if (request.IsPublic.HasValue)
            liveStream.IsPublic = request.IsPublic.Value;

        if (request.AllowChat.HasValue)
            liveStream.AllowChat = request.AllowChat.Value;

        if (request.AllowReactions.HasValue)
            liveStream.AllowReactions = request.AllowReactions.Value;

        if (request.RecordStream.HasValue)
            liveStream.RecordStream = request.RecordStream.Value;

        if (request.Category != null)
            liveStream.Category = request.Category;

        if (request.Tags != null)
            liveStream.Tags = request.Tags;

        if (request.MaxDurationMinutes.HasValue)
            liveStream.MaxDurationMinutes = request.MaxDurationMinutes.Value;

        liveStream.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(liveStream);
    }

    public async Task DeleteStreamAsync(Guid streamId, Guid userId, Guid tenantId)
    {
        var liveStream = await _dbContext.LiveStreams
            .Include(ls => ls.User)
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null || liveStream.UserId != userId || liveStream.User.TenantId != tenantId)
            throw new Exception("Stream not found or access denied");

        if (liveStream.Status == LiveStreamStatus.Live)
            throw new Exception("Cannot delete live stream");

        _dbContext.LiveStreams.Remove(liveStream);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<StreamAccessDto> GetStreamAccessAsync(Guid streamId, Guid userId, Guid tenantId)
    {
        var liveStream = await _dbContext.LiveStreams
            .Include(ls => ls.User)
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null || liveStream.UserId != userId || liveStream.User.TenantId != tenantId)
            throw new Exception("Stream not found or access denied");

        return new StreamAccessDto
        {
            StreamKey = liveStream.StreamKey ?? string.Empty,
            IngestUrl = liveStream.IngestUrl ?? string.Empty,
            PlaybackUrl = liveStream.PlaybackUrl ?? string.Empty
        };
    }

    public async Task JoinStreamAsync(Guid streamId, Guid userId, Guid tenantId)
    {
        var liveStream = await _dbContext.LiveStreams
            .Include(ls => ls.User)
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null || liveStream.Status != LiveStreamStatus.Live)
            throw new Exception("Stream not found or not live");

        // Check if user is already a viewer
        var existingViewer = await _dbContext.LiveStreamViewers
            .FirstOrDefaultAsync(lsv => lsv.LiveStreamId == streamId && lsv.UserId == userId && lsv.LeftAt == null);

        if (existingViewer == null)
        {
            var viewer = new LiveStreamViewer
            {
                Id = Guid.NewGuid(),
                LiveStreamId = streamId,
                UserId = userId,
                JoinedAt = DateTimeOffset.UtcNow
            };

            _dbContext.LiveStreamViewers.Add(viewer);

            // Update concurrent viewers count
            liveStream.ConcurrentViewers = await _dbContext.LiveStreamViewers
                .CountAsync(lsv => lsv.LiveStreamId == streamId && lsv.LeftAt == null);

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task LeaveStreamAsync(Guid streamId, Guid userId, Guid tenantId)
    {
        var viewer = await _dbContext.LiveStreamViewers
            .FirstOrDefaultAsync(lsv => lsv.LiveStreamId == streamId && lsv.UserId == userId && lsv.LeftAt == null);

        if (viewer != null)
        {
            viewer.LeftAt = DateTimeOffset.UtcNow;

            // Update concurrent viewers count
            var liveStream = await _dbContext.LiveStreams
                .FirstOrDefaultAsync(ls => ls.Id == streamId);

            if (liveStream != null)
            {
                liveStream.ConcurrentViewers = await _dbContext.LiveStreamViewers
                    .CountAsync(lsv => lsv.LiveStreamId == streamId && lsv.LeftAt == null);
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<LiveStreamChatMessageDto> SendChatMessageAsync(Guid streamId, SendChatMessageRequest request, Guid userId, Guid tenantId)
    {
        var liveStream = await _dbContext.LiveStreams
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null || liveStream.Status != LiveStreamStatus.Live)
            throw new Exception("Stream not found or not live");

        if (!liveStream.AllowChat)
            throw new Exception("Chat is disabled for this stream");

        var message = new LiveStreamChatMessage
        {
            Id = Guid.NewGuid(),
            LiveStreamId = streamId,
            UserId = userId,
            Message = request.Message,
            Type = request.Type,
            Metadata = request.Metadata,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.LiveStreamChatMessages.Add(message);
        await _dbContext.SaveChangesAsync();

        // Load user data
        await _dbContext.Entry(message).Reference(m => m.User).LoadAsync();

        return MapChatMessageToDto(message);
    }

    public async Task<List<LiveStreamChatMessageDto>> GetChatMessagesAsync(Guid streamId, Guid userId, Guid tenantId, int? limit = null, DateTimeOffset? before = null)
    {
        var liveStream = await _dbContext.LiveStreams
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null)
            throw new Exception("Stream not found");

        var query = _dbContext.LiveStreamChatMessages
            .Include(m => m.User)
            .Where(m => m.LiveStreamId == streamId);

        if (before.HasValue)
            query = query.Where(m => m.CreatedAt < before.Value);

        query = query.OrderByDescending(m => m.CreatedAt);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var messages = await query.ToListAsync();

        return messages.Select(MapChatMessageToDto).ToList();
    }

    public async Task<LiveStreamStatsDto> GetStreamStatsAsync(Guid streamId, Guid userId, Guid tenantId)
    {
        var liveStream = await _dbContext.LiveStreams
            .FirstOrDefaultAsync(ls => ls.Id == streamId);

        if (liveStream == null || liveStream.UserId != userId)
            throw new Exception("Stream not found or access denied");

        var totalViews = await _dbContext.LiveStreamViewers
            .CountAsync(lsv => lsv.LiveStreamId == streamId);

        var chatMessagesCount = await _dbContext.LiveStreamChatMessages
            .CountAsync(m => m.LiveStreamId == streamId);

        TimeSpan? duration = null;
        if (liveStream.StartedAt.HasValue)
        {
            var endTime = liveStream.EndedAt ?? DateTimeOffset.UtcNow;
            duration = endTime - liveStream.StartedAt.Value;
        }

        return new LiveStreamStatsDto
        {
            StreamId = streamId,
            ConcurrentViewers = liveStream.ConcurrentViewers,
            TotalViews = totalViews,
            Duration = duration,
            ChatMessagesCount = chatMessagesCount,
            StartedAt = liveStream.StartedAt,
            EndedAt = liveStream.EndedAt
        };
    }

    private static LiveStreamDto MapToDto(LiveStream liveStream)
    {
        return new LiveStreamDto
        {
            Id = liveStream.Id,
            UserId = liveStream.UserId,
            Title = liveStream.Title,
            Description = liveStream.Description,
            ThumbnailUrl = liveStream.ThumbnailUrl,
            PlaybackUrl = liveStream.PlaybackUrl,
            Status = liveStream.Status,
            ScheduledAt = liveStream.ScheduledAt,
            StartedAt = liveStream.StartedAt,
            EndedAt = liveStream.EndedAt,
            ConcurrentViewers = liveStream.ConcurrentViewers,
            TotalViews = liveStream.TotalViews,
            IsPublic = liveStream.IsPublic,
            AllowChat = liveStream.AllowChat,
            AllowReactions = liveStream.AllowReactions,
            RecordStream = liveStream.RecordStream,
            RecordedVideoId = liveStream.RecordedVideoId,
            Category = liveStream.Category,
            Tags = liveStream.Tags,
            CreatedAt = liveStream.CreatedAt,
            UpdatedAt = liveStream.UpdatedAt,
            User = new UserDto
            {
                Id = liveStream.User.Id,
                Email = liveStream.User.Email,
                FirstName = liveStream.User.FirstName,
                LastName = liveStream.User.LastName,
                AvatarUrl = liveStream.User.AvatarUrl
            }
        };
    }

    private static LiveStreamChatMessageDto MapChatMessageToDto(LiveStreamChatMessage message)
    {
        return new LiveStreamChatMessageDto
        {
            Id = message.Id,
            LiveStreamId = message.LiveStreamId,
            UserId = message.UserId,
            Message = message.Message,
            Type = message.Type,
            Metadata = message.Metadata,
            CreatedAt = message.CreatedAt,
            User = new UserDto
            {
                Id = message.User.Id,
                Email = message.User.Email,
                FirstName = message.User.FirstName,
                LastName = message.User.LastName,
                AvatarUrl = message.User.AvatarUrl
            }
        };
    }

    private static string GenerateStreamKey()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }
}
