using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Videos;
using StreamVault.Application.Videos.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/videos/{videoId}/chapters")]
[Authorize]
public class VideoChapterController : ControllerBase
{
    private readonly IVideoChapterService _chapterService;

    public VideoChapterController(IVideoChapterService chapterService)
    {
        _chapterService = chapterService;
    }

    [HttpPost]
    public async Task<ActionResult<VideoChapterDto>> CreateChapter(Guid videoId, [FromBody] CreateChapterRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var chapter = await _chapterService.CreateChapterAsync(videoId, userId, tenantId, request);
            return Ok(chapter);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<VideoChapterDto>>> GetVideoChapters(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var chapters = await _chapterService.GetVideoChaptersAsync(videoId, userId, tenantId);
            return Ok(chapters);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{chapterId}")]
    public async Task<ActionResult<VideoChapterDto>> GetChapter(Guid videoId, Guid chapterId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var chapter = await _chapterService.GetChapterAsync(chapterId, userId, tenantId);
            if (chapter == null)
                return NotFound();

            return Ok(chapter);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{chapterId}")]
    public async Task<ActionResult<bool>> UpdateChapter(Guid videoId, Guid chapterId, [FromBody] UpdateChapterRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _chapterService.UpdateChapterAsync(chapterId, userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{chapterId}")]
    public async Task<ActionResult<bool>> DeleteChapter(Guid videoId, Guid chapterId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _chapterService.DeleteChapterAsync(chapterId, userId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("reorder")]
    public async Task<ActionResult<bool>> ReorderChapters(Guid videoId, [FromBody] List<Guid> chapterIds)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _chapterService.ReorderChaptersAsync(videoId, userId, tenantId, chapterIds);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("auto-generate")]
    public async Task<ActionResult<VideoChapterDto>> AutoGenerateChapters(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var chapter = await _chapterService.AutoGenerateChaptersAsync(videoId, userId, tenantId);
            return Ok(chapter);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("import")]
    public async Task<ActionResult<bool>> ImportChapters(Guid videoId, [FromBody] ImportChaptersRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _chapterService.ImportChaptersAsync(videoId, userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("export")]
    public async Task<ActionResult<string>> ExportChapters(Guid videoId, [FromQuery] string format = "json")
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var exportData = await _chapterService.ExportChaptersAsync(videoId, userId, tenantId, format);
            
            var contentType = format.ToLower() switch
            {
                "json" => "application/json",
                "csv" => "text/csv",
                "youtube" => "text/plain",
                "vtt" => "text/vtt",
                _ => "application/json"
            };

            return Content(exportData, contentType);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("navigation")]
    public async Task<ActionResult<List<ChapterNavigationDto>>> GetChapterNavigation(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var navigation = await _chapterService.GetChapterNavigationAsync(videoId, userId, tenantId);
            return Ok(navigation);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{chapterId}/thumbnail")]
    public async Task<ActionResult<bool>> SetChapterThumbnail(Guid videoId, Guid chapterId, [FromBody] SetThumbnailRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _chapterService.SetChapterThumbnailAsync(chapterId, userId, tenantId, request.ThumbnailUrl);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

[ApiController]
[Route("api/v1/videos/{videoId}/markers")]
[Authorize]
public class VideoMarkerController : ControllerBase
{
    private readonly IVideoChapterService _chapterService;

    public VideoMarkerController(IVideoChapterService chapterService)
    {
        _chapterService = chapterService;
    }

    [HttpPost]
    public async Task<ActionResult<VideoMarkerDto>> AddMarker(Guid videoId, [FromBody] CreateMarkerRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var marker = await _chapterService.AddMarkerAsync(videoId, userId, tenantId, request);
            return Ok(marker);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<VideoMarkerDto>>> GetVideoMarkers(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var markers = await _chapterService.GetVideoMarkersAsync(videoId, userId, tenantId);
            return Ok(markers);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<List<VideoMarkerDto>>> GetMarkersByType(Guid videoId, string type)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var markers = await _chapterService.GetMarkersByTypeAsync(videoId, userId, tenantId, type);
            return Ok(markers);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{markerId}")]
    public async Task<ActionResult<bool>> UpdateMarker(Guid videoId, Guid markerId, [FromBody] UpdateMarkerRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _chapterService.UpdateMarkerAsync(markerId, userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{markerId}")]
    public async Task<ActionResult<bool>> DeleteMarker(Guid videoId, Guid markerId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _chapterService.DeleteMarkerAsync(markerId, userId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class SetThumbnailRequest
{
    public string ThumbnailUrl { get; set; } = string.Empty;
}
