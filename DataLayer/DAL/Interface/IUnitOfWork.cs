using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    /// <summary>
    /// Unit of Work interface for managing database transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Add repository properties here
        IUserRepository User { get; }
        IProfileRepository Profile { get; }
        IPostRepository Post { get; }
        IGameRepository Game { get; }
        // Add other repositories as needed

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementation of the Unit of Work pattern for managing database transactions
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HUDBContext _context;
        private bool _disposed;
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction _transaction;

        // Repository instances
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private IPostRepository _postRepository;
        private IGameRepository _gameRepository;
        // Add other repository instances as needed

        public UnitOfWork(HUDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Repository properties
        public IUserRepository User => _userRepository ??= new UserRepository(_context);
        public IProfileRepository Profile => _profileRepository ??= new ProfileRepository(_context, null);
        public IPostRepository Post => _postRepository ??= new PostRepository(_context, null);
        public IGameRepository Game => _gameRepository ??= new GameRepository(_context);
        // Implement other repository properties as needed

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
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
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                _transaction.Dispose();
                _transaction = null;
            }
        }

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}