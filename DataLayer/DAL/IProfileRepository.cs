using Domain;

namespace DataLayer.DAL
{
    public interface IProfileRepository : IDisposable
    {
        Task<List<Profile>> GetProfiles();
        Task<List<Profile>> GetFollowingProfilesByProfileId(string ProfileId);
        Task<List<Profile>> GetFollowerProfilesByProfileId(string ProfileId);
        Task<Profile> GetProfileById(string ProfileId);
        Task<List<Game>> GetProfileGameHistory(string ProfileId);
        Task UpdateProfile(Profile model);
        Task UpdateWinnerPoints(string ProfileId);
        Task UpdateSetProfileWithBestRecord(string ProfileId);
        Task UpdateSetProfileWithBestRecordToFalse(string ProfileId);
        Task UpdateLastRunDate(string ProfileId, string LastRunDate);
        Task UpdateProfileUserName(Profile model);
        Task UpdateSetting(Setting model);
        Task<bool> IsUserNameAvailable(string UserName);
        Task<bool> IsEmailAvailable(string email);
        Task<int> Save();

    }
}
