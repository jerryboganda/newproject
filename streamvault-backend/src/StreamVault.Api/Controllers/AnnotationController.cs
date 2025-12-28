using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Annotations;
using StreamVault.Application.Annotations.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AnnotationController : ControllerBase
{
    private readonly IAnnotationService _annotationService;

    public AnnotationController(IAnnotationService annotationService)
    {
        _annotationService = annotationService;
    }

    [HttpGet("video/{videoId}")]
    public async Task<ActionResult<List<AnnotationDto>>> GetAnnotations(Guid videoId, [FromQuery] int? startTime = null, [FromQuery] int? endTime = null)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var annotations = await _annotationService.GetAnnotationsAsync(videoId, tenantId, startTime, endTime);
            return Ok(annotations);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{annotationId}")]
    public async Task<ActionResult<AnnotationDto>> GetAnnotation(Guid annotationId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var annotation = await _annotationService.GetAnnotationAsync(annotationId, userId, tenantId);
            
            if (annotation == null)
                return NotFound();

            return Ok(annotation);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AnnotationDto>> CreateAnnotation([FromBody] CreateAnnotationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var annotation = await _annotationService.CreateAnnotationAsync(request, userId, tenantId);
            return Ok(annotation);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{annotationId}")]
    public async Task<ActionResult<AnnotationDto>> UpdateAnnotation(Guid annotationId, [FromBody] UpdateAnnotationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var annotation = await _annotationService.UpdateAnnotationAsync(annotationId, request, userId, tenantId);
            return Ok(annotation);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{annotationId}")]
    public async Task<IActionResult> DeleteAnnotation(Guid annotationId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _annotationService.DeleteAnnotationAsync(annotationId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{annotationId}/resolve")]
    public async Task<ActionResult<AnnotationDto>> ResolveAnnotation(Guid annotationId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var annotation = await _annotationService.ResolveAnnotationAsync(annotationId, userId, tenantId);
            return Ok(annotation);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{annotationId}/replies")]
    public async Task<ActionResult<AnnotationReplyDto>> AddReply(Guid annotationId, [FromBody] CreateReplyRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var reply = await _annotationService.AddReplyAsync(annotationId, request, userId, tenantId);
            return Ok(reply);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("replies/{replyId}")]
    public async Task<IActionResult> DeleteReply(Guid replyId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _annotationService.DeleteReplyAsync(replyId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
