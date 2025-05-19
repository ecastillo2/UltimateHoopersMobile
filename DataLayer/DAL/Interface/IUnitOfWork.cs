using DataLayer.DAL.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    /// <summary>
    /// Interface for the Unit of Work pattern to manage multiple repositories and transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Gets the user repository
        /// </summary>
        IUserRepository User { get; }

        /// <summary>
        /// Gets the profile repository
        /// </summary>
        IProfileRepository Profile { get; }

        /// <summary>
        /// Gets the post repository
        /// </summary>
        IPostRepository Post { get; }

        /// <summary>
        /// Gets the game repository
        /// </summary>
        IGameRepository Game { get; }

        /// <summary>
        /// Gets the notification repository
        /// </summary>
        INotificationRepository Notification { get; }

        /// <summary>
        /// Gets the court repository
        /// </summary>
        ICourtRepository Court { get; }

        /// <summary>
        /// Gets the product repository
        /// </summary>
        IProductRepository Product { get; }

        /// <summary>
        /// Gets the order repository
        /// </summary>
        IOrderRepository Order { get; }

        /// <summary>
        /// Gets the private run repository
        /// </summary>
        IPrivateRunRepository PrivateRun { get; }

        /// <summary>
        /// Gets the private run invite repository
        /// </summary>
        IPrivateRunInviteRepository PrivateRunInvite { get; }

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Save changes to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of entities written to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}