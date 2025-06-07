using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing joined runs
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class JoinedRunController : ControllerBase
    {
        private readonly IJoinedRunRepository _repository;
        private readonly ILogger<JoinedRunController> _logger;

        /// <summary>
        /// Initializes a new instance of the JoinedRunController
        /// </summary>
        /// <param name="repository">The joined run repository</param>
        /// <param name="logger">The logger</param>
        public JoinedRunController(IJoinedRunRepository repository, ILogger<JoinedRunController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all joined runs
        /// </summary>
        /// <returns>A list of all joined runs</returns>
        /// <response code="200">Returns the list of joined runs</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(List<JoinedRun>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJoinedRuns()
        {
            try
            {
                var joinedRuns = await _repository.GetJoinedRuns();
                return Ok(joinedRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving joined runs");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving joined runs" });
            }
        }

        /// <summary>
        /// Get a joined run by ID
        /// </summary>
        /// <param name="joinedRunId">The ID of the joined run to retrieve</param>
        /// <returns>The joined run with the specified ID</returns>
        /// <response code="200">Returns the joined run</response>
        /// <response code="404">If the joined run was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{joinedRunId}")]
        [Authorize]
        [ProducesResponseType(typeof(JoinedRun), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJoinedRunById(string joinedRunId)
        {
            try
            {
                var joinedRun = await _repository.GetJoinedRunById(joinedRunId);

                if (joinedRun == null)
                {
                    return NotFound(new { message = $"Joined run with ID {joinedRunId} not found" });
                }

                return Ok(joinedRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving joined run {JoinedRunId}", joinedRunId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the joined run" });
            }
        }

        /// <summary>
        /// Get a joined run by ID
        /// </summary>
        /// <param name="joinedRunId">The ID of the joined run to retrieve</param>
        /// <returns>The joined run with the specified ID</returns>
        /// <response code="200">Returns the joined run</response>
        /// <response code="404">If the joined run was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{runId}/run")]
        //[Authorize]
       
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJoinedRunProfilesByRunIdAsync(string runId)
        {
            try
            {
                var joinedRun = await _repository.GetJoinedRunProfilesByRunIdAsync(runId);

                if (joinedRun == null)
                {
                    return NotFound(new { message = $"Joined run with ID {runId} not found" });
                }

                return Ok(joinedRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving joined run {JoinedRunId}", runId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the joined run" });
            }
        }


        /// <summary>
        /// Get a joined run by ID
        /// </summary>
        /// <param name="joinedRunId">The ID of the joined run to retrieve</param>
        /// <returns>The joined run with the specified ID</returns>
        /// <response code="200">Returns the joined run</response>
        /// <response code="404">If the joined run was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{profileId}/{runId}/run")]
        //[Authorize]

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveProfileJoinRunAsync(string profile, string runId)
        {
            try
            {
                var joinedRun = await _repository.RemoveProfileJoinRunAsync(profile, runId);

                if (joinedRun == null)
                {
                    return NotFound(new { message = $"Joined run with ID {runId} not found" });
                }

                return Ok(joinedRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving joined run {JoinedRunId}", runId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the joined run" });
            }
        }

        /// <summary>
        /// Get a joined run by ID
        /// </summary>
        /// <param name="joinedRunId">The ID of the joined run to retrieve</param>
        /// <returns>The joined run with the specified ID</returns>
        /// <response code="200">Returns the joined run</response>
        /// <response code="404">If the joined run was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("{profileId}/{runId}/{status}/joinedrun")]
        //[Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // If already joined
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProfileToJoinedRunAsync(string profileId, string runId, string status)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(runId))
                {
                    return BadRequest(new { message = "ProfileId and RunId are required" });
                }

                // Check if profile is already joined (optional validation)
                // var existingJoin = await _repository.GetJoinedRunAsync(profileId, runId);
                // if (existingJoin != null)
                // {
                //     return Conflict(new { message = "Profile is already joined to this run" });
                // }

                await _repository.AddProfileToJoinedRunAsync(profileId, runId, status);

                return Ok(new { message = "Profile successfully joined to run", profileId, runId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid parameters for joining run. ProfileId: {ProfileId}, RunId: {RunId}", profileId, runId);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot join profile {ProfileId} to run {RunId}", profileId, runId);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding profile {ProfileId} to joined run {RunId}", profileId, runId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while joining the run" });
            }
        }

        /// <summary>
        /// Get joined runs by profile ID
        /// </summary>
        /// <param name="profileId">The ID of the profile</param>
        /// <returns>A list of joined runs for the specified profile</returns>
        /// <response code="200">Returns the list of joined runs for the profile</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetUserJoinedRunsAsync/{profileId}")]
        //[Authorize]
        //[ProducesResponseType(typeof(List<JoinedRun>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserJoinedRunsAsync(string profileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var joinedRuns = await _repository.GetJoinedRunsByProfileId(profileId);

                // Create a list to hold our detailed profile view models
                var detailedViewModels = new List<JoinedRunDetailViewModelDto>();

                // Enrich each profile with additional data
                foreach (var item in joinedRuns)
                {
                    // Get additional profile data using the profile's ID
                    
                    var run = await _repository.GetRunById(item.RunId);
                   

                    // Create a detailed view model with all the additional data
                    var detailedViewModel = new JoinedRunDetailViewModelDto()
                    {
                        JoinedRun = item,
                        Run = run,
                       
                    };

                    // Add to our list
                    detailedViewModels.Add(detailedViewModel);
                }

                // Create the result with our detailed view models
                var result = detailedViewModels;
                

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving joined runs for profile {ProfileId}", profileId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving joined runs for the profile" });
            }
        }


        /// <summary>
        /// Check if a profile is already invited to a run
        /// </summary>
        /// <param name="profileId">The ID of the profile</param>
        /// <param name="runId">The ID of the run</param>
        /// <returns>True if the profile is already invited to the run, false otherwise</returns>
        /// <response code="200">Returns the result of the check</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("IsAlreadyInvited")]
        [Authorize]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IsProfileAlreadyInvitedToRun(string profileId, string runId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(runId))
                {
                    return BadRequest(new { message = "Profile ID and Run ID are required" });
                }

                var isInvited = await _repository.IsProfileIdIdAlreadyInvitedToRunInJoinedRuns(profileId, runId);
                return Ok(isInvited);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if profile {ProfileId} is already invited to run {RunId}", profileId, runId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while checking if the profile is already invited to the run" });
            }
        }

        /// <summary>
        /// Clear all joined runs for a specific run
        /// </summary>
        /// <param name="runId">The ID of the run</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the joined runs were successfully cleared</response>
        /// <response code="400">If the run ID is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("ClearByRun/{runId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearJoinedRunByRun(string runId)
        {
            try
            {
                if (string.IsNullOrEmpty(runId))
                {
                    return BadRequest(new { message = "Run ID is required" });
                }

                await _repository.ClearJoinedRunByRun(runId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing joined runs for run {RunId}", runId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while clearing the joined runs" });
            }
        }


        /// <summary>
        /// Response model for joined runs with counts
        /// </summary>
        public class JoinedRunsWithCountsResponse
        {
            /// <summary>
            /// List of profiles invited to the run
            /// </summary>
            public List<Profile> Profiles { get; set; }

            /// <summary>
            /// Count of accepted invites
            /// </summary>
            public int AcceptedCount { get; set; }

            /// <summary>
            /// Count of undecided invites
            /// </summary>
            public int UndecidedCount { get; set; }

            /// <summary>
            /// Count of declined invites
            /// </summary>
            public int DeclinedCount { get; set; }

            /// <summary>
            /// Total count of invites
            /// </summary>
            public int TotalCount { get; set; }
        }
    }
}