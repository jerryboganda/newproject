using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Application.Playlists.DTOs;
using StreamVault.Application.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Playlists;

public class PlaylistService : IPlaylistService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IStorageService _storageService;

    public PlaylistService(StreamVaultDbContext dbContext, IStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    public async Task<PlaylistDto> CreatePlaylistAsync(CreatePlaylistRequest request, Guid userId, Guid tenantId)
    {
        var playlist = new Playlist
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            UserId = userId,
            TenantId = tenantId,
            IsPublic = request.IsPublic,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Playlists.Add(playlist);
        await _dbContext.SaveChangesAsync();

        return await GetPlaylistDtoAsync(playlist.Id, tenantId);
    }

    public async Task<PlaylistDto> UpdatePlaylistAsync(Guid id, UpdatePlaylistRequest request, Guid userId, Guid tenantId)
    {
        var playlist = await _dbContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.TenantId == tenantId);

        if (playlist == null)
            throw new Exception("Playlist not found");

        if (!string.IsNullOrEmpty(request.Name))
            playlist.Name = request.Name;

        if (request.Description != null)
            playlist.Description = request.Description;

        if (request.IsPublic.HasValue)
            playlist.IsPublic = request.IsPublic.Value;

        playlist.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetPlaylistDtoAsync(playlist.Id, tenantId);
    }

    public async Task DeletePlaylistAsync(Guid id, Guid userId, Guid tenantId)
    {
        var playlist = await _dbContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.TenantId == tenantId);

        if (playlist == null)
            throw new Exception("Playlist not found");

        playlist.IsDeleted = true;
        playlist.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<PlaylistDto?> GetPlaylistAsync(Guid id, Guid tenantId)
    {
        var playlist = await _dbContext.Playlists
            .Include(p => p.User)
            .Include(p => p.PlaylistVideos)
                .ThenInclude(pv => pv.Video)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && !p.IsDeleted);

        if (playlist == null)
            return null;

        return await GetPlaylistDtoAsync(playlist.Id, tenantId);
    }

    public async Task<List<PlaylistDto>> GetUserPlaylistsAsync(Guid userId, Guid tenantId)
    {
        var playlists = await _dbContext.Playlists
            .Include(p => p.User)
            .Where(p => p.UserId == userId && p.TenantId == tenantId && !p.IsDeleted)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        var playlistDtos = new List<PlaylistDto>();

        foreach (var playlist in playlists)
        {
            var playlistDto = await GetPlaylistDtoAsync(playlist.Id, tenantId);
            if (playlistDto != null)
                playlistDtos.Add(playlistDto);
        }

        return playlistDtos;
    }

    public async Task AddVideoToPlaylistAsync(Guid playlistId, Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify playlist exists and belongs to user
        var playlist = await _dbContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId && p.TenantId == tenantId);

        if (playlist == null)
            throw new Exception("Playlist not found");

        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Check if video already in playlist
        var existing = await _dbContext.PlaylistVideos
            .FirstOrDefaultAsync(pv => pv.PlaylistId == playlistId && pv.VideoId == videoId);

        if (existing != null)
            throw new Exception("Video already in playlist");

        // Get next position
        var maxPosition = await _dbContext.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId)
            .Select(pv => pv.Position)
            .DefaultIfEmpty()
            .MaxAsync();

        var playlistVideo = new PlaylistVideo
        {
            Id = Guid.NewGuid(),
            PlaylistId = playlistId,
            VideoId = videoId,
            Position = maxPosition + 1,
            AddedAt = DateTimeOffset.UtcNow
        };

        _dbContext.PlaylistVideos.Add(playlistVideo);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveVideoFromPlaylistAsync(Guid playlistId, Guid videoId, Guid userId, Guid tenantId)
    {
        var playlistVideo = await _dbContext.PlaylistVideos
            .Include(pv => pv.Playlist)
            .FirstOrDefaultAsync(pv => pv.PlaylistId == playlistId && pv.VideoId == videoId && pv.Playlist.UserId == userId);

        if (playlistVideo == null)
            throw new Exception("Video not found in playlist");

        _dbContext.PlaylistVideos.Remove(playlistVideo);
        await _dbContext.SaveChangesAsync();

        // Reorder remaining videos
        await ReorderPlaylistVideosAsync(playlistId);
    }

    public async Task ReorderPlaylistAsync(Guid playlistId, List<PlaylistVideoOrder> videoOrders, Guid userId, Guid tenantId)
    {
        // Verify playlist ownership
        var playlist = await _dbContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId && p.TenantId == tenantId);

        if (playlist == null)
            throw new Exception("Playlist not found");

        foreach (var order in videoOrders)
        {
            var playlistVideo = await _dbContext.PlaylistVideos
                .FirstOrDefaultAsync(pv => pv.PlaylistId == playlistId && pv.VideoId == order.VideoId);

            if (playlistVideo != null)
            {
                playlistVideo.Position = order.Position;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task<PlaylistDto> GetPlaylistDtoAsync(Guid playlistId, Guid tenantId)
    {
        var playlist = await _dbContext.Playlists
            .Include(p => p.User)
            .Include(p => p.PlaylistVideos)
                .ThenInclude(pv => pv.Video)
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.TenantId == tenantId && !p.IsDeleted);

        if (playlist == null)
            throw new Exception("Playlist not found");

        var videos = new List<PlaylistVideoDto>();
        foreach (var pv in playlist.PlaylistVideos.OrderBy(pv => pv.Position))
        {
            var videoUrl = await _storageService.GeneratePresignedUrlAsync(pv.Video.StoragePath, TimeSpan.FromHours(1));
            var thumbnailUrl = pv.Video.ThumbnailPath != null 
                ? await _storageService.GeneratePresignedUrlAsync(pv.Video.ThumbnailPath, TimeSpan.FromHours(1))
                : null;

            videos.Add(new PlaylistVideoDto
            {
                Id = pv.Video.Id,
                Title = pv.Video.Title,
                ThumbnailUrl = thumbnailUrl,
                VideoUrl = videoUrl,
                DurationSeconds = pv.Video.DurationSeconds,
                Position = pv.Position,
                AddedAt = pv.AddedAt
            });
        }

        return new PlaylistDto
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            IsPublic = playlist.IsPublic,
            CreatedAt = playlist.CreatedAt,
            UpdatedAt = playlist.UpdatedAt,
            VideoCount = videos.Count,
            User = new UserDto
            {
                Id = playlist.User.Id,
                Email = playlist.User.Email,
                FirstName = playlist.User.FirstName,
                LastName = playlist.User.LastName
            },
            Videos = videos
        };
    }

    private async Task ReorderPlaylistVideosAsync(Guid playlistId)
    {
        var playlistVideos = await _dbContext.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId)
            .OrderBy(pv => pv.Position)
            .ToListAsync();

        for (int i = 0; i < playlistVideos.Count; i++)
        {
            playlistVideos[i].Position = i + 1;
        }

        await _dbContext.SaveChangesAsync();
    }
}
