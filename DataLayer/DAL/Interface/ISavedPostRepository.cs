using Domain;

namespace DataLayer.DAL.Interface
{
    public interface ISavedPostRepository : IDisposable
    {
        Task<List<SavedPost>> GetSavedPosts();
        Task<SavedPost> GetSavedPostById(string PostId);
        Task<List<SavedPost>> GetSavedPostByProfileId(string ProfileId);
        Task InsertSavedPost(SavedPost model);
        Task DeleteSavedPost(string PostId, string ProfileId); 
        Task<int> Save();
    }
}
