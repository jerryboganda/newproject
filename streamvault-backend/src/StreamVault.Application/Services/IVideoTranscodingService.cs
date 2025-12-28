namespace StreamVault.Application.Services;

public interface IVideoTranscodingService
{
    Task<string> TranscodeVideoAsync(string inputPath, string outputPath, VideoTranscodeOptions options);
    Task<VideoMetadata> ExtractMetadataAsync(string videoPath);
}

public class VideoTranscodeOptions
{
    public string OutputFormat { get; set; } = "mp4";
    public string Quality { get; set; } = "high";
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Bitrate { get; set; }
    public bool GenerateThumbnail { get; set; } = true;
}

public class VideoMetadata
{
    public int DurationSeconds { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }
    public string Format { get; set; } = string.Empty;
    public string Codec { get; set; } = string.Empty;
    public double FrameRate { get; set; }
    public int Bitrate { get; set; }
}
