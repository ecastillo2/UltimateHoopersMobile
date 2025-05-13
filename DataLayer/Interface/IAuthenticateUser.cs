using Domain;
using System.Threading.Tasks;

namespace ApiClient.Authentication
{
    /// <summary>
    /// Interface for user authentication
    /// </summary>
    public interface IAuthenticateUser
    {
        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        /// <param name="email">User's email</param>
        /// <param name="password">User's password</param>
        /// <returns>Authenticated user if successful, null otherwise</returns>
        Task<User> AuthenticateAsync(string email, string password);
    }
}