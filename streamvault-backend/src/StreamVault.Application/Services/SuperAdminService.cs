using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Services
{
    public interface ISuperAdminService
    {
        Task<IEnumerable<Tenant>> GetAllTenantsAsync(int page = 1, int pageSize = 20, string? search = null);
        Task<Tenant?> GetTenantAsync(Guid tenantId);
        Task<Tenant> CreateTenantAsync(CreateTenantRequest request);
        Task UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request);
        Task SuspendTenantAsync(Guid tenantId, string reason);
        Task UnsuspendTenantAsync(Guid tenantId);
        Task DeleteTenantAsync(Guid tenantId);
        Task<PlatformAnalytics> GetPlatformAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<TenantAnalytics> GetTenantAnalyticsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
        Task<UserImpersonationToken> CreateUserImpersonationTokenAsync(Guid adminUserId, Guid targetUserId);
        Task ValidateImpersonationTokenAsync(string token);
        Task<SystemSettings> GetSystemSettingsAsync();
        Task UpdateSystemSettingsAsync(SystemSettings settings);
        Task<IEnumerable<SystemNotification>> GetSystemNotificationsAsync();
        Task CreateSystemNotificationAsync(CreateSystemNotificationRequest request);
        Task<SystemMetrics> GetSystemMetricsAsync();
    }

    public class SuperAdminService : ISuperAdminService
    {
        private readonly StreamVaultDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;

        public SuperAdminService(StreamVaultDbContext context, IEmailService emailService, IJwtService jwtService)
        {
            _context = context;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        public async Task<IEnumerable<Tenant>> GetAllTenantsAsync(int page = 1, int pageSize = 20, string? search = null)
        {
            var query = _context.Tenants.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => 
                    t.Name.Contains(search) || 
                    t.Slug.Contains(search) || 
                    t.ContactEmail.Contains(search));
            }

            return await query
                .Include(t => t.Subscription)
                .Include(t => t.Users)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Tenant?> GetTenantAsync(Guid tenantId)
        {
            return await _context.Tenants
                .Include(t => t.Subscription)
                .Include(t => t.Users)
                .Include(t => t.Videos)
                .FirstOrDefaultAsync(t => t.Id == tenantId);
        }

        public async Task<Tenant> CreateTenantAsync(CreateTenantRequest request)
        {
            // Check if slug is available
            if (await _context.Tenants.AnyAsync(t => t.Slug == request.Slug))
                throw new InvalidOperationException("Slug is already taken");

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = request.Slug,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                Address = request.Address,
                LogoUrl = request.LogoUrl,
                Settings = request.Settings ?? new Dictionary<string, object>(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // Create admin user for the tenant
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.AdminEmail,
                FirstName = request.AdminFirstName,
                LastName = request.AdminLastName,
                TenantId = tenant.Id,
                Roles = new List<string> { "Admin" },
                IsActive = true,
                EmailVerified = true,
                TwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Set password
            adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword);

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(adminUser.Email, adminUser.FirstName, tenant.Name);

            return tenant;
        }

        public async Task UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                throw new ArgumentException("Tenant not found", nameof(tenantId));

            if (request.Name != null) tenant.Name = request.Name;
            if (request.ContactEmail != null) tenant.ContactEmail = request.ContactEmail;
            if (request.ContactPhone != null) tenant.ContactPhone = request.ContactPhone;
            if (request.Address != null) tenant.Address = request.Address;
            if (request.LogoUrl != null) tenant.LogoUrl = request.LogoUrl;
            if (request.Settings != null) tenant.Settings = request.Settings;

            tenant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task SuspendTenantAsync(Guid tenantId, string reason)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                throw new ArgumentException("Tenant not found", nameof(tenantId));

            tenant.IsActive = false;
            tenant.SuspensionReason = reason;
            tenant.SuspendedAt = DateTime.UtcNow;
            tenant.UpdatedAt = DateTime.UtcNow;

            // Deactivate all users
            var users = await _context.Users.Where(u => u.TenantId == tenantId).ToListAsync();
            foreach (var user in users)
            {
                user.IsActive = false;
            }

            await _context.SaveChangesAsync();

            // Send suspension email
            await _emailService.SendTenantSuspensionEmailAsync(tenant.ContactEmail, tenant.Name, reason);
        }

        public async Task UnsuspendTenantAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                throw new ArgumentException("Tenant not found", nameof(tenantId));

            tenant.IsActive = true;
            tenant.SuspensionReason = null;
            tenant.SuspendedAt = null;
            tenant.UpdatedAt = DateTime.UtcNow;

            // Reactivate users
            var users = await _context.Users.Where(u => u.TenantId == tenantId).ToListAsync();
            foreach (var user in users)
            {
                user.IsActive = true;
            }

            await _context.SaveChangesAsync();

            // Send reactivation email
            await _emailService.SendTenantReactivationEmailAsync(tenant.ContactEmail, tenant.Name);
        }

        public async Task DeleteTenantAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Users)
                .Include(t => t.Videos)
                .Include(t => t.Subscription)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                throw new ArgumentException("Tenant not found", nameof(tenantId));

            // Soft delete by marking as deleted
            tenant.IsDeleted = true;
            tenant.DeletedAt = DateTime.UtcNow;
            tenant.UpdatedAt = DateTime.UtcNow;

            // Mark all related entities as deleted
            foreach (var user in tenant.Users)
            {
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
            }

            foreach (var video in tenant.Videos)
            {
                video.IsDeleted = true;
                video.DeletedAt = DateTime.UtcNow;
            }

            if (tenant.Subscription != null)
            {
                tenant.Subscription.IsDeleted = true;
                tenant.Subscription.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<PlatformAnalytics> GetPlatformAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Tenants.AsQueryable();
            var userQuery = _context.Users.AsQueryable();
            var videoQuery = _context.Videos.AsQueryable();
            var viewQuery = _context.VideoViews.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate.Value);
                userQuery = userQuery.Where(u => u.CreatedAt >= startDate.Value);
                videoQuery = videoQuery.Where(v => v.CreatedAt >= startDate.Value);
                viewQuery = viewQuery.Where(vv => vv.ViewedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= endDate.Value);
                userQuery = userQuery.Where(u => u.CreatedAt <= endDate.Value);
                videoQuery = videoQuery.Where(v => v.CreatedAt <= endDate.Value);
                viewQuery = viewQuery.Where(vv => vv.ViewedAt <= endDate.Value);
            }

            var tenants = await query.ToListAsync();
            var users = await userQuery.ToListAsync();
            var videos = await videoQuery.ToListAsync();
            var views = await viewQuery.ToListAsync();

            var dailyStats = views
                .GroupBy(v => v.ViewedAt.Date)
                .Select(g => new DailyStat
                {
                    Date = g.Key,
                    Views = g.Count(),
                    UniqueViews = g.Count(v => v.IsUniqueView)
                })
                .OrderBy(d => d.Date)
                .ToList();

            var topTenants = tenants
                .Select(t => new TenantStat
                {
                    TenantId = t.Id,
                    Name = t.Name,
                    Videos = videos.Count(v => v.TenantId == t.Id),
                    Views = views.Count(vv => vv.Video.TenantId == t.Id),
                    Users = users.Count(u => u.TenantId == t.Id),
                    StorageUsed = videos.Where(v => v.TenantId == t.Id).Sum(v => v.FileSize)
                })
                .OrderByDescending(t => t.Views)
                .Take(10)
                .ToList();

            return new PlatformAnalytics
            {
                TotalTenants = tenants.Count,
                ActiveTenants = tenants.Count(t => t.IsActive),
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                TotalVideos = videos.Count,
                TotalViews = views.Count,
                TotalStorageUsed = videos.Sum(v => v.FileSize),
                DailyStats = dailyStats,
                TopTenants = topTenants,
                NewTenantsThisMonth = tenants.Count(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                NewUsersThisMonth = users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                NewVideosThisMonth = videos.Count(v => v.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            };
        }

        public async Task<TenantAnalytics> GetTenantAnalyticsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Users)
                .Include(t => t.Videos)
                .ThenInclude(v => v.Views)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                throw new ArgumentException("Tenant not found", nameof(tenantId));

            var videoQuery = tenant.Videos.AsQueryable();
            var viewQuery = tenant.Videos.SelectMany(v => v.Views).AsQueryable();

            if (startDate.HasValue)
            {
                videoQuery = videoQuery.Where(v => v.CreatedAt >= startDate.Value);
                viewQuery = viewQuery.Where(vv => vv.ViewedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                videoQuery = videoQuery.Where(v => v.CreatedAt <= endDate.Value);
                viewQuery = viewQuery.Where(vv => vv.ViewedAt <= endDate.Value);
            }

            var videos = videoQuery.ToList();
            var views = viewQuery.ToList();

            return new TenantAnalytics
            {
                TenantId = tenantId,
                TenantName = tenant.Name,
                TotalUsers = tenant.Users.Count,
                ActiveUsers = tenant.Users.Count(u => u.IsActive),
                TotalVideos = videos.Count,
                TotalViews = views.Count,
                TotalStorageUsed = videos.Sum(v => v.FileSize),
                UniqueViewers = views.Select(v => v.UserId).Distinct().Count(),
                AverageViewsPerVideo = videos.Any() ? (double)views.Count / videos.Count : 0,
                TopVideos = videos
                    .OrderByDescending(v => v.ViewCount)
                    .Take(5)
                    .Select(v => new VideoStat
                    {
                        VideoId = v.Id,
                        Title = v.Title,
                        Views = v.ViewCount
                    })
                    .ToList()
            };
        }

        public async Task<UserImpersonationToken> CreateUserImpersonationTokenAsync(Guid adminUserId, Guid targetUserId)
        {
            // Verify admin has super admin role
            var admin = await _context.Users.FindAsync(adminUserId);
            if (admin == null || !admin.Roles.Contains("SuperAdmin"))
                throw new UnauthorizedAccessException("User is not a super admin");

            var targetUser = await _context.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (targetUser == null)
                throw new ArgumentException("Target user not found", nameof(targetUserId));

            var token = _jwtService.GenerateImpersonationToken(adminUserId, targetUserId);

            var impersonationToken = new UserImpersonationToken
            {
                Id = Guid.NewGuid(),
                AdminUserId = adminUserId,
                TargetUserId = targetUserId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // 1 hour expiry
                IsActive = true
            };

            _context.UserImpersonationTokens.Add(impersonationToken);
            await _context.SaveChangesAsync();

            return impersonationToken;
        }

        public async Task ValidateImpersonationTokenAsync(string token)
        {
            var impersonationToken = await _context.UserImpersonationTokens
                .FirstOrDefaultAsync(t => t.Token == token && t.IsActive && t.ExpiresAt > DateTime.UtcNow);

            if (impersonationToken == null)
                throw new UnauthorizedAccessException("Invalid or expired impersonation token");

            // Mark as used
            impersonationToken.IsActive = false;
            impersonationToken.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<SystemSettings> GetSystemSettingsAsync()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                // Create default settings
                settings = new SystemSettings
                {
                    Id = Guid.NewGuid(),
                    AllowNewRegistrations = true,
                    RequireEmailVerification = true,
                    DefaultSubscriptionPlanId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    MaxFileSizeMB = 2048,
                    SupportedVideoFormats = new List<string> { "mp4", "avi", "mov", "wmv", "flv", "webm" },
                    MaintenanceMode = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        public async Task UpdateSystemSettingsAsync(SystemSettings settings)
        {
            var existingSettings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (existingSettings == null)
            {
                settings.Id = Guid.NewGuid();
                settings.CreatedAt = DateTime.UtcNow;
                _context.SystemSettings.Add(settings);
            }
            else
            {
                existingSettings.AllowNewRegistrations = settings.AllowNewRegistrations;
                existingSettings.RequireEmailVerification = settings.RequireEmailVerification;
                existingSettings.DefaultSubscriptionPlanId = settings.DefaultSubscriptionPlanId;
                existingSettings.MaxFileSizeMB = settings.MaxFileSizeMB;
                existingSettings.SupportedVideoFormats = settings.SupportedVideoFormats;
                existingSettings.MaintenanceMode = settings.MaintenanceMode;
                existingSettings.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SystemNotification>> GetSystemNotificationsAsync()
        {
            return await _context.SystemNotifications
                .Where(n => n.IsActive && (!n.ExpiresAt.HasValue || n.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateSystemNotificationAsync(CreateSystemNotificationRequest request)
        {
            var notification = new SystemNotification
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                TargetRole = request.TargetRole,
                IsActive = true,
                StartsAt = request.StartsAt ?? DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SystemNotifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<SystemMetrics> GetSystemMetricsAsync()
        {
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);

            return new SystemMetrics
            {
                ActiveConnections = await GetActiveConnectionsCount(),
                CpuUsage = await GetCpuUsage(),
                MemoryUsage = await GetMemoryUsage(),
                DiskUsage = await GetDiskUsage(),
                BandwidthUsage24h = await GetBandwidthUsage(last24Hours),
                BandwidthUsage7d = await GetBandwidthUsage(last7Days),
                BandwidthUsage30d = await GetBandwidthUsage(last30Days),
                ProcessingQueueLength = await GetProcessingQueueLength(),
                ErrorRate24h = await GetErrorRate(last24Hours),
                AverageResponseTime = await GetAverageResponseTime()
            };
        }

        private async Task<int> GetActiveConnectionsCount()
        {
            // This would typically query a connection tracking system
            return 150; // Placeholder
        }

        private async Task<double> GetCpuUsage()
        {
            // This would typically query system performance counters
            return 45.5; // Placeholder
        }

        private async Task<double> GetMemoryUsage()
        {
            // This would typically query system performance counters
            return 62.3; // Placeholder
        }

        private async Task<double> GetDiskUsage()
        {
            // This would typically query disk usage
            return 78.9; // Placeholder
        }

        private async Task<long> GetBandwidthUsage(DateTime since)
        {
            // Calculate from CDN logs or internal tracking
            var views = await _context.VideoViews
                .Where(vv => vv.ViewedAt >= since)
                .Include(vv => vv.Video)
                .ToListAsync();

            return views.Sum(v => v.Video.FileSize);
        }

        private async Task<int> GetProcessingQueueLength()
        {
            return await _context.EncodingJobs
                .CountAsync(j => j.Status == EncodingJobStatus.Pending || j.Status == EncodingJobStatus.Processing);
        }

        private async Task<double> GetErrorRate(DateTime since)
        {
            // This would typically query error logs
            return 2.5; // Placeholder percentage
        }

        private async Task<double> GetAverageResponseTime()
        {
            // This would typically query performance monitoring
            return 125.5; // Placeholder in milliseconds
        }
    }

    // DTOs
    public class CreateTenantRequest
    {
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? LogoUrl { get; set; }
        public Dictionary<string, object>? Settings { get; set; }
        public string AdminEmail { get; set; } = null!;
        public string AdminFirstName { get; set; } = null!;
        public string AdminLastName { get; set; } = null!;
        public string AdminPassword { get; set; } = null!;
    }

    public class UpdateTenantRequest
    {
        public string? Name { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? LogoUrl { get; set; }
        public Dictionary<string, object>? Settings { get; set; }
    }

    public class CreateSystemNotificationRequest
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string? TargetRole { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    // Analytics DTOs
    public class PlatformAnalytics
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalVideos { get; set; }
        public int TotalViews { get; set; }
        public long TotalStorageUsed { get; set; }
        public List<DailyStat> DailyStats { get; set; } = new();
        public List<TenantStat> TopTenants { get; set; } = new();
        public int NewTenantsThisMonth { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewVideosThisMonth { get; set; }
    }

    public class TenantStat
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public int Videos { get; set; }
        public int Views { get; set; }
        public int Users { get; set; }
        public long StorageUsed { get; set; }
    }

    public class SystemMetrics
    {
        public int ActiveConnections { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public long BandwidthUsage24h { get; set; }
        public long BandwidthUsage7d { get; set; }
        public long BandwidthUsage30d { get; set; }
        public int ProcessingQueueLength { get; set; }
        public double ErrorRate24h { get; set; }
        public double AverageResponseTime { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Maintenance
    }
}
