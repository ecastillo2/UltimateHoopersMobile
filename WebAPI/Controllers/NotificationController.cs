using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace API.Controllers
{
    /// <summary>
    /// Notification Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private INotificationRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Tag Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public NotificationController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new NotificationRepository(context);

        }


        /// <summary>
        /// Retrieves a list of products from the repository.
        /// </summary>
        /// <returns>A list of <see cref="Product"/> objects.</returns>
        [HttpGet("GetNotifications")]
        // Uncomment the line below to enable authorization for this endpoint
        // [Authorize]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                // Fetch the list of products from the repository
                var notifications = await repository.GetNotifications();

                // Return the products with an OK status code
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                // Log the exception (replace with your preferred logging mechanism)
                // Example: _logger.LogError(ex, "An error occurred while retrieving products.");

                // Return a generic error message with a 500 status code
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving the Notifications. Please try again later.",
                    Error = ex.Message // Consider removing this in production for security
                });
            }
        }


        /// <summary>
        /// Create Notification
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        [HttpPost("CreateNotification")]
        public async Task CreateNotification([FromBody] Notification notification)
        {
            
            try
            {
                  await  repository.InsertNotification(notification);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Get Product By Id
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>

        [HttpGet("GetNotificationById")]
        public async Task<Notification> GetNotificationById(string notificationId)
        {
            try
            {
                return await repository.GetNotificationById(notificationId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Notifications By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("GetNotificationByProfileId")]
        public async Task<List<Notification>> GetNotificationByProfileId(string profileId)
        {

            try
            {
                return await repository.GetNotificationByProfileId(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("UpdateNotification")]
        public async Task UpdateNotification([FromBody] Notification notification)
        {

            try
            {
                await repository.UpdateNotification(notification);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// UpdateNotificationRead
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        [HttpGet("UpdateNotificationRead")]
        public async Task UpdateNotificationRead(string notificationId)
        {

            try
            {
                await repository.UpdateNotificationRead(notificationId);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Delete Notification
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeleteNotification")]
        public async Task<HttpResponseMessage> DeleteNotification(string notificationId)
        {
            try
            {
                await repository.DeleteNotification(notificationId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteNotification");

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
