using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Interfaces;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/collections")]
[Authorize]
public class CollectionsController : ControllerBase
{
	private readonly StreamVaultDbContext _dbContext;
	private readonly ITenantContext _tenantContext;

	public CollectionsController(StreamVaultDbContext dbContext, ITenantContext tenantContext)
	{
		_dbContext = dbContext;
		_tenantContext = tenantContext;
	}

	[HttpGet]
	public async Task<ActionResult<IReadOnlyList<CollectionDto>>> List(CancellationToken cancellationToken = default)
	{
		var tenantId = await RequireTenantAsync(cancellationToken);

		var items = await _dbContext.Playlists
			.AsNoTracking()
			.Where(p => p.TenantId == tenantId && !p.IsDeleted)
			.OrderByDescending(p => p.CreatedAt)
			.Select(p => new CollectionDto(
				p.Id,
				p.Name,
				p.Description,
				p.IsPublic,
				_dbContext.PlaylistVideos.Count(pv => pv.PlaylistId == p.Id),
				p.CreatedAt,
				p.UpdatedAt))
			.ToListAsync(cancellationToken);

		return Ok(items);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<CollectionDto>> Get([FromRoute] Guid id, CancellationToken cancellationToken = default)
	{
		var tenantId = await RequireTenantAsync(cancellationToken);

		var playlist = await _dbContext.Playlists
			.AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && !p.IsDeleted, cancellationToken);

		if (playlist == null)
			return NotFound(new { error = "Collection not found" });

		var videoCount = await _dbContext.PlaylistVideos.CountAsync(pv => pv.PlaylistId == playlist.Id, cancellationToken);
		return Ok(new CollectionDto(
			playlist.Id,
			playlist.Name,
			playlist.Description,
			playlist.IsPublic,
			videoCount,
			playlist.CreatedAt,
			playlist.UpdatedAt));
	}

	[HttpPost]
	public async Task<ActionResult<CollectionDto>> Create([FromBody] CreateCollectionRequest request, CancellationToken cancellationToken = default)
	{
		var tenantId = await RequireTenantAsync(cancellationToken);
		var userId = GetRequiredUserId();

		var name = request.Name ?? request.Title;
		if (string.IsNullOrWhiteSpace(name))
			name = "Untitled";

		var playlist = new Playlist
		{
			Id = Guid.NewGuid(),
			TenantId = tenantId,
			UserId = userId,
			Name = name.Trim(),
			Description = request.Description,
			IsPublic = request.IsPublic
		};

		_dbContext.Playlists.Add(playlist);
		await _dbContext.SaveChangesAsync(cancellationToken);

		return Ok(new CollectionDto(
			playlist.Id,
			playlist.Name,
			playlist.Description,
			playlist.IsPublic,
			0,
			playlist.CreatedAt,
			playlist.UpdatedAt));
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCollectionRequest request, CancellationToken cancellationToken = default)
	{
		var tenantId = await RequireTenantAsync(cancellationToken);

		var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && !p.IsDeleted, cancellationToken);
		if (playlist == null) return NotFound(new { error = "Collection not found" });

		var name = request.Name ?? request.Title;
		if (!string.IsNullOrWhiteSpace(name)) playlist.Name = name.Trim();
		if (request.Description != null) playlist.Description = request.Description;
		if (request.IsPublic.HasValue) playlist.IsPublic = request.IsPublic.Value;
		playlist.UpdatedAt = DateTimeOffset.UtcNow;

		await _dbContext.SaveChangesAsync(cancellationToken);

		var videoCount = await _dbContext.PlaylistVideos.CountAsync(pv => pv.PlaylistId == playlist.Id, cancellationToken);
		return Ok(new CollectionDto(
			playlist.Id,
			playlist.Name,
			playlist.Description,
			playlist.IsPublic,
			videoCount,
			playlist.CreatedAt,
			playlist.UpdatedAt));
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
	{
		var tenantId = await RequireTenantAsync(cancellationToken);

		var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && !p.IsDeleted, cancellationToken);
		if (playlist == null) return NotFound(new { error = "Collection not found" });

		playlist.IsDeleted = true;
		playlist.UpdatedAt = DateTimeOffset.UtcNow;
		await _dbContext.SaveChangesAsync(cancellationToken);
		return Ok(new { success = true });
	}

	[HttpPost("{id:guid}/videos")]
	public async Task<IActionResult> AddVideo([FromRoute] Guid id, [FromBody] AddVideoRequest request, CancellationToken cancellationToken = default)
	{
		var tenantId = await RequireTenantAsync(cancellationToken);
		if (request.VideoId == Guid.Empty)
			return BadRequest(new { error = "videoId is required" });

		var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && !p.IsDeleted, cancellationToken);
		if (playlist == null)
			return NotFound(new { error = "Collection not found" });

		var videoExists = await _dbContext.Videos.AnyAsync(v => v.Id == request.VideoId && v.TenantId == tenantId, cancellationToken);
		if (!videoExists)
			return NotFound(new { error = "Video not found" });

		var already = await _dbContext.PlaylistVideos.AnyAsync(pv => pv.PlaylistId == playlist.Id && pv.VideoId == request.VideoId, cancellationToken);
		if (already)
			return Ok(new { success = true });

		var nextPos = await _dbContext.PlaylistVideos
			.Where(pv => pv.PlaylistId == playlist.Id)
			.Select(pv => (int?)pv.Position)
			.MaxAsync(cancellationToken) ?? 0;

		_dbContext.PlaylistVideos.Add(new PlaylistVideo
		{
			Id = Guid.NewGuid(),
			PlaylistId = playlist.Id,
			VideoId = request.VideoId,
			Position = nextPos + 1,
			AddedAt = DateTimeOffset.UtcNow
		});

		playlist.UpdatedAt = DateTimeOffset.UtcNow;
		await _dbContext.SaveChangesAsync(cancellationToken);
		return Ok(new { success = true });
	}

	[HttpDelete("{id:guid}/videos/{videoId:guid}")]
	public async Task<IActionResult> RemoveVideo([FromRoute] Guid id, [FromRoute] Guid videoId, CancellationToken cancellationToken = default)
	{
		var tenantId = await RequireTenantAsync(cancellationToken);

		var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && !p.IsDeleted, cancellationToken);
		if (playlist == null)
			return NotFound(new { error = "Collection not found" });

		var link = await _dbContext.PlaylistVideos.FirstOrDefaultAsync(pv => pv.PlaylistId == playlist.Id && pv.VideoId == videoId, cancellationToken);
		if (link == null)
			return Ok(new { success = true });

		_dbContext.PlaylistVideos.Remove(link);
		playlist.UpdatedAt = DateTimeOffset.UtcNow;
		await _dbContext.SaveChangesAsync(cancellationToken);
		return Ok(new { success = true });
	}

	[HttpPut("{id:guid}/videos/reorder")]
	public async Task<IActionResult> ReorderVideos([FromRoute] Guid id, [FromBody] ReorderVideosRequest request, CancellationToken cancellationToken = default)
	{
		var tenantId = await RequireTenantAsync(cancellationToken);

		var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && !p.IsDeleted, cancellationToken);
		if (playlist == null)
			return NotFound(new { error = "Collection not found" });

		var orders = request.VideoOrders ?? Array.Empty<VideoOrder>();
		if (orders.Length == 0)
			return Ok(new { success = true });

		var ids = orders.Select(o => o.VideoId).Where(g => g != Guid.Empty).ToArray();
		var links = await _dbContext.PlaylistVideos
			.Where(pv => pv.PlaylistId == playlist.Id && ids.Contains(pv.VideoId))
			.ToListAsync(cancellationToken);

		var byVideo = orders
			.Where(o => o.VideoId != Guid.Empty)
			.GroupBy(o => o.VideoId)
			.ToDictionary(g => g.Key, g => g.First().Order);

		foreach (var link in links)
		{
			if (byVideo.TryGetValue(link.VideoId, out var pos))
				link.Position = pos;
		}

		playlist.UpdatedAt = DateTimeOffset.UtcNow;
		await _dbContext.SaveChangesAsync(cancellationToken);
		return Ok(new { success = true });
	}

	private Guid GetRequiredUserId()
	{
		var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		if (!Guid.TryParse(id, out var userId))
			throw new InvalidOperationException("User id missing from token");
		return userId;
	}

	private async Task<Guid> RequireTenantAsync(CancellationToken cancellationToken)
	{
		if (_tenantContext.TenantId.HasValue) return _tenantContext.TenantId.Value;

		var tenantClaim = User.FindFirstValue("tenant_id");
		if (Guid.TryParse(tenantClaim, out var tenantIdFromToken))
		{
			var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantIdFromToken, cancellationToken);
			if (tenant != null)
			{
				_tenantContext.SetCurrentTenant(tenant);
				return tenant.Id;
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
					return tenant.Id;
				}
			}
		}

		throw new InvalidOperationException("Tenant could not be resolved");
	}
}

public record CollectionDto(
	Guid Id,
	string Name,
	string? Description,
	bool IsPublic,
	int VideoCount,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt);

public record CreateCollectionRequest(string? Name, string? Title, string? Description, bool IsPublic = false);

public record UpdateCollectionRequest(string? Name, string? Title, string? Description, bool? IsPublic);

public record AddVideoRequest(Guid VideoId);

public record VideoOrder(Guid VideoId, int Order);

public record ReorderVideosRequest(VideoOrder[]? VideoOrders);
