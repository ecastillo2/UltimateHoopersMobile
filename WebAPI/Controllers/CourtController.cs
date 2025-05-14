using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace API.Controllers
{
    /// <summary>
    /// Court Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class CourtController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private ICourtRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Court Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public CourtController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new CourtRepository(context);

        }

        /// <summary>
        /// Get Courts
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCourts")]
        //[Authorize]
        public async Task<List<Court>> GetCourts()
        {
            return await repository.GetCourts();

        }

        /// <summary>
        /// Get Court By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetCourtById")]
        public async Task<Court> GetCourtById(string courtId)
        {
            try
            {
                return await repository.GetCourtById(courtId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create Court
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpPost("CreateCourt")]
        public async Task CreateCourt([FromBody] Court court)
        {
            
            try
            {
                  await  repository.InsertCourt(court);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("UpdateCourt")]
        public async Task UpdateCourt([FromBody] Court court)
        {

            try
            {
                await repository.UpdateCourt(court);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Delete Court
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteCourt")]
        public async Task<HttpResponseMessage> DeleteCourt(string courtId)
        {
            try
            {
                await repository.DeleteCourt(courtId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteCourt");

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
