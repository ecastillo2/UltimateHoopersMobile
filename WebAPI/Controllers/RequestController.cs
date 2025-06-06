using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly IRequestRepository _repository;
        private readonly ILogger<RequestController> _logger;

        public RequestController(IRequestRepository repository, ILogger<RequestController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get Requests
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        //[ProducesResponseType(typeof(IEnumerable<RunViewModelDto>), 200)]
        public async Task<IActionResult> GetRequests(CancellationToken cancellationToken)
        {
            try
            {
                var requests = await _repository.GetRequestsAsync(cancellationToken);
               

                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving PrivateRuns");
            }
        }

        /// <summary>
        /// Get Requests with cursor-based pagination for efficient scrolling
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<RequestViewModelDto>), 200)]
        public async Task<IActionResult> GetRequestsWithCursor([FromQuery] string cursor = null,[FromQuery] int limit = 20,[FromQuery] string direction = "next",[FromQuery] string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                var (requests, nextCursor) = await _repository
                    .GetRequestsWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                // Create a list to hold our detailed profile view models
                var detailedViewModels = new List<RequestDetailViewModelDto>();


                // Enrich each profile with additional data
                foreach (var item in requests)
                {
                    // Get additional profile data using the profile's ID
                    var privateRun = item;
                   



                    // Create a detailed view model with all the additional data
                    var detailedViewModel = new RequestDetailViewModelDto()
                    {
                        Request = item,
                        
                    };

                    // Add to our list
                    detailedViewModels.Add(detailedViewModel);
                }



                var result = new CursorPaginatedResultDto<RequestDetailViewModelDto>
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
        /// Create Product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("CreateRequest")]
        //[Authorize]
        public async Task<IActionResult> CreateRequest([FromBody] Request request)
        {
            try
            {
                await _repository.InsertRequest(request);


                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Adding Request ");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the joined run" });
            }
        }

        /// <summary>
        /// Get Requests By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IList<Request>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRequestById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _repository.GetRequestByIdAsync(id, cancellationToken);

                if (product == null)
                    return NotFound();

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Product {ProductId}", id);
                return StatusCode(500, "An error occurred while retrieving the Product");
            }
        }

        /// <summary>
        /// Update Requests
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("UpdateRequest")]
        //[Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateRequest(Request model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.RequestId != model.RequestId)
                return BadRequest("Requests ID mismatch");

            try
            {
                var products = await _repository.GetRequestByIdAsync(model.RequestId, cancellationToken);

                if (products == null)
                    return NotFound($"Requests with ID {model.RequestId} not found");

            

                var success = await _repository.UpdateRequestAsync(model, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update Requests");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", model.RequestId);
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }


        /// <summary>
        /// Get Requests By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}/DeleteRequestAsync")]
        public async Task<IActionResult> DeleteRequestAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Request ID cannot be null or empty");
                }

                var result = await _repository.DeleteRequestAsync(id, cancellationToken);

                if (!result)
                {
                    return NotFound($"Request with ID {id} not found");
                }

                return NoContent(); // 204 No Content is more appropriate for successful deletion
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Request {RequestId}", id);
                return StatusCode(500, "An error occurred while deleting the product");
            }
        }
    }
}

