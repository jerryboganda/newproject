using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Api.Services;
using StreamVault.Application.Interfaces;
using StreamVault.Application.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/videos")]
public class VideosController : ControllerBase
{
	private readonly StreamVaultDbContext _dbContext;
	private readonly ITenantContext _tenantContext;
	private readonly IPlaybackTokenService _playbackTokenService;
	private readonly IBunnyNetService _bunnyNetService;
	private readonly IConfiguration _configuration;
	private readonly IWebhookPublisher _webhookPublisher;

	public VideosController(
		StreamVaultDbContext dbContext,
		ITenantContext tenantContext,
		IPlaybackTokenService playbackTokenService,
		IBunnyNetService bunnyNetService,
		IConfiguration configuration,
		IWebhookPublisher webhookPublisher)
	{
		_dbContext = dbContext;
		_tenantContext = tenantContext;
		_playbackTokenService = playbackTokenService;
		_bunnyNetService = bunnyNetService;
		_configuration = configuration;
		_webhookPublisher = webhookPublisher;
	}

	[HttpGet]
	[AllowAnonymous]
	public async Task<ActionResult<VideoListResponse>> List(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 20,
		CancellationToken cancellationToken = default)
	{
		if (page < 1) page = 1;
		if (pageSize is < 1 or > 200) pageSize = 20;

		var (tenantId, isAuthenticated) = await ResolveTenantAsync(cancellationToken);
		if (tenantId == null)
			return Ok(new VideoListResponse(Array.Empty<VideoListItem>(), 0, page, pageSize));

		var query = _dbContext.Videos
			.AsNoTracking()
			.Where(v => v.TenantId == tenantId && v.Status != VideoStatus.Deleted);

		if (!isAuthenticated)
			query = query.Where(v => v.IsPublic);

		var total = await query.CountAsync(cancellationToken);

		var items = await query
			.OrderByDescending(v => v.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(v => new VideoListItem(
				v.Id,
				v.Title,
				v.Description,
				v.ViewCount,
				v.IsPublic,
				v.Status.ToString(),
				v.CreatedAt,
				v.ThumbnailPath,
				null))
			.ToListAsync(cancellationToken);

		var frontendBase = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";
		var withWatch = items.Select(i => i with { WatchUrl = $"{frontendBase}/embed/{i.Id}" }).ToArray();
		return Ok(new VideoListResponse(withWatch, total, page, pageSize));
	}

	[HttpGet("{id:guid}")]
	[AllowAnonymous]
	public async Task<ActionResult<VideoDetailsResponse>> Get([FromRoute] Guid id, CancellationToken cancellationToken = default)
	{
		var (_, isAuthenticated) = await ResolveTenantAsync(cancellationToken);

		var video = await _dbContext.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
		if (video == null)
			return NotFound(new { error = "Video not found" });

		if (!isAuthenticated && !video.IsPublic)
			return NotFound(new { error = "Video not found" });

		var frontendBase = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";
		return Ok(new VideoDetailsResponse(
			video.Id,
			video.Title,
			video.Description,
			video.ViewCount,
			video.IsPublic,
			video.Status.ToString(),
			video.CreatedAt,
			video.PublishedAt,
			video.ThumbnailPath,
			$"{frontendBase}/embed/{video.Id}"));
	}

	[HttpPost("upload/initiate")]
	[Authorize]
	public async Task<ActionResult<InitiateTusUploadResponse>> InitiateUpload(
		[FromBody] InitiateTusUploadRequest request,
		CancellationToken cancellationToken = default)
	{
		var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
		if (tenantId == null)
			return BadRequest(new { error = "Tenant could not be resolved" });

		var userId = GetRequiredUserId();
		var video = new Video
		{
			Id = Guid.NewGuid(),
			Title = string.IsNullOrWhiteSpace(request.Title)
				? Path.GetFileNameWithoutExtension(request.FileName ?? "Untitled")
				: request.Title!.Trim(),
			Description = request.Description,
			UserId = userId,
			TenantId = tenantId.Value,
			OriginalFileName = request.FileName,
			MimeType = request.ContentType,
			Status = VideoStatus.Uploading,
			IsPublic = request.IsPublic,
			StoragePath = "pending"
		};

		_dbContext.Videos.Add(video);
		await _dbContext.SaveChangesAsync(cancellationToken);

		await _webhookPublisher.PublishAsync(
			tenantId.Value,
			"video.created",
			new
			{
				id = video.Id,
				title = video.Title,
				isPublic = video.IsPublic,
				status = video.Status.ToString(),
				createdAt = video.CreatedAt
			},
			cancellationToken);

		return Ok(new InitiateTusUploadResponse(
			VideoId: video.Id,
			TusEndpoint: "/api/v1/uploads/tus",
			UploadMetadata: new Dictionary<string, string>
			{
				["videoId"] = video.Id.ToString(),
				["fileName"] = request.FileName ?? "video",
				["contentType"] = request.ContentType ?? "application/octet-stream"
			}));
	}

	[HttpGet("{id:guid}/status")]
	[Authorize]
	public async Task<ActionResult<VideoStatusResponse>> Status([FromRoute] Guid id, CancellationToken cancellationToken = default)
	{
		var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
		if (tenantId == null)
			return BadRequest(new { error = "Tenant could not be resolved" });

		var video = await _dbContext.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId, cancellationToken);
		if (video == null) return NotFound(new { error = "Video not found" });

		return Ok(new VideoStatusResponse(MapStatus(video.Status), video.Status.ToString()));
	}

	[HttpPut("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateVideoRequest request, CancellationToken cancellationToken = default)
	{
		var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
		if (tenantId == null)
			return BadRequest(new { error = "Tenant could not be resolved" });

		var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId, cancellationToken);
		if (video == null) return NotFound(new { error = "Video not found" });

		if (!string.IsNullOrWhiteSpace(request.Title)) video.Title = request.Title.Trim();
		if (request.Description != null) video.Description = request.Description;
		if (request.IsPublic.HasValue) video.IsPublic = request.IsPublic.Value;
		video.UpdatedAt = DateTimeOffset.UtcNow;

		await _dbContext.SaveChangesAsync(cancellationToken);
		return Ok(new { success = true });
	}

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
	{
		var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
		if (tenantId == null)
			return BadRequest(new { error = "Tenant could not be resolved" });

		var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId, cancellationToken);
		if (video == null) return NotFound(new { error = "Video not found" });

		video.Status = VideoStatus.Deleted;
		video.UpdatedAt = DateTimeOffset.UtcNow;
		await _dbContext.SaveChangesAsync(cancellationToken);
		return Ok(new { success = true });
	}

	[HttpGet("{id:guid}/chapters")]
	[Authorize]
	public async Task<ActionResult<IReadOnlyList<VideoChapterResponse>>> Chapters([FromRoute] Guid id, CancellationToken cancellationToken = default)
	{
		var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
		if (tenantId == null)
			return BadRequest(new { error = "Tenant could not be resolved" });

		var exists = await _dbContext.Videos.AsNoTracking().AnyAsync(v => v.Id == id && v.TenantId == tenantId, cancellationToken);
		if (!exists) return NotFound(new { error = "Video not found" });

		var chapters = await _dbContext.VideoChapters
			.AsNoTracking()
			.Where(c => c.VideoId == id)
			.OrderBy(c => c.SortOrder)
			.Select(c => new VideoChapterResponse(c.Id, c.Title, c.StartTimeSeconds, c.EndTimeSeconds, c.Description, c.ThumbnailPath, c.SortOrder))
			.ToListAsync(cancellationToken);

		return Ok(chapters);
	}

	[HttpPost("{id:guid}/chapters")]
	[Authorize]
	public async Task<ActionResult<VideoChapterResponse>> AddChapter([FromRoute] Guid id, [FromBody] UpsertChapterRequest request, CancellationToken cancellationToken = default)
	{
		var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
		if (tenantId == null)
			return BadRequest(new { error = "Tenant could not be resolved" });

		var exists = await _dbContext.Videos.AsNoTracking().AnyAsync(v => v.Id == id && v.TenantId == tenantId, cancellationToken);
		if (!exists) return NotFound(new { error = "Video not found" });

		var chapter = new VideoChapter
		{
			Id = Guid.NewGuid(),
			VideoId = id,
			Title = request.Title?.Trim() ?? "Chapter",
			StartTimeSeconds = request.StartTimeSeconds,
			EndTimeSeconds = request.EndTimeSeconds,
			Description = request.Description,
			ThumbnailPath = request.ThumbnailPath,
			SortOrder = request.SortOrder
		};

		_dbContext.VideoChapters.Add(chapter);
		await _dbContext.SaveChangesAsync(cancellationToken);

		return Ok(new VideoChapterResponse(chapter.Id, chapter.Title, chapter.StartTimeSeconds, chapter.EndTimeSeconds, chapter.Description, chapter.ThumbnailPath, chapter.SortOrder));
	}

	[HttpDelete("{id:guid}/chapters/{chapterId:guid}")]
	[Authorize]
	public async Task<IActionResult> DeleteChapter([FromRoute] Guid id, [FromRoute] Guid chapterId, CancellationToken cancellationToken = default)
	{
		var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
		if (tenantId == null)
			return BadRequest(new { error = "Tenant could not be resolved" });

		var exists = await _dbContext.Videos.AsNoTracking().AnyAsync(v => v.Id == id && v.TenantId == tenantId, cancellationToken);
		if (!exists) return NotFound(new { error = "Video not found" });

		var chapter = await _dbContext.VideoChapters.FirstOrDefaultAsync(c => c.Id == chapterId && c.VideoId == id, cancellationToken);
		if (chapter == null) return NotFound(new { error = "Chapter not found" });

		_dbContext.VideoChapters.Remove(chapter);
		await _dbContext.SaveChangesAsync(cancellationToken);
		return Ok(new { success = true });
	}

	[HttpPut("{id:guid}/thumbnail")]
	[Authorize]
	public async Task<IActionResult> SetThumbnail([FromRoute] Guid id, [FromBody] SetThumbnailRequest request, CancellationToken cancellationToken = default)
	{
		var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
		if (tenantId == null)
			return BadRequest(new { error = "Tenant could not be resolved" });

		var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId, cancellationToken);
		if (video == null) return NotFound(new { error = "Video not found" });

		video.ThumbnailPath = request.ThumbnailUrl;
		video.UpdatedAt = DateTimeOffset.UtcNow;
		await _dbContext.SaveChangesAsync(cancellationToken);
		return Ok(new { success = true });
	}

	[HttpPost("{id:guid}/playback-token")]
	[AllowAnonymous]
	public async Task<ActionResult<PlaybackTokenResponse>> CreatePlaybackToken(
		[FromRoute] Guid id,
		[FromQuery] int expiresInSeconds = 3600,
		CancellationToken cancellationToken = default)
	{
		if (expiresInSeconds is < 60 or > 24 * 60 * 60)
			expiresInSeconds = 3600;

		var video = await _dbContext.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
		if (video == null) return NotFound(new { error = "Video not found" });

		if (!User.Identity?.IsAuthenticated ?? true)
		{
			if (!video.IsPublic)
				return Unauthorized(new { error = "Authentication required" });
		}

		var token = _playbackTokenService.GenerateVideoPlaybackToken(video.Id, video.TenantId, TimeSpan.FromSeconds(expiresInSeconds));
		var frontendBase = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";
		return Ok(new PlaybackTokenResponse(token, DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds), $"{frontendBase}/embed/{video.Id}?token={Uri.EscapeDataString(token)}"));
	}

	[HttpGet("{id:guid}/playback")]
	[AllowAnonymous]
	public async Task<ActionResult<PlaybackInfoResponse>> GetPlaybackInfo(
		[FromRoute] Guid id,
		[FromQuery] string token,
		CancellationToken cancellationToken = default)
	{
		if (!_playbackTokenService.TryValidateVideoPlaybackToken(token, id, out var tenantId))
			return Unauthorized(new { error = "Invalid or expired token" });

		var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
		if (tenant == null) return Unauthorized(new { error = "Invalid token" });
		_tenantContext.SetCurrentTenant(tenant);

		var video = await _dbContext.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId, cancellationToken);
		if (video == null) return NotFound(new { error = "Video not found" });

		var mp4Url = _bunnyNetService.GetStreamMp4Url(video.StoragePath) ?? string.Empty;
		var thumbnailUrl = video.ThumbnailPath ?? _bunnyNetService.GetStreamThumbnailUrl(video.StoragePath);
		return Ok(new PlaybackInfoResponse(mp4Url, thumbnailUrl));
	}

	private Guid GetRequiredUserId()
	{
		var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		if (!Guid.TryParse(id, out var userId))
			throw new InvalidOperationException("User id missing from token");
		return userId;
	}

	private async Task<(Guid? tenantId, bool isAuthenticated)> ResolveTenantAsync(CancellationToken cancellationToken)
	{
		if (_tenantContext.TenantId.HasValue)
			return (_tenantContext.TenantId, User.Identity?.IsAuthenticated == true);

		var tenantClaim = User.FindFirstValue("tenant_id");
		if (Guid.TryParse(tenantClaim, out var tenantIdFromToken))
		{
			var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantIdFromToken, cancellationToken);
			if (tenant != null)
			{
				_tenantContext.SetCurrentTenant(tenant);
				return (tenant.Id, User.Identity?.IsAuthenticated == true);
			}
		}

		if (Request.Headers.TryGetValue("X-Tenant-Slug", out var slugValues))
		{
			var slug = slugValues.ToString();
			if (!string.IsNullOrWhiteSpace(slug))
			{
				var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
				if (tenant != null)
				{
					_tenantContext.SetCurrentTenant(tenant);
					return (tenant.Id, User.Identity?.IsAuthenticated == true);
				}
			}
		}

		return (null, User.Identity?.IsAuthenticated == true);
	}

	private static string MapStatus(VideoStatus status)
	{
		return status switch
		{
			VideoStatus.Uploading => "uploading",
			VideoStatus.Uploaded => "uploaded",
			VideoStatus.Processing => "processing",
			VideoStatus.Processed => "ready",
			VideoStatus.Failed => "failed",
			VideoStatus.Deleted => "deleted",
			_ => "unknown"
		};
	}
}

public record VideoListResponse(IReadOnlyList<VideoListItem> Items, int TotalCount, int Page, int PageSize);

public record VideoListItem(
	Guid Id,
	string Title,
	string? Description,
	int ViewCount,
	bool IsPublic,
	string Status,
	DateTimeOffset CreatedAt,
	string? ThumbnailUrl,
	string? WatchUrl);

public record VideoDetailsResponse(
	Guid Id,
	string Title,
	string? Description,
	int ViewCount,
	bool IsPublic,
	string Status,
	DateTimeOffset CreatedAt,
	DateTimeOffset? PublishedAt,
	string? ThumbnailUrl,
	string WatchUrl);

public record InitiateTusUploadRequest(
	string? Title,
	string? Description,
	string? FileName,
	string? ContentType,
	bool IsPublic = false);

public record InitiateTusUploadResponse(Guid VideoId, string TusEndpoint, IDictionary<string, string> UploadMetadata);

public record VideoStatusResponse(string Status, string RawStatus);

public record UpdateVideoRequest(string? Title, string? Description, bool? IsPublic);

public record UpsertChapterRequest(string? Title, int StartTimeSeconds, int EndTimeSeconds, string? Description, string? ThumbnailPath, int SortOrder = 0);

public record VideoChapterResponse(Guid Id, string Title, int StartTimeSeconds, int EndTimeSeconds, string? Description, string? ThumbnailPath, int SortOrder);

public record SetThumbnailRequest(string? ThumbnailUrl);

public record PlaybackTokenResponse(string Token, DateTimeOffset ExpiresAt, string EmbedUrl);

public record PlaybackInfoResponse(string Mp4Url, string? ThumbnailUrl);
