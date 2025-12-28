using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Chapters;
using StreamVault.Application.Chapters.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ChapterController : ControllerBase
{
    private readonly IChapterService _chapterService;

    public ChapterController(IChapterService chapterService)
    {
        _chapterService = chapterService;
    }

    [HttpGet("video/{videoId}")]
    public async Task<ActionResult<List<ChapterDto>>> GetChapters(Guid videoId)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var chapters = await _chapterService.GetChaptersAsync(videoId, tenantId);
            return Ok(chapters);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{chapterId}/video/{videoId}")]
    public async Task<ActionResult<ChapterDto>> GetChapter(Guid chapterId, Guid videoId)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var chapter = await _chapterService.GetChapterAsync(chapterId, videoId, tenantId);
            
            if (chapter == null)
                return NotFound();

            return Ok(chapter);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ChapterDto>> CreateChapter([FromBody] CreateChapterRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var chapter = await _chapterService.CreateChapterAsync(request, userId, tenantId);
            return Ok(chapter);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{chapterId}")]
    public async Task<ActionResult<ChapterDto>> UpdateChapter(Guid chapterId, [FromBody] UpdateChapterRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var chapter = await _chapterService.UpdateChapterAsync(chapterId, request, userId, tenantId);
            return Ok(chapter);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{chapterId}")]
    public async Task<IActionResult> DeleteChapter(Guid chapterId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _chapterService.DeleteChapterAsync(chapterId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/reorder")]
    public async Task<ActionResult<List<ChapterDto>>> ReorderChapters(Guid videoId, [FromBody] List<Guid> chapterIds)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var chapters = await _chapterService.ReorderChaptersAsync(videoId, chapterIds, userId, tenantId);
            return Ok(chapters);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
