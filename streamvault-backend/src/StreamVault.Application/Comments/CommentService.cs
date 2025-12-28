using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Application.Comments.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Comments;

public class CommentService : ICommentService
{
    private readonly StreamVaultDbContext _dbContext;

    public CommentService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommentDto> AddCommentAsync(CreateCommentRequest request, Guid userId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Verify parent comment exists if provided
        if (request.ParentId.HasValue)
        {
            var parentComment = await _dbContext.Comments
                .FirstOrDefaultAsync(c => c.Id == request.ParentId.Value && c.VideoId == request.VideoId);

            if (parentComment == null)
                throw new Exception("Parent comment not found");
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            VideoId = request.VideoId,
            UserId = userId,
            Content = request.Content,
            ParentId = request.ParentId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync();

        return await GetCommentDtoAsync(comment.Id);
    }

    public async Task<CommentDto> UpdateCommentAsync(Guid id, UpdateCommentRequest request, Guid userId, Guid tenantId)
    {
        var comment = await _dbContext.Comments
            .Include(c => c.Video)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (comment == null || comment.Video.TenantId != tenantId)
            throw new Exception("Comment not found");

        comment.Content = request.Content;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetCommentDtoAsync(comment.Id);
    }

    public async Task DeleteCommentAsync(Guid id, Guid userId, Guid tenantId)
    {
        var comment = await _dbContext.Comments
            .Include(c => c.Video)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (comment == null || comment.Video.TenantId != tenantId)
            throw new Exception("Comment not found");

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<CommentDto>> GetVideoCommentsAsync(Guid videoId, Guid tenantId, int page = 1, int pageSize = 20)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var comments = await _dbContext.Comments
            .Include(c => c.User)
            .Where(c => c.VideoId == videoId && !c.IsDeleted && c.ParentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var commentDtos = new List<CommentDto>();

        foreach (var comment in comments)
        {
            var commentDto = await GetCommentDtoAsync(comment.Id);
            commentDtos.Add(commentDto);
        }

        return commentDtos;
    }

    private async Task<CommentDto> GetCommentDtoAsync(Guid commentId)
    {
        var comment = await _dbContext.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new Exception("Comment not found");

        var replies = new List<CommentDto>();
        foreach (var reply in comment.Replies.Where(r => !r.IsDeleted))
        {
            replies.Add(new CommentDto
            {
                Id = reply.Id,
                VideoId = reply.VideoId,
                Content = reply.Content,
                ParentId = reply.ParentId,
                CreatedAt = reply.CreatedAt,
                UpdatedAt = reply.UpdatedAt,
                User = new UserDto
                {
                    Id = reply.User.Id,
                    Email = reply.User.Email,
                    FirstName = reply.User.FirstName,
                    LastName = reply.User.LastName
                },
                Replies = new List<CommentDto>()
            });
        }

        return new CommentDto
        {
            Id = comment.Id,
            VideoId = comment.VideoId,
            Content = comment.Content,
            ParentId = comment.ParentId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            User = new UserDto
            {
                Id = comment.User.Id,
                Email = comment.User.Email,
                FirstName = comment.User.FirstName,
                LastName = comment.User.LastName
            },
            Replies = replies
        };
    }
}
