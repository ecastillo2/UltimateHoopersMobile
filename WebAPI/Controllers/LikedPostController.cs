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
    /// LikedPost Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class LikedPostController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private ILikedPostRepository repository;        
        private readonly IConfiguration _configuration;


        /// <summary>
        /// LikedPost Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public LikedPostController(ApplicationContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new LikedPostRepository(context);

        }

        /// <summary>
        /// Get LikedPosts
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLikedPosts")]
        //[Authorize]
        public async Task<List<LikedPost>> GetLikedPosts()
        {

            return await repository.GetLikedPosts();

        }

        /// <summary>
        /// Get LikedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetLikedPostById")]
        public async Task<LikedPost> GetLikedPostById(string postId)
        {
            try
            {
                return await repository.GetLikedPostById(postId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get LikedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetLikedPostByProfileId")]
        public async Task<List<LikedPost>> GetLikedPostByProfileId(string profileId)
        {
            try
            {
                return await repository.GetLikedPostByProfileId(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create LikedPost
        /// </summary>
        /// <param name="savedPost"></param>
        /// <returns></returns>
        [HttpPost("CreateLikedPost")]
        public async Task CreateLikedPost([FromBody] LikedPost likedPost)
        {
            
            try
            {
                  await  repository.InsertLikedPost(likedPost);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Delete LikedPost
        /// </summary>
        /// <param name="savedPostId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteLikedPost")]
        public async Task<HttpResponseMessage> DeleteLikedPost(string postId, string profileId)
        {
            try
            {
                await repository.DeleteLikedPost(postId, profileId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteLikedPost");

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
