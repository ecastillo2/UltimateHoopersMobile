using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;
using DataLayer.DAL.Repository;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Court Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class ScoutingReportController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IScoutingReportRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// ScoutingReport Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public ScoutingReportController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new ScoutingReportRepository(context);

        }

        

        /// <summary>
        /// Get Court By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetScoutingReportById")]
        public async Task<ScoutingReport> GetScoutingReportById(string scoutingReportId)
        {
            try
            {
                return await repository.GetScoutingReportById(scoutingReportId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

       

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("UpdateScoutingReport")]
        public async Task UpdateScoutingReport([FromBody] ScoutingReport scoutingReport)
        {

            try
            {
                await repository.UpdateScoutingReport(scoutingReport);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Delete ScoutingReport
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteCourt")]
        public async Task<HttpResponseMessage> DeleteScoutingReport(string scoutingReportId)
        {
            try
            {
                await repository.DeleteScoutingReport(scoutingReportId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteScoutingReport");

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
