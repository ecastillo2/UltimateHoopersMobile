using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using System.Net;

namespace API.Controllers
{
    /// <summary>
    /// Organization Controller
    /// </summary>
    [Route("api/[controller]")]
  
    public class OrganizationController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IOrganizationRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Organization Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public OrganizationController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new OrganizationRepository(context);

        }


        /// <summary>
        /// Get Organization Info
        /// </summary>
        /// <returns></returns>
        
        [HttpGet("GetOrganizationInfo")]
        public async Task<Organization> GetOrganizationInfo()
        {
            try
            {
                return await repository.GetOrganizationInfo();
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }


        /// <summary>
        /// Update Organization
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("UpdateOrganization")]
        public async Task UpdateOrganization([FromBody] Organization organization)
        {

            try
            {
                await repository.UpdateOrganization(organization);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }


    }
}
