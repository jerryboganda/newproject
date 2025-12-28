using StreamVault.Application.Chapters.DTOs;
using StreamVault.Application.Transcripts.DTOs;

namespace StreamVault.Application.Embed.DTOs;

public class EmbedOptionsDto
{
    public int Width { get; set; } = 640;
    public int Height { get; set; } = 360;
    public bool Autoplay { get; set; } = false;
    public bool Controls { get; set; } = true;
    public bool Loop { get; set; } = false;
    public bool Muted { get; set; } = false;
    public string? Color { get; set; } = "#00adef";
    public bool ShowTitle { get; set; } = true;
    public bool ShowPortrait { get; set; } = true;
    public bool ShowByline { get; set; } = true;
    public bool Responsive { get; set; } = true;
    public string? StartAt { get; set; } // Time in seconds
}

public class EmbedConfigDto
{
    public Guid VideoId { get; set; }
    public string EmbedUrl { get; set; } = string.Empty;
    public string EmbedCode { get; set; } = string.Empty;
    public EmbedOptionsDto Options { get; set; } = new();
    public string PlayerUrl { get; set; } = string.Empty;
}

public class EmbedAnalyticsDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string? Referrer { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public int PlayCount { get; set; }
    public int WatchTimeSeconds { get; set; }
}

public class VideoPlayerConfigDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int DurationSeconds { get; set; }
    public EmbedOptionsDto Options { get; set; } = new();
    public List<ChapterDto> Chapters { get; set; } = new();
    public List<TranscriptDto> Transcripts { get; set; } = new();
}
