using WebAdmin.Models;
using WebAdmin.ViewModels;

namespace WebAdmin.Services
{
    public interface IUserService
    {
        bool ValidateClientCredentials(string email, string password);
        bool ValidateStaffCredentials(string email, string password);
        UserViewModel? GetUserByEmail(string email);
    }

    public class UserService : IUserService
    {
        // In a real application, these would be stored in a database
        private readonly List<UserViewModel> _users = new List<UserViewModel>
        {
            new UserViewModel
            {
                Id = "1",
                Name = "John Client",
                Email = "john@client.com",
                Role = "Client",
                ProfileImage = "user-1.jpg"
            },
            new UserViewModel
            {
                Id = "2",
                Name = "Jane Client",
                Email = "jane@client.com",
                Role = "Client",
                ProfileImage = "user-2.jpg"
            },
            new UserViewModel
            {
                Id = "3",
                Name = "Admin Staff",
                Email = "admin@company.com",
                Role = "Staff",
                ProfileImage = "user-3.jpg"
            },
            new UserViewModel
            {
                Id = "4",
                Name = "Support Staff",
                Email = "support@company.com",
                Role = "Staff",
                ProfileImage = "user-4.jpg"
            }
        };

        public bool ValidateClientCredentials(string email, string password)
        {
            // For demo purposes, accept any @client.com email with password "client123"
            return email.EndsWith("@client.com") && password == "client123";
        }

        public bool ValidateStaffCredentials(string email, string password)
        {
            // For demo purposes, accept any @company.com email with password "staff123"
            return email.EndsWith("@company.com") && password == "staff123";
        }

        public UserViewModel? GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}