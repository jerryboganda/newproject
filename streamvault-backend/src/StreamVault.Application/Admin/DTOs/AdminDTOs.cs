using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.Admin.DTOs;

public class AdminDashboardDto
{
    public long TotalUsers { get; set; }
    public long ActiveUsers { get; set; }
    public long TotalTenants { get; set; }
    public long ActiveTenants { get; set; }
    public long TotalVideos { get; set; }
    public long PublicVideos { get; set; }
    public double TotalStorageUsed { get; set; } // in GB
    public double MonthlyRevenue { get; set; }
    public List<UserActivityDto> RecentUserActivity { get; set; } = new();
    public List<TenantDto> RecentTenants { get; set; } = new();
}

public class UserActivityDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Activity { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastLoginAt { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SubscriptionPlan { get; set; } = string.Empty;
    public long UserCount { get; set; }
    public long VideoCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class UserStatusUpdateRequest
{
    [Required]
    public bool IsActive { get; set; }
    
    public string? Role { get; set; }
}

public class TenantStatusUpdateRequest
{
    [Required]
    public string Status { get; set; }
}

public class SystemStatsDto
{
    public long TotalUsers { get; set; }
    public long TotalTenants { get; set; }
    public long TotalVideos { get; set; }
    public double TotalStorageUsed { get; set; }
    public double MonthlyRevenue { get; set; }
    public Dictionary<string, long> UserGrowth { get; set; } = new();
    public Dictionary<string, long> VideoUploads { get; set; } = new();
}
