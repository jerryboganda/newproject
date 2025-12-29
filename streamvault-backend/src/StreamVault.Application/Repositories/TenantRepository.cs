using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Interfaces;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Repositories
{
    /// <summary>
    /// Generic repository interface with tenant awareness
    /// </summary>
    public interface ITenantRepository<T> where T : class, ITenantEntity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> GetByIdAsync(Guid id, Guid tenantId);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Guid id);
        IQueryable<T> Query();
    }

    /// <summary>
    /// Implementation of tenant-aware repository
    /// </summary>
    public class TenantRepository<T> : ITenantRepository<T> where T : class, ITenantEntity
    {
        protected readonly StreamVaultDbContext _context;
        protected readonly ITenantContext _tenantContext;
        protected readonly DbSet<T> _dbSet;

        public TenantRepository(
            StreamVaultDbContext context,
            ITenantContext tenantContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            if (!_tenantContext.HasCurrentTenant)
                throw new InvalidOperationException("No current tenant set");

            return await _dbSet
                .ForTenant(_tenantContext)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, Guid tenantId)
        {
            return await _dbSet
                .Where(e => e.TenantId == tenantId)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            if (!_tenantContext.HasCurrentTenant)
                throw new InvalidOperationException("No current tenant set");

            return await _dbSet
                .ForTenant(_tenantContext)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            if (!_tenantContext.HasCurrentTenant)
                throw new InvalidOperationException("No current tenant set");

            return await _dbSet
                .ForTenant(_tenantContext)
                .Where(predicate)
                .ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Ensure tenant is set
            if (entity.TenantId == Guid.Empty && _tenantContext.HasCurrentTenant)
            {
                entity.TenantId = _tenantContext.TenantId ?? Guid.Empty;
            }

            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Verify entity belongs to current tenant
            if (_tenantContext.HasCurrentTenant && !entity.BelongsToTenant(_tenantContext))
                throw new InvalidOperationException("Entity does not belong to current tenant");

            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Verify entity belongs to current tenant
            if (_tenantContext.HasCurrentTenant && !entity.BelongsToTenant(_tenantContext))
                throw new InvalidOperationException("Entity does not belong to current tenant");

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<int> CountAsync()
        {
            if (!_tenantContext.HasCurrentTenant)
                throw new InvalidOperationException("No current tenant set");

            return await _dbSet
                .ForTenant(_tenantContext)
                .CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            if (!_tenantContext.HasCurrentTenant)
                throw new InvalidOperationException("No current tenant set");

            return await _dbSet
                .ForTenant(_tenantContext)
                .Where(predicate)
                .CountAsync();
        }

        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            if (!_tenantContext.HasCurrentTenant)
                throw new InvalidOperationException("No current tenant set");

            return await _dbSet
                .ForTenant(_tenantContext)
                .AnyAsync(e => e.Id == id);
        }

        public virtual IQueryable<T> Query()
        {
            if (!_tenantContext.HasCurrentTenant)
                throw new InvalidOperationException("No current tenant set");

            return _dbSet.ForTenant(_tenantContext);
        }
    }

    /// <summary>
    /// Specialized repository for Tenant entity (doesn't implement ITenantEntity)
    /// </summary>
    public interface ITenantRepository
    {
        Task<Tenant?> GetByIdAsync(Guid id);
        Task<Tenant?> GetBySlugAsync(string slug);
        Task<Tenant?> GetByCustomDomainAsync(string domain);
        Task<IEnumerable<Tenant>> GetAllAsync();
        Task<Tenant> AddAsync(Tenant entity);
        Task UpdateAsync(Tenant entity);
        Task DeleteAsync(Tenant entity);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> SlugExistsAsync(string slug);
        IQueryable<Tenant> Query();
    }

    public class TenantRepository : ITenantRepository
    {
        protected readonly StreamVaultDbContext _context;
        protected readonly DbSet<Tenant> _dbSet;

        public TenantRepository(StreamVaultDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<Tenant>();
        }

        public virtual async Task<Tenant?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<Tenant?> GetBySlugAsync(string slug)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Slug == slug);
        }

        public virtual async Task<Tenant?> GetByCustomDomainAsync(string domain)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.CustomDomain == domain);
        }

        public virtual async Task<IEnumerable<Tenant>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<Tenant> AddAsync(Tenant entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(Tenant entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(Tenant entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(t => t.Id == id);
        }

        public virtual async Task<bool> SlugExistsAsync(string slug)
        {
            return await _dbSet.AnyAsync(t => t.Slug == slug);
        }

        public virtual IQueryable<Tenant> Query()
        {
            return _dbSet;
        }
    }
}
