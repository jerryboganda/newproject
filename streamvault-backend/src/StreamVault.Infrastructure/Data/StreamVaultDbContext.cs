using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;

namespace StreamVault.Infrastructure.Data;

public class StreamVaultDbContext : DbContext
{
    public StreamVaultDbContext(DbContextOptions<StreamVaultDbContext> options) : base(options) { }

    // Master DB tables
    public DbSet<Tenant> Tenants { get; set; } = null!;

    public DbSet<TenantBranding> TenantBrandings { get; set; } = null!;

    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;

    public DbSet<TenantSubscription> TenantSubscriptions { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Role> Roles { get; set; } = null!;

    public DbSet<Permission> Permissions { get; set; } = null!;

    public DbSet<UserRole> UserRoles { get; set; } = null!;

    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite keys for junction tables
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Configure relationships
        modelBuilder.Entity<TenantBranding>()
            .HasOne(tb => tb.Tenant)
            .WithOne(t => t.Branding)
            .HasForeignKey<TenantBranding>(tb => tb.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TenantSubscription>()
            .HasOne(ts => ts.Tenant)
            .WithMany(t => t.Subscriptions)
            .HasForeignKey(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TenantSubscription>()
            .HasOne(ts => ts.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(ts => ts.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelBuilder.Entity<SubscriptionPlan>()
            .HasIndex(sp => sp.Slug)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => new { r.NormalizedName, r.TenantId })
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.NormalizedName)
            .IsUnique();

        // Configure JSONB columns for PostgreSQL
        modelBuilder.Entity<Tenant>()
            .Property(t => t.Settings)
            .HasColumnType("jsonb");

        modelBuilder.Entity<SubscriptionPlan>()
            .Property(sp => sp.Features)
            .HasColumnType("jsonb");

        modelBuilder.Entity<SubscriptionPlan>()
            .Property(sp => sp.Limits)
            .HasColumnType("jsonb");
    }
}
