using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Transcripts;
using StreamVault.Application.Transcripts.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TranscriptController : ControllerBase
{
    private readonly ITranscriptService _transcriptService;

    public TranscriptController(ITranscriptService transcriptService)
    {
        _transcriptService = transcriptService;
    }

    [HttpGet("video/{videoId}")]
    public async Task<ActionResult<List<TranscriptDto>>> GetTranscript(Guid videoId)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var transcript = await _transcriptService.GetTranscriptAsync(videoId, tenantId);
            return Ok(transcript);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{segmentId}/video/{videoId}")]
    public async Task<ActionResult<TranscriptDto>> GetTranscriptSegment(Guid segmentId, Guid videoId)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var segment = await _transcriptService.GetTranscriptSegmentAsync(segmentId, videoId, tenantId);
            
            if (segment == null)
                return NotFound();

            return Ok(segment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<List<TranscriptDto>>> CreateTranscript([FromBody] CreateTranscriptRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var transcript = await _transcriptService.CreateTranscriptAsync(request, userId, tenantId);
            return Ok(transcript);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{segmentId}")]
    public async Task<ActionResult<TranscriptDto>> UpdateTranscriptSegment(Guid segmentId, [FromBody] UpdateTranscriptRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var segment = await _transcriptService.UpdateTranscriptSegmentAsync(segmentId, request, userId, tenantId);
            return Ok(segment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{segmentId}")]
    public async Task<IActionResult> DeleteTranscriptSegment(Guid segmentId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _transcriptService.DeleteTranscriptSegmentAsync(segmentId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/search")]
    public async Task<ActionResult<List<TranscriptDto>>> SearchTranscript(Guid videoId, [FromQuery] string query)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var results = await _transcriptService.SearchTranscriptAsync(videoId, query, tenantId);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
