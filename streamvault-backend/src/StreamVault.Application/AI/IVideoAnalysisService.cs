using StreamVault.Application.AI.DTOs;

namespace StreamVault.Application.AI;

public interface IVideoAnalysisService
{
    // Video Content Analysis
    Task<VideoAnalysisDto> AnalyzeVideoAsync(Guid videoId, Guid tenantId, AnalysisRequest request);
    Task<VideoTranscriptionDto> TranscribeVideoAsync(Guid videoId, Guid tenantId, TranscriptionRequest request);
    Task<VideoObjectDetectionDto> DetectObjectsAsync(Guid videoId, Guid tenantId, ObjectDetectionRequest request);
    Task<VideoSceneDetectionDto> DetectScenesAsync(Guid videoId, Guid tenantId, SceneDetectionRequest request);
    Task<VideoFaceDetectionDto> DetectFacesAsync(Guid videoId, Guid tenantId, FaceDetectionRequest request);
    
    // Audio Analysis
    Task<AudioAnalysisDto> AnalyzeAudioAsync(Guid videoId, Guid tenantId, AudioAnalysisRequest request);
    Task<AudioTranscriptionDto> TranscribeAudioAsync(Guid videoId, Guid tenantId, AudioTranscriptionRequest request);
    Task<AudioMoodDto> AnalyzeMoodAsync(Guid videoId, Guid tenantId, MoodAnalysisRequest request);
    Task<AudioClassificationDto> ClassifyAudioAsync(Guid videoId, Guid tenantId, AudioClassificationRequest request);
    
    // Visual Analysis
    Task<VisualAnalysisDto> AnalyzeVisualsAsync(Guid videoId, Guid tenantId, VisualAnalysisRequest request);
    Task<ColorAnalysisDto> AnalyzeColorsAsync(Guid videoId, Guid tenantId, ColorAnalysisRequest request);
    Task<MotionAnalysisDto> AnalyzeMotionAsync(Guid videoId, Guid tenantId, MotionAnalysisRequest request);
    Task<QualityAnalysisDto> AnalyzeQualityAsync(Guid videoId, Guid tenantId, QualityAnalysisRequest request);
    
    // Text Analysis
    Task<TextAnalysisDto> AnalyzeTextAsync(Guid videoId, Guid tenantId, TextAnalysisRequest request);
    Task<SentimentAnalysisDto> AnalyzeSentimentAsync(Guid videoId, Guid tenantId, SentimentAnalysisRequest request);
    Task<KeywordExtractionDto> ExtractKeywordsAsync(Guid videoId, Guid tenantId, KeywordExtractionRequest request);
    Task<TopicModelingDto> ModelTopicsAsync(Guid videoId, Guid tenantId, TopicModelingRequest request);
    
    // Content Classification
    Task<ContentClassificationDto> ClassifyContentAsync(Guid videoId, Guid tenantId, ClassificationRequest request);
    Task<AgeRatingDto> DetermineAgeRatingAsync(Guid videoId, Guid tenantId, AgeRatingRequest request);
    Task<GenreClassificationDto> ClassifyGenreAsync(Guid videoId, Guid tenantId, GenreClassificationRequest request);
    Task<LanguageDetectionDto> DetectLanguageAsync(Guid videoId, Guid tenantId, LanguageDetectionRequest request);
    
    // Thumbnail Generation
    Task<ThumbnailGenerationDto> GenerateThumbnailsAsync(Guid videoId, Guid tenantId, ThumbnailRequest request);
    Task<ThumbnailAnalysisDto> AnalyzeThumbnailsAsync(Guid videoId, Guid tenantId, ThumbnailAnalysisRequest request);
    Task<ThumbnailOptimizationDto> OptimizeThumbnailsAsync(Guid videoId, Guid tenantId, ThumbnailOptimizationRequest request);
    
    // Video Summarization
    Task<VideoSummaryDto> SummarizeVideoAsync(Guid videoId, Guid tenantId, SummaryRequest request);
    Task<KeyMomentDetectionDto> DetectKeyMomentsAsync(Guid videoId, Guid tenantId, KeyMomentRequest request);
    Task<HighlightGenerationDto> GenerateHighlightsAsync(Guid videoId, Guid tenantId, HighlightRequest request);
    
    // Content Moderation
    Task<ModerationAnalysisDto> AnalyzeForModerationAsync(Guid videoId, Guid tenantId, ModerationRequest request);
    Task<ContentSafetyDto> AnalyzeSafetyAsync(Guid videoId, Guid tenantId, SafetyRequest request);
    Task<CopyrightDetectionDto> DetectCopyrightAsync(Guid videoId, Guid tenantId, CopyrightRequest request);
    
    // Performance Metrics
    Task<EngagementPredictionDto> PredictEngagementAsync(Guid videoId, Guid tenantId, EngagementPredictionRequest request);
    Task<ViewCountPredictionDto> PredictViewCountAsync(Guid videoId, Guid tenantId, ViewCountPredictionRequest request);
    Task<ViralPotentialDto> PredictViralPotentialAsync(Guid videoId, Guid tenantId, ViralPredictionRequest request);
    
    // Batch Processing
    Task<BatchAnalysisDto> ProcessBatchAsync(BatchAnalysisRequest request);
    Task<BatchAnalysisStatusDto> GetBatchStatusAsync(Guid batchId, Guid tenantId);
    Task<bool> CancelBatchAsync(Guid batchId, Guid tenantId);
    
    // Model Training
    Task<ModelTrainingDto> TrainCustomModelAsync(Guid tenantId, ModelTrainingRequest request);
    Task<ModelEvaluationDto> EvaluateModelAsync(Guid modelId, Guid tenantId);
    Task<bool> DeployModelAsync(Guid modelId, Guid tenantId, DeploymentRequest request);
    
    // Analysis History
    Task<List<AnalysisHistoryDto>> GetAnalysisHistoryAsync(Guid videoId, Guid tenantId, int page = 1, int pageSize = 20);
    Task<AnalysisDetailsDto> GetAnalysisDetailsAsync(Guid analysisId, Guid tenantId);
    Task<bool> DeleteAnalysisAsync(Guid analysisId, Guid tenantId);
    
    // Real-time Analysis
    Task<RealTimeAnalysisDto> StartRealTimeAnalysisAsync(Guid videoId, Guid tenantId, RealTimeAnalysisRequest request);
    Task<bool> UpdateRealTimeAnalysisAsync(Guid sessionId, Guid tenantId, RealTimeUpdateRequest request);
    Task<bool> StopRealTimeAnalysisAsync(Guid sessionId, Guid tenantId);
    
    // Export and Import
    Task<byte[]> ExportAnalysisAsync(Guid analysisId, Guid tenantId, ExportRequest request);
    Task<ImportResultDto> ImportAnalysisAsync(Guid tenantId, ImportRequest request);
    
    // Analysis Configuration
    Task<AnalysisConfigurationDto> GetConfigurationAsync(Guid tenantId);
    Task<bool> UpdateConfigurationAsync(Guid tenantId, AnalysisConfigurationDto configuration);
    Task<bool> ResetConfigurationAsync(Guid tenantId);
    
    // Analysis Templates
    Task<List<AnalysisTemplateDto>> GetTemplatesAsync(Guid tenantId);
    Task<AnalysisTemplateDto> GetTemplateAsync(Guid templateId, Guid tenantId);
    Task<Guid> CreateTemplateAsync(Guid tenantId, CreateTemplateRequest request);
    Task<bool> UpdateTemplateAsync(Guid templateId, Guid tenantId, UpdateTemplateRequest request);
    Task<bool> DeleteTemplateAsync(Guid templateId, Guid tenantId);
    
    // Analysis Scheduling
    Task<Guid> ScheduleAnalysisAsync(Guid videoId, Guid tenantId, ScheduleAnalysisRequest request);
    Task<bool> UpdateScheduledAnalysisAsync(Guid scheduleId, Guid tenantId, UpdateScheduleRequest request);
    Task<bool> CancelScheduledAnalysisAsync(Guid scheduleId, Guid tenantId);
    Task<List<ScheduledAnalysisDto>> GetScheduledAnalysesAsync(Guid tenantId, int page = 1, int pageSize = 20);
    
    // Analysis Notifications
    Task<bool> SubscribeToAnalysisNotificationsAsync(Guid userId, Guid tenantId, NotificationSubscriptionRequest request);
    Task<bool> UnsubscribeFromAnalysisNotificationsAsync(Guid userId, Guid tenantId, string eventType);
    Task<List<AnalysisNotificationDto>> GetNotificationsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    
    // Analysis Analytics
    Task<AnalysisAnalyticsDto> GetAnalysisAnalyticsAsync(Guid tenantId, AnalyticsRequest request);
    Task<List<PopularAnalysisDto>> GetPopularAnalysesAsync(Guid tenantId, int limit = 10);
    Task<AnalysisTrendsDto> GetAnalysisTrendsAsync(Guid tenantId, TrendsRequest request);
    
    // API Integration
    Task<bool> IntegrateExternalAPIAsync(Guid tenantId, ExternalAPIIntegrationRequest request);
    Task<List<ExternalAPIIntegrationDto>> GetExternalIntegrationsAsync(Guid tenantId);
    Task<bool> TestExternalAPIAsync(Guid integrationId, Guid tenantId);
    Task<bool> RemoveExternalAPIAsync(Guid integrationId, Guid tenantId);
}
