using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.Documentation.DTOs;

public class ApiDocumentationDto
{
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public List<ApiSectionDto> Sections { get; set; } = new();
    public List<AuthenticationMethodDto> AuthenticationMethods { get; set; } = new();
    public List<ResponseCodeDto> CommonResponseCodes { get; set; } = new();
    public List<ApiModelDto> Models { get; set; } = new();
    public DateTimeOffset LastUpdated { get; set; }
    public List<string> SupportedVersions { get; set; } = new();
}

public class ApiSectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ApiEndpointDto> Endpoints { get; set; } = new();
    public int Order { get; set; }
}

public class ApiEndpointDto
{
    public string Id { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ParameterDto> Parameters { get; set; } = new();
    public RequestBodyDto? RequestBody { get; set; }
    public List<ResponseDto> Responses { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public bool IsDeprecated { get; set; }
    public string? DeprecationMessage { get; set; }
    public List<string> Produces { get; set; } = new();
    public List<string> Consumes { get; set; } = new();
    public List<SecurityRequirementDto> Security { get; set; } = new();
}

public class ParameterDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty; // query, path, header, cookie
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; }
    public bool Deprecated { get; set; }
    public bool AllowEmptyValue { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public object? Default { get; set; }
    public List<string> Enum { get; set; } = new();
    public SchemaDto? Schema { get; set; }
}

public class RequestBodyDto
{
    public string Description { get; set; } = string.Empty;
    public List<MediaTypeDto> Content { get; set; } = new();
    public bool Required { get; set; }
}

public class MediaTypeDto
{
    public string MimeType { get; set; } = string.Empty;
    public SchemaDto Schema { get; set; } = new();
    public Dictionary<string, object> Examples { get; set; } = new();
}

public class SchemaDto
{
    public string Type { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string? Description { get; set; }
    public object? Default { get; set; }
    public List<string> Enum { get; set; } = new();
    public Dictionary<string, SchemaDto> Properties { get; set; } = new();
    public List<string> Required { get; set; } = new();
    public SchemaDto? Items { get; set; }
    public bool Nullable { get; set; }
    public double? Minimum { get; set; }
    public double? Maximum { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public Dictionary<string, object> Extensions { get; set; } = new();
}

public class ResponseDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<MediaTypeDto> Content { get; set; } = new();
    public List<HeaderDto> Headers { get; set; } = new();
}

public class HeaderDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; }
    public bool Deprecated { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
}

public class SecurityRequirementDto
{
    public string Scheme { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
}

public class AuthenticationMethodDto
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public List<string> Scopes { get; set; } = new();
}

public class ResponseCodeDto
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ApiModelDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, PropertyDto> Properties { get; set; } = new();
    public List<string> Required { get; set; } = new();
    public string Type { get; set; } = string.Empty;
}

public class PropertyDto
{
    public string Type { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; }
    public object? Default { get; set; }
    public List<string> Enum { get; set; } = new();
    public PropertyDto? Items { get; set; }
}

public class CodeSampleDto
{
    public string Id { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public bool IsExecutable { get; set; }
    public string? ExecutionCommand { get; set; }
}

public class GenerateCodeSampleRequest
{
    [Required]
    public string EndpointId { get; set; } = string.Empty;
    
    [Required]
    public string Language { get; set; } = string.Empty;
    
    public Dictionary<string, object>? Parameters { get; set; }
    
    public bool IncludeAuthentication { get; set; } = true;
    
    public string? ApiVersion { get; set; }
}

public class TestEndpointRequest
{
    [Required]
    public string EndpointId { get; set; } = string.Empty;
    
    [Required]
    public string Method { get; set; } = string.Empty;
    
    public string? Url { get; set; }
    
    public Dictionary<string, string>? Headers { get; set; }
    
    public object? Body { get; set; }
    
    public Dictionary<string, string>? QueryParameters { get; set; }
    
    public string? AuthenticationToken { get; set; }
    
    public string? ApiKey { get; set; }
}

public class TestResultDto
{
    public Guid Id { get; set; }
    public string EndpointId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TestHistoryDto
{
    public Guid Id { get; set; }
    public string EndpointId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
    public bool Success { get; set; }
}

public class GenerateSdkRequest
{
    [Required]
    public string Language { get; set; } = string.Empty;
    
    [Required]
    public string Version { get; set; } = string.Empty;
    
    public List<string> IncludeEndpoints { get; set; } = new();
    
    public List<string> ExcludeEndpoints { get; set; } = new();
    
    public string? PackageName { get; set; }
    
    public bool IncludeExamples { get; set; } = true;
    
    public bool IncludeTests { get; set; } = false;
}

public class SdkLanguageDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public bool IsStable { get; set; }
}

public class ApiChangeDto
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public ChangeType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> AffectedEndpoints { get; set; } = new();
    public bool IsBreaking { get; set; }
    public DateTimeOffset ReleaseDate { get; set; }
    public string? MigrationGuide { get; set; }
}

public class ApiStatusDto
{
    public bool IsHealthy { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTimeOffset Uptime { get; set; }
    public Dictionary<string, ServiceStatusDto> Services { get; set; } = new();
    public List<string> ActiveVersions { get; set; } = new();
    public List<string> DeprecatedVersions { get; set; } = new();
}

public class ServiceStatusDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public DateTimeOffset LastChecked { get; set; }
}

public class ApiMetricsDto
{
    public DateTimeOffset Date { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public Dictionary<string, int> EndpointUsage { get; set; } = new();
    public Dictionary<string, int> ErrorCodes { get; set; } = new();
    public int UniqueUsers { get; set; }
}

public class InteractiveSessionDto
{
    public Guid Id { get; set; }
    public string Language { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> AvailableCommands { get; set; } = new();
}

public class CreateSessionRequest
{
    [Required]
    public string Language { get; set; } = string.Empty;
    
    public TimeSpan? Timeout { get; set; }
    
    public List<string> InitialCommands { get; set; } = new();
}

public class ExecuteCommandRequest
{
    [Required]
    public string Command { get; set; } = string.Empty;
    
    public Dictionary<string, object>? Variables { get; set; }
}

public class SessionHistoryDto
{
    public Guid SessionId { get; set; }
    public List<CommandExecutionDto> Executions { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastActivity { get; set; }
}

public class CommandExecutionDto
{
    public string Command { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string? Error { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
}

public class DocumentationAnalyticsDto
{
    public int TotalViews { get; set; }
    public int UniqueVisitors { get; set; }
    public Dictionary<string, int> PageViews { get; set; } = new();
    public List<PopularEndpointDto> PopularEndpoints { get; set; } = new();
    public Dictionary<string, int> CodeSampleDownloads { get; set; } = new();
    public int SdkDownloads { get; set; }
    public List<SearchQueryDto> PopularSearches { get; set; } = new();
}

public class PopularEndpointDto
{
    public string EndpointId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Tests { get; set; }
    public double AverageRating { get; set; }
}

public class SearchQueryDto
{
    public string Query { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageResults { get; set; }
}

public class SubmitFeedbackRequest
{
    [Required]
    public string Type { get; set; } = string.Empty;
    
    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public string? Page { get; set; }
    
    public string? Endpoint { get; set; }
    
    public int? Rating { get; set; }
    
    public string? Email { get; set; }
}

public class DocumentationFeedbackDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Page { get; set; }
    public string? Endpoint { get; set; }
    public int? Rating { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string? Response { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
}

public class TutorialDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<TutorialStepDto> Steps { get; set; } = new();
    public string? VideoUrl { get; set; }
    public List<string> CodeSamples { get; set; } = new();
    public int Views { get; set; }
    public int Completions { get; set; }
    public double AverageRating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

public class TutorialStepDto
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Image { get; set; }
    public bool IsOptional { get; set; }
    public List<string> Prerequisites { get; set; } = new();
}

public class ApiKeyInfoDto
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DocumentationUrl { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
    public bool IsRecommended { get; set; }
}

public class AuthenticationGuideDto
{
    public string Method { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<GuideStepDto> Steps { get; set; } = new();
    public List<CodeSampleDto> Examples { get; set; } = new();
    public List<string> CommonErrors { get; set; } = new();
    public Dictionary<string, string> Troubleshooting { get; set; } = new();
}

public class GuideStepDto
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Note { get; set; }
}

// Enums
public enum ChangeType
{
    Added,
    Modified,
    Deprecated,
    Removed,
    Fixed,
    Security
}
