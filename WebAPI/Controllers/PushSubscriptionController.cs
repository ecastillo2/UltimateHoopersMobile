using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.Swagger;
using WebPush;
using System.Text.Json;

namespace API.Controllers
{
    /// <summary>
    /// Notification Controller
    /// </summary>
    [Route("api/[controller]")]
    public class PushController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IPushSubscriptionRepository repository;
        private readonly IConfiguration _configuration;
  
        /// <summary>
        /// Tag Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public PushController(HUDBContext context, IConfiguration configuration)
        {

            this._configuration = configuration;
            this.repository = new PushSubscriptionRepository(context, configuration);

        }

        [HttpPost("Subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] Domain.PushSubscription subscription)
        {
            try
            {
                await repository.Subscribe(subscription);
            }
            catch (Exception ex)
            {
                var x = ex;
            }
            return Ok();
        }

        [HttpPost("SendNotification")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationMessage message, [FromQuery] string userId)
        {
            try
            {
                 await repository.SendNotification(message, userId);
            }
            catch (Exception ex)
            {
                var x = ex;
            }
            return Ok();
        }

    }
}
