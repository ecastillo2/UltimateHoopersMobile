using Domain;

namespace DataLayer.DAL
{
    public interface IFollowingRepository : IDisposable
    {
        Task<List<Following>> GetFollowings();
        Task<Following> GetFollowingById(string FollowingId);
        Task UnFollow(string FollowingId, string ProfileId);
        Task InsertFollowing(Following model);
        Task DeleteFollowing(string TagId); 
        Task<int> Save();

    }
}
