using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for Profile repository
    /// </summary>
    public interface IProfileRepository : IGenericRepository<Profile>
    {
        Task<List<Profile>> GetFollowingProfilesAsync(string profileId);
        Task<List<Profile>> GetFollowerProfilesAsync(string profileId);
        Task<bool> IsUserNameAvailableAsync(string userName);
        Task UpdateSettingsAsync(Setting setting);
    }
}