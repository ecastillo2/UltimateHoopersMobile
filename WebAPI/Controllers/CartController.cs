using Microsoft.AspNetCore.Mvc;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Repositories;
using System;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Cart Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Cart Controller
        /// </summary>
        /// <param name="repository">Cart repository</param>
        /// <param name="configuration">Configuration</param>
        public CartController(ICartRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get Carts
        /// </summary>
        /// <returns>List of carts</returns>
        [HttpGet("GetCarts")]
        public async Task<List<Cart>> GetCarts()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get Cart By Id
        /// </summary>
        /// <param name="cartId">Cart ID</param>
        /// <returns>Cart</returns>
        [HttpGet("GetCartById")]
        public async Task<Cart> GetCartById(string cartId)
        {
            try
            {
                return await _repository.GetByIdAsync(cartId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Get Cart By Profile Id
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <returns>Cart</returns>
        [HttpGet("GetCartByProfileId")]
        public async Task<Cart> GetCartByProfileId(string profileId)
        {
            try
            {
                return await _repository.GetCartByProfileIdAsync(profileId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Create Cart
        /// </summary>
        /// <param name="cart">Cart to create</param>
        /// <returns>Task</returns>
        [HttpPost("CreateCart")]
        public async Task<IActionResult> CreateCart([FromBody] Cart cart)
        {
            try
            {
                await _repository.AddAsync(cart);
                await _repository.SaveAsync();
                return Ok(new { message = "Cart created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Cart
        /// </summary>
        /// <param name="cart">Cart to update</param>
        /// <returns>Result</returns>
        [HttpPost("UpdateCart")]
        public async Task<IActionResult> UpdateCart([FromBody] Cart cart)
        {
            try
            {
                _repository.Update(cart);
                await _repository.SaveAsync();
                return Ok(new { message = "Cart updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Cart
        /// </summary>
        /// <param name="cartId">Cart ID</param>
        /// <returns>Result</returns>
        [HttpDelete("DeleteCart")]
        public async Task<IActionResult> DeleteCart(string cartId)
        {
            try
            {
                await _repository.DeleteByIdAsync(cartId);
                await _repository.SaveAsync();
                return Ok(new { message = "Cart deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the cart", error = ex.Message });
            }
        }
    }
}