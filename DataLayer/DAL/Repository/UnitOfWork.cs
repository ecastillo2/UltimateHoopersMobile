using DataLayer.Context;
using DataLayer.DAL.Interface;
using DataLayer.DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    /// <summary>
    /// Implementation of the Unit of Work pattern
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HUDBContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction _transaction;

        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private IPostRepository _postRepository;
        private IGameRepository _gameRepository;
        private INotificationRepository _notificationRepository;
        private ICourtRepository _courtRepository;
        private IProductRepository _productRepository;
        private IOrderRepository _orderRepository;
        private IPrivateRunRepository _privateRunRepository;
        private IPrivateRunInviteRepository _privateRunInviteRepository;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Logger for diagnostic information</param>
        public UnitOfWork(
            HUDBContext context,
            ILogger<UnitOfWork> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        /// <summary>
        /// Gets the user repository
        /// </summary>
        public IUserRepository User => _userRepository ??= new UserRepository(_context);

        /// <summary>
        /// Gets the profile repository
        /// </summary>
        public IProfileRepository Profile => _profileRepository ??= new ProfileRepository(_context, null);

        /// <summary>
        /// Gets the post repository
        /// </summary>
        public IPostRepository Post => _postRepository ??= new PostRepository(_context, null);

        /// <summary>
        /// Gets the game repository
        /// </summary>
        public IGameRepository Game => _gameRepository ??= new GameRepository(_context);

        /// <summary>
        /// Gets the notification repository
        /// </summary>
        public INotificationRepository Notification => _notificationRepository ??= new NotificationRepository(_context);

        /// <summary>
        /// Gets the court repository
        /// </summary>
        public ICourtRepository Court => _courtRepository ??= new CourtRepository(_context, null);

        /// <summary>
        /// Gets the product repository
        /// </summary>
        public IProductRepository Product => _productRepository ??= new ProductRepository(_context);

        /// <summary>
        /// Gets the order repository
        /// </summary>
        public IOrderRepository Order => _orderRepository ??= new OrderRepository(_context);

        /// <summary>
        /// Gets the private run repository
        /// </summary>
        public IPrivateRunRepository PrivateRun => _privateRunRepository ??= new PrivateRunRepository(_context, null);

        /// <summary>
        /// Gets the private run invite repository
        /// </summary>
        public IPrivateRunInviteRepository PrivateRunInvite => _privateRunInviteRepository ??= new PrivateRunInviteRepository(_context);

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger?.LogInformation("Transaction started");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting transaction");
                throw;
            }
        }

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                    _logger?.LogInformation("Transaction committed");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error committing transaction");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync(cancellationToken);
                    _logger?.LogInformation("Transaction rolled back");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error rolling back transaction");
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        /// <summary>
        /// Save changes to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of entities written to the database</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of resources asynchronously
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                }

                await _context.DisposeAsync();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}