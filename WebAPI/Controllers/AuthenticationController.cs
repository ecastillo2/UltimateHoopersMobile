using DataLayer.Context;
using DataLayer.DAL;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.DAL.Repository;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using WebAPI.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for handling authentication operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;
        private IProfileRepository repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the AuthenticationController
        /// </summary>
        /// <param name="authenticateService">Service for authentication operations</param>
        public AuthenticationController(IAuthService authenticateService, ApplicationContext context, IConfiguration configuration)
        {
            _authService = authenticateService ?? throw new ArgumentNullException(nameof(authenticateService));
            _configuration = configuration;
            this.repository = new ProfileRepository(context, _configuration);
            
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
                    _authService.Authenticate(model.Token, model.Email, model.Password));
               
                if (userResult == null)
                {
                    return BadRequest(new { message = "Authentication failed. Invalid credentials." });
                }

                // Get the user's profile if authentication was successful
                if (!string.IsNullOrEmpty(userResult.ProfileId))
                {
                    try
                    {
                        // Get the user's profile
                        var profile = await repository.GetProfileByIdAsync(userResult.ProfileId);

                        //Get Client info if part of a client
                        var client = await repository.GetClientByUserIdAsync(userResult.UserId);


                        if (profile != null)
                        {
                            // Assign the profile to the user object
                            userResult.Profile = profile;

                            if (client != null)
                            {
                                userResult.Client = client;


                                //Get Client info if part of a client
                                var courts = await repository.GetCourtsByClientIdAsync(userResult.ClientId);

                                if (courts != null)
                                {
                                    userResult.Client.CourtList = courts;
                                }

                                //Get Client info if part of a client
                                var clientUsers = await repository.GetUsersByClientIdAsync(userResult.ClientId);

                                if (courts != null)
                                {
                                    userResult.Client.UserList = clientUsers;
                                }

                            }

                            // You may want to log successful profile retrieval
                            //_logger.LogInformation($"Retrieved profile for user {userResult.UserId}, ProfileId: {userResult.ProfileId}");
                        }
                        else
                        {
                            // Log that profile was not found but continue with authentication
                           // _logger.LogWarning($"Profile not found for user {userResult.UserId}, ProfileId: {userResult.ProfileId}");
                        }
                    }
                    catch (Exception profileEx)
                    {
                        // Log the profile retrieval error but continue with authentication
                        //_logger.LogError(profileEx, $"Error retrieving profile for user {userResult.UserId}, ProfileId: {userResult.ProfileId}");
                    }
                }
                else
                {
                    //_logger.LogWarning($"User {userResult.UserId} does not have a ProfileId assigned");
                }

                
                // Hide sensitive information before returning
                userResult.Password = null;
                userResult.PasswordHash = null;
               

                return Ok(userResult);
            }
            catch (Exception ex)
            {
                // Log exception
                //_logger.LogError(ex, "Exception occurred during authentication");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred during authentication", error = ex.Message });
            }
        }
    }
}