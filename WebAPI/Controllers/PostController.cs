using System.Net;
using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;
//using static Org.BouncyCastle.Math.EC.ECCurve;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Post Controller
    /// </summary>
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IPostRepository repository;        
        private readonly IConfiguration _configuration;
        
        /// <summary>
        /// PostController
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public PostController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new PostRepository(context, _configuration);
        }


        /// <summary>
        /// Retrieves a list of posts based on the specified time zone.
        /// </summary>
        /// <param name="timeZone">The time zone to filter or adjust post data.</param>
        /// <returns>A list of <see cref="Post"/> objects.</returns>
        [HttpGet("GetPosts")]
    
        public async Task<IActionResult> GetPosts(string timeZone)
        {
            try
            {
                // Fetch the list of posts from the repository
                var posts = await repository.GetPosts(timeZone);

                // Return the posts with an OK status code
                return Ok(posts);
            }
            catch (Exception ex)
            {
                // Log the exception (replace with your preferred logging mechanism)
                // Example: _logger.LogError(ex, "An error occurred while retrieving posts.");

                // Return a generic error message with a 500 status code
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving the posts. Please try again later.",
                    Error = ex.Message // Consider removing this in production for security
                });
            }
        }

        /// <summary>
        /// Retrieves a list of posts based on the specified time zone.
        /// </summary>
        /// <param name="timeZone">The time zone to filter or adjust post data.</param>
        /// <returns>A list of <see cref="Post"/> objects.</returns>
        [HttpGet("GetBlogs")]

        public async Task<IActionResult> GetBlogs(string timeZone)
        {
            try
            {
                // Fetch the list of posts from the repository
                var posts = await repository.GetBlogs(timeZone);

                // Return the posts with an OK status code
                return Ok(posts);
            }
            catch (Exception ex)
            {
                // Log the exception (replace with your preferred logging mechanism)
                // Example: _logger.LogError(ex, "An error occurred while retrieving posts.");

                // Return a generic error message with a 500 status code
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving the posts. Please try again later.",
                    Error = ex.Message // Consider removing this in production for security
                });
            }
        }

        /// <summary>
        /// Retrieves a list of posts based on the specified time zone.
        /// </summary>
        /// <param name="timeZone">The time zone to filter or adjust post data.</param>
        /// <returns>A list of <see cref="Post"/> objects.</returns>
        [HttpGet("GetNews")]

        public async Task<IActionResult> GetNews(string timeZone)
        {
            try
            {
                // Fetch the list of posts from the repository
                var posts = await repository.GetHoopNews(timeZone);

                // Return the posts with an OK status code
                return Ok(posts);
            }
            catch (Exception ex)
            {
                // Log the exception (replace with your preferred logging mechanism)
                // Example: _logger.LogError(ex, "An error occurred while retrieving posts.");

                // Return a generic error message with a 500 status code
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving the posts. Please try again later.",
                    Error = ex.Message // Consider removing this in production for security
                });
            }
        }


        /// <summary>
        /// Get Posts
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPublicPosts")]
        public async Task<List<Post>> GetPublicPosts()
        {
            return await repository.GetPublicPosts();

        }

        /// <summary>
        /// GetPostsMentionProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        [HttpGet("GetPostsMentionProfileId")]
        [Authorize]
        public async Task<List<Post>> GetPostsMentionProfileId(string profileId, string timeZone)
        {
            return await repository.GetPostsMentionProfileId(profileId,timeZone);

        }


        /// <summary>
        /// GetSavedPostsByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        [HttpGet("GetSavedPostsByProfileId")]
        [Authorize]
        public async Task<List<Post>> GetSavedPostsByProfileId(string profileId, string timeZone)
        {
            return await repository.GetSavedPostsByProfileId(profileId, timeZone);

        }

        /// <summary>
        /// GetPostsWithTagByTagId
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        [HttpGet("GetPostsWithTagByTagId")]
        [Authorize]
        public async Task<List<Post>> GetPostsWithTagByTagId(string tagId, string timeZone)
        {
            return await repository.GetPostsWithTagByTagId(tagId, timeZone);

        }

        /// <summary>
        /// GetPostsByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        [HttpGet("GetPostsByProfileId")]
        [Authorize]
        public async Task<List<Post>> GetPostsByProfileId(string profileId, string timeZone)
        {
            return await repository.GetPostsByProfileId(profileId, timeZone);

        }


        /// <summary>
        /// GetPostById
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        [HttpGet("GetPostById")]
        public async Task<Post> GetPostId(string postId, string timeZone)
        {
            try
            {
                return await repository.GetPostById(postId, timeZone);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create Post
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("CreatePost")]
        public async Task CreatePost([FromBody] Post post)
        {
            
            try
            {
                  await  repository.InsertPost(post);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Delete Post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeletePost")]
        public async Task<HttpResponseMessage> DeletePost(string postId)
        {
            try
            {
                await repository.DeletePost(postId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeletePost");

                return await Task.FromResult(returnMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return await Task.FromResult(returnMessage);
        }

        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdatePost")]
        public async Task UpdatePost([FromBody] Post post)
        {

            try
            {
               await repository.UpdatePost(post);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// UpdatePostStatus
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("UpdatePostStatus")]
        public async Task UpdatePostStatus(string postId, string status)
        {

            try
            {
                await repository.UpdatePostStatus(postId, status);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

    }
}
