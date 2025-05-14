using Domain;

namespace DataLayer.DAL
{
    public interface IUserRepository : IDisposable
    {
        Task<List<User>> GetUsers();
        Task<List<User>> GetAdminUsers();
        Task<User> GetUserById(string userId);
        Task InsertUser(User user);
        Task DeleteUser(string userId);
        Task UpdateUser(User user);
        Task UpdateUserEmail(User user);
        Task UpdateName(User user);
        Task UpdatePassword(User user);
        Task UpdatePlayerName(User user);
        Task UpdateUserName(User user);
        Task UpdateSeg(User user);
        Task UpdateSubId(User user);
        Task ResetForgottenPassword(User user);
        Task PasswordReset(User user);
        Task<bool> IsEmailAvailable(string email);
        Task<User> GetUserByEmail(string email);
        Task GeneratePassword(string userId);
        Task UpdateLastLoginDate(string userId);
        Task UnActivateAccount(string userId);

        #region Followers
        Task<List<User>> GetUserFollowersByUserId(string userId);
        Task<List<User>> GetUserFollowingByUserId(string userId);
        Task StartFollowingUserId(Following following);
        Task StopFollowingUserId(Following following);
        #endregion

        Task<int> Save();

    }
}
