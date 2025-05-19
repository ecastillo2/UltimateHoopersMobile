using DataLayer.DAL.Interface;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    /// <summary>
    /// User repository interface
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
        Task UpdateLastLoginDateAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<User>> GetAdminUsersAsync(CancellationToken cancellationToken = default);
        // Add other specialized methods as needed
    }

    /// <summary>
    /// Implementation of the user repository
    /// </summary>
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(HUDBContext context, ILogger<UserRepository> logger = null)
            : base(context)
        {
            _logger = logger;
        }

        public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                return !await _dbSet
                    .AnyAsync(u => u.Email == email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking email availability: {Email}", email);
                throw;
            }
        }

        public async Task UpdateLastLoginDateAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);

                if (user != null)
                {
                    user.LastLoginDate = DateTime.Now.ToString();
                    await SaveAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating last login date for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<User>> GetAdminUsersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Where(u => u.AccessLevel == "Admin")
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting admin users");
                throw;
            }
        }
    }
}