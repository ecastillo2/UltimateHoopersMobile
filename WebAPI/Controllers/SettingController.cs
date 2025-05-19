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
    /// Tag Controller
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class SettingController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private ISettingRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Tag Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public SettingController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new SettingRepository(context);

        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("UpdateSetting")]
        public async Task UpdateUser([FromBody] Setting setting)
        {

            try
            {
                await repository.UpdateSetting(setting);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }
    }
}
