using StreamVault.Application.Transcripts.DTOs;

namespace StreamVault.Application.Transcripts;

public interface ITranscriptService
{
    Task<List<TranscriptDto>> GetTranscriptAsync(Guid videoId, Guid tenantId);
    Task<TranscriptDto?> GetTranscriptSegmentAsync(Guid segmentId, Guid videoId, Guid tenantId);
    Task<List<TranscriptDto>> CreateTranscriptAsync(CreateTranscriptRequest request, Guid userId, Guid tenantId);
    Task<TranscriptDto> UpdateTranscriptSegmentAsync(Guid segmentId, UpdateTranscriptRequest request, Guid userId, Guid tenantId);
    Task DeleteTranscriptSegmentAsync(Guid segmentId, Guid userId, Guid tenantId);
    Task<List<TranscriptDto>> SearchTranscriptAsync(Guid videoId, string query, Guid tenantId);
}
