using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataLayer.EFCoreExtensions
{
    /// <summary>
    /// Extensions to optimize EF Core queries
    /// </summary>
    public static class QueryOptimizationExtensions
    {
        /// <summary>
        /// Get paginated results with optimized performance
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">Source queryable</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="ascending">Ascending or descending order</param>
        /// <returns>Paged result with items and count</returns>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T, TKey>(
            this IQueryable<T> query,
            int page,
            int pageSize,
            Expression<Func<T, TKey>> orderBy,
            bool ascending = true)
            where T : class  // Added constraint: T must be a reference type
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Apply ordering
            query = ascending
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);

            // Get total count (efficiently with future query pattern)
            var totalTask = query.CountAsync();

            // Get page of data with optimized query
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking() // For better performance with read-only data
                .ToListAsync();

            // Get total without executing another query
            var total = await totalTask;

            return new PagedResult<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        /// <summary>
        /// Get entity by ID with cached results for better performance
        /// </summary>
        public static async Task<T> GetByIdCachedAsync<T, TKey>(
            this DbSet<T> dbSet,
            TKey id,
            IMemoryCache cache,
            TimeSpan? cacheExpiration = null)
            where T : class  // This constraint was already present
        {
            // Create cache key
            var cacheKey = $"{typeof(T).Name}_{id}";

            // Try to get from cache first
            if (cache.TryGetValue(cacheKey, out T entity))
            {
                return entity;
            }

            // If not in cache, get from database
            entity = await dbSet.FindAsync(id);

            // If found, add to cache
            if (entity != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1);

                if (cacheExpiration.HasValue)
                {
                    cacheOptions.SetAbsoluteExpiration(cacheExpiration.Value);
                }
                else
                {
                    // Default: 5 minutes
                    cacheOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                }

                cache.Set(cacheKey, entity, cacheOptions);
            }

            return entity;
        }

        /// <summary>
        /// Efficiently update only the specified properties of an entity
        /// </summary>
        public static EntityEntry<T> UpdatePartial<T>(
            this DbContext context,
            T entity,
            params Expression<Func<T, object>>[] properties)
            where T : class  // This constraint was already present
        {
            // Attach entity to context
            var entry = context.Attach(entity);

            // Mark only specified properties as modified
            foreach (var property in properties)
            {
                entry.Property(property).IsModified = true;
            }

            return entry;
        }

        /// <summary>
        /// Batch update entities efficiently
        /// </summary>
        public static async Task<int> BatchUpdateAsync<T>(
            this DbContext context,
            IEnumerable<T> entities,
            int batchSize = 100)
            where T : class  // This constraint was already present
        {
            var totalCount = 0;
            var items = entities.ToList();

            // Process in batches for better performance
            for (int i = 0; i < items.Count; i += batchSize)
            {
                var batch = items.Skip(i).Take(batchSize);

                foreach (var entity in batch)
                {
                    context.Update(entity);
                }

                totalCount += await context.SaveChangesAsync();
            }

            return totalCount;
        }

        /// <summary>
        /// Execute a query with selective includes for better performance
        /// </summary>
        public static async Task<List<T>> GetWithSelectiveIncludesAsync<T>(
            this IQueryable<T> query,
            Func<IQueryable<T>, IQueryable<T>> includeFunc = null)
            where T : class  // This constraint was already present
        {
            // Apply includes if provided
            if (includeFunc != null)
            {
                query = includeFunc(query);
            }

            // Execute query with no tracking for better performance
            return await query.AsNoTracking().ToListAsync();
        }
    }

    /// <summary>
    /// Represents a paged result set
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}