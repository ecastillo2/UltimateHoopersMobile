using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for User entity operations
    /// </summary>
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get all users with profiles
        /// </summary>
        public override async Task<List<User>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.Profile)
                .ToListAsync();
        }

        /// <summary>
        /// Get user by ID with profile
        /// </summary>
        public override async Task<User> GetByIdAsync(object id)
        {
            string userId = id.ToString();
            return await _dbSet
                .Include(u => u.Profile)
                    .ThenInclude(p => p.Setting)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Profile)
                    .ThenInclude(p => p.Setting)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Check if email is available (not already used)
        /// </summary>
        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return !await _dbSet.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Update user's last login date
        /// </summary>
        public async Task UpdateLastLoginDateAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginDate = DateTime.Now.ToString();
                Update(user);
                await SaveAsync();
            }
        }

        /// <summary>
        /// Update user password
        /// </summary>
        public async Task UpdatePasswordAsync(string userId, string newPasswordHash)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.PasswordHash = newPasswordHash;
                Update(user);
                await SaveAsync();
            }
        }

        /// <summary>
        /// Get users with admin access level
        /// </summary>
        public async Task<List<User>> GetAdminUsersAsync()
        {
            return await _dbSet
                .Where(u => u.AccessLevel == "Admin")
                .Include(u => u.Profile)
                .ToListAsync();
        }

        /// <summary>
        /// Deactivate user account
        /// </summary>
        public async Task DeactivateAccountAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.Status = "Inactive";
                Update(user);
                await SaveAsync();
            }
        }

        /// <summary>
        /// Get user followers
        /// </summary>
        public async Task<List<User>> GetFollowersAsync(string userId)
        {
            return await _context.Followers
                .Where(f => f.UserId == userId)
                .Join(_context.User,
                    follower => follower.FollowerUserId,
                    user => user.UserId,
                    (follower, user) => user)
                .ToListAsync();
        }

        /// <summary>
        /// Get users that this user is following
        /// </summary>
        public async Task<List<User>> GetFollowingAsync(string userId)
        {
            return await _context.Following
                .Where(f => f.UserId == userId)
                .Join(_context.User,
                    following => following.FollowingUserId,
                    user => user.UserId,
                    (following, user) => user)
                .ToListAsync();
        }
    }

    /// <summary>
    /// Interface for User repository
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> IsEmailAvailableAsync(string email);
        Task UpdateLastLoginDateAsync(string userId);
        Task UpdatePasswordAsync(string userId, string newPasswordHash);
        Task<List<User>> GetAdminUsersAsync();
        Task DeactivateAccountAsync(string userId);
        Task<List<User>> GetFollowersAsync(string userId);
        Task<List<User>> GetFollowingAsync(string userId);
    }
}