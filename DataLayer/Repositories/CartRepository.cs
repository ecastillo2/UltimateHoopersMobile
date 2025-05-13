using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Cart entity operations
    /// </summary>
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get cart by ID
        /// </summary>
        public override async Task<Cart> GetByIdAsync(object id)
        {
            string cartId = id.ToString();
            return await _dbSet.FirstOrDefaultAsync(c => c.CartId == cartId);
        }

        /// <summary>
        /// Get cart by profile ID
        /// </summary>
        public async Task<Cart> GetCartByProfileIdAsync(string profileId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.ProfileId == profileId);
        }

        /// <summary>
        /// Add new cart
        /// </summary>
        public override async Task AddAsync(Cart cart)
        {
            if (string.IsNullOrEmpty(cart.CartId))
                cart.CartId = Guid.NewGuid().ToString();

            cart.CreatedDate = DateTime.Now;

            await base.AddAsync(cart);
        }
    }
}