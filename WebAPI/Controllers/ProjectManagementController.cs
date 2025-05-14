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
    public class ProjectManagementController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IProjectManagementRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// ProjectManagement Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public ProjectManagementController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new ProjectManagementRepository(context);

        }

        /// <summary>
        /// Get ProjectManagements
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetProjectManagements")]
        //[Authorize]
        public async Task<List<ProjectManagement>> GetProjectManagements()
        {
            return await repository.GetProjectManagements();

        }

        /// <summary>
        /// Get ProjectManagement By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetProjectManagementById")]
        public async Task<ProjectManagement> GetProjectManagementById(string projectManagementId)
        {
            try
            {
                return await repository.GetProjectManagementById(projectManagementId);
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
        [HttpPost("CreateProjectManagement")]
        public async Task CreateProjectManagement([FromBody] ProjectManagement projectManagement)
        {
            
            try
            {
                  await  repository.InsertProjectManagement(projectManagement);
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
        [HttpPost("UpdateProjectManagement")]
        public async Task UpdateProjectManagement([FromBody] ProjectManagement projectManagement)
        {

            try
            {
                await repository.UpdateProjectManagement(projectManagement);

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
        [HttpDelete("DeleteProjectManagement")]
        public async Task<HttpResponseMessage> DeleteProjectManagement(string projectManagementId)
        {
            try
            {
                await repository.DeleteProjectManagement(projectManagementId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteProjectManagement");

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
