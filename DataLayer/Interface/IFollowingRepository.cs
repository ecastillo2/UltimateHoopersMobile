using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for Following repository
    /// </summary>
    public interface IFollowingRepository : IGenericRepository<Following>
    {
        Task<bool> IsFollowingAsync(string profileId, string followingProfileId);
        Task<int> GetFollowingCountAsync(string profileId);
        Task<List<Profile>> GetFollowingProfilesAsync(string profileId);
        Task FollowAsync(string profileId, string followingProfileId);
        Task UnfollowAsync(string profileId, string followingProfileId);
    }
}