using Microsoft.AspNetCore.Mvc;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace WebAPI.Controllers
{
    /// <summary>
    /// PrivateRun Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class PrivateRunController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IPrivateRunRepository repository;        
        private readonly IConfiguration _configuration;


        /// <summary>
        /// PrivateRun Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public PrivateRunController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new PrivateRunRepository(context);

        }

        /// <summary>
        /// Get PrivateRuns
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPrivateRuns")]
        //[Authorize]
        public async Task<List<PrivateRun>> GetPrivateRuns()
        {

            return await repository.GetPrivateRuns();

        }


        /// <summary>
        /// GetProfilesByPrivateRunId
        /// </summary>
        /// <param name="privateRunId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetProfilesByPrivateRunId")]
        public async Task<List<Profile>> GetProfilesByPrivateRunId(string privateRunId)
        {
            try
            {
                return await repository.GetProfilesByPrivateRunId(privateRunId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }


        /// <summary>
        /// GetProfileInvitesByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("GetProfileInvitesByProfileId")]
        public async Task<List<PrivateRun>> GetProfileInvitesByProfileId(string profileId)
        {

            return await repository.GetProfileInvitesByProfileId(profileId);

        }

        /// <summary>
        /// Get PrivateRun By Id
        /// </summary>
        /// <param name="privateRunId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetPrivateRunById")]
        public async Task<PrivateRun> GetPrivateRunById(string privateRunId)
        {
            try
            {
                return await repository.GetPrivateRunById(privateRunId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get PrivateRuns By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("GetPrivateRunsByProfileId")]
        //[Authorize]
        public async Task<List<PrivateRun>> GetPrivateRunsByProfileIdId(string profileId)
        {

            return await repository.GetPrivateRunsByProfileId(profileId);

        }

        /// <summary>
        /// Create PrivateRun
        /// </summary>
        /// <param name="privateRun"></param>
        /// <returns></returns>
        [HttpPost("CreatePrivateRun")]
        public async Task CreatePrivateRun([FromBody] PrivateRun privateRun)
        {
            
            try
            {
                  await repository.InsertPrivateRun(privateRun);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// UpdatePrivateRun
        /// </summary>
        /// <param name="privateRun"></param>
        /// <returns></returns>
        [HttpPost("UpdatePrivateRun")]
        public async Task UpdatePrivateRun([FromBody] PrivateRun privateRun)
        {

            try
            {
                await repository.UpdatePrivateRun(privateRun);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Delete PrivateRun
        /// </summary>
        /// <param name="privateRunId"></param>
        /// <returns></returns>
        [HttpGet("RemovePrivateRun")]
        public async Task<HttpResponseMessage> RemovePrivateRun(string privateRunId)
        {
            try
            {
                await repository.RemovePrivateRun(privateRunId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "RemovePrivateRun");

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
