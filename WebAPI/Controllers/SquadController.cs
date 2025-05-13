using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Domain;
using Swashbuckle.Swagger;
using System.Diagnostics;
using Activity = Domain.Activity;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Activity Controller
    /// </summary>
    [Route("api/[controller]")]
    public class SquadController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private ISquadRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Activity Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public SquadController(HUDBContext context, IConfiguration configuration)
        {
            this._configuration = configuration;
            this.repository = new SquadRepository(context);

        }

        /// <summary>
        /// Get Courts
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSquads")]
        [Authorize]
        public async Task<List<Squad>> GetSquads()
        {
            return await repository.GetSquads();

        }
        /// <summary>
        /// Get Courts
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSquadTeams")]
        //[Authorize]
        public async Task<List<SquadTeam>> GetSquadTeams()
        {
            return await repository.GetSquadTeams();

        }


        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetSquadByOwnerProfileId")]
        public async Task<Squad> GetSquadByOwnerProfileId(string profileId)
        {
            try
            {
                return await repository.GetSquadByOwnerProfileId(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetPendingRequestsProfileById")]
        public async Task<List<SquadTeam>> GetPendingRequestsProfileById(string profileId)
        {
            try
            {
                return await repository.GetPendingRequestsProfileById(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("AddPlayerToSquad")]
        public async Task<string> AddPlayerToSquad(string profileId, string squadId)
        {
            try
            {
                 return await repository.AddPlayerToSquad(profileId, squadId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("SendPlayerRequestToJoinSquad")]
        public async Task SendPlayerRequestToJoinSquad(string profileId, string squadId)
        {
            try
            {
                await repository.SendPlayerRequestToJoinSquad(profileId, squadId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RemovePlayerFromSquad")]
        public async Task<string> RemovePlayerFromSquad(string profileId, string squadId)
        {
            try
            {
                return await repository.RemovePlayerFromSquad(profileId, squadId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        
    }
}
