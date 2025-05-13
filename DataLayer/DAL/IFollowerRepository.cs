using Domain;

namespace DataLayer.DAL
{
    public interface IFollowerRepository : IDisposable
    {
        Task<List<Follower>> GetFollowers();
        Task<Follower> GetFollowerById(string FollowerId);
        Task InsertFollower(Follower model);
        Task DeleteFollower(string TagId); 
        Task<int> Save();

    }
}
