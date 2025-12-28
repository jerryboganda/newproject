using StreamVault.Application.Comments.DTOs;

namespace StreamVault.Application.Comments;

public interface ICommentService
{
    Task<CommentDto> AddCommentAsync(CreateCommentRequest request, Guid userId, Guid tenantId);
    Task<CommentDto> UpdateCommentAsync(Guid id, UpdateCommentRequest request, Guid userId, Guid tenantId);
    Task DeleteCommentAsync(Guid id, Guid userId, Guid tenantId);
    Task<List<CommentDto>> GetVideoCommentsAsync(Guid videoId, Guid tenantId, int page = 1, int pageSize = 20);
}
