using System.Threading.Tasks;

namespace WebAPI.Services
{
    /// <summary>
    /// Interface for authentication service operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Gets the current user ID
        /// </summary>
        /// <returns>The current user ID</returns>
        string GetUserId();

        /// <summary>
        /// Gets the authentication token
        /// </summary>
        /// <returns>The authentication token</returns>
        string GetToken();

        /// <summary>
        /// Checks if the user is authenticated
        /// </summary>
        /// <returns>True if the user is authenticated, false otherwise</returns>
        bool IsAuthenticated();

        /// <summary>
        /// Gets the user's role or access level
        /// </summary>
        /// <returns>The user's role or access level</returns>
        string GetUserRole();
    }
}