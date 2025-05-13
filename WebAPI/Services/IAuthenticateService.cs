using Domain;
using System.Threading.Tasks;

namespace WebAPI.Services
{
    /// <summary>
    /// IAuthenticate Service
    /// </summary>
    public interface IAuthenticateService
    {
        /// <summary>
        /// Authenticate a user with credentials or token
        /// </summary>
        /// <param name="authToken">Authentication token (optional)</param>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <returns>Authenticated user or null if authentication fails</returns>
        Task<User> Authenticate(string authToken, string email, string password);

        /// <summary>
        /// Hash a password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hashed password</returns>
        string HashPassword(string password);
    }
}