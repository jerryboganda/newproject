using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Annotations.DTOs;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Annotations;

public class AnnotationService : IAnnotationService
{
    private readonly StreamVaultDbContext _dbContext;

    public AnnotationService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AnnotationDto>> GetAnnotationsAsync(Guid videoId, Guid tenantId, int? startTime = null, int? endTime = null)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var query = _dbContext.VideoAnnotations
            .Include(a => a.User)
            .Include(a => a.Replies)
                .ThenInclude(r => r.User)
            .Where(a => a.VideoId == videoId);

        if (startTime.HasValue)
            query = query.Where(a => a.StartTimeSeconds >= startTime.Value);

        if (endTime.HasValue)
            query = query.Where(a => a.EndTimeSeconds <= endTime.Value);

        var annotations = await query
            .OrderBy(a => a.StartTimeSeconds)
            .ToListAsync();

        return annotations.Select(a => new AnnotationDto
        {
            Id = a.Id,
            VideoId = a.VideoId,
            UserId = a.UserId,
            Title = a.Title,
            Content = a.Content,
            StartTimeSeconds = a.StartTimeSeconds,
            EndTimeSeconds = a.EndTimeSeconds,
            Type = a.Type,
            Color = a.Color,
            PositionX = a.PositionX,
            PositionY = a.PositionY,
            IsPublic = a.IsPublic,
            IsResolved = a.IsResolved,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            User = new UserDto
            {
                Id = a.User.Id,
                Email = a.User.Email,
                FirstName = a.User.FirstName,
                LastName = a.User.LastName,
                AvatarUrl = a.User.AvatarUrl
            },
            Replies = a.Replies.Select(r => new AnnotationReplyDto
            {
                Id = r.Id,
                AnnotationId = r.AnnotationId,
                UserId = r.UserId,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                User = new UserDto
                {
                    Id = r.User.Id,
                    Email = r.User.Email,
                    FirstName = r.User.FirstName,
                    LastName = r.User.LastName,
                    AvatarUrl = r.User.AvatarUrl
                }
            }).ToList()
        }).ToList();
    }

    public async Task<AnnotationDto?> GetAnnotationAsync(Guid annotationId, Guid userId, Guid tenantId)
    {
        var annotation = await _dbContext.VideoAnnotations
            .Include(a => a.Video)
            .Include(a => a.User)
            .Include(a => a.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(a => a.Id == annotationId);

        if (annotation == null || annotation.Video.TenantId != tenantId)
            return null;

        // Check if user can access this annotation
        if (!annotation.IsPublic && annotation.UserId != userId)
            return null;

        return new AnnotationDto
        {
            Id = annotation.Id,
            VideoId = annotation.VideoId,
            UserId = annotation.UserId,
            Title = annotation.Title,
            Content = annotation.Content,
            StartTimeSeconds = annotation.StartTimeSeconds,
            EndTimeSeconds = annotation.EndTimeSeconds,
            Type = annotation.Type,
            Color = annotation.Color,
            PositionX = annotation.PositionX,
            PositionY = annotation.PositionY,
            IsPublic = annotation.IsPublic,
            IsResolved = annotation.IsResolved,
            CreatedAt = annotation.CreatedAt,
            UpdatedAt = annotation.UpdatedAt,
            User = new UserDto
            {
                Id = annotation.User.Id,
                Email = annotation.User.Email,
                FirstName = annotation.User.FirstName,
                LastName = annotation.User.LastName,
                AvatarUrl = annotation.User.AvatarUrl
            },
            Replies = annotation.Replies.Select(r => new AnnotationReplyDto
            {
                Id = r.Id,
                AnnotationId = r.AnnotationId,
                UserId = r.UserId,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                User = new UserDto
                {
                    Id = r.User.Id,
                    Email = r.User.Email,
                    FirstName = r.User.FirstName,
                    LastName = r.User.LastName,
                    AvatarUrl = r.User.AvatarUrl
                }
            }).ToList()
        };
    }

    public async Task<AnnotationDto> CreateAnnotationAsync(CreateAnnotationRequest request, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var annotation = new VideoAnnotation
        {
            Id = Guid.NewGuid(),
            VideoId = request.VideoId,
            UserId = userId,
            Title = request.Title,
            Content = request.Content,
            StartTimeSeconds = request.StartTimeSeconds,
            EndTimeSeconds = request.EndTimeSeconds,
            Type = request.Type,
            Color = request.Color,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            IsPublic = request.IsPublic,
            IsResolved = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.VideoAnnotations.Add(annotation);
        await _dbContext.SaveChangesAsync();

        // Reload with user data
        await _dbContext.Entry(annotation).Reference(a => a.User).LoadAsync();

        return new AnnotationDto
        {
            Id = annotation.Id,
            VideoId = annotation.VideoId,
            UserId = annotation.UserId,
            Title = annotation.Title,
            Content = annotation.Content,
            StartTimeSeconds = annotation.StartTimeSeconds,
            EndTimeSeconds = annotation.EndTimeSeconds,
            Type = annotation.Type,
            Color = annotation.Color,
            PositionX = annotation.PositionX,
            PositionY = annotation.PositionY,
            IsPublic = annotation.IsPublic,
            IsResolved = annotation.IsResolved,
            CreatedAt = annotation.CreatedAt,
            UpdatedAt = annotation.UpdatedAt,
            User = new UserDto
            {
                Id = annotation.User.Id,
                Email = annotation.User.Email,
                FirstName = annotation.User.FirstName,
                LastName = annotation.User.LastName,
                AvatarUrl = annotation.User.AvatarUrl
            },
            Replies = new List<AnnotationReplyDto>()
        };
    }

    public async Task<AnnotationDto> UpdateAnnotationAsync(Guid annotationId, UpdateAnnotationRequest request, Guid userId, Guid tenantId)
    {
        var annotation = await _dbContext.VideoAnnotations
            .Include(a => a.Video)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == annotationId);

        if (annotation == null || annotation.Video.TenantId != tenantId)
            throw new Exception("Annotation not found");

        if (annotation.UserId != userId)
            throw new Exception("You can only update your own annotations");

        if (request.Title != null)
            annotation.Title = request.Title;

        if (request.Content != null)
            annotation.Content = request.Content;

        if (request.StartTimeSeconds.HasValue)
            annotation.StartTimeSeconds = request.StartTimeSeconds.Value;

        if (request.EndTimeSeconds.HasValue)
            annotation.EndTimeSeconds = request.EndTimeSeconds.Value;

        if (request.Type.HasValue)
            annotation.Type = request.Type.Value;

        if (request.Color != null)
            annotation.Color = request.Color;

        if (request.PositionX.HasValue)
            annotation.PositionX = request.PositionX.Value;

        if (request.PositionY.HasValue)
            annotation.PositionY = request.PositionY.Value;

        if (request.IsPublic.HasValue)
            annotation.IsPublic = request.IsPublic.Value;

        annotation.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new AnnotationDto
        {
            Id = annotation.Id,
            VideoId = annotation.VideoId,
            UserId = annotation.UserId,
            Title = annotation.Title,
            Content = annotation.Content,
            StartTimeSeconds = annotation.StartTimeSeconds,
            EndTimeSeconds = annotation.EndTimeSeconds,
            Type = annotation.Type,
            Color = annotation.Color,
            PositionX = annotation.PositionX,
            PositionY = annotation.PositionY,
            IsPublic = annotation.IsPublic,
            IsResolved = annotation.IsResolved,
            CreatedAt = annotation.CreatedAt,
            UpdatedAt = annotation.UpdatedAt,
            User = new UserDto
            {
                Id = annotation.User.Id,
                Email = annotation.User.Email,
                FirstName = annotation.User.FirstName,
                LastName = annotation.User.LastName,
                AvatarUrl = annotation.User.AvatarUrl
            },
            Replies = new List<AnnotationReplyDto>()
        };
    }

    public async Task DeleteAnnotationAsync(Guid annotationId, Guid userId, Guid tenantId)
    {
        var annotation = await _dbContext.VideoAnnotations
            .Include(a => a.Video)
            .FirstOrDefaultAsync(a => a.Id == annotationId);

        if (annotation == null || annotation.Video.TenantId != tenantId)
            throw new Exception("Annotation not found");

        if (annotation.UserId != userId)
            throw new Exception("You can only delete your own annotations");

        _dbContext.VideoAnnotations.Remove(annotation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<AnnotationDto> ResolveAnnotationAsync(Guid annotationId, Guid userId, Guid tenantId)
    {
        var annotation = await _dbContext.VideoAnnotations
            .Include(a => a.Video)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == annotationId);

        if (annotation == null || annotation.Video.TenantId != tenantId)
            throw new Exception("Annotation not found");

        if (annotation.UserId != userId)
            throw new Exception("You can only resolve your own annotations");

        annotation.IsResolved = true;
        annotation.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new AnnotationDto
        {
            Id = annotation.Id,
            VideoId = annotation.VideoId,
            UserId = annotation.UserId,
            Title = annotation.Title,
            Content = annotation.Content,
            StartTimeSeconds = annotation.StartTimeSeconds,
            EndTimeSeconds = annotation.EndTimeSeconds,
            Type = annotation.Type,
            Color = annotation.Color,
            PositionX = annotation.PositionX,
            PositionY = annotation.PositionY,
            IsPublic = annotation.IsPublic,
            IsResolved = annotation.IsResolved,
            CreatedAt = annotation.CreatedAt,
            UpdatedAt = annotation.UpdatedAt,
            User = new UserDto
            {
                Id = annotation.User.Id,
                Email = annotation.User.Email,
                FirstName = annotation.User.FirstName,
                LastName = annotation.User.LastName,
                AvatarUrl = annotation.User.AvatarUrl
            },
            Replies = new List<AnnotationReplyDto>()
        };
    }

    public async Task<AnnotationReplyDto> AddReplyAsync(Guid annotationId, CreateReplyRequest request, Guid userId, Guid tenantId)
    {
        var annotation = await _dbContext.VideoAnnotations
            .Include(a => a.Video)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == annotationId);

        if (annotation == null || annotation.Video.TenantId != tenantId)
            throw new Exception("Annotation not found");

        // Check if user can access this annotation
        if (!annotation.IsPublic && annotation.UserId != userId && annotation.Video.UserId != userId)
            throw new Exception("You cannot reply to this annotation");

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var reply = new AnnotationReply
        {
            Id = Guid.NewGuid(),
            AnnotationId = annotationId,
            UserId = userId,
            Content = request.Content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.AnnotationReplies.Add(reply);
        await _dbContext.SaveChangesAsync();

        return new AnnotationReplyDto
        {
            Id = reply.Id,
            AnnotationId = reply.AnnotationId,
            UserId = reply.UserId,
            Content = reply.Content,
            CreatedAt = reply.CreatedAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl
            }
        };
    }

    public async Task DeleteReplyAsync(Guid replyId, Guid userId, Guid tenantId)
    {
        var reply = await _dbContext.AnnotationReplies
            .Include(r => r.Annotation)
                .ThenInclude(a => a.Video)
            .FirstOrDefaultAsync(r => r.Id == replyId);

        if (reply == null || reply.Annotation.Video.TenantId != tenantId)
            throw new Exception("Reply not found");

        if (reply.UserId != userId)
            throw new Exception("You can only delete your own replies");

        _dbContext.AnnotationReplies.Remove(reply);
        await _dbContext.SaveChangesAsync();
    }
}
