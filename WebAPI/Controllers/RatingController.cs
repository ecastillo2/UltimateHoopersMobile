using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Rating Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class RatingController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IRatingRepository repository;        
        private readonly IConfiguration _configuration;


        /// <summary>
        /// Rating Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public RatingController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new RatingRepository(context);

        }

        /// <summary>
        /// Get Ratings
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetRatings")]
        public async Task<List<Domain.Rating>> GetRatings()
        {

            return await repository.GetRatings();

        }


        /// <summary>
        /// Get Rating By Id
        /// </summary>
        /// <param name="ratingId"></param>
        /// <returns></returns>
        [HttpGet("GetRatingById")]
        public async Task<Domain.Rating> GetRatingById(string ratingId)
        {
            try
            {
                return await repository.GetRatingById(ratingId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create Rating
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        [HttpPost("CreateRating")]
        public async Task CreateRating([FromBody] Domain.Rating rating)
        {
            
            try
            {
                  await  repository.InsertRating(rating);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Delete Rating
        /// </summary>
        /// <param name="ratingId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteRating")]
        public async Task<HttpResponseMessage> DeleteRating(string ratingId)
        {
            try
            {
                await repository.DeleteRating(ratingId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteRating");

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
