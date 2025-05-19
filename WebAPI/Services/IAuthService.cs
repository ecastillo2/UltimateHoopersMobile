using Domain;
using System.Threading.Tasks;

namespace WebAPI.Services
{
    /// <summary>
    /// Interface for authentication service operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user using token or email/password combination
        /// </summary>
        /// <param name="token">JWT token for token-based authentication</param>
        /// <param name="email">Email for credential-based authentication</param>
        /// <param name="password">Password for credential-based authentication</param>
        /// <returns>Authenticated user if successful, null otherwise</returns>
        Task<User> Authenticate(string token, string email, string password);
    }
}