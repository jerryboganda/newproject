using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/kb")]
[Authorize]
public class KnowledgeBaseController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;

    public KnowledgeBaseController(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<KbCategoryDto>>> ListCategories(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var query = _dbContext.KnowledgeBaseCategories
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId);

        if (!isSuperAdmin)
            query = query.Where(c => c.IsActive);

        var items = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new KbCategoryDto(c.Id, c.Name, c.Slug, c.Description, c.SortOrder, c.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<KbCategoryDto>> CreateCategory([FromBody] CreateKbCategoryRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required" });

        var slug = Slugify(request.Slug ?? request.Name);

        var exists = await _dbContext.KnowledgeBaseCategories
            .AsNoTracking()
            .AnyAsync(c => c.TenantId == tenantId && c.Slug == slug, cancellationToken);

        if (exists)
            return BadRequest(new { error = "Category slug already exists" });

        var entity = new KnowledgeBaseCategory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder ?? 0,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.KnowledgeBaseCategories.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new KbCategoryDto(entity.Id, entity.Name, entity.Slug, entity.Description, entity.SortOrder, entity.IsActive));
    }

    [HttpGet("articles")]
    public async Task<ActionResult<IReadOnlyList<KbArticleListItemDto>>> ListArticles(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var query = _dbContext.KnowledgeBaseArticles
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .Include(a => a.Category)
            .AsQueryable();

        if (!isSuperAdmin)
            query = query.Where(a => a.IsPublished);

        if (categoryId.HasValue)
            query = query.Where(a => a.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(a => a.Title.Contains(term) || (a.Summary != null && a.Summary.Contains(term)));
        }

        var items = await query
            .OrderByDescending(a => a.UpdatedAt)
            .Take(200)
            .Select(a => new KbArticleListItemDto(
                a.Id,
                a.Title,
                a.Slug,
                a.Summary,
                a.CategoryId,
                a.Category.Name,
                a.Tags,
                a.IsPublished,
                a.Views,
                a.HelpfulVotes,
                a.CreatedAt,
                a.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("articles/{slug}")]
    public async Task<ActionResult<KbArticleDetailsDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();

        var article = await _dbContext.KnowledgeBaseArticles
            .Where(a => a.TenantId == tenantId && a.Slug == slug)
            .Include(a => a.Category)
            .FirstOrDefaultAsync(cancellationToken);

        if (article == null)
            return NotFound(new { error = "Article not found" });

        // Increment views
        article.Views += 1;
        article.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new KbArticleDetailsDto(
            article.Id,
            article.Title,
            article.Slug,
            article.Content,
            article.Summary,
            article.CategoryId,
            article.Category.Name,
            article.Tags,
            article.IsPublished,
            article.PublishedAt,
            article.Views,
            article.HelpfulVotes,
            article.CreatedAt,
            article.UpdatedAt));
    }

    [HttpPost("articles")]
    public async Task<ActionResult<KbArticleDetailsDto>> CreateArticle([FromBody] CreateKbArticleRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Title and content are required" });

        var category = await _dbContext.KnowledgeBaseCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.CategoryId, cancellationToken);

        if (category == null)
            return BadRequest(new { error = "Invalid category" });

        var slug = Slugify(request.Slug ?? request.Title);

        var exists = await _dbContext.KnowledgeBaseArticles
            .AsNoTracking()
            .AnyAsync(a => a.TenantId == tenantId && a.Slug == slug, cancellationToken);

        if (exists)
            return BadRequest(new { error = "Article slug already exists" });

        var entity = new KnowledgeBaseArticle
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = request.Title.Trim(),
            Slug = slug,
            Content = request.Content,
            Summary = request.Summary?.Trim(),
            CategoryId = category.Id,
            Tags = request.Tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).Distinct().ToList() ?? new(),
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished ? now : null,
            Views = 0,
            HelpfulVotes = 0,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = userId,
            UpdatedByUserId = userId
        };

        _dbContext.KnowledgeBaseArticles.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetBySlug(entity.Slug, cancellationToken);
    }

    [HttpPut("articles/{id:guid}")]
    public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] UpdateKbArticleRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var userId = RequireUserId();

        var entity = await _dbContext.KnowledgeBaseArticles
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id, cancellationToken);

        if (entity == null)
            return NotFound(new { error = "Article not found" });

        if (request.Title != null) entity.Title = request.Title.Trim();
        if (request.Content != null) entity.Content = request.Content;
        if (request.Summary != null) entity.Summary = request.Summary.Trim();
        if (request.Tags != null) entity.Tags = request.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).Distinct().ToList();
        if (request.CategoryId.HasValue) entity.CategoryId = request.CategoryId.Value;

        if (request.IsPublished.HasValue)
        {
            entity.IsPublished = request.IsPublished.Value;
            entity.PublishedAt = entity.IsPublished ? (entity.PublishedAt ?? DateTime.UtcNow) : null;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = userId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { success = true });
    }

    private Guid RequireTenantId()
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        if (tenantClaim == null || !Guid.TryParse(tenantClaim, out var tenantId))
            throw new UnauthorizedAccessException("Missing tenant_id claim");
        return tenantId;
    }

    private Guid RequireUserId()
    {
        var userClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userClaim == null || !Guid.TryParse(userClaim, out var userId))
            throw new UnauthorizedAccessException("Missing user id claim");
        return userId;
    }

    private static string Slugify(string value)
    {
        var raw = value.Trim().ToLowerInvariant();
        var chars = raw
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var collapsed = new string(chars);
        while (collapsed.Contains("--")) collapsed = collapsed.Replace("--", "-");
        return collapsed.Trim('-');
    }
}

public record KbCategoryDto(Guid Id, string Name, string Slug, string? Description, int SortOrder, bool IsActive);
public record CreateKbCategoryRequest(string Name, string? Slug, string? Description, int? SortOrder);

public record KbArticleListItemDto(
    Guid Id,
    string Title,
    string Slug,
    string? Summary,
    Guid CategoryId,
    string CategoryName,
    IReadOnlyList<string> Tags,
    bool IsPublished,
    int Views,
    int HelpfulVotes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record KbArticleDetailsDto(
    Guid Id,
    string Title,
    string Slug,
    string Content,
    string? Summary,
    Guid CategoryId,
    string CategoryName,
    IReadOnlyList<string> Tags,
    bool IsPublished,
    DateTime? PublishedAt,
    int Views,
    int HelpfulVotes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateKbArticleRequest(
    string Title,
    string? Slug,
    Guid CategoryId,
    string Content,
    string? Summary,
    IReadOnlyList<string>? Tags,
    bool IsPublished);

public record UpdateKbArticleRequest(
    string? Title,
    Guid? CategoryId,
    string? Content,
    string? Summary,
    IReadOnlyList<string>? Tags,
    bool? IsPublished);
