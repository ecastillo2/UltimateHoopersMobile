using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DataLayer.DAL
{
    /// <summary>
    /// Unit of Work implementation for managing database transactions across multiple repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HUDBContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private bool _disposed;
        private IDbContextTransaction _transaction;

        // Repository fields
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private IPostRepository _postRepository;
        private IGameRepository _gameRepository;
        private ICourtRepository _courtRepository;
        private IPrivateRunRepository _privateRunRepository;
        private IProductRepository _productRepository;
        private INotificationRepository _notificationRepository;
        private IOrderRepository _orderRepository;
        // Add other repository instances as needed

        public UnitOfWork(HUDBContext context, ILogger<UnitOfWork> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        // Repository properties with lazy loading
        public IUserRepository User =>
            _userRepository ??= new UserRepository(_context, _logger);

        public IProfileRepository Profile =>
            _profileRepository ??= new ProfileRepository(_context, null, _logger);

        public IPostRepository Post =>
            _postRepository ??= new PostRepository(_context, null);

        public IGameRepository Game =>
            _gameRepository ??= new GameRepository(_context);

        public ICourtRepository Court =>
            _courtRepository ??= new CourtRepository(_context, null, _logger);

        public IPrivateRunRepository PrivateRun =>
            _privateRunRepository ??= new PrivateRunRepository(_context, null, _logger);

        public IProductRepository Product =>
            _productRepository ??= new ProductRepository(_context);

        public INotificationRepository Notification =>
            _notificationRepository ??= new NotificationRepository(_context);

        public IOrderRepository Order =>
            _orderRepository ??= new OrderRepository(_context);

        // Add other repository properties as needed

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

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger?.LogInformation("Database transaction started");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting database transaction");
                throw;
            }
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                    _logger?.LogInformation("Database transaction committed");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error committing database transaction");
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

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync(cancellationToken);
                    _logger?.LogInformation("Database transaction rolled back");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error rolling back database transaction");
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context?.Dispose();
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