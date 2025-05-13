using Microsoft.AspNetCore.Mvc;
using Domain;
using WebAPI.Services;

namespace WebAPI.Controllers
{

    /// <summary>
    /// Authentication Controller
    /// </summary>
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
	{
        private IAuthenticateService _authenticateService;

        /// <summary>
        /// Authentication Controller
        /// </summary>
        /// <param name="authenticateService"></param>
        public AuthenticationController(IAuthenticateService authenticateService)
        {
            _authenticateService = authenticateService;
        }

        /// <summary>
        ///  Authenticate
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("Authenticate")]
        public ActionResult Authenticate([FromBody] User model)
        {
            var userResult = _authenticateService.Authenticate(model.AuthToken, model.Email, model.Password);

            if (userResult == null)
                return BadRequest();

            return Ok(userResult);

        }

    }
}
