using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using Domain;
using Microsoft.AspNetCore.Authorization;
using DataLayer.DAL.Context;
using DataLayer.Context;
using DataLayer.DAL.Interface;
using DataLayer.DAL.Repository;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Following Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class FollowingController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IFollowingRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Following Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public FollowingController(ApplicationContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new FollowingRepository(context);

        }

        /// <summary>
        /// Get Followings
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetFollowings")]
        public async Task<List<Following>> GetFollowings()
        {

            return await repository.GetFollowings();

        }

        /// <summary>
        /// Get Following By Id
        /// </summary>
        /// <param name="followingId"></param>
        /// <returns></returns>
        [HttpGet("GetFollowingById")]
        public async Task<Following> GetFollowingById(string followingId)
        {
            try
            {
                return await repository.GetFollowingById(followingId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create Following
        /// </summary>
        /// <param name="following"></param>
        /// <returns></returns>
        [HttpPost("CreateFollowing")]
        public async Task CreateFollowing([FromBody] Following following)
        {
            
            try
            {
                  await  repository.InsertFollowing(following);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Create Following
        /// </summary>
        /// <param name="following"></param>
        /// <returns></returns>
        [HttpGet("UnFollow")]
        public async Task UnFollow(string unfollowingProfileId, string profileId)
        {

            try
            {
                await repository.UnFollow(unfollowingProfileId, profileId);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Delete Following
        /// </summary>
        /// <param name="followingId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteFollowing")]
        public async Task<HttpResponseMessage> DeleteFollowing(string followingId)
        {
            try
            {
                await repository.DeleteFollowing(followingId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteFollowing");

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
