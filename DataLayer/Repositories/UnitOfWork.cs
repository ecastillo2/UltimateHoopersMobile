using System;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Implementation of the Unit of Work pattern for managing transactions
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        // Repository instances
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private IPostRepository _postRepository;
        private IGameRepository _gameRepository;
        private ICourtRepository _courtRepository;
        private IPrivateRunRepository _privateRunRepository;
        private IProductRepository _productRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Repository properties with lazy initialization
        public IUserRepository UserRepository =>
            _userRepository ??= new UserRepository(_context);

        public IProfileRepository ProfileRepository =>
            _profileRepository ??= new ProfileRepository(_context);

        public IPostRepository PostRepository =>
            _postRepository ??= new PostRepository(_context);

        public IGameRepository GameRepository =>
            _gameRepository ??= new GameRepository(_context);

        public ICourtRepository CourtRepository =>
            _courtRepository ??= new CourtRepository(_context);

        public IPrivateRunRepository PrivateRunRepository =>
            _privateRunRepository ??= new PrivateRunRepository(_context);

        public IProductRepository ProductRepository =>
            _productRepository ??= new ProductRepository(_context);

        // Add other repositories as needed

        /// <summary>
        /// Save all changes to the database
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Begin a new transaction
        /// </summary>
        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Interface for Unit of Work pattern
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        IProfileRepository ProfileRepository { get; }
        IPostRepository PostRepository { get; }
        IGameRepository GameRepository { get; }
        ICourtRepository CourtRepository { get; }
        IPrivateRunRepository PrivateRunRepository { get; }
        IProductRepository ProductRepository { get; }

        Task<int> SaveChangesAsync();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
    }
}