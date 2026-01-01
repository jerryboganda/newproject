using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StreamVault.Api.Hubs;

[Authorize]
public class SupportHub : Hub
{
    public Task JoinTenant()
    {
        var tenantId = GetRequiredTenantId();
        return Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(tenantId));
    }

    public Task JoinTicket(Guid ticketId)
    {
        var tenantId = GetRequiredTenantId();
        return Groups.AddToGroupAsync(Context.ConnectionId, TicketGroup(tenantId, ticketId));
    }

    public Task LeaveTicket(Guid ticketId)
    {
        var tenantId = GetRequiredTenantId();
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, TicketGroup(tenantId, ticketId));
    }

    internal static string TenantGroup(Guid tenantId) => $"tenant:{tenantId}:support";

    internal static string TicketGroup(Guid tenantId, Guid ticketId) => $"tenant:{tenantId}:ticket:{ticketId}:support";

    private Guid GetRequiredTenantId()
    {
        var tenantClaim = Context.User?.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantClaim, out var tenantId))
            throw new HubException("tenant_id missing from token");

        return tenantId;
    }
}
