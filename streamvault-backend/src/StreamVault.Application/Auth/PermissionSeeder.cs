using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Auth;

/// <summary>
/// Seeder for core permissions in the system
/// Defines ~40 core permissions across all modules
/// </summary>
public class PermissionSeeder
{
    public static async Task SeedPermissionsAsync(StreamVaultDbContext dbContext)
    {
        // Check if permissions already exist
        if (await dbContext.Permissions.AnyAsync())
            return;

        var permissions = new List<Permission>
        {
            // Video Permissions (13)
            new() { Name = "videos.view", NormalizedName = "VIDEOS.VIEW", Description = "View videos", IsSystemPermission = true },
            new() { Name = "videos.create", NormalizedName = "VIDEOS.CREATE", Description = "Create new videos", IsSystemPermission = true },
            new() { Name = "videos.update", NormalizedName = "VIDEOS.UPDATE", Description = "Update video details", IsSystemPermission = true },
            new() { Name = "videos.delete", NormalizedName = "VIDEOS.DELETE", Description = "Delete videos", IsSystemPermission = true },
            new() { Name = "videos.upload", NormalizedName = "VIDEOS.UPLOAD", Description = "Upload video files", IsSystemPermission = true },
            new() { Name = "videos.bulk_operations", NormalizedName = "VIDEOS.BULK_OPERATIONS", Description = "Perform bulk video operations", IsSystemPermission = true },
            new() { Name = "videos.export", NormalizedName = "VIDEOS.EXPORT", Description = "Export video data", IsSystemPermission = true },
            new() { Name = "videos.settings", NormalizedName = "VIDEOS.SETTINGS", Description = "Configure video settings", IsSystemPermission = true },
            new() { Name = "videos.publish", NormalizedName = "VIDEOS.PUBLISH", Description = "Publish/unpublish videos", IsSystemPermission = true },
            new() { Name = "videos.watermark", NormalizedName = "VIDEOS.WATERMARK", Description = "Add watermarks to videos", IsSystemPermission = true },
            new() { Name = "videos.geo_block", NormalizedName = "VIDEOS.GEO_BLOCK", Description = "Configure geo-blocking", IsSystemPermission = true },
            new() { Name = "videos.download", NormalizedName = "VIDEOS.DOWNLOAD", Description = "Download videos", IsSystemPermission = true },
            new() { Name = "videos.share", NormalizedName = "VIDEOS.SHARE", Description = "Share videos", IsSystemPermission = true },

            // Collections/Folders (5)
            new() { Name = "collections.view", NormalizedName = "COLLECTIONS.VIEW", Description = "View collections", IsSystemPermission = true },
            new() { Name = "collections.create", NormalizedName = "COLLECTIONS.CREATE", Description = "Create collections", IsSystemPermission = true },
            new() { Name = "collections.update", NormalizedName = "COLLECTIONS.UPDATE", Description = "Update collections", IsSystemPermission = true },
            new() { Name = "collections.delete", NormalizedName = "COLLECTIONS.DELETE", Description = "Delete collections", IsSystemPermission = true },
            new() { Name = "collections.organize", NormalizedName = "COLLECTIONS.ORGANIZE", Description = "Organize collections and videos", IsSystemPermission = true },

            // Captions (4)
            new() { Name = "captions.view", NormalizedName = "CAPTIONS.VIEW", Description = "View captions", IsSystemPermission = true },
            new() { Name = "captions.create", NormalizedName = "CAPTIONS.CREATE", Description = "Create/upload captions", IsSystemPermission = true },
            new() { Name = "captions.update", NormalizedName = "CAPTIONS.UPDATE", Description = "Update captions", IsSystemPermission = true },
            new() { Name = "captions.delete", NormalizedName = "CAPTIONS.DELETE", Description = "Delete captions", IsSystemPermission = true },

            // Analytics (3)
            new() { Name = "analytics.view", NormalizedName = "ANALYTICS.VIEW", Description = "View analytics", IsSystemPermission = true },
            new() { Name = "analytics.export", NormalizedName = "ANALYTICS.EXPORT", Description = "Export analytics data", IsSystemPermission = true },
            new() { Name = "analytics.realtime", NormalizedName = "ANALYTICS.REALTIME", Description = "View realtime analytics", IsSystemPermission = true },

            // Users Management (5)
            new() { Name = "users.view", NormalizedName = "USERS.VIEW", Description = "View users", IsSystemPermission = true },
            new() { Name = "users.create", NormalizedName = "USERS.CREATE", Description = "Create users", IsSystemPermission = true },
            new() { Name = "users.update", NormalizedName = "USERS.UPDATE", Description = "Update user details", IsSystemPermission = true },
            new() { Name = "users.delete", NormalizedName = "USERS.DELETE", Description = "Delete users", IsSystemPermission = true },
            new() { Name = "users.roles_manage", NormalizedName = "USERS.ROLES_MANAGE", Description = "Manage user roles", IsSystemPermission = true },

            // Billing (4)
            new() { Name = "billing.view", NormalizedName = "BILLING.VIEW", Description = "View billing information", IsSystemPermission = true },
            new() { Name = "billing.manage", NormalizedName = "BILLING.MANAGE", Description = "Manage billing and subscriptions", IsSystemPermission = true },
            new() { Name = "billing.invoices", NormalizedName = "BILLING.INVOICES", Description = "View and download invoices", IsSystemPermission = true },
            new() { Name = "billing.payments", NormalizedName = "BILLING.PAYMENTS", Description = "Manage payment methods", IsSystemPermission = true },

            // Settings (5)
            new() { Name = "settings.view", NormalizedName = "SETTINGS.VIEW", Description = "View settings", IsSystemPermission = true },
            new() { Name = "settings.branding", NormalizedName = "SETTINGS.BRANDING", Description = "Configure branding", IsSystemPermission = true },
            new() { Name = "settings.api_keys", NormalizedName = "SETTINGS.API_KEYS", Description = "Manage API keys", IsSystemPermission = true },
            new() { Name = "settings.webhooks", NormalizedName = "SETTINGS.WEBHOOKS", Description = "Configure webhooks", IsSystemPermission = true },
            new() { Name = "settings.advanced", NormalizedName = "SETTINGS.ADVANCED", Description = "Access advanced settings", IsSystemPermission = true },

            // Support (3)
            new() { Name = "support.view_tickets", NormalizedName = "SUPPORT.VIEW_TICKETS", Description = "View support tickets", IsSystemPermission = true },
            new() { Name = "support.create_tickets", NormalizedName = "SUPPORT.CREATE_TICKETS", Description = "Create support tickets", IsSystemPermission = true },
            new() { Name = "support.manage_tickets", NormalizedName = "SUPPORT.MANAGE_TICKETS", Description = "Manage support tickets", IsSystemPermission = true },

            // Roles Management (3)
            new() { Name = "roles.view", NormalizedName = "ROLES.VIEW", Description = "View roles", IsSystemPermission = true },
            new() { Name = "roles.create", NormalizedName = "ROLES.CREATE", Description = "Create custom roles", IsSystemPermission = true },
            new() { Name = "roles.manage", NormalizedName = "ROLES.MANAGE", Description = "Manage roles and permissions", IsSystemPermission = true }
        };

        dbContext.Permissions.AddRange(permissions);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Create default roles for a tenant with appropriate permissions
    /// </summary>
    public static async Task SeedDefaultRolesAsync(StreamVaultDbContext dbContext, Tenant tenant)
    {
        // Check if roles already exist for this tenant
        if (await dbContext.Roles.AnyAsync(r => r.TenantId == tenant.Id))
            return;

        var allPermissions = await dbContext.Permissions.ToListAsync();

        // Admin Role - Full access
        var adminRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Admin",
            NormalizedName = "ADMIN",
            IsSystemRole = true,
            Description = "Full access administrator",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Editor Role - Can manage videos and collections
        var editorRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Editor",
            NormalizedName = "EDITOR",
            IsSystemRole = true,
            Description = "Can manage videos and collections",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Viewer Role - Can only view content
        var viewerRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Viewer",
            NormalizedName = "VIEWER",
            IsSystemRole = true,
            Description = "Can view content only",
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Roles.AddRange(adminRole, editorRole, viewerRole);
        await dbContext.SaveChangesAsync();

        // Assign permissions to roles
        var adminPermissions = allPermissions.Where(p => !p.Name.StartsWith("support.")).ToList(); // Admin gets everything except support

        foreach (var perm in adminPermissions)
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = perm.Id
            });
        }

        // Editor permissions
        var editorPermissionNames = new[]
        {
            "videos.view", "videos.create", "videos.update", "videos.delete",
            "videos.upload", "videos.publish", "videos.watermark",
            "collections.view", "collections.create", "collections.update", "collections.delete",
            "captions.view", "captions.create", "captions.update", "captions.delete",
            "analytics.view", "analytics.export",
            "settings.view", "settings.branding"
        };

        foreach (var permName in editorPermissionNames)
        {
            var perm = allPermissions.FirstOrDefault(p => p.NormalizedName == permName.ToUpper());
            if (perm != null)
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = editorRole.Id,
                    PermissionId = perm.Id
                });
            }
        }

        // Viewer permissions
        var viewerPermissionNames = new[] { "videos.view", "collections.view", "captions.view", "analytics.view" };

        foreach (var permName in viewerPermissionNames)
        {
            var perm = allPermissions.FirstOrDefault(p => p.NormalizedName == permName.ToUpper());
            if (perm != null)
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = viewerRole.Id,
                    PermissionId = perm.Id
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }
}
