using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.AI.DTOs;

public class GeneratedTagDto
{
    public string Tag { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Source { get; set; } = string.Empty; // transcript, thumbnail, audio, metadata
    public string Type { get; set; } = string.Empty; // topic, object, person, location, emotion, etc.
    public int RelevanceScore { get; set; }
    public int Frequency { get; set; }
    public List<string> Context { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class GeneratedCategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string> SupportingTags { get; set; } = new();
}

public class VideoMetadataDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ExistingTags { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string Language { get; set; } = string.Empty;
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class VideoContentAnalysisDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<GeneratedTagDto> GeneratedTags { get; set; } = new();
    public List<GeneratedCategoryDto> SuggestedCategories { get; set; } = new();
    public List<DetectedObjectDto> DetectedObjects { get; set; } = new();
    public List<DetectedPersonDto> DetectedPersons { get; set; } = new();
    public List<DetectedLocationDto> DetectedLocations { get; set; } = new();
    public List<DetectedTextDto> DetectedText { get; set; } = new();
    public VideoEmotionAnalysisDto EmotionAnalysis { get; set; } = new();
    public VideoSceneAnalysisDto SceneAnalysis { get; set; } = new();
    public VideoAudioAnalysisDto AudioAnalysis { get; set; } = new();
    public List<string> KeyTopics { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public double ContentQualityScore { get; set; }
    public Dictionary<string, object> AnalysisMetadata { get; set; } = new();
    public DateTimeOffset AnalyzedAt { get; set; }
}

public class DetectedObjectDto
{
    public string ObjectName { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<string> Attributes { get; set; } = new();
    public TimeSpan Timestamp { get; set; }
}

public class DetectedPersonDto
{
    public string PersonId { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<string> Attributes { get; set; } = new();
    public TimeSpan Timestamp { get; set; }
    public bool IsSpeaker { get; set; }
    public double ScreenTimePercentage { get; set; }
}

public class DetectedLocationDto
{
    public string LocationName { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty; // indoor, outdoor, landmark, etc.
    public double Confidence { get; set; }
    public List<string> SupportingFeatures { get; set; } = new();
    public TimeSpan Timestamp { get; set; }
}

public class DetectedTextDto
{
    public string Text { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public TimeSpan Timestamp { get; set; }
    public TextType Type { get; set; }
}

public enum TextType
{
    OnScreen,
    Subtitle,
    Caption,
    Title,
    Credits
}

public class VideoEmotionAnalysisDto
{
    public string DominantEmotion { get; set; } = string.Empty;
    public Dictionary<string, double> EmotionScores { get; set; } = new();
    public List<EmotionTimelineDto> EmotionTimeline { get; set; } = new();
    public double SentimentScore { get; set; }
    public string SentimentLabel { get; set; } = string.Empty;
}

public class EmotionTimelineDto
{
    public TimeSpan Timestamp { get; set; }
    public string Emotion { get; set; } = string.Empty;
    public double Intensity { get; set; }
}

public class VideoSceneAnalysisDto
{
    public int SceneCount { get; set; }
    public List<SceneDto> Scenes { get; set; } = new();
    public string SceneComplexity { get; set; } = string.Empty;
    public double AverageSceneLength { get; set; }
    public List<string> SceneTypes { get; set; } = new();
}

public class SceneDto
{
    public int SceneNumber { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string SceneType { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public double TransitionScore { get; set; }
}

public class VideoAudioAnalysisDto
{
    public bool HasSpeech { get; set; }
    public string DetectedLanguage { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public bool HasMusic { get; set; }
    public List<string> MusicGenres { get; set; } = new();
    public bool HasSoundEffects { get; set; }
    public double AudioQuality { get; set; }
    public List<SpeechSegmentDto> SpeechSegments { get; set; } = new();
    public double AverageVolume { get; set; }
    public List<string> DetectedSpeakers { get; set; } = new();
}

public class SpeechSegmentDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string SpeakerId { get; set; } = string.Empty;
    public string Transcription { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class VideoInsightDto
{
    public string InsightType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan Timestamp { get; set; }
    public double Confidence { get; set; }
    public List<string> RelatedTags { get; set; } = new();
    public Dictionary<string, object> Data { get; set; } = new();
    public InsightPriority Priority { get; set; }
}

public enum InsightPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class TrendingTagDto
{
    public string Tag { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public double GrowthRate { get; set; }
    public List<string> RelatedVideos { get; set; } = new();
    public DateOnly Date { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class TagSuggestionDto
{
    public string Tag { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public bool IsExisting { get; set; }
    public Guid? TagId { get; set; }
}

public class BatchTagGenerationRequest
{
    public List<Guid> VideoIds { get; set; } = new();
    public List<string> Sources { get; set; } = new(); // transcript, thumbnail, audio, metadata
    public bool AutoApply { get; set; } = false;
    public double ConfidenceThreshold { get; set; } = 0.7;
    public int MaxTagsPerVideo { get; set; } = 20;
}

public class BatchTagGenerationResult
{
    public Dictionary<Guid, List<GeneratedTagDto>> Results { get; set; } = new();
    public int TotalVideos { get; set; }
    public int ProcessedVideos { get; set; }
    public int FailedVideos { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

public class TagTrainingDataDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ManualTags { get; set; } = new();
    public List<string> GeneratedTags { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public double EngagementRate { get; set; }
    public Dictionary<string, object> Features { get; set; } = new();
}

public class TagModelMetricsDto
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public int TrainingSamples { get; set; }
    public int ValidationSamples { get; set; }
    public DateTimeOffset LastTrainedAt { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public Dictionary<string, double> ClassMetrics { get; set; } = new();
}
