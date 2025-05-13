using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for OrderItem repository
    /// </summary>
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
        // Add any order item-specific repository methods here
    }
}