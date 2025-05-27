using DataLayer.Context;
using DataLayer.DAL.Interface;
using DataLayer.DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    /// <summary>
    /// Refactored Unit of Work implementation that consistently manages repositories
    /// and database transactions
    /// </summary>
    public class RefactoredUnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction _transaction;

        // Repository fields with lazy initialization
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private IPostRepository _postRepository;
        private IGameRepository _gameRepository;
        private INotificationRepository _notificationRepository;
        private ICourtRepository _courtRepository;
        private IProductRepository _productRepository;
        private IOrderRepository _orderRepository;
        private IRunRepository _runRepository;
        private IJoinedRunRepository _joinedRunRepository;
        private bool _disposed = false;

        /// <summary>
        /// Creates a new instance of the Unit of Work
        /// </summary>
        public RefactoredUnitOfWork(ApplicationContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Property implementations with lazy loading
        public IUserRepository User => _userRepository ??= new UserRepository(_context, (ILogger<UserRepository>)_logger);
        public IProfileRepository Profile => _profileRepository ??= new ProfileRepository(_context, null);
        public IPostRepository Post => _postRepository ??= new PostRepository(_context, null);
        public IGameRepository Game => _gameRepository ??= new GameRepository(_context);
        public INotificationRepository Notification => _notificationRepository ??= new NotificationRepository(_context);
        public ICourtRepository Court => _courtRepository ??= new CourtRepository(_context, null);
        public IProductRepository Product => _productRepository ??= new ProductRepository(_context);
        public IOrderRepository Order => _orderRepository ??= new OrderRepository(_context);
        public IRunRepository Run => _runRepository ??= new RunRepository(_context, null);
        public IJoinedRunRepository JoinedRun => _joinedRunRepository ??= new JoinedRunRepository(_context);

        /// <summary>
        /// Begin a database transaction
        /// </summary>
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
        /// Asynchronously dispose of resources
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