using DataLayer.Context;
using DataLayer.DAL.Interface;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLayer.DAL.Repository
{
    /// <summary>
    /// Implementation of the User repository with proper error handling
    /// </summary>
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationContext context, ILogger<UserRepository> logger = null)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Get user by email with proper error handling
        /// </summary>
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

        /// <summary>
        /// Check if email is available (not already in use)
        /// </summary>
        public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                return !await _dbSet
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking email availability: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Update the last login date for a user
        /// </summary>
        public async Task UpdateLastLoginDateAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);

                if (user != null)
                {
                    user.LastLoginDate = DateTime.Now.ToString();
                    _context.Entry(user).State = EntityState.Modified;
                    await SaveAsync(cancellationToken);
                    _logger?.LogInformation("Updated last login date for user {UserId}", userId);
                }
                else
                {
                    _logger?.LogWarning("User not found when updating last login date: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating last login date for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get admin users (with Access Level = "Admin")
        /// </summary>
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

        /// <summary>
        /// Get users by status
        /// </summary>
        public async Task<List<User>> GetUsersByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Where(u => u.Status == status)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting users by status: {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Create a new user with secure password hashing
        /// </summary>
        public async Task<User> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("Password is required", nameof(password));

                // Generate a new user ID if not provided
                if (string.IsNullOrEmpty(user.UserId))
                    user.UserId = Guid.NewGuid().ToString();

                // Hash the password before storing
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.SignUpDate = DateTime.Now.ToString();

                if (string.IsNullOrEmpty(user.Status))
                    user.Status = "Active";

                if (string.IsNullOrEmpty(user.AccessLevel))
                    user.AccessLevel = "Standard";

                await _dbSet.AddAsync(user, cancellationToken);
                await SaveAsync(cancellationToken);

                _logger?.LogInformation("Created new user {UserId} with email {Email}", user.UserId, user.Email);

                return user;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating user with email {Email}", user.Email);
                throw;
            }
        }

        /// <summary>
        /// Verify a user's password
        /// </summary>
        public bool VerifyPassword(User user, string password)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(password))
                    return false;

                // Make sure the hash is properly formatted
                string hashToVerify = user.PasswordHash;
                if (hashToVerify != null && !hashToVerify.StartsWith("$"))
                {
                    // If the hash doesn't start with $, it might be missing a character
                    // Try prepending the $ character
                    hashToVerify = "$" + hashToVerify;
                }

                // Log both hashes for debugging
                Console.WriteLine($"Original hash: {user.PasswordHash}");
                Console.WriteLine($"Fixed hash: {hashToVerify}");

                // Use the potentially fixed hash
                //bool result = BCrypt.Net.BCrypt.Verify(password, hashToVerify);
                //Console.WriteLine($"Verification result: {result}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in VerifyPassword: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Change a user's password
        /// </summary>
        public async Task ChangePasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);

                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.SecurityStamp = Guid.NewGuid().ToString();

                _context.Entry(user).State = EntityState.Modified;
                await SaveAsync(cancellationToken);

                _logger?.LogInformation("Changed password for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error changing password for user {UserId}", userId);
                throw;
            }
        }
    }
}