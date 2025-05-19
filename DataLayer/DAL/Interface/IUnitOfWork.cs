using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    /// <summary>
    /// Interface for the Unit of Work pattern to coordinate multiple repository operations
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repository properties
        IUserRepository User { get; }
        IProfileRepository Profile { get; }
        IPostRepository Post { get; }
        IGameRepository Game { get; }
        ICourtRepository Court { get; }
        IPrivateRunRepository PrivateRun { get; }
        IProductRepository Product { get; }
        INotificationRepository Notification { get; }
        IOrderRepository Order { get; }
        // Add other repository properties as needed

        /// <summary>
        /// Save all changes to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of entities written to database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commit the current database transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback the current database transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}