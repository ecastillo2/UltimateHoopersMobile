using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for PostComment repository
    /// </summary>
    public interface IPostCommentRepository : IGenericRepository<PostComment>
    {
        Task<List<PostComment>> GetCommentsByPostIdAsync(string postId, string timeZone);
        Task<int> GetCommentCountAsync(string postId);
    }
}