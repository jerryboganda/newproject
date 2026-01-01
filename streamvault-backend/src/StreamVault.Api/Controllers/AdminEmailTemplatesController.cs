using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Api.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/admin/email-templates")]
[Authorize(Roles = "SuperAdmin")]
public class AdminEmailTemplatesController : ControllerBase
{
    private static readonly Regex VariableRegex = new(@"\{\{\s*([a-zA-Z0-9_\.\-]+)\s*\}\}", RegexOptions.Compiled);

    private readonly StreamVaultDbContext _db;
    private readonly AuditLogger _audit;

    public AdminEmailTemplatesController(StreamVaultDbContext db, AuditLogger audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmailTemplateDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.EmailTemplates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(t => t.Category == category.Trim());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t => EF.Functions.ILike(t.Name, $"%{term}%") || EF.Functions.ILike(t.Subject, $"%{term}%"));
        }

        var templates = await query
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(templates.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmailTemplateDto>> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var template = await _db.EmailTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (template == null) return NotFound();
        return Ok(Map(template));
    }

    [HttpPost]
    public async Task<ActionResult<EmailTemplateDto>> Create([FromBody] UpsertEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required" });
        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest(new { error = "Subject is required" });
        if (string.IsNullOrWhiteSpace(request.Category))
            return BadRequest(new { error = "Category is required" });

        var userId = GetRequiredUserId();
        var now = DateTime.UtcNow;

        var html = request.HtmlContent ?? string.Empty;
        var text = request.TextContent ?? string.Empty;

        var variables = ComputeVariables(request.Variables, html, text);

        var template = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Subject = request.Subject.Trim(),
            HtmlContent = html,
            TextContent = text,
            Category = request.Category.Trim(),
            Variables = variables,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = userId
        };

        _db.EmailTemplates.Add(template);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "admin.email_template.create",
            entityType: nameof(EmailTemplate),
            entityId: template.Id,
            oldValues: null,
            newValues: new Dictionary<string, object>
            {
                ["name"] = template.Name,
                ["category"] = template.Category,
                ["isActive"] = template.IsActive
            },
            cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = template.Id }, Map(template));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmailTemplateDto>> Update([FromRoute] Guid id, [FromBody] UpsertEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = await _db.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (template == null) return NotFound();

        var old = new Dictionary<string, object>
        {
            ["name"] = template.Name,
            ["subject"] = template.Subject,
            ["category"] = template.Category,
            ["isActive"] = template.IsActive
        };

        if (!string.IsNullOrWhiteSpace(request.Name)) template.Name = request.Name.Trim();
        if (!string.IsNullOrWhiteSpace(request.Subject)) template.Subject = request.Subject.Trim();
        if (request.HtmlContent != null) template.HtmlContent = request.HtmlContent;
        if (request.TextContent != null) template.TextContent = request.TextContent;
        if (!string.IsNullOrWhiteSpace(request.Category)) template.Category = request.Category.Trim();
        template.IsActive = request.IsActive;

        template.Variables = ComputeVariables(request.Variables, template.HtmlContent, template.TextContent);
        template.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "admin.email_template.update",
            entityType: nameof(EmailTemplate),
            entityId: template.Id,
            oldValues: old,
            newValues: new Dictionary<string, object>
            {
                ["name"] = template.Name,
                ["subject"] = template.Subject,
                ["category"] = template.Category,
                ["isActive"] = template.IsActive
            },
            cancellationToken: cancellationToken);

        return Ok(Map(template));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var template = await _db.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (template == null) return NotFound();

        _db.EmailTemplates.Remove(template);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "admin.email_template.delete",
            entityType: nameof(EmailTemplate),
            entityId: id,
            oldValues: new Dictionary<string, object> { ["name"] = template.Name, ["category"] = template.Category },
            newValues: null,
            cancellationToken: cancellationToken);

        return NoContent();
    }

    private static EmailTemplateDto Map(EmailTemplate t) => new(
        t.Id,
        t.Name,
        t.Subject,
        t.HtmlContent,
        t.TextContent,
        t.Category,
        t.Variables,
        t.IsActive,
        t.CreatedAt,
        t.UpdatedAt,
        t.CreatedByUserId);

    private static List<string> ComputeVariables(List<string>? requestVariables, string html, string text)
    {
        var vars = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (requestVariables != null)
        {
            foreach (var v in requestVariables)
            {
                var trimmed = v?.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed)) vars.Add(trimmed);
            }
        }

        foreach (Match m in VariableRegex.Matches(html ?? string.Empty))
            if (m.Groups.Count > 1) vars.Add(m.Groups[1].Value);

        foreach (Match m in VariableRegex.Matches(text ?? string.Empty))
            if (m.Groups.Count > 1) vars.Add(m.Groups[1].Value);

        return vars.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private Guid GetRequiredUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(id, out var userId))
            throw new InvalidOperationException("User id missing from token");
        return userId;
    }

    public sealed record EmailTemplateDto(
        Guid Id,
        string Name,
        string Subject,
        string HtmlContent,
        string TextContent,
        string Category,
        IReadOnlyList<string> Variables,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        Guid CreatedByUserId);

    public sealed class UpsertEmailTemplateRequest
    {
        public string? Name { get; set; }
        public string? Subject { get; set; }
        public string? HtmlContent { get; set; }
        public string? TextContent { get; set; }
        public string? Category { get; set; }
        public List<string>? Variables { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
