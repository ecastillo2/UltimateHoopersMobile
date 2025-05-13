using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Player Comment Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class PlayerCommentController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IPlayerCommentRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Player CommentController
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public PlayerCommentController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new PlayerCommentRepository(context);

        }

        /// <summary>
        /// Get Player Comments
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPlayerComments")]
        //[Authorize]
        public async Task<List<PlayerComment>> GetPlayerComments()
        {

            return await repository.GetPlayerComments();

        }


        /// <summary>
        /// Get Player Comment By Id
        /// </summary>
        /// <param name="playerCommentId"></param>
        /// <returns></returns>
        [HttpGet("GetPlayerCommentById")]
        public async Task<PlayerComment> GetPlayerCommentById(string playerCommentId)
        {
            try
            {
                return await repository.GetPlayerCommentById(playerCommentId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Player Comment By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("GetPlayerCommentByProfileId")]
        //[Authorize]
        public async Task<List<PlayerComment>> GetPlayerCommentByProfileId(string profileId,string timeZone)
        {

            return await repository.GetPlayerCommentByProfileId(profileId, timeZone);

        }

        /// <summary>
        /// Create PlayerComment
        /// </summary>
        /// <param name="playerComment"></param>
        /// <returns></returns>
        [HttpPost("CreatePlayerComment")]
        public async Task CreatePlayerComment([FromBody] PlayerComment playerComment)
        {
            
            try
            {
                  await  repository.InsertPlayerComment(playerComment);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Delete PlayerComment
        /// </summary>
        /// <param name="playerCommentId"></param>
        /// <returns></returns>
       
        [HttpDelete("DeletePlayerComment")]
        public async Task<HttpResponseMessage> DeletePlayerComment(string playerCommentId)
        {
            try
            {
                await repository.DeletePlayerComment(playerCommentId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeletePlayerComment");

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
