using Microsoft.AspNetCore.Mvc;
using API.Services;
using Domain;

namespace API.Controllers
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
        /// SocialMedia Authenticate
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("SocialMediaAuthenticate")]
        public ActionResult SocialMediaAuthenticate([FromBody] User model)
        {
            var userResult = _authenticateService.SocialMediaAuthenticate(model.AuthToken, model.Email, model.Password);

            if (userResult == null)
                return BadRequest();

            return Ok(userResult);

        }

    }
}
