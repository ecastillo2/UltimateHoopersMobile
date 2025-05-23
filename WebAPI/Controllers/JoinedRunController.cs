using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using Domain;
using Microsoft.AspNetCore.Authorization;
using DataLayer.DAL.Context;
using DataLayer.Context;
using DataLayer.DAL.Repository;
using DataLayer.DAL.Interface;

namespace WebAPI.Controllers
{
    /// <summary>
    /// PrivateRunInvite Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class JoinedRunController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IJoinedRunRepository repository;        
        private readonly IConfiguration _configuration;


        /// <summary>
        /// PrivateRun Invite Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public JoinedRunController(ApplicationContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new JoinedRunRepository(context);

        }

        /// <summary>
        /// Get PrivateRunInvites
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetJoinedRuns")]
        public async Task<List<JoinedRun>> GetJoinedRuns()
        {

            return await repository.GetJoinedRuns();

        }


        /// <summary>
        /// GetPrivateRunInviteById
        /// </summary>
        /// <param name="privateRunInviteId"></param>
        /// <returns></returns>
        [HttpGet("GetJoinedRunById")]
        public async Task<JoinedRun> GetJoinedRunById(string joinedRunId)
        {
            try
            {
                return await repository.GetJoinedRunById(joinedRunId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get PrivateRunInvites By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("GetJoinedRunsByProfileId")]
        //[Authorize]
        public async Task<List<JoinedRun>> GetJoinedRunsByProfileIdId(string profileId)
        {

            return await repository.GetJoinedRunsByProfileId(profileId);

        }

        /// <summary>
        /// Get PrivateRunInvites By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("IsProfileIdIdAlreadyInvitedToRunInJoinedRuns")]
        //[Authorize]
        public async Task<bool> IsProfileIdIdAlreadyInvitedToRunInJoinedRuns(string profileId, string privateRunId)
        {

            return await repository.IsProfileIdIdAlreadyInvitedToRunInJoinedRuns(profileId, privateRunId);

        }


        /// <summary>
        /// Get PrivateRunInvites By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("UpdatePlayerJoinedRun")]
        //[Authorize]
        public async Task UpdatePlayerJoinedRun(string profileId, string joinedRunId, string acceptedInvite )
        {
            try
            {
                await repository.UpdatePlayerJoinedRun(profileId, joinedRunId, acceptedInvite);
            }
            catch (Exception ex)
            {
                var x = ex;
            }
        }


        /// <summary>
        /// Get JoinedRuns By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("UpdatePlayerPresentJoinedRun")]
        //[Authorize]
        public async Task UpdatePlayerPresentJoinedRun(string profileId, string privateRunId, bool present)
        {
            try
            {
                await repository.UpdatePlayerPresentJoinedRun(profileId, privateRunId, present);
            }
            catch (Exception ex)
            {
                var x = ex;
            }
        }


        /// <summary>
        /// Get PrivateRunInvites By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("RemoveProfileFromRun")]
        //[Authorize]
        public async Task<IActionResult> RemoveProfileFromRun(string profileId, string runId)
        {
            try
            {
                bool result = await repository.RemoveProfileFromRun(profileId, runId);
                if (result)
                {
                    return Ok(new { success = true, message = "Profile removed successfully." });
                }
                return NotFound(new { success = false, message = "Profile not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred.", error = ex.Message });
            }
        }



        /// <summary>
        /// Create PrivateRun
        /// </summary>
        /// <param name="privateRun"></param>
        /// <returns></returns>
        [HttpPost("CreateJoinedRun")]
        public async Task CreateJoinedRun([FromBody] JoinedRun joinedRun)
        {
            
            try
            {
                  await  repository.InsertJoinedRun(joinedRun);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Delete PrivateRun
        /// </summary>
        /// <param name="privateRunId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteJoinedRun")]
        public async Task<HttpResponseMessage> DeleteJoinedRun(string joinedRunId)
        {
            try
            {
                await repository.DeleteJoinedRun(joinedRunId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteJoinedRun");

                return await Task.FromResult(returnMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return await Task.FromResult(returnMessage);
        }




        /// <summary>
        /// Delete PrivateRun
        /// </summary>
        /// <param name="privateRunId"></param>
        /// <returns></returns>
        [HttpGet("ClearRunInviteByRun")]
        public async Task<HttpResponseMessage> ClearRunInviteByRun(string runId)
        {
            try
            {
                await repository.ClearJoinedRunByRun(runId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "ClearRunInviteByRun");

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
