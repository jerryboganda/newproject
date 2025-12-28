using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.Transcripts.DTOs;

public class CreateTranscriptRequest
{
    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public List<TranscriptSegmentRequest> Segments { get; set; } = new();
}

public class TranscriptSegmentRequest
{
    [Required]
    public int StartTimeSeconds { get; set; }

    [Required]
    public int EndTimeSeconds { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    public float Confidence { get; set; } = 0.0f;

    public string? Language { get; set; }

    public string? Speaker { get; set; }
}

public class UpdateTranscriptRequest
{
    public int? StartTimeSeconds { get; set; }

    public int? EndTimeSeconds { get; set; }

    public string? Text { get; set; }

    public float? Confidence { get; set; }

    public string? Language { get; set; }

    public string? Speaker { get; set; }
}

public class TranscriptDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public int StartTimeSeconds { get; set; }
    public int EndTimeSeconds { get; set; }
    public string Text { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string? Language { get; set; }
    public string? Speaker { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
