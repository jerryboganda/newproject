using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using StreamVault.Api.Hubs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Jobs;

public class SupportSlaEscalationJob
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<SupportSlaEscalationJob> _logger;
    private readonly IHubContext<SupportHub> _supportHub;

    public SupportSlaEscalationJob(StreamVaultDbContext dbContext, ILogger<SupportSlaEscalationJob> logger, IHubContext<SupportHub> supportHub)
    {
        _dbContext = dbContext;
        _logger = logger;
        _supportHub = supportHub;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var activeRules = await _dbContext.SupportEscalationRules
            .AsNoTracking()
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);

        if (activeRules.Count == 0)
            return;

        // Only consider tickets that are still in-flight
        var inFlightStatuses = new[]
        {
            TicketStatus.Open,
            TicketStatus.InProgress,
            TicketStatus.WaitingForCustomer,
            TicketStatus.WaitingForSupport,
            TicketStatus.Reopened
        };

        var tickets = await _dbContext.SupportTickets
            .Where(t => inFlightStatuses.Contains(t.Status))
            .Where(t => t.FirstResponseDueAt != null || t.ResolutionDueAt != null)
            .ToListAsync(cancellationToken);

        if (tickets.Count == 0)
            return;

        var ticketIds = tickets.Select(t => t.Id).ToArray();
        var recentActivities = await _dbContext.SupportTicketActivities
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .Where(a => a.Type == SupportTicketActivityType.SlaBreached || a.Type == SupportTicketActivityType.Escalated)
            .Select(a => new { a.TicketId, a.Message })
            .ToListAsync(cancellationToken);

        var activitySet = new HashSet<(Guid TicketId, string Message)>(recentActivities.Select(a => (a.TicketId, a.Message)));

        var changed = 0;
        var changedTickets = new HashSet<(Guid TenantId, Guid TicketId)>();

        foreach (var ticket in tickets)
        {
            var rulesForTenant = activeRules.Where(r => r.TenantId == ticket.TenantId).ToList();
            if (rulesForTenant.Count == 0)
                continue;

            foreach (var rule in rulesForTenant)
            {
                if (rule.Trigger == SupportEscalationTrigger.FirstResponseOverdue)
                {
                    if (ticket.FirstResponseAt != null || ticket.FirstResponseDueAt == null)
                        continue;

                    var overdueMinutes = (now - ticket.FirstResponseDueAt.Value).TotalMinutes;
                    if (overdueMinutes < rule.ThresholdMinutes)
                        continue;

                    var message = $"SLA breach: first response overdue (rule: {rule.Name})";
                    if (activitySet.Contains((ticket.Id, message)))
                        continue;

                    ApplyEscalation(ticket, rule, now, message);
                    activitySet.Add((ticket.Id, message));
                    changed++;
                    changedTickets.Add((ticket.TenantId, ticket.Id));
                }

                if (rule.Trigger == SupportEscalationTrigger.ResolutionOverdue)
                {
                    if (ticket.ResolutionDueAt == null)
                        continue;

                    // If already resolved/closed/escalated, ignore
                    if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed)
                        continue;

                    var overdueMinutes = (now - ticket.ResolutionDueAt.Value).TotalMinutes;
                    if (overdueMinutes < rule.ThresholdMinutes)
                        continue;

                    var message = $"SLA breach: resolution overdue (rule: {rule.Name})";
                    if (activitySet.Contains((ticket.Id, message)))
                        continue;

                    ApplyEscalation(ticket, rule, now, message);
                    activitySet.Add((ticket.Id, message));
                    changed++;
                    changedTickets.Add((ticket.TenantId, ticket.Id));
                }
            }
        }

        if (changed == 0)
            return;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Support SLA escalation job applied {Count} updates", changed);

        foreach (var (tenantId, ticketId) in changedTickets)
        {
            await _supportHub.Clients.Group(SupportHub.TenantGroup(tenantId))
                .SendAsync("ticketChanged", new { ticketId, kind = "sla" }, cancellationToken);
            await _supportHub.Clients.Group(SupportHub.TicketGroup(tenantId, ticketId))
                .SendAsync("ticketChanged", new { ticketId, kind = "sla" }, cancellationToken);
        }
    }

    private void ApplyEscalation(SupportTicket ticket, SupportEscalationRule rule, DateTime now, string message)
    {
        if (rule.SetStatusToEscalated)
        {
            ticket.Status = TicketStatus.Escalated;
            ticket.EscalatedAt ??= now;
        }

        if (ticket.Priority < rule.EscalateToPriority)
            ticket.Priority = rule.EscalateToPriority;

        ticket.UpdatedAt = now;

        _dbContext.SupportTicketActivities.Add(new SupportTicketActivity
        {
            Id = Guid.NewGuid(),
            TenantId = ticket.TenantId,
            TicketId = ticket.Id,
            Type = SupportTicketActivityType.SlaBreached,
            Message = message,
            CreatedAt = now
        });

        if (rule.SetStatusToEscalated)
        {
            _dbContext.SupportTicketActivities.Add(new SupportTicketActivity
            {
                Id = Guid.NewGuid(),
                TenantId = ticket.TenantId,
                TicketId = ticket.Id,
                Type = SupportTicketActivityType.Escalated,
                Message = $"Ticket escalated (rule: {rule.Name})",
                CreatedAt = now
            });
        }
    }
}
