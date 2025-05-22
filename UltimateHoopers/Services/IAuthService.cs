using Domain;
using System.Threading.Tasks;

namespace UltimateHoopers.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <param name="password">The user's password</param>
        /// <returns>The authenticated user if successful</returns>
        /// <exception cref="ApiClient.Authentication.AuthenticationException">Thrown when authentication fails</exception>
        Task<User> LoginAsync(string email, string password);

        /// <summary>
        /// Logs out the current user
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Checks if the user is authenticated
        /// </summary>
        /// <returns>True if the user is authenticated, false otherwise</returns>
        Task<bool> IsAuthenticatedAsync();

        /// <summary>
        /// Gets the current authentication token
        /// </summary>
        /// <returns>The current token, or null if not authenticated</returns>
        Task<string> GetTokenAsync();

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <param name="username">The user's username</param>
        /// <param name="fullName">The user's full name</param>
        /// <param name="password">The user's password</param>
        /// <param name="accountType">The type of account (Free or Host)</param>
        /// <returns>True if registration was successful</returns>
        Task<bool> RegisterAsync(string email, string username, string fullName, string password, AccountType accountType);

        /// <summary>
        /// Checks if the current user has a Host account
        /// </summary>
        /// <returns>True if the user is a host, false otherwise</returns>
        Task<bool> IsUserHostAsync();

        /// <summary>
        /// Upgrades the current user's account to Host account
        /// </summary>
        /// <returns>True if upgrade was successful</returns>
        Task<bool> UpgradeToHostAccountAsync();
    }
}