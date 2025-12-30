using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using StreamVault.Application.Interfaces;
using StreamVault.Domain.Entities;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework query filter for tenant data isolation
    /// </summary>
    public static class TenantDataIsolation
    {
        /// <summary>
        /// Applies tenant filtering to the given model builder
        /// </summary>
        public static void ApplyTenantFilters(ModelBuilder modelBuilder, ITenantContext tenantContext)
        {
            // Get all entity types that have TenantId property
            var entityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(StreamVault.Domain.Interfaces.ITenantEntity).IsAssignableFrom(e.ClrType));

            foreach (var entityType in entityTypes)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var tenantIdProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                var currentTenantId = Expression.Constant(tenantContext.TenantId ?? Guid.Empty, typeof(Guid));
                var filterExpression = Expression.Equal(tenantIdProperty, currentTenantId);

                var lambda = Expression.Lambda(filterExpression, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// Extension methods for queryable tenant filtering
    /// </summary>
    public static class TenantQueryableExtensions
    {
        /// <summary>
        /// Filters a query to only include entities for the current tenant
        /// </summary>
        public static IQueryable<T> ForTenant<T>(this IQueryable<T> query, ITenantContext tenantContext)
            where T : class, ITenantEntity
        {
            if (!tenantContext.HasCurrentTenant)
                throw new InvalidOperationException("No current tenant set");

            return query.Where(e => e.TenantId == tenantContext.TenantId);
        }

        /// <summary>
        /// Checks if an entity belongs to the current tenant
        /// </summary>
        public static bool BelongsToTenant<T>(this T entity, ITenantContext tenantContext)
            where T : class, ITenantEntity
        {
            if (!tenantContext.HasCurrentTenant)
                return false;

            return entity.TenantId == tenantContext.TenantId;
        }
    }

    /// <summary>
    /// Interceptor for automatically setting TenantId on entity creation
    /// </summary>
    public class TenantInterceptor : SaveChangesInterceptor
    {
        private readonly ITenantContext _tenantContext;

        public TenantInterceptor(ITenantContext tenantContext)
        {
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            SetTenantIds(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            SetTenantIds(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void SetTenantIds(DbContext? context)
        {
            if (context == null || !_tenantContext.HasCurrentTenant)
                return;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is ITenantEntity && e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                var entity = (ITenantEntity)entry.Entity;
                if (entity.TenantId == Guid.Empty)
                {
                    entity.TenantId = _tenantContext.TenantId ?? Guid.Empty;
                }
            }
        }
    }
}
