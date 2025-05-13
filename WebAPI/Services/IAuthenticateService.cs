
using Domain;

namespace WebAPI.Services
{
    /// <summary>
    /// IAuthenticate Service
    /// </summary>
    public interface IAuthenticateService
    {

        /// <summary>
        /// SocialMedia Authenticate
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        User SocialMediaAuthenticate(string authToken, string email, string password);
        
    }
}
