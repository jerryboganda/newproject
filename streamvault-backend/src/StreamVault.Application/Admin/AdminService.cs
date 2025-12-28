using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Admin.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Admin;

public class AdminService : IAdminService
{
    private readonly StreamVaultDbContext _dbContext;

    public AdminService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(Guid tenantId)
    {
        var totalUsers = await _dbContext.Users.CountAsync(u => u.TenantId == tenantId);
        var activeUsers = await _dbContext.Users.CountAsync(u => u.TenantId == tenantId && u.Status == UserStatus.Active);
        var totalVideos = await _dbContext.Videos.CountAsync(v => v.TenantId == tenantId);
        var publicVideos = await _dbContext.Videos.CountAsync(v => v.TenantId == tenantId && v.IsPublic);

        // Get recent user activity (mock data)
        var recentActivity = new List<UserActivityDto>();
        var recentUsers = await _dbContext.Users
            .Where(u => u.TenantId == tenantId)
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();

        foreach (var user in recentUsers)
        {
            recentActivity.Add(new UserActivityDto
            {
                UserId = user.Id,
                Email = user.Email,
                Activity = "Registered",
                Timestamp = user.CreatedAt
            });
        }

        // Get tenant info
        var tenant = await _dbContext.Tenants
            .Include(t => t.Subscriptions)
                .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalTenants = 1, // Current tenant only for tenant admin
            ActiveTenants = 1,
            TotalVideos = totalVideos,
            PublicVideos = publicVideos,
            TotalStorageUsed = totalVideos * 0.5, // Mock: 500MB per video average
            MonthlyRevenue = (double)(tenant?.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .Sum(s => s.Plan.PriceMonthly) ?? 0),
            RecentUserActivity = recentActivity,
            RecentTenants = new List<TenantDto>() // Empty for tenant admin
        };
    }

    public async Task<List<UserDto>> GetUsersAsync(Guid tenantId, int page = 1, int pageSize = 20)
    {
        var users = await _dbContext.Users
            .Where(u => u.TenantId == tenantId && u.Status == UserStatus.Active)
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.Status == UserStatus.Active,
            IsEmailVerified = u.IsEmailVerified,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt ?? u.CreatedAt,
            Role = "User" // TODO: Implement roles
        }).ToList();
    }

    public async Task<UserDto?> GetUserAsync(Guid userId, Guid tenantId)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.Status == UserStatus.Active,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt ?? user.CreatedAt,
            Role = "User"
        };
    }

    public async Task UpdateUserStatusAsync(Guid userId, UserStatusUpdateRequest request, Guid tenantId)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        user.Status = request.IsActive ? UserStatus.Active : UserStatus.Inactive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<TenantDto>> GetTenantsAsync(int page = 1, int pageSize = 20)
    {
        var tenants = await _dbContext.Tenants
            .Include(t => t.Users)
            .Include(t => t.Subscriptions)
                .ThenInclude(s => s.Plan)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get video counts separately
        var tenantIds = tenants.Select(t => t.Id).ToList();
        var videoCounts = await _dbContext.Videos
            .Where(v => tenantIds.Contains(v.TenantId))
            .GroupBy(v => v.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TenantId, x => x.Count);

        return tenants.Select(t => new TenantDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            Status = t.Status.ToString(),
            SubscriptionPlan = t.Subscriptions
                .FirstOrDefault(s => s.Status == SubscriptionStatus.Active)?.Plan.Name ?? "Free",
            UserCount = t.Users.Count,
            VideoCount = videoCounts.TryGetValue(t.Id, out var count) ? count : 0,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    public async Task<TenantDto?> GetTenantAsync(Guid tenantId)
    {
        var tenant = await _dbContext.Tenants
            .Include(t => t.Users)
            .Include(t => t.Subscriptions)
                .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
            return null;

        // Get video count
        var videoCount = await _dbContext.Videos
            .CountAsync(v => v.TenantId == tenantId);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            Status = tenant.Status.ToString(),
            SubscriptionPlan = tenant.Subscriptions
                .FirstOrDefault(s => s.Status == SubscriptionStatus.Active)?.Plan.Name ?? "Free",
            UserCount = tenant.Users.Count,
            VideoCount = videoCount,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task UpdateTenantStatusAsync(Guid tenantId, TenantStatusUpdateRequest request)
    {
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
            throw new Exception("Tenant not found");

        tenant.Status = request.Status == "Active" ? TenantStatus.Active : TenantStatus.Suspended;
        tenant.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<SystemStatsDto> GetSystemStatsAsync()
    {
        var totalUsers = await _dbContext.Users.CountAsync();
        var totalTenants = await _dbContext.Tenants.CountAsync();
        var totalVideos = await _dbContext.Videos.CountAsync();
        
        // Mock data for demonstration
        var userGrowth = new Dictionary<string, long>();
        var videoUploads = new Dictionary<string, long>();
        
        for (int i = 30; i >= 0; i--)
        {
            var date = DateTimeOffset.UtcNow.Date.AddDays(-i).ToString("yyyy-MM-dd");
            userGrowth[date] = Random.Shared.Next(0, 50);
            videoUploads[date] = Random.Shared.Next(0, 100);
        }

        return new SystemStatsDto
        {
            TotalUsers = totalUsers,
            TotalTenants = totalTenants,
            TotalVideos = totalVideos,
            TotalStorageUsed = totalVideos * 0.5, // Mock: 500MB per video average
            MonthlyRevenue = 5000, // Mock data
            UserGrowth = userGrowth,
            VideoUploads = videoUploads
        };
    }
}
