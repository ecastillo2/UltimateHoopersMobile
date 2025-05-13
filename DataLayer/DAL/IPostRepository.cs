using Domain;

namespace DataLayer.DAL
{
    public interface IPostRepository : IDisposable
    {
        Task<List<Post>> GetPosts(string timezone);
        Task<List<Post>> GetAllPosts(string timezone);
        Task<List<Post>> GetBlogs(string timezone);
        Task<List<Post>> GetHoopNews(string timezone);
        Task<List<Post>> GetEvents(string timezone);
        Task<List<Post>> GetPublicPosts();
        Task<List<Post>> GetPostsByProfileId(string ProfileId, string timezone);
        Task<List<Post>> GetPostsMentionProfileId(string ProfileId, string timezone);
        Task<List<Post>> GetPostsWithTagByTagId(string ProfileId, string timezone);
        Task<List<Post>> GetSavedPostsByProfileId(string ProfileId, string timezone);
        Task<Post> GetPostById(string PostId, string timeZone);
        Task InsertPost(Post model);
        Task DeletePost(string PostId);
        Task UpdatePost(Post model);
        Task UpdatePostStatus(string postId, string status);
        Task<int> Save();

    }
}
