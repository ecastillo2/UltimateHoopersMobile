using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for Follower repository
    /// </summary>
    public interface IFollowerRepository : IGenericRepository<Follower>
    {
        Task<bool> IsFollowingAsync(string profileId, string followerProfileId);
        Task<int> GetFollowerCountAsync(string profileId);
        Task<List<Profile>> GetFollowerProfilesAsync(string profileId);
        Task FollowAsync(string profileId, string followerProfileId);
        Task UnfollowAsync(string profileId, string followerProfileId);
    }
}