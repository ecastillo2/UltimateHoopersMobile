using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAPI.Services;
using Domain;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for handling authentication operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticateService _authenticateService;

        /// <summary>
        /// Initializes a new instance of the AuthenticationController
        /// </summary>
        /// <param name="authenticateService">Service for authentication operations</param>
        public AuthenticationController(IAuthenticateService authenticateService)
        {
            _authenticateService = authenticateService ?? throw new ArgumentNullException(nameof(authenticateService));
        }

        /// <summary>
        /// Authenticates a user using various methods (token, email/password)
        /// </summary>
        /// <param name="model">User model containing authentication credentials</param>
        /// <returns>Authenticated user with JWT token if successful</returns>
        /// <response code="200">Returns the authenticated user with token</response>
        /// <response code="400">If the credentials are invalid or authentication fails</response>
        /// <response code="500">If an unexpected error occurs during authentication</response>
        [HttpPost("Authenticate")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Authenticate([FromBody] User model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { message = "Authentication model is null" });
                }

                // Validate required fields based on authentication type
                if (string.IsNullOrEmpty(model.Token) && (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password)))
                {
                    return BadRequest(new { message = "Provide either a token or email and password" });
                }

                // Convert synchronous method to asynchronous using Task.Run
                var userResult = await Task.Run(() =>
                    _authenticateService.Authenticate(model.Token, model.Email, model.Password));

                if (userResult == null)
                {
                    return BadRequest(new { message = "Authentication failed. Invalid credentials." });
                }

                // Hide sensitive information before returning
                userResult.Password = null;
                userResult.PasswordHash = null;

                return Ok(userResult);
            }
            catch (Exception ex)
            {
                // Log exception (implement logging)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred during authentication", error = ex.Message });
            }
        }
    }
}