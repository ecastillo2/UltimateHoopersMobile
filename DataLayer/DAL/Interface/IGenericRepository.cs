using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Generic repository interface defining common CRUD operations with consistent cancellation token support
    /// </summary>
    /// <typeparam name="TEntity">The entity type this repository handles</typeparam>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Get all entities with optional filtering, ordering, and includes
        /// </summary>
        /// <param name="filter">Filter expression</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="includeProperties">Comma-separated list of properties to include</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of entities</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Entity with matching ID</returns>
        Task<TEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get first entity matching filter with optional includes
        /// </summary>
        /// <param name="filter">Filter expression</param>
        /// <param name="includeProperties">Comma-separated list of properties to include</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First entity matching filter</returns>
        Task<TEntity> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> filter = null,
            string includeProperties = "",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Add a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        void Update(TEntity entity);

        /// <summary>
        /// Remove an entity by ID
        /// </summary>
        /// <param name="id">ID of entity to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RemoveAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove an entity
        /// </summary>
        /// <param name="entity">Entity to remove</param>
        void Remove(TEntity entity);

        /// <summary>
        /// Save changes to database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of entities written to database</returns>
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
    }
}