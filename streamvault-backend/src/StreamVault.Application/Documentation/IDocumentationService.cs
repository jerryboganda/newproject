using StreamVault.Application.Documentation.DTOs;

namespace StreamVault.Application.Documentation;

public interface IDocumentationService
{
    // API Documentation
    Task<ApiDocumentationDto> GetApiDocumentationAsync(string version = "v1", string? section = null);
    Task<List<ApiEndpointDto>> GetEndpointsAsync(string version = "v1", string? controller = null);
    Task<ApiEndpointDto> GetEndpointDetailsAsync(string endpointId, string version = "v1");
    
    // Code Samples
    Task<List<CodeSampleDto>> GetCodeSamplesAsync(string endpointId, string language, string version = "v1");
    Task<CodeSampleDto> GenerateCodeSampleAsync(GenerateCodeSampleRequest request);
    
    // API Testing
    Task<TestResultDto> TestEndpointAsync(TestEndpointRequest request);
    Task<List<TestHistoryDto>> GetTestHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
    
    // SDK Generation
    Task<byte[]> GenerateSdkAsync(GenerateSdkRequest request);
    Task<List<SdkLanguageDto>> GetSupportedSdkLanguagesAsync();
    
    // OpenAPI/Swagger
    Task<string> GetOpenApiSpecAsync(string version = "v1", string format = "json");
    Task<bool> ValidateOpenApiSpecAsync(string specContent);
    
    // API Changelog
    Task<List<ApiChangeDto>> GetApiChangesAsync(string version, int page = 1, int pageSize = 20);
    Task<ApiChangeDto> GetChangeDetailsAsync(Guid changeId);
    
    // API Status
    Task<ApiStatusDto> GetApiStatusAsync();
    Task<List<ApiMetricsDto>> GetApiMetricsAsync(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    
    // Interactive Documentation
    Task<InteractiveSessionDto> CreateInteractiveSessionAsync(CreateSessionRequest request);
    Task<bool> ExecuteInteractiveCommandAsync(Guid sessionId, ExecuteCommandRequest request);
    Task<SessionHistoryDto> GetSessionHistoryAsync(Guid sessionId);
    
    // Documentation Analytics
    Task<DocumentationAnalyticsDto> GetDocumentationAnalyticsAsync(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<List<PopularEndpointDto>> GetPopularEndpointsAsync(int limit = 10);
    
    // Feedback and Support
    Task<bool> SubmitDocumentationFeedbackAsync(SubmitFeedbackRequest request);
    Task<List<DocumentationFeedbackDto>> GetFeedbackAsync(int page = 1, int pageSize = 20);
    
    // Tutorials and Guides
    Task<List<TutorialDto>> GetTutorialsAsync(string? category = null, string? difficulty = null);
    Task<TutorialDto> GetTutorialAsync(Guid tutorialId);
    Task<bool> CompleteTutorialAsync(Guid tutorialId, Guid userId);
    
    // API Keys and Authentication
    Task<List<ApiKeyInfoDto>> GetAuthenticationMethodsAsync();
    Task<AuthenticationGuideDto> GetAuthenticationGuideAsync(string method);
}
