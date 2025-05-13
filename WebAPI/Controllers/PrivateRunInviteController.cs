using Microsoft.AspNetCore.Mvc;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    /// <summary>
    /// PrivateRunInvite Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class PrivateRunInviteController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IPrivateRunInviteRepository repository;        
        private readonly IConfiguration _configuration;


        /// <summary>
        /// PrivateRun Invite Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public PrivateRunInviteController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new PrivateRunInviteRepository(context);

        }

        /// <summary>
        /// Get PrivateRunInvites
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPrivateRunInvites")]
        public async Task<List<PrivateRunInvite>> GetPrivateRunInvites()
        {

            return await repository.GetPrivateRunInvites();

        }


        /// <summary>
        /// GetPrivateRunInviteById
        /// </summary>
        /// <param name="privateRunInviteId"></param>
        /// <returns></returns>
        [HttpGet("GetPrivateRunInviteById")]
        public async Task<PrivateRunInvite> GetPrivateRunInviteById(string privateRunInviteId)
        {
            try
            {
                return await repository.GetPrivateRunInviteById(privateRunInviteId);
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
        [HttpGet("GetPrivateRunInvitesByProfileId")]
        //[Authorize]
        public async Task<List<PrivateRunInvite>> GetPrivateRunInvitesByProfileIdId(string profileId)
        {

            return await repository.GetPrivateRunInvitesByProfileId(profileId);

        }

        /// <summary>
        /// Get PrivateRunInvites By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("IsProfileIdIdAlreadyInvitedToRunInPrivateRunInvites")]
        //[Authorize]
        public async Task<bool> IsProfileIdIdAlreadyInvitedToRunInPrivateRunInvites(string profileId, string privateRunId)
        {

            return await repository.IsProfileIdIdAlreadyInvitedToRunInPrivateRunInvites(profileId, privateRunId);

        }


        /// <summary>
        /// Get PrivateRunInvites By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("UpdatePlayerPrivateRunInvite")]
        //[Authorize]
        public async Task UpdatePlayerPrivateRunInvite(string profileId, string privateRunInviteId, string acceptedInvite )
        {
            try
            {
                await repository.UpdatePlayerPrivateRunInvite(profileId, privateRunInviteId, acceptedInvite);
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
        [HttpGet("UpdatePlayerPresentPrivateRunInvite")]
        //[Authorize]
        public async Task UpdatePlayerPresentPrivateRunInvite(string profileId, string privateRunId, bool present)
        {
            try
            {
                await repository.UpdatePlayerPresentPrivateRunInvite(profileId, privateRunId, present);
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
        [HttpGet("RemoveProfileFromPrivateRun")]
        //[Authorize]
        public async Task<IActionResult> RemoveProfileFromPrivateRun(string profileId, string privateRunId)
        {
            try
            {
                bool result = await repository.RemoveProfileFromPrivateRun(profileId, privateRunId);
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
        [HttpPost("CreatePrivateRunInvite")]
        public async Task CreatePrivateRunInvite([FromBody] PrivateRunInvite privateRunInvite)
        {
            
            try
            {
                  await  repository.InsertPrivateRunInvite(privateRunInvite);
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
        [HttpDelete("DeletePrivateRunInvite")]
        public async Task<HttpResponseMessage> DeletePrivateRunInvite(string privateRunInviteId)
        {
            try
            {
                await repository.DeletePrivateRunInvite(privateRunInviteId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeletePrivateRunInvite");

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
        [HttpGet("ClearPrivateRunInviteByPrivateRun")]
        public async Task<HttpResponseMessage> ClearPrivateRunInviteByPrivateRun(string PrivateRunId)
        {
            try
            {
                await repository.ClearPrivateRunInviteByPrivateRun(PrivateRunId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "ClearPrivateRunInviteByPrivateRun");

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
