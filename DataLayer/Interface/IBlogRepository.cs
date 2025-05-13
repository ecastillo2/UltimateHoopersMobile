using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for Blog repository
    /// </summary>
    public interface IBlogRepository : IGenericRepository<Blog>
    {
        // Add any blog-specific repository methods here
    }
}
