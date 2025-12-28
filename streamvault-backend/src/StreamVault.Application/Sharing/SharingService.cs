using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Application.Services;
using StreamVault.Application.Sharing.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Sharing;

public class SharingService : ISharingService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IStorageService _storageService;

    public SharingService(StreamVaultDbContext dbContext, IStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    public async Task<ShareLinkDto> GenerateShareLinkAsync(Guid videoId, CreateShareLinkRequest request, Guid userId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Generate unique token
        var token = GenerateShareToken();

        var videoShare = new VideoShare
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            UserId = userId,
            Token = token,
            ShareType = request.ShareType,
            AllowDownload = request.AllowDownload,
            ShowComments = request.ShowComments,
            ExpiresAt = request.ExpiresAt,
            Password = request.Password,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.VideoShares.Add(videoShare);
        await _dbContext.SaveChangesAsync();

        return await GetShareLinkDtoAsync(videoShare.Id);
    }

    public async Task<List<ShareLinkDto>> GetVideoShareLinksAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var shares = await _dbContext.VideoShares
            .Include(vs => vs.User)
            .Include(vs => vs.Video)
            .Where(vs => vs.VideoId == videoId && vs.UserId == userId)
            .OrderByDescending(vs => vs.CreatedAt)
            .ToListAsync();

        var shareDtos = new List<ShareLinkDto>();
        foreach (var share in shares)
        {
            shareDtos.Add(await GetShareLinkDtoAsync(share.Id));
        }

        return shareDtos;
    }

    public async Task<ShareLinkDto?> GetShareLinkAsync(Guid shareId, string token)
    {
        var share = await _dbContext.VideoShares
            .Include(vs => vs.User)
            .Include(vs => vs.Video)
            .FirstOrDefaultAsync(vs => vs.Id == shareId && vs.Token == token);

        if (share == null)
            return null;

        return await GetShareLinkDtoAsync(share.Id);
    }

    public async Task UpdateShareLinkAsync(Guid shareId, UpdateShareLinkRequest request, Guid userId, Guid tenantId)
    {
        var share = await _dbContext.VideoShares
            .Include(vs => vs.Video)
            .FirstOrDefaultAsync(vs => vs.Id == shareId && vs.UserId == userId);

        if (share == null || share.Video.TenantId != tenantId)
            throw new Exception("Share link not found");

        if (request.ShareType.HasValue)
            share.ShareType = request.ShareType.Value;

        if (request.AllowDownload.HasValue)
            share.AllowDownload = request.AllowDownload.Value;

        if (request.ShowComments.HasValue)
            share.ShowComments = request.ShowComments.Value;

        if (request.ExpiresAt.HasValue)
            share.ExpiresAt = request.ExpiresAt;

        if (request.Password != null)
            share.Password = request.Password;

        share.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteShareLinkAsync(Guid shareId, Guid userId, Guid tenantId)
    {
        var share = await _dbContext.VideoShares
            .Include(vs => vs.Video)
            .FirstOrDefaultAsync(vs => vs.Id == shareId && vs.UserId == userId);

        if (share == null || share.Video.TenantId != tenantId)
            throw new Exception("Share link not found");

        _dbContext.VideoShares.Remove(share);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<VideoDto?> GetSharedVideoAsync(string token)
    {
        var share = await _dbContext.VideoShares
            .Include(vs => vs.Video)
                .ThenInclude(v => v.User)
            .Include(vs => vs.User)
            .FirstOrDefaultAsync(vs => vs.Token == token);

        if (share == null)
            return null;

        // Check if expired
        if (share.ExpiresAt.HasValue && share.ExpiresAt < DateTimeOffset.UtcNow)
            return null;

        // Increment view count
        share.ViewCount++;
        await _dbContext.SaveChangesAsync();

        // Generate video URLs
        var videoUrl = await _storageService.GeneratePresignedUrlAsync(share.Video.StoragePath, TimeSpan.FromHours(1));
        var thumbnailUrl = share.Video.ThumbnailPath != null 
            ? await _storageService.GeneratePresignedUrlAsync(share.Video.ThumbnailPath, TimeSpan.FromHours(1))
            : null;

        return new VideoDto
        {
            Id = share.Video.Id,
            Title = share.Video.Title,
            Description = share.Video.Description,
            VideoUrl = videoUrl,
            ThumbnailUrl = thumbnailUrl,
            DurationSeconds = share.Video.DurationSeconds,
            ViewCount = share.Video.ViewCount,
            CreatedAt = share.Video.CreatedAt,
            User = new UserDto
            {
                Id = share.Video.User.Id,
                Email = share.Video.User.Email,
                FirstName = share.Video.User.FirstName,
                LastName = share.Video.User.LastName
            }
        };
    }

    private async Task<ShareLinkDto> GetShareLinkDtoAsync(Guid shareId)
    {
        var share = await _dbContext.VideoShares
            .Include(vs => vs.User)
            .Include(vs => vs.Video)
            .FirstOrDefaultAsync(vs => vs.Id == shareId);

        if (share == null)
            throw new Exception("Share link not found");

        var baseUrl = "https://your-domain.com"; // TODO: Get from configuration
        var shareUrl = $"{baseUrl}/shared/{share.Token}";

        return new ShareLinkDto
        {
            Id = share.Id,
            VideoId = share.VideoId,
            VideoTitle = share.Video.Title,
            ShareUrl = shareUrl,
            Token = share.Token,
            ShareType = share.ShareType,
            AllowDownload = share.AllowDownload,
            ShowComments = share.ShowComments,
            ExpiresAt = share.ExpiresAt,
            Password = share.Password,
            ViewCount = share.ViewCount,
            CreatedAt = share.CreatedAt,
            CreatedBy = new UserDto
            {
                Id = share.User.Id,
                Email = share.User.Email,
                FirstName = share.User.FirstName,
                LastName = share.User.LastName
            }
        };
    }

    private string GenerateShareToken()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var token = new char[12];
        
        for (int i = 0; i < token.Length; i++)
        {
            token[i] = chars[random.Next(chars.Length)];
        }
        
        return new string(token);
    }
}
