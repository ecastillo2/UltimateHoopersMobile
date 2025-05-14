using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    /// <summary>
    /// Tag Controller
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class TagController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private ITagRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Tag Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public TagController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new TagRepository(context);

        }

        /// <summary>
        /// Get Tags
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetTags")]
        public async Task<List<Domain.Tag>> GetTags()
        {
            try
            {
                return await repository.GetTags();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get Tag By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        
        [HttpGet("GetTagById")]
        public async Task<Domain.Tag> GetTagById(string tagId)
        {
            try
            {
                return await repository.GetTagById(tagId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create Tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpPost("CreateTag")]
        public async Task CreateTag([FromBody] Domain.Tag tag)
        {
            
            try
            {
                  await  repository.InsertTag(tag);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Delete Tag
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns
        [HttpDelete("DeleteTag")]
        public async Task<HttpResponseMessage> DeleteTag(string tagId)
        {
            try
            {
                await repository.DeleteTag(tagId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteTag");

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
