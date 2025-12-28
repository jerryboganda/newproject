using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StreamVault.Application.Notifications.DTOs;

namespace StreamVault.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private static readonly Dictionary<Guid, List<string>> UserConnections = new();
    private static readonly Dictionary<string, Guid> ConnectionUsers = new();

    public async Task JoinNotificationGroup()
    {
        var userId = Guid.Parse(Context.UserIdentifier ?? "");
        var connectionId = Context.ConnectionId;

        lock (UserConnections)
        {
            if (!UserConnections.ContainsKey(userId))
            {
                UserConnections[userId] = new List<string>();
            }
            UserConnections[userId].Add(connectionId);
            ConnectionUsers[connectionId] = userId;
        }

        await Groups.AddToGroupAsync(connectionId, $"User_{userId}");
        
        // Send unread count to the newly connected client
        // This would typically be injected and called from a service
        // await _notificationService.SendUnreadCountAsync(userId, connectionId);
    }

    public async Task LeaveNotificationGroup()
    {
        var userId = Guid.Parse(Context.UserIdentifier ?? "");
        var connectionId = Context.ConnectionId;

        lock (UserConnections)
        {
            if (UserConnections.ContainsKey(userId))
            {
                UserConnections[userId].Remove(connectionId);
                if (UserConnections[userId].Count == 0)
                {
                    UserConnections.Remove(userId);
                }
            }
            ConnectionUsers.Remove(connectionId);
        }

        await Groups.RemoveFromGroupAsync(connectionId, $"User_{userId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        lock (UserConnections)
        {
            if (ConnectionUsers.TryGetValue(connectionId, out var userId))
            {
                if (UserConnections.ContainsKey(userId))
                {
                    UserConnections[userId].Remove(connectionId);
                    if (UserConnections[userId].Count == 0)
                    {
                        UserConnections.Remove(userId);
                    }
                }
                ConnectionUsers.Remove(connectionId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Method to get all connections for a user
    public static List<string> GetUserConnections(Guid userId)
    {
        lock (UserConnections)
        {
            return UserConnections.TryGetValue(userId, out var connections) ? connections : new List<string>();
        }
    }

    // Method to check if a user is online
    public static bool IsUserOnline(Guid userId)
    {
        lock (UserConnections)
        {
            return UserConnections.ContainsKey(userId) && UserConnections[userId].Any();
        }
    }

    // Method to get all online users
    public static List<Guid> GetOnlineUsers()
    {
        lock (UserConnections)
        {
            return UserConnections.Keys.ToList();
        }
    }

    // Send real-time notification to specific user
    public async Task SendNotificationToUser(Guid userId, NotificationDto notification)
    {
        var connections = GetUserConnections(userId);
        if (connections.Any())
        {
            await Clients.Clients(connections).SendAsync("ReceiveNotification", notification);
        }
    }

    // Send real-time event to specific user
    public async Task SendEventToUser(Guid userId, string eventName, object data)
    {
        var connections = GetUserConnections(userId);
        if (connections.Any())
        {
            await Clients.Clients(connections).SendAsync(eventName, data);
        }
    }

    // Broadcast to all online users
    public async Task BroadcastToAll(string eventName, object data)
    {
        await Clients.All.SendAsync(eventName, data);
    }

    // Send to users in a specific role
    public async Task SendToRole(string roleName, string eventName, object data)
    {
        await Clients.Group(roleName).SendAsync(eventName, data);
    }

    // Join a custom group
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    // Leave a custom group
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
