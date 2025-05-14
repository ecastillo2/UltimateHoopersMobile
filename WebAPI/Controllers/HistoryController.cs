using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    /// <summary>
    /// History Controller
    /// </summary>
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IHistoryRepository repository;        
        private readonly IConfiguration _configuration;


        /// <summary>
        /// History Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public HistoryController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new HistoryRepository(context);

        }

        /// <summary>
        /// Get Historys
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetHistorys")]
        //[Authorize]
        public async Task<List<History>> GetHistorys()
        {

            return await repository.GetHistorys();

        }

        /// <summary>
        /// Get History By Id
        /// </summary>
        /// <param name="historyId"></param>
        /// <returns></returns>
        [HttpGet("GetHistoryById")]
        public async Task<History> GetHistoryById(string historyId)
        {
            try
            {
                return await repository.GetHistoryById(historyId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create History
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        [HttpPost("CreateHistory")]
        public async Task CreateHistory([FromBody] History history)
        {
            
            try
            {
                  await  repository.InsertHistory(history);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Delete History
        /// </summary>
        /// <param name="historyId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeleteHistory")]
        public async Task<HttpResponseMessage> DeleteHistory(string historyId)
        {
            try
            {
                await repository.DeleteHistory(historyId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteHistory");

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
