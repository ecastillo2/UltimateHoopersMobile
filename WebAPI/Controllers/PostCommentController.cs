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
    /// Post Comment Controller
    /// </summary>
    [Route("api/[controller]")]
    public class PostCommentController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IPostCommentRepository repository;        
        private readonly IConfiguration _configuration;


        /// <summary>
        /// Post Comment Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public PostCommentController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new PostCommentRepository(context);

        }

        /// <summary>
        /// Get Post Comments
        /// </summary>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        [HttpGet("GetPostComments")]
        //[Authorize]
        public async Task<List<PostComment>> GetPostComments(string timeZone)
        {

            return await repository.GetPostComments(timeZone);

        }


        /// <summary>
        /// Get Post Comment By Id
        /// </summary>
        /// <param name="postCommentId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetPostCommentById")]
        public async Task<PostComment> GetPostCommentById(string postCommentId)
        {
            try
            {
                return await repository.GetPostCommentById(postCommentId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Post Comment By PostId
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        [HttpGet("GetPostCommentByPostId")]
        //[Authorize]
        public async Task<List<PostComment>> GetPostCommentByPostId(string postId, string timeZone)
        {

            return await repository.GetPostCommentByPostId(postId, timeZone);

        }

        /// <summary>
        /// Create Post Comment
        /// </summary>
        /// <param name="postComment"></param>
        /// <returns></returns>
        [HttpPost("CreatePostComment")]
        public async Task CreatePostComment([FromBody] PostComment postComment)
        {
            
            try
            {
                  await  repository.InsertPostComment(postComment);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Delete Post Comment
        /// </summary>
        /// <param name="postCommentId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeletePostComment")]
        public async Task<HttpResponseMessage> DeletePostComment(string postCommentId)
        {
            try
            {
                await repository.DeletePostComment(postCommentId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeletePostComment");

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
