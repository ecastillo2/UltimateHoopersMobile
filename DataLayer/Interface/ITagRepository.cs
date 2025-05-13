using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for Tag repository
    /// </summary>
    public interface ITagRepository : IGenericRepository<Tag>
    {
        Task<List<Tag>> GetPopularTagsAsync(int count = 10);
        Task<List<Post>> GetPostsByTagIdAsync(string tagId);
    }
}