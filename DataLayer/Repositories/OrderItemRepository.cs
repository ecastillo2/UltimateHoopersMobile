using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for OrderItem entity operations
    /// </summary>
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get order item by ID
        /// </summary>
        public override async Task<OrderItem> GetByIdAsync(object id)
        {
            string orderItemId = id.ToString();
            return await _dbSet.FirstOrDefaultAsync(oi => oi.OrderItemId == orderItemId);
        }

        /// <summary>
        /// Add new order item
        /// </summary>
        public override async Task AddAsync(OrderItem orderItem)
        {
            if (string.IsNullOrEmpty(orderItem.OrderItemId))
                orderItem.OrderItemId = Guid.NewGuid().ToString();

            await base.AddAsync(orderItem);
        }
    }
}
