using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RunController : ControllerBase
    {
        private readonly IRunRepository _runRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<RunController> _logger;

        public RunController(IRunRepository privateRunRepository, IClientRepository clientRepository, ILogger<RunController> logger)
        {
            _runRepository = privateRunRepository ?? throw new ArgumentNullException(nameof(privateRunRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get Runs
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RunViewModelDto>), 200)]
        public async Task<IActionResult> GetRuns(CancellationToken cancellationToken)
        {
            try
            {
                var privateRuns = await _runRepository.GetRunsAsync(cancellationToken);
                var viewModels = privateRuns.Select(p => new RunViewModelDto(p));

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving PrivateRuns");
            }
        }

        /// <summary>
        /// Get courts for a specific client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{runId}/requests")]
        [ProducesResponseType(typeof(List<Request>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRunRequestsAsync(string runId, CancellationToken cancellationToken)
        {
            try
            {
                var privateRuns = await _runRepository.GetRunRequestsAsync(runId, cancellationToken);

                return Ok(privateRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving PrivateRuns");
            }
        }

        /// <summary>
        /// Get Runs Paginated
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResultDto<RunViewModelDto>), 200)]
        public async Task<IActionResult> GetRunsPaginated([FromQuery] int page = 1,[FromQuery] int pageSize = 20,CancellationToken cancellationToken = default)
        {
            try
            {
                var (runs, totalCount, totalPages) = await _runRepository
                    .GetRunsPaginatedAsync(page, pageSize, cancellationToken);

                var viewModels = runs.Select(p => new RunViewModelDto(p)).ToList();

                var result = new PaginatedResultDto<RunViewModelDto>
                {
                    Items = viewModels,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated profiles");
                return StatusCode(500, "An error occurred while retrieving paginated profiles");
            }
        }

        /// <summary>
        /// Get Runs With Cursor
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<RunViewModelDto>), 200)]
        public async Task<IActionResult> GetRunsWithCursor([FromQuery] string cursor = null,[FromQuery] int limit = 20,[FromQuery] string direction = "next",[FromQuery] string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                var (privateRuns, nextCursor) = await _runRepository.GetRunsWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                // Create a list to hold our detailed profile view models
                var detailedViewModels = new List<RunDetailViewModelDto>();


                // Enrich each profile with additional data
                foreach (var item in privateRuns)
                {
                    // Get additional profile data using the profile's ID
                    var privateRun = item;
                    var court = await _runRepository.GetCourtAsync(item.CourtId, cancellationToken);
                    var client = await _clientRepository.GetClientByIdAsync(item.ClientId, cancellationToken);
                    var JoinRunProfileList = await _runRepository.GetJoinedRunAsync(item.RunId, cancellationToken);
                    

                    // Create a detailed view model with all the additional data
                    var detailedViewModel = new RunDetailViewModelDto(item)
                    {
                        Run = item,
                        Court = court != null ? new Court(court) : null,
                        Client = client != null ? new Client(client) : null,
                        JoinedRunProfileList = JoinRunProfileList,
                        
                    };

                    // Add to our list
                    detailedViewModels.Add(detailedViewModel);
                }



                var result = new CursorPaginatedResultDto<RunDetailViewModelDto>
                {
                    Items = detailedViewModels,
                    NextCursor = nextCursor,
                    //HasMore = !string.IsNullOrEmpty(nextCursor),
                    Direction = direction,
                    SortBy = sortBy
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cursor-based PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving cursor-based PrivateRuns");
            }
        }

        /// <summary>
        /// Get Run By RunId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("GetRunById/{id}")]
        [ProducesResponseType(typeof(Run), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRunById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var privateRun = await _runRepository.GetRunByIdAsync(id, cancellationToken);

                if (privateRun == null)
                    return NotFound();

                return Ok(privateRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PrivateRun {PrivateRunId}", id);
                return StatusCode(500, "An error occurred while retrieving the PrivateRun");
            }
        }

        /// <summary>
        /// Update Run
        /// </summary>
        /// <param name="run"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("UpdateRun")]
        //[Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateRun([FromBody] Run run, CancellationToken cancellationToken)
        {

            try
            {
                var privateRun = await _runRepository.GetRunByIdAsync(run.RunId, cancellationToken);

                if (privateRun == null)
                    return NotFound($"PrivateRun with ID {run.RunId} not found");

                var success = await _runRepository.UpdateRunAsync(run, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update PrivateRun");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", run.RunId);
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }

    }
}

