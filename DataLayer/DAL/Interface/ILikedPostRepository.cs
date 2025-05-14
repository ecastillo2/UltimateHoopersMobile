using Domain;

namespace DataLayer.DAL
{
    public interface ILikedPostRepository : IDisposable
    {
        Task<List<LikedPost>> GetLikedPosts();
        Task<LikedPost> GetLikedPostById(string PostId);
        Task<List<LikedPost>> GetLikedPostByProfileId(string ProfileId);
        Task InsertLikedPost(LikedPost model);
        Task DeleteLikedPost(string PostId, string ProfileId); 
        Task<int> Save();

    }
}
