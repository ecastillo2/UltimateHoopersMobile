using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _repository;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientRepository repository, ILogger<ClientController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GetClients
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RunViewModelDto>), 200)]
        public async Task<IActionResult> GetClients(CancellationToken cancellationToken)
        {
            try
            {
                var privateRuns = await _repository.GetClientsAsync(cancellationToken);
                var viewModels = privateRuns.Select(p => new ClientViewModelDto(p));

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
        [HttpGet("{clientId}/courts")]
        [ProducesResponseType(typeof(List<Court>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetClientCourtsAsync(string clientId, CancellationToken cancellationToken)
        {
            try
            {
                var privateRuns = await _repository.GetClientCourtsAsync(clientId, cancellationToken);
                
                return Ok(privateRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving PrivateRuns");
            }
        }


        /// <summary>
        /// Get profiles with cursor-based pagination for efficient scrolling
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<ClientViewModelDto>), 200)]
        public async Task<IActionResult> GetClientsWithCursor([FromQuery] string cursor = null,[FromQuery] int limit = 20,[FromQuery] string direction = "next",[FromQuery] string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                var (privateRuns, nextCursor) = await _repository
                    .GetClientsWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                // Create a list to hold our detailed profile view models
                var detailedViewModels = new List<ClientDetailViewModelDto>();


                // Enrich each profile with additional data
                foreach (var item in privateRuns)
                {
                    // Get additional profile data using the profile's ID
                    var privateRun = item;
                    var court = await _repository.GetCourtByClientIdAsync(item.ClientId, cancellationToken);



                    // Create a detailed view model with all the additional data
                    var detailedViewModel = new ClientDetailViewModelDto()
                    {
                        Client = item,
                        CourtList = court,
                    };

                    // Add to our list
                    detailedViewModels.Add(detailedViewModel);
                }



                var result = new CursorPaginatedResultDto<ClientDetailViewModelDto>
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
        /// Get Client By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClientDetailViewModelDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetClientById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var privateRun = await _repository.GetClientByIdAsync(id, cancellationToken);

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
        /// Update Client
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("UpdateClient")]
        //[Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateClient([FromBody] Client model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.ClientId != model.ClientId)
                return BadRequest("Client ID mismatch");

            try
            {
                var products = await _repository.GetClientByIdAsync(model.ClientId, cancellationToken);

                if (products == null)
                    return NotFound($"Client with ID {model.ClientId} not found");



                var success = await _repository.UpdateClientAsync(model, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update products");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", model.ClientId);
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }

    }
}

