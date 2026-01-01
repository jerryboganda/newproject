using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using StreamVault.Api.Hubs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/support/tickets")]
[Authorize]
public class SupportTicketsController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<SupportTicketsController> _logger;
    private readonly IHubContext<SupportHub> _supportHub;

    public SupportTicketsController(StreamVaultDbContext dbContext, ILogger<SupportTicketsController> logger, IHubContext<SupportHub> supportHub)
    {
        _dbContext = dbContext;
        _logger = logger;
        _supportHub = supportHub;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupportTicketListItemDto>>> List(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();

        var query = _dbContext.SupportTickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Include(t => t.User)
            .Include(t => t.AssignedTo)
            .Include(t => t.Department)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(t => t.Subject.Contains(term) || t.TicketNumber.Contains(term));
        }

        var items = await query
            .OrderByDescending(t => t.UpdatedAt)
            .Take(200)
            .Select(t => new SupportTicketListItemDto(
                t.Id,
                t.TicketNumber,
                t.Subject,
                t.Description,
                t.DepartmentId,
                t.Department.Name,
                t.Priority.ToString(),
                t.Status.ToString(),
                t.UserId,
                t.User.FirstName + " " + t.User.LastName,
                t.User.Email,
                t.AssignedToId,
                t.AssignedTo == null ? null : (t.AssignedTo.FirstName + " " + t.AssignedTo.LastName),
                t.CreatedAt,
                t.UpdatedAt,
                t.FirstResponseAt,
                t.FirstResponseDueAt,
                t.ResolutionDueAt))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<ActionResult<SupportTicketDetailsDto>> Get(Guid ticketId, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var ticket = await _dbContext.SupportTickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.Id == ticketId)
            .Include(t => t.User)
            .Include(t => t.AssignedTo)
            .Include(t => t.Department)
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket == null)
            return NotFound(new { error = "Ticket not found" });

        var replies = await _dbContext.SupportTicketReplies
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.TicketId == ticketId)
            .Where(r => isSuperAdmin || !r.IsInternal)
            .Include(r => r.User)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new SupportTicketReplyDto(
                r.Id,
                r.Content,
                r.IsInternal,
                r.UserId,
                r.User.FirstName + " " + r.User.LastName,
                r.User.Email,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        var activities = await _dbContext.SupportTicketActivities
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.TicketId == ticketId)
            .Where(a => isSuperAdmin || !(a.Type == SupportTicketActivityType.ReplyAdded && a.Message.Contains("Internal note")))
            .OrderBy(a => a.CreatedAt)
            .Select(a => new SupportTicketActivityDto(
                a.Id,
                a.Type.ToString(),
                a.Message,
                a.MetadataJson,
                a.CreatedByUserId,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(new SupportTicketDetailsDto(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Subject,
            ticket.Description,
            ticket.DepartmentId,
            ticket.Department.Name,
            ticket.Priority.ToString(),
            ticket.Status.ToString(),
            ticket.UserId,
            ticket.User.FirstName + " " + ticket.User.LastName,
            ticket.User.Email,
            ticket.AssignedToId,
            ticket.AssignedTo == null ? null : (ticket.AssignedTo.FirstName + " " + ticket.AssignedTo.LastName),
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.FirstResponseAt,
            ticket.FirstResponseDueAt,
            ticket.ResolutionDueAt,
            replies,
            activities));
    }

    [HttpPost]
    public async Task<ActionResult<SupportTicketDetailsDto>> Create([FromBody] CreateSupportTicketRequest request, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();

        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "Subject and description are required" });

        var department = await _dbContext.SupportDepartments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == request.DepartmentId && d.IsActive, cancellationToken);

        if (department == null)
            return BadRequest(new { error = "Invalid department" });

        SupportSlaPolicy? sla = null;
        if (department.DefaultSlaPolicyId.HasValue)
        {
            sla = await _dbContext.SupportSlaPolicies
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == department.DefaultSlaPolicyId.Value && s.IsActive, cancellationToken);
        }

        var now = DateTime.UtcNow;

        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            TicketNumber = await GenerateTicketNumberAsync(tenantId, cancellationToken),
            DepartmentId = department.Id,
            SlaPolicyId = sla?.Id,
            Subject = request.Subject.Trim(),
            Description = request.Description.Trim(),
            Category = TicketCategory.General,
            Priority = request.Priority ?? TicketPriority.Normal,
            Status = TicketStatus.Open,
            FirstResponseDueAt = sla == null ? null : now.AddMinutes(sla.FirstResponseMinutes),
            ResolutionDueAt = sla == null ? null : now.AddMinutes(sla.ResolutionMinutes),
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.SupportTickets.Add(ticket);
        _dbContext.SupportTicketActivities.Add(new SupportTicketActivity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TicketId = ticket.Id,
            Type = SupportTicketActivityType.Created,
            Message = "Ticket created",
            CreatedByUserId = userId,
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _supportHub.Clients.Group(SupportHub.TenantGroup(tenantId))
            .SendAsync("ticketChanged", new { ticketId = ticket.Id, kind = "created" }, cancellationToken);

        return await Get(ticket.Id, cancellationToken);
    }

    [HttpPost("{ticketId:guid}/messages")]
    public async Task<IActionResult> AddMessage(Guid ticketId, [FromBody] AddSupportTicketMessageRequest request, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Content is required" });

        var ticket = await _dbContext.SupportTickets
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == ticketId, cancellationToken);

        if (ticket == null)
            return NotFound(new { error = "Ticket not found" });

        var now = DateTime.UtcNow;
        var isInternal = isSuperAdmin && request.IsInternal;

        _dbContext.SupportTicketReplies.Add(new SupportTicketReply
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TicketId = ticketId,
            UserId = userId,
            Content = request.Content.Trim(),
            IsInternal = isInternal,
            CreatedAt = now
        });

        _dbContext.SupportTicketActivities.Add(new SupportTicketActivity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TicketId = ticketId,
            Type = SupportTicketActivityType.ReplyAdded,
            Message = isInternal ? "Internal note added" : "Message added",
            CreatedByUserId = userId,
            CreatedAt = now
        });

        // Track first response time (support-side)
        if (isSuperAdmin && ticket.FirstResponseAt == null && !isInternal)
        {
            ticket.FirstResponseAt = now;
        }

        // Status transitions
        if (isSuperAdmin)
        {
            if (!isInternal && ticket.Status is TicketStatus.Open or TicketStatus.WaitingForSupport)
                ticket.Status = TicketStatus.WaitingForCustomer;
        }
        else
        {
            if (ticket.Status is TicketStatus.Closed or TicketStatus.Resolved)
                ticket.Status = TicketStatus.Reopened;
            else
                ticket.Status = TicketStatus.WaitingForSupport;
        }

        ticket.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _supportHub.Clients.Group(SupportHub.TenantGroup(tenantId))
            .SendAsync("ticketChanged", new { ticketId, kind = "message" }, cancellationToken);
        await _supportHub.Clients.Group(SupportHub.TicketGroup(tenantId, ticketId))
            .SendAsync("ticketChanged", new { ticketId, kind = "message" }, cancellationToken);
        return Ok(new { success = true });
    }

    [HttpPut("{ticketId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid ticketId, [FromBody] UpdateSupportTicketStatusRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var userId = RequireUserId();

        var ticket = await _dbContext.SupportTickets
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == ticketId, cancellationToken);

        if (ticket == null)
            return NotFound(new { error = "Ticket not found" });

        var now = DateTime.UtcNow;
        var previous = ticket.Status;
        ticket.Status = request.Status;

        if (request.Status is TicketStatus.Closed or TicketStatus.Resolved)
            ticket.ClosedAt = now;

        ticket.UpdatedAt = now;

        _dbContext.SupportTicketActivities.Add(new SupportTicketActivity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TicketId = ticketId,
            Type = SupportTicketActivityType.StatusChanged,
            Message = $"Status changed from {previous} to {ticket.Status}",
            CreatedByUserId = userId,
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _supportHub.Clients.Group(SupportHub.TenantGroup(tenantId))
            .SendAsync("ticketChanged", new { ticketId, kind = "status" }, cancellationToken);
        await _supportHub.Clients.Group(SupportHub.TicketGroup(tenantId, ticketId))
            .SendAsync("ticketChanged", new { ticketId, kind = "status" }, cancellationToken);
        return Ok(new { success = true });
    }

    [HttpPut("{ticketId:guid}/assign")]
    public async Task<IActionResult> Assign(Guid ticketId, [FromBody] AssignSupportTicketRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("SuperAdmin"))
            return Forbid();

        var tenantId = RequireTenantId();
        var userId = RequireUserId();

        var ticket = await _dbContext.SupportTickets
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == ticketId, cancellationToken);

        if (ticket == null)
            return NotFound(new { error = "Ticket not found" });

        var now = DateTime.UtcNow;

        Guid? newAssigneeId = request.AssignedToUserId;
        string? newAssigneeName = null;

        if (newAssigneeId.HasValue)
        {
            var assignee = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == newAssigneeId.Value && u.Status == UserStatus.Active, cancellationToken);

            if (assignee == null)
                return BadRequest(new { error = "Invalid assignee" });

            newAssigneeName = assignee.FirstName + " " + assignee.LastName;
        }

        ticket.AssignedToId = newAssigneeId;
        ticket.UpdatedAt = now;

        var message = newAssigneeId.HasValue
            ? $"Assigned to {newAssigneeName}"
            : "Unassigned";

        _dbContext.SupportTicketActivities.Add(new SupportTicketActivity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TicketId = ticketId,
            Type = SupportTicketActivityType.Assigned,
            Message = message,
            MetadataJson = JsonSerializer.Serialize(new { assignedToUserId = newAssigneeId }),
            CreatedByUserId = userId,
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _supportHub.Clients.Group(SupportHub.TenantGroup(tenantId))
            .SendAsync("ticketChanged", new { ticketId, kind = "assigned" }, cancellationToken);
        await _supportHub.Clients.Group(SupportHub.TicketGroup(tenantId, ticketId))
            .SendAsync("ticketChanged", new { ticketId, kind = "assigned" }, cancellationToken);

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
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
        if (userClaim == null || !Guid.TryParse(userClaim, out var userId))
            throw new UnauthorizedAccessException("Missing user id claim");
        return userId;
    }

    private async Task<string> GenerateTicketNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        // Not strictly sequential; designed to be unique + human-friendly.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = Random.Shared.Next(1000, 9999);
            var candidate = $"SV-{DateTime.UtcNow:yyyyMMdd}-{suffix}";

            var exists = await _dbContext.SupportTickets
                .AsNoTracking()
                .AnyAsync(t => t.TenantId == tenantId && t.TicketNumber == candidate, cancellationToken);

            if (!exists)
                return candidate;
        }

        return $"SV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
    }
}

public record SupportTicketListItemDto(
    Guid Id,
    string TicketNumber,
    string Subject,
    string Description,
    Guid DepartmentId,
    string DepartmentName,
    string Priority,
    string Status,
    Guid UserId,
    string UserName,
    string UserEmail,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? FirstResponseAt,
    DateTime? FirstResponseDueAt,
    DateTime? ResolutionDueAt);

public record SupportTicketReplyDto(
    Guid Id,
    string Content,
    bool IsInternal,
    Guid UserId,
    string UserName,
    string UserEmail,
    DateTime CreatedAt);

public record SupportTicketActivityDto(
    Guid Id,
    string Type,
    string Message,
    string? MetadataJson,
    Guid? CreatedByUserId,
    DateTime CreatedAt);

public record SupportTicketDetailsDto(
    Guid Id,
    string TicketNumber,
    string Subject,
    string Description,
    Guid DepartmentId,
    string DepartmentName,
    string Priority,
    string Status,
    Guid UserId,
    string UserName,
    string UserEmail,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? FirstResponseAt,
    DateTime? FirstResponseDueAt,
    DateTime? ResolutionDueAt,
    IReadOnlyList<SupportTicketReplyDto> Replies,
    IReadOnlyList<SupportTicketActivityDto> Activities);

public record CreateSupportTicketRequest(
    string Subject,
    string Description,
    Guid DepartmentId,
    TicketPriority? Priority);

public record AddSupportTicketMessageRequest(
    string Content,
    bool IsInternal);

public record UpdateSupportTicketStatusRequest(
    TicketStatus Status);

public record AssignSupportTicketRequest(
    Guid? AssignedToUserId);
