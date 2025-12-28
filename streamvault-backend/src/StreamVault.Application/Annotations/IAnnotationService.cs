using StreamVault.Application.Annotations.DTOs;

namespace StreamVault.Application.Annotations;

public interface IAnnotationService
{
    Task<List<AnnotationDto>> GetAnnotationsAsync(Guid videoId, Guid tenantId, int? startTime = null, int? endTime = null);
    Task<AnnotationDto?> GetAnnotationAsync(Guid annotationId, Guid userId, Guid tenantId);
    Task<AnnotationDto> CreateAnnotationAsync(CreateAnnotationRequest request, Guid userId, Guid tenantId);
    Task<AnnotationDto> UpdateAnnotationAsync(Guid annotationId, UpdateAnnotationRequest request, Guid userId, Guid tenantId);
    Task DeleteAnnotationAsync(Guid annotationId, Guid userId, Guid tenantId);
    Task<AnnotationDto> ResolveAnnotationAsync(Guid annotationId, Guid userId, Guid tenantId);
    Task<AnnotationReplyDto> AddReplyAsync(Guid annotationId, CreateReplyRequest request, Guid userId, Guid tenantId);
    Task DeleteReplyAsync(Guid replyId, Guid userId, Guid tenantId);
}
