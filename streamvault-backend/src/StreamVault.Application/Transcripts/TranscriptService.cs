using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Transcripts.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Transcripts;

public class TranscriptService : ITranscriptService
{
    private readonly StreamVaultDbContext _dbContext;

    public TranscriptService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TranscriptDto>> GetTranscriptAsync(Guid videoId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var transcripts = await _dbContext.VideoTranscripts
            .Where(vt => vt.VideoId == videoId)
            .OrderBy(vt => vt.SortOrder)
            .ThenBy(vt => vt.StartTimeSeconds)
            .ToListAsync();

        return transcripts.Select(vt => new TranscriptDto
        {
            Id = vt.Id,
            VideoId = vt.VideoId,
            StartTimeSeconds = vt.StartTimeSeconds,
            EndTimeSeconds = vt.EndTimeSeconds,
            Text = vt.Text,
            Confidence = vt.Confidence,
            Language = vt.Language,
            Speaker = vt.Speaker,
            SortOrder = vt.SortOrder,
            CreatedAt = vt.CreatedAt
        }).ToList();
    }

    public async Task<TranscriptDto?> GetTranscriptSegmentAsync(Guid segmentId, Guid videoId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var transcript = await _dbContext.VideoTranscripts
            .FirstOrDefaultAsync(vt => vt.Id == segmentId && vt.VideoId == videoId);

        if (transcript == null)
            return null;

        return new TranscriptDto
        {
            Id = transcript.Id,
            VideoId = transcript.VideoId,
            StartTimeSeconds = transcript.StartTimeSeconds,
            EndTimeSeconds = transcript.EndTimeSeconds,
            Text = transcript.Text,
            Confidence = transcript.Confidence,
            Language = transcript.Language,
            Speaker = transcript.Speaker,
            SortOrder = transcript.SortOrder,
            CreatedAt = transcript.CreatedAt
        };
    }

    public async Task<List<TranscriptDto>> CreateTranscriptAsync(CreateTranscriptRequest request, Guid userId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Delete existing transcripts for this video
        var existingTranscripts = await _dbContext.VideoTranscripts
            .Where(vt => vt.VideoId == request.VideoId)
            .ToListAsync();

        _dbContext.VideoTranscripts.RemoveRange(existingTranscripts);

        // Add new transcript segments
        var transcriptDtos = new List<TranscriptDto>();
        for (int i = 0; i < request.Segments.Count; i++)
        {
            var segment = request.Segments[i];
            
            var transcript = new VideoTranscript
            {
                Id = Guid.NewGuid(),
                VideoId = request.VideoId,
                StartTimeSeconds = segment.StartTimeSeconds,
                EndTimeSeconds = segment.EndTimeSeconds,
                Text = segment.Text,
                Confidence = segment.Confidence,
                Language = segment.Language,
                Speaker = segment.Speaker,
                SortOrder = i,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.VideoTranscripts.Add(transcript);

            transcriptDtos.Add(new TranscriptDto
            {
                Id = transcript.Id,
                VideoId = transcript.VideoId,
                StartTimeSeconds = transcript.StartTimeSeconds,
                EndTimeSeconds = transcript.EndTimeSeconds,
                Text = transcript.Text,
                Confidence = transcript.Confidence,
                Language = transcript.Language,
                Speaker = transcript.Speaker,
                SortOrder = transcript.SortOrder,
                CreatedAt = transcript.CreatedAt
            });
        }

        await _dbContext.SaveChangesAsync();

        return transcriptDtos.OrderBy(t => t.SortOrder).ToList();
    }

    public async Task<TranscriptDto> UpdateTranscriptSegmentAsync(Guid segmentId, UpdateTranscriptRequest request, Guid userId, Guid tenantId)
    {
        var transcript = await _dbContext.VideoTranscripts
            .Include(vt => vt.Video)
            .FirstOrDefaultAsync(vt => vt.Id == segmentId);

        if (transcript == null || transcript.Video.TenantId != tenantId)
            throw new Exception("Transcript segment not found");

        if (request.StartTimeSeconds.HasValue)
            transcript.StartTimeSeconds = request.StartTimeSeconds.Value;

        if (request.EndTimeSeconds.HasValue)
            transcript.EndTimeSeconds = request.EndTimeSeconds.Value;

        if (request.Text != null)
            transcript.Text = request.Text;

        if (request.Confidence.HasValue)
            transcript.Confidence = request.Confidence.Value;

        if (request.Language != null)
            transcript.Language = request.Language;

        if (request.Speaker != null)
            transcript.Speaker = request.Speaker;

        await _dbContext.SaveChangesAsync();

        return new TranscriptDto
        {
            Id = transcript.Id,
            VideoId = transcript.VideoId,
            StartTimeSeconds = transcript.StartTimeSeconds,
            EndTimeSeconds = transcript.EndTimeSeconds,
            Text = transcript.Text,
            Confidence = transcript.Confidence,
            Language = transcript.Language,
            Speaker = transcript.Speaker,
            SortOrder = transcript.SortOrder,
            CreatedAt = transcript.CreatedAt
        };
    }

    public async Task DeleteTranscriptSegmentAsync(Guid segmentId, Guid userId, Guid tenantId)
    {
        var transcript = await _dbContext.VideoTranscripts
            .Include(vt => vt.Video)
            .FirstOrDefaultAsync(vt => vt.Id == segmentId);

        if (transcript == null || transcript.Video.TenantId != tenantId)
            throw new Exception("Transcript segment not found");

        _dbContext.VideoTranscripts.Remove(transcript);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<TranscriptDto>> SearchTranscriptAsync(Guid videoId, string query, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var transcripts = await _dbContext.VideoTranscripts
            .Where(vt => vt.VideoId == videoId && vt.Text.ToLower().Contains(query.ToLower()))
            .OrderBy(vt => vt.SortOrder)
            .ThenBy(vt => vt.StartTimeSeconds)
            .ToListAsync();

        return transcripts.Select(vt => new TranscriptDto
        {
            Id = vt.Id,
            VideoId = vt.VideoId,
            StartTimeSeconds = vt.StartTimeSeconds,
            EndTimeSeconds = vt.EndTimeSeconds,
            Text = vt.Text,
            Confidence = vt.Confidence,
            Language = vt.Language,
            Speaker = vt.Speaker,
            SortOrder = vt.SortOrder,
            CreatedAt = vt.CreatedAt
        }).ToList();
    }
}
