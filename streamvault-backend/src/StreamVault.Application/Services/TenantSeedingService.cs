using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Services
{
    /// <summary>
    /// Service for seeding initial tenant data
    /// </summary>
    public class TenantSeedingService
    {
        private readonly StreamVaultDbContext _context;
        private readonly ILogger<TenantSeedingService> _logger;

        public TenantSeedingService(
            StreamVaultDbContext context,
            ILogger<TenantSeedingService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Seeds the initial tenant and system data
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();

                // Seed system roles
                await SeedSystemRolesAsync();

                // Seed default tenant if none exists
                await SeedDefaultTenantAsync();

                await _context.SaveChangesAsync();
                _logger.LogInformation("Tenant seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during tenant seeding");
                throw;
            }
        }

        private async Task SeedSystemRolesAsync()
        {
            var systemRoles = new[]
            {
                new Role
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    Description = "System super administrator with full access",
                    IsSystemRole = true,
                    TenantId = null,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new Role
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Tenant administrator",
                    IsSystemRole = true,
                    TenantId = null,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new Role
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Editor",
                    NormalizedName = "EDITOR",
                    Description = "Can edit and manage content",
                    IsSystemRole = true,
                    TenantId = null,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new Role
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Viewer",
                    NormalizedName = "VIEWER",
                    Description = "Read-only access",
                    IsSystemRole = true,
                    TenantId = null,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            foreach (var role in systemRoles)
            {
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == role.Id || 
                                           (r.NormalizedName == role.NormalizedName && r.IsSystemRole));

                if (existingRole == null)
                {
                    await _context.Roles.AddAsync(role);
                    _logger.LogInformation("Created system role: {RoleName}", role.Name);
                }
            }
        }

        private async Task SeedDefaultTenantAsync()
        {
            // Check if any tenant exists
            var hasTenant = await _context.Tenants.AnyAsync();
            if (hasTenant)
            {
                _logger.LogInformation("Tenants already exist, skipping default tenant creation");
                return;
            }

            // Create default tenant
            var defaultTenant = new Tenant(
                "StreamVault Demo",
                "demo",
                Guid.Parse("55555555-5555-5555-5555-555555555555") // Default plan ID
            );

            defaultTenant.SetBunnyConfiguration(
                "demo-library-id",
                "demo-api-key"
            );

            defaultTenant.SetStripeCustomerId("cus_demo");

            // Create tenant branding
            var branding = new TenantBranding
            {
                Id = Guid.NewGuid(),
                TenantId = defaultTenant.Id,
                LogoUrl = "/assets/default-logo.png",
                PrimaryColor = "#3B82F6",
                SecondaryColor = "#10B981",
                CustomCss = "",
                FaviconUrl = "/favicon.ico",
                CustomFooterText = "Powered by StreamVault",
                HideBranding = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Create tenant-specific roles
            var tenantAdminRole = new Role
            {
                Id = Guid.NewGuid(),
                TenantId = defaultTenant.Id,
                Name = "TenantAdmin",
                NormalizedName = "TENANTADMIN",
                Description = "Tenant-specific administrator",
                IsSystemRole = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Create default admin user
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = defaultTenant.Id,
                Email = "admin@demo.streamvault.com",
                PasswordHash = "$2a$11$example_hash_replace_in_production", // Should be properly hashed
                FirstName = "Admin",
                LastName = "User",
                Status = UserStatus.Active,
                IsEmailVerified = true,
                EmailVerifiedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Assign admin role to user
            var userRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = tenantAdminRole.Id,
                AssignedAt = DateTimeOffset.UtcNow,
                AssignedBy = null // System assigned
            };

            // Create default video categories
            var categories = new[]
            {
                new VideoCategory
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenant.Id,
                    Name = "General",
                    Slug = "general",
                    Description = "General videos",
                    SortOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new VideoCategory
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenant.Id,
                    Name = "Tutorials",
                    Slug = "tutorials",
                    Description = "Tutorial and how-to videos",
                    SortOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new VideoCategory
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenant.Id,
                    Name = "Entertainment",
                    Slug = "entertainment",
                    Description = "Entertainment content",
                    SortOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            // Add all entities to context
            await _context.Tenants.AddAsync(defaultTenant);
            await _context.TenantBrandings.AddAsync(branding);
            await _context.Roles.AddAsync(tenantAdminRole);
            await _context.Users.AddAsync(adminUser);
            await _context.UserRoles.AddAsync(userRole);
            await _context.VideoCategories.AddRangeAsync(categories);

            _logger.LogInformation("Created default tenant: {TenantName}", defaultTenant.Name);
        }

        /// <summary>
        /// Creates a new tenant with default configuration
        /// </summary>
        public async Task<Tenant> CreateTenantAsync(string name, string slug, Guid? planId = null)
        {
            // Check if slug is already taken
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Slug == slug);

            if (existingTenant != null)
                throw new InvalidOperationException($"Tenant with slug '{slug}' already exists");

            // Create new tenant
            var tenant = new Tenant(name, slug, planId);
            tenant.StartTrial(14); // 14-day trial

            // Create default branding
            var branding = new TenantBranding
            {
                TenantId = tenant.Id,
                PrimaryColor = "#3B82F6",
                SecondaryColor = "#10B981",
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Create default categories
            var categories = new[]
            {
                new VideoCategory
                {
                    TenantId = tenant.Id,
                    Name = "General",
                    Slug = "general",
                    Description = "General videos",
                    SortOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            await _context.Tenants.AddAsync(tenant);
            await _context.TenantBrandings.AddAsync(branding);
            await _context.VideoCategories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new tenant: {TenantName} ({TenantSlug})", name, slug);
            return tenant;
        }
    }
}
