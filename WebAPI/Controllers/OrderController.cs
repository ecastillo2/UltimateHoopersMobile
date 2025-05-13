using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Order = Domain.Order;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Order Controller
    /// </summary>
    [Route("api/[controller]")]
    //[Authorize]
    public class OrderController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IOrderRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Order Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public OrderController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new OrderRepository(context);

        }

        /// <summary>
        /// Get Orders
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetOrders")]
        //[Authorize]
        public async Task<List<Order>> GetOrders()
        {
            return await repository.GetOrders();

        }

        /// <summary>
        /// Get Order By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetOrderById")]
        public async Task<Order> GetOrderById(string orderId)
        {
            try
            {
                return await repository.GetOrderById(orderId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Order By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetOrderByProfileId")]
        public async Task<List<Order>> GetOrderByProfileId(string profileId)
        {
            try
            {
                return await repository.GetOrderByProfileId(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create Order
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpPost("CreateOrder")]
        public async Task CreateOrder([FromBody] Order order)
        {
            
            try
            {
                  await  repository.InsertOrder(order);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Update Order
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("UpdateOrder")]
        public async Task UpdateOrder([FromBody] Order order)
        {

            try
            {
                await repository.UpdateOrder(order);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Delete Order
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteOrder")]
        public async Task<HttpResponseMessage> DeleteOrder(string orderId)
        {
            try
            {
                await repository.DeleteOrder(orderId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteOrder");

                return await Task.FromResult(returnMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return await Task.FromResult(returnMessage);
        }
    }
}
