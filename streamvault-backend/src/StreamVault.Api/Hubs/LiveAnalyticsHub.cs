using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StreamVault.Api.Hubs;

[Authorize]
public class LiveAnalyticsHub : Hub
{
    public Task JoinTenant()
    {
        var tenantId = GetRequiredTenantId();
        return Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(tenantId));
    }

    public Task JoinVideo(Guid videoId)
    {
        var tenantId = GetRequiredTenantId();
        return Groups.AddToGroupAsync(Context.ConnectionId, VideoGroup(tenantId, videoId));
    }

    public Task LeaveVideo(Guid videoId)
    {
        var tenantId = GetRequiredTenantId();
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, VideoGroup(tenantId, videoId));
    }

    internal static string TenantGroup(Guid tenantId) => $"tenant:{tenantId}:analytics";

    internal static string VideoGroup(Guid tenantId, Guid videoId) => $"tenant:{tenantId}:video:{videoId}:analytics";

    private Guid GetRequiredTenantId()
    {
        var tenantClaim = Context.User?.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantClaim, out var tenantId))
            throw new HubException("tenant_id missing from token");

        return tenantId;
    }
}
