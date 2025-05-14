using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace WebAPI.Controllers
{
    /// <summary>
    /// ThirdPartyService Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class ThirdPartyServiceController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IThirdPartyServiceRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// ThirdPartyService Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public ThirdPartyServiceController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new ThirdPartyServiceRepository(context);

        }

        /// <summary>
        /// Get ThirdPartyServices
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetThirdPartyServices")]
        //[Authorize]
        public async Task<List<ThirdPartyService>> GetThirdPartyServices()
        {
            return await repository.GetThirdPartyServices();

        }

        /// <summary>
        /// Get ThirdPartyService By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetThirdPartyServiceById")]
        public async Task<ThirdPartyService> GetThirdPartyServiceById(string thirdPartyServiceId)
        {
            try
            {
                return await repository.GetThirdPartyServiceById(thirdPartyServiceId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create ThirdPartyService
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpPost("CreateThirdPartyService")]
        public async Task CreateThirdPartyService([FromBody] ThirdPartyService thirdPartyService)
        {
            
            try
            {
                  await  repository.InsertThirdPartyService(thirdPartyService);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("UpdateThirdPartyService")]
        public async Task UpdateThirdPartyService([FromBody] ThirdPartyService thirdPartyService)
        {

            try
            {
                await repository.UpdateThirdPartyService(thirdPartyService);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Delete ThirdPartyService
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteThirdPartyService")]
        public async Task<HttpResponseMessage> DeleteThirdPartyService(string thirdPartyServiceId)
        {
            try
            {
                await repository.DeleteThirdPartyService(thirdPartyServiceId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteThirdPartyService");

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
