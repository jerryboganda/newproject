using Microsoft.EntityFrameworkCore;
using StreamVault.Application.VideoSettings.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using VideoSettings = StreamVault.Domain.Entities.VideoSettings;

namespace StreamVault.Application.VideoSettings;

public class VideoSettingsService : IVideoSettingsService
{
    private readonly StreamVaultDbContext _dbContext;

    public VideoSettingsService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VideoSettingsDto> GetSettingsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var settings = await _dbContext.VideoSettings
            .FirstOrDefaultAsync(vs => vs.VideoId == videoId && vs.UserId == userId);

        // If no settings exist, return default settings
        if (settings == null)
        {
            return new VideoSettingsDto
            {
                Id = Guid.Empty,
                UserId = userId,
                VideoId = videoId,
                PlaybackSpeed = 1.0,
                Volume = 100,
                IsMuted = false,
                Autoplay = true,
                Quality = VideoQuality.Auto,
                CaptionsEnabled = false,
                PictureInPicture = false,
                TheaterMode = false,
                Fullscreen = false,
                LastPositionSeconds = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        return new VideoSettingsDto
        {
            Id = settings.Id,
            UserId = settings.UserId,
            VideoId = settings.VideoId,
            PlaybackSpeed = settings.PlaybackSpeed,
            Volume = settings.Volume,
            IsMuted = settings.IsMuted,
            Autoplay = settings.Autoplay,
            Quality = settings.Quality,
            CaptionsEnabled = settings.CaptionsEnabled,
            CaptionsLanguage = settings.CaptionsLanguage,
            PictureInPicture = settings.PictureInPicture,
            TheaterMode = settings.TheaterMode,
            Fullscreen = settings.Fullscreen,
            LastPositionSeconds = settings.LastPositionSeconds,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }

    public async Task<VideoSettingsDto> UpdateSettingsAsync(Guid videoId, UpdateVideoSettingsRequest request, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var settings = await _dbContext.VideoSettings
            .FirstOrDefaultAsync(vs => vs.VideoId == videoId && vs.UserId == userId);

        // Create new settings if they don't exist
        if (settings == null)
        {
            settings = new StreamVault.Domain.Entities.VideoSettings
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                UserId = userId,
                PlaybackSpeed = 1.0,
                Volume = 100,
                IsMuted = false,
                Autoplay = true,
                Quality = VideoQuality.Auto,
                CaptionsEnabled = false,
                PictureInPicture = false,
                TheaterMode = false,
                Fullscreen = false,
                LastPositionSeconds = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.VideoSettings.Add(settings);
        }

        // Update provided settings
        if (request.PlaybackSpeed.HasValue)
            settings.PlaybackSpeed = request.PlaybackSpeed.Value;

        if (request.Volume.HasValue)
            settings.Volume = request.Volume.Value;

        if (request.IsMuted.HasValue)
            settings.IsMuted = request.IsMuted.Value;

        if (request.Autoplay.HasValue)
            settings.Autoplay = request.Autoplay.Value;

        if (request.Quality.HasValue)
            settings.Quality = request.Quality.Value;

        if (request.CaptionsEnabled.HasValue)
            settings.CaptionsEnabled = request.CaptionsEnabled.Value;

        if (request.CaptionsLanguage != null)
            settings.CaptionsLanguage = request.CaptionsLanguage;

        if (request.PictureInPicture.HasValue)
            settings.PictureInPicture = request.PictureInPicture.Value;

        if (request.TheaterMode.HasValue)
            settings.TheaterMode = request.TheaterMode.Value;

        if (request.Fullscreen.HasValue)
            settings.Fullscreen = request.Fullscreen.Value;

        settings.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new VideoSettingsDto
        {
            Id = settings.Id,
            UserId = settings.UserId,
            VideoId = settings.VideoId,
            PlaybackSpeed = settings.PlaybackSpeed,
            Volume = settings.Volume,
            IsMuted = settings.IsMuted,
            Autoplay = settings.Autoplay,
            Quality = settings.Quality,
            CaptionsEnabled = settings.CaptionsEnabled,
            CaptionsLanguage = settings.CaptionsLanguage,
            PictureInPicture = settings.PictureInPicture,
            TheaterMode = settings.TheaterMode,
            Fullscreen = settings.Fullscreen,
            LastPositionSeconds = settings.LastPositionSeconds,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }

    public async Task<VideoSettingsDto> UpdatePlaybackSpeedAsync(Guid videoId, double speed, Guid userId, Guid tenantId)
    {
        if (speed < 0.25 || speed > 2.0)
            throw new Exception("Playback speed must be between 0.25 and 2.0");

        var settings = await GetOrCreateSettingsAsync(videoId, userId, tenantId);
        
        settings.PlaybackSpeed = speed;
        settings.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(settings);
    }

    public async Task<VideoSettingsDto> UpdateVolumeAsync(Guid videoId, int volume, Guid userId, Guid tenantId)
    {
        if (volume < 0 || volume > 100)
            throw new Exception("Volume must be between 0 and 100");

        var settings = await GetOrCreateSettingsAsync(videoId, userId, tenantId);
        
        settings.Volume = volume;
        settings.IsMuted = volume == 0;
        settings.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(settings);
    }

    public async Task<VideoSettingsDto> UpdatePositionAsync(Guid videoId, double positionSeconds, Guid userId, Guid tenantId)
    {
        if (positionSeconds < 0)
            throw new Exception("Position cannot be negative");

        var settings = await GetOrCreateSettingsAsync(videoId, userId, tenantId);
        
        settings.LastPositionSeconds = positionSeconds;
        settings.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(settings);
    }

    public async Task ResetSettingsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        var settings = await _dbContext.VideoSettings
            .FirstOrDefaultAsync(vs => vs.VideoId == videoId && vs.UserId == userId);

        if (settings != null)
        {
            _dbContext.VideoSettings.Remove(settings);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task<StreamVault.Domain.Entities.VideoSettings> GetOrCreateSettingsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var settings = await _dbContext.VideoSettings.FirstOrDefaultAsync(vs => vs.VideoId == videoId && vs.UserId == userId);

        if (settings == null)
        {
            settings = new StreamVault.Domain.Entities.VideoSettings
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                UserId = userId,
                PlaybackSpeed = 1.0,
                Volume = 100,
                IsMuted = false,
                Autoplay = true,
                Quality = VideoQuality.Auto,
                CaptionsEnabled = false,
                PictureInPicture = false,
                TheaterMode = false,
                Fullscreen = false,
                LastPositionSeconds = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.VideoSettings.Add(settings);
        }

        return settings;
    }

    private static VideoSettingsDto MapToDto(StreamVault.Domain.Entities.VideoSettings settings)
    {
        return new VideoSettingsDto
        {
            Id = settings.Id,
            UserId = settings.UserId,
            VideoId = settings.VideoId,
            PlaybackSpeed = settings.PlaybackSpeed,
            Volume = settings.Volume,
            IsMuted = settings.IsMuted,
            Autoplay = settings.Autoplay,
            Quality = settings.Quality,
            CaptionsEnabled = settings.CaptionsEnabled,
            CaptionsLanguage = settings.CaptionsLanguage,
            PictureInPicture = settings.PictureInPicture,
            TheaterMode = settings.TheaterMode,
            Fullscreen = settings.Fullscreen,
            LastPositionSeconds = settings.LastPositionSeconds,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }
}
