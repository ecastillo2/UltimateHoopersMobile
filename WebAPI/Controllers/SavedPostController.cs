using Microsoft.AspNetCore.Mvc;
using Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    /// <summary>
    /// SavedPost Controller
    /// </summary>
     [Authorize]
    [Route("api/[controller]")]
    public class SavedPostController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private ISavedPostRepository repository;        
        private readonly IConfiguration _configuration;


        /// <summary>
        /// SavedPost Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public SavedPostController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new SavedPostRepository(context);

        }

        /// <summary>
        /// Get SavedPosts
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSavedPosts")]
        public async Task<List<SavedPost>> GetSavedPosts()
        {

            return await repository.GetSavedPosts();

        }


        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("GetSavedPostById")]
        public async Task<SavedPost> GetSavedPostById(string postId)
        {
            try
            {
                return await repository.GetSavedPostById(postId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("GetSavedPostByProfileId")]
        public async Task<List<SavedPost>> GetSavedPostByProfileId(string profileId)
        {
            try
            {
                return await repository.GetSavedPostByProfileId(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create SavedPost
        /// </summary>
        /// <param name="savedPost"></param>
        /// <returns></returns>
        [HttpPost("CreateSavedPost")]
        public async Task CreateSavedPost([FromBody] SavedPost savedPost)
        {
            
            try
            {
                  await  repository.InsertSavedPost(savedPost);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Delete SavedPost
        /// </summary>
        /// <param name="savedPostId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteSavedPost")]
        public async Task<HttpResponseMessage> DeleteSavedPost(string postId, string profileId)
        {
            try
            {
                await repository.DeleteSavedPost(postId, profileId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteSavedPost");

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
