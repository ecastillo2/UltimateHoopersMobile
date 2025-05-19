using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.DAL.Interface;
using Domain;

namespace DataLayer.DAL
{
    /// <summary>
    /// Interface for user repository operations
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">Email to search for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User with matching email</returns>
        Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if email is available (not already in use)
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if email is available, false if already in use</returns>
        Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the last login date for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateLastLoginDateAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get admin users (with Access Level = "Admin")
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of admin users</returns>
        Task<List<User>> GetAdminUsersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get users by status
        /// </summary>
        /// <param name="status">Status to filter by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of users with matching status</returns>
        Task<List<User>> GetUsersByStatusAsync(string status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new user with secure password hashing
        /// </summary>
        /// <param name="user">User to create</param>
        /// <param name="password">Plain text password (will be hashed)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created user</returns>
        Task<User> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify a user's password
        /// </summary>
        /// <param name="user">User to verify password for</param>
        /// <param name="password">Plain text password to verify</param>
        /// <returns>True if password is correct, false otherwise</returns>
        bool VerifyPassword(User user, string password);

        /// <summary>
        /// Change a user's password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newPassword">New plain text password (will be hashed)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ChangePasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default);
    }
}