using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using Microsoft.AspNetCore.Authorization;
using Domain;
using Swashbuckle.Swagger;
using System.Diagnostics;
using Activity = Domain.Activity;
using DataLayer;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Activity Controller
    /// </summary>
    [Route("api/[controller]")]
    public class ActivityController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IActivityRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Activity Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public ActivityController(HUDBContext context, IConfiguration configuration)
        {
            this._configuration = configuration;
            this.repository = new ActivityRepository(context);

        }

        /// <summary>
        /// Get Courts
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetActivitys")]
        //[Authorize]
        public async Task<List<Activity>> GetActivitys()
        {
            return await repository.GetActivitys();

        }

        /// <summary>
        /// Create Activity
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        [HttpPost("CreateActivity")]
        public async Task CreateActivity([FromBody] Activity activity)
        {
            
            try
            {
                  await  repository.InsertActivity(activity);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetActivityById")]
        public async Task<Activity> GetActivityById(string activityId)
        {
            try
            {
                return await repository.GetActivityById(activityId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Delete Activity
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeleteActivity")]
        public async Task<HttpResponseMessage> DeleteActivity(string activityId)
        {
            try
            {
                await repository.DeleteActivity(activityId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteActivity");

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
