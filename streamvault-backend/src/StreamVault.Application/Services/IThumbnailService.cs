namespace StreamVault.Application.Services;

public interface IThumbnailService
{
    Task<string> GenerateThumbnailAsync(string videoPath, string outputPath, ThumbnailOptions options);
    Task<List<string>> GenerateMultipleThumbnailsAsync(string videoPath, string outputDirectory, ThumbnailOptions options);
}

public class ThumbnailOptions
{
    public int Width { get; set; } = 320;
    public int Height { get; set; } = 240;
    public string Format { get; set; } = "jpg";
    public int Quality { get; set; } = 90;
    public TimeSpan? AtTime { get; set; } // Where to extract the thumbnail
    public int Count { get; set; } = 1; // Number of thumbnails to generate
}
