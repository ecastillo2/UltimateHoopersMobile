using Domain;

namespace DataLayer.DAL
{
    public interface IPostCommentRepository : IDisposable
    {
        Task<List<PostComment>> GetPostComments(string timeZone);
        Task<PostComment> GetPostCommentById(string TagId);
        Task<List<PostComment>> GetPostCommentByPostId(string PostId,string timeZone);
        Task InsertPostComment(PostComment model);
        Task DeletePostComment(string PostCommentId); 
        Task<int> Save();

    }
}
