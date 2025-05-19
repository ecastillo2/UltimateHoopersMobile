using DataLayer.Context;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL.Repository
{
    /// <summary>
    /// Generic repository implementation that provides common CRUD operations
    /// with proper error handling and disposal
    /// </summary>
    /// <typeparam name="TEntity">The entity type this repository handles</typeparam>
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly HUDBContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly ILogger _logger;
        private bool _disposed = false;

        public GenericRepository(HUDBContext context, ILogger logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<TEntity>();
            _logger = logger;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "",
            CancellationToken cancellationToken = default)
        {
            try
            {
                IQueryable<TEntity> query = _dbSet;

                // Apply filter if provided
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                // Include related entities
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }

                // Apply ordering if provided
                if (orderBy != null)
                {
                    return await orderBy(query).ToListAsync(cancellationToken);
                }

                return await query.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in GetAllAsync for {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<TEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in GetByIdAsync for {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                throw;
            }
        }

        public virtual async Task<TEntity> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> filter = null,
            string includeProperties = "",
            CancellationToken cancellationToken = default)
        {
            try
            {
                IQueryable<TEntity> query = _dbSet;

                // Apply filter if provided
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                // Include related entities
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }

                return await query.FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in GetFirstOrDefaultAsync for {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbSet.AddAsync(entity, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in AddAsync for {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual void Update(TEntity entity)
        {
            try
            {
                _dbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in Update for {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                TEntity entityToDelete = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
                if (entityToDelete != null)
                {
                    Remove(entityToDelete);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in RemoveAsync for {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                throw;
            }
        }

        public virtual void Remove(TEntity entity)
        {
            try
            {
                if (_context.Entry(entity).State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }
                _dbSet.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in Remove for {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in SaveAsync for {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // No need to dispose _context here as it's injected and managed by DI container
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}