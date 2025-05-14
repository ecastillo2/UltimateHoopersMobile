// Services/IAuthService.cs
// Services/IAuthService.cs
using Domain;
using System.Threading.Tasks;

namespace UltimateHoopers.Services
{
    public interface IAuthService
    {
        Task<User> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetTokenAsync();
    }
}