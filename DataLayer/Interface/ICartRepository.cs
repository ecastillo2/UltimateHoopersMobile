using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for Cart repository
    /// </summary>
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetCartByProfileIdAsync(string profileId);
    }
}