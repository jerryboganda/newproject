using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Infrastructure.Services;

public class DatabaseSeeder
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(StreamVaultDbContext dbContext, ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == "demo", cancellationToken);
        if (tenant == null)
        {
            tenant = new Tenant("StreamVault Demo", "demo");
            tenant.StartTrial(14);

            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created default tenant: {TenantSlug}", tenant.Slug);
        }

        var superAdminRole = await EnsureRoleAsync(tenant.Id, "SuperAdmin", cancellationToken);
        var businessAdminRole = await EnsureRoleAsync(tenant.Id, "BusinessAdmin", cancellationToken);

        await EnsureUserAsync(
            tenant.Id,
            "admin@streamvault.com",
            "Admin",
            "User",
            "SuperAdmin123!",
            superAdminRole.Id,
            cancellationToken);

        await EnsureUserAsync(
            tenant.Id,
            "business@streamvault.com",
            "Business",
            "Admin",
            "BusinessAdmin123!",
            businessAdminRole.Id,
            cancellationToken);

        await EnsureSystemSettingsAsync(cancellationToken);
        await EnsureDefaultEmailTemplatesAsync(tenant.Id, cancellationToken);
        await EnsureSupportDefaultsAsync(tenant.Id, cancellationToken);
    }

    private async Task EnsureSystemSettingsAsync(CancellationToken cancellationToken)
    {
        var existing = await _dbContext.SystemSettings.FirstOrDefaultAsync(cancellationToken);
        if (existing != null) return;

        var now = DateTime.UtcNow;

        _dbContext.SystemSettings.Add(new SystemSettings
        {
            Id = Guid.NewGuid(),
            AllowNewRegistrations = true,
            RequireEmailVerification = true,
            DefaultSubscriptionPlanId = Guid.Empty,
            MaxFileSizeMB = 2048,
            SupportedVideoFormats = new List<string> { "mp4", "mov", "webm" },
            MaintenanceMode = false,
            MaintenanceMessage = null,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded SystemSettings");
    }

    private async Task EnsureDefaultEmailTemplatesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        // Use the super admin user (seeded above) as template author.
        var createdBy = await _dbContext.Users
            .Where(u => u.TenantId == tenantId && u.Email == "admin@streamvault.com")
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (createdBy == Guid.Empty)
            return;

        await EnsureEmailTemplateAsync(
            name: "email_verification",
            category: "auth",
            subject: "Verify your StreamVault email",
            html: "<p>Hello,</p><p>Please verify your email by clicking: <a href=\"{{verify_url}}\">Verify Email</a></p>",
            text: "Please verify your email: {{verify_url}}",
            createdByUserId: createdBy,
            cancellationToken: cancellationToken);

        await EnsureEmailTemplateAsync(
            name: "password_reset",
            category: "auth",
            subject: "Reset your StreamVault password",
            html: "<p>Hello,</p><p>Reset your password here: <a href=\"{{reset_url}}\">Reset Password</a></p>",
            text: "Reset your password: {{reset_url}}",
            createdByUserId: createdBy,
            cancellationToken: cancellationToken);

        await EnsureEmailTemplateAsync(
            name: "two_factor",
            category: "auth",
            subject: "Your StreamVault verification code",
            html: "<p>Your code is: <strong>{{code}}</strong></p>",
            text: "Your code is: {{code}}",
            createdByUserId: createdBy,
            cancellationToken: cancellationToken);
    }

    private async Task EnsureEmailTemplateAsync(
        string name,
        string category,
        string subject,
        string html,
        string text,
        Guid createdByUserId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.EmailTemplates.AnyAsync(t => t.Name == name, cancellationToken);
        if (exists) return;

        var now = DateTime.UtcNow;

        _dbContext.EmailTemplates.Add(new EmailTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = category,
            Subject = subject,
            HtmlContent = html,
            TextContent = text,
            Variables = ExtractVariables(subject, html, text).ToList(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = createdByUserId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded EmailTemplate: {TemplateName}", name);
    }

    private static string[] ExtractVariables(string subject, string html, string text)
    {
        static IEnumerable<string> Read(string value)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(value ?? string.Empty, @"\{\{\s*([a-zA-Z0-9_\.\-]+)\s*\}\}");
            foreach (System.Text.RegularExpressions.Match m in matches)
                if (m.Groups.Count > 1) yield return m.Groups[1].Value;
        }

        return Read(subject).Concat(Read(html)).Concat(Read(text))
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task EnsureSupportDefaultsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var defaultSla = await _dbContext.SupportSlaPolicies
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Name == "Default", cancellationToken);

        if (defaultSla == null)
        {
            defaultSla = new SupportSlaPolicy
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Default",
                FirstResponseMinutes = 60,
                ResolutionMinutes = 24 * 60,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            _dbContext.SupportSlaPolicies.Add(defaultSla);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var generalDepartment = await _dbContext.SupportDepartments
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Slug == "general", cancellationToken);

        if (generalDepartment == null)
        {
            generalDepartment = new SupportDepartment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "General",
                Slug = "general",
                IsActive = true,
                DefaultSlaPolicyId = defaultSla.Id,
                CreatedAt = now,
                UpdatedAt = now
            };

            _dbContext.SupportDepartments.Add(generalDepartment);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var kbCategory = await _dbContext.KnowledgeBaseCategories
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Slug == "general", cancellationToken);

        if (kbCategory == null)
        {
            kbCategory = new KnowledgeBaseCategory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "General",
                Slug = "general",
                Description = "General help articles",
                SortOrder = 0,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            _dbContext.KnowledgeBaseCategories.Add(kbCategory);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var firstResponseRule = await _dbContext.SupportEscalationRules
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == "First response overdue", cancellationToken);

        if (firstResponseRule == null)
        {
            _dbContext.SupportEscalationRules.Add(new SupportEscalationRule
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "First response overdue",
                Trigger = SupportEscalationTrigger.FirstResponseOverdue,
                ThresholdMinutes = 0,
                EscalateToPriority = TicketPriority.High,
                SetStatusToEscalated = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var resolutionRule = await _dbContext.SupportEscalationRules
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == "Resolution overdue", cancellationToken);

        if (resolutionRule == null)
        {
            _dbContext.SupportEscalationRules.Add(new SupportEscalationRule
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Resolution overdue",
                Trigger = SupportEscalationTrigger.ResolutionOverdue,
                ThresholdMinutes = 0,
                EscalateToPriority = TicketPriority.Critical,
                SetStatusToEscalated = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<Role> EnsureRoleAsync(Guid tenantId, string roleName, CancellationToken cancellationToken)
    {
        var normalizedName = roleName.Trim().ToUpperInvariant();

        var existing = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.NormalizedName == normalizedName, cancellationToken);

        if (existing != null)
            return existing;

        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = roleName,
            NormalizedName = normalizedName,
            IsSystemRole = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created role {RoleName} for tenant {TenantId}", roleName, tenantId);

        return role;
    }

    private async Task EnsureUserAsync(
        Guid tenantId,
        string email,
        string firstName,
        string lastName,
        string password,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email, cancellationToken);

        if (existingUser == null)
        {
            existingUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Status = UserStatus.Active,
                IsEmailVerified = true,
                EmailVerifiedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Users.Add(existingUser);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created user {Email} for tenant {TenantId}", email, tenantId);
        }

        var hasRole = await _dbContext.UserRoles
            .AnyAsync(ur => ur.UserId == existingUser.Id && ur.RoleId == roleId, cancellationToken);

        if (!hasRole)
        {
            _dbContext.UserRoles.Add(new UserRole
            {
                UserId = existingUser.Id,
                RoleId = roleId,
                AssignedAt = DateTimeOffset.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Assigned role {RoleId} to user {Email}", roleId, email);
        }
    }
}
