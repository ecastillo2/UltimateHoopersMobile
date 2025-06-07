using DataLayer.DAL.Interface;
using Domain;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for user repository operations
    /// </summary>
    public interface IUserRepository : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Get all PrivateRuns
        /// </summary>
        Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all PrivateRuns
        /// </summary>
        Task<IList<User>> GetUsersSearchAsync(string searchQuery,  CancellationToken cancellationToken = default);



        /// <summary>
        /// Get PrivateRuns with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "Points", "PlayerNumber", "Status")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Courts and the next cursor value</returns>
        Task<(List<User> Users, string NextCursor)> GetUsersWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all PrivateRuns for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<User> StreamAllUsersAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<User> GetUserByIdAsync(
            string privateRunId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<Profile> GetProfileByUserId(
            string privateRunId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<ScoutingReport> GetProfileScoutingReportByUserId(
            string privateRunId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a profile
        /// </summary>
        Task<bool> UpdateUserAsync(
            User privateRun,
            CancellationToken cancellationToken = default);

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
        /// Update the last login date for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateLastLoginDateAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}