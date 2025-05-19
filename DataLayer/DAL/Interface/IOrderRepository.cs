using Domain;

namespace DataLayer.DAL.Interface
{
    public interface IOrderRepository : IDisposable
    {
        Task<List<Order>> GetOrders();
        Task<Order> GetOrderById(string OrderId);
        Task<List<Order>> GetOrderByProfileId(string ProfileId);
        Task InsertOrder(Order model);
        Task DeleteOrder(string CourtId);
        Task UpdateOrder(Order model);
        Task<int> Save();

    }
}
