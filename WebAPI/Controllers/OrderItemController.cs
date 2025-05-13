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
    /// OrderItem Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class OrderItemController : Controller
    {
        private readonly IOrderItemRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// OrderItem Controller
        /// </summary>
        /// <param name="repository">OrderItem repository</param>
        /// <param name="configuration">Configuration</param>
        public OrderItemController(IOrderItemRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get OrderItems
        /// </summary>
        /// <returns>List of order items</returns>
        [HttpGet("GetOrderItems")]
        public async Task<List<OrderItem>> GetOrderItems()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get OrderItem By Id
        /// </summary>
        /// <param name="orderItemId">OrderItem ID</param>
        /// <returns>OrderItem</returns>
        [HttpGet("GetOrderItemById")]
        public async Task<OrderItem> GetOrderItemById(string orderItemId)
        {
            try
            {
                return await _repository.GetByIdAsync(orderItemId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Get OrderItems By OrderId
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>List of order items</returns>
        [HttpGet("GetOrderItemsByOrderId")]
        public async Task<List<OrderItem>> GetOrderItemsByOrderId(string orderId)
        {
            try
            {
                return await _repository.FindAsync(oi => oi.OrderId == orderId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Create OrderItem
        /// </summary>
        /// <param name="orderItem">OrderItem to create</param>
        /// <returns>Task</returns>
        [HttpPost("CreateOrderItem")]
        public async Task<IActionResult> CreateOrderItem([FromBody] OrderItem orderItem)
        {
            try
            {
                await _repository.AddAsync(orderItem);
                await _repository.SaveAsync();
                return Ok(new { message = "OrderItem created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the order item", error = ex.Message });
            }
        }

        /// <summary>
        /// Update OrderItem
        /// </summary>
        /// <param name="orderItem">OrderItem to update</param>
        /// <returns>Result</returns>
        [HttpPost("UpdateOrderItem")]
        public async Task<IActionResult> UpdateOrderItem([FromBody] OrderItem orderItem)
        {
            try
            {
                _repository.Update(orderItem);
                await _repository.SaveAsync();
                return Ok(new { message = "OrderItem updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the order item", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete OrderItem
        /// </summary>
        /// <param name="orderItemId">OrderItem ID</param>
        /// <returns>Result</returns>
        [HttpDelete("DeleteOrderItem")]
        public async Task<IActionResult> DeleteOrderItem(string orderItemId)
        {
            try
            {
                await _repository.DeleteByIdAsync(orderItemId);
                await _repository.SaveAsync();
                return Ok(new { message = "OrderItem deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the order item", error = ex.Message });
            }
        }
    }
}