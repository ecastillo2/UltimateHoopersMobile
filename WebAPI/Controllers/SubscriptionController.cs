using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(ISubscriptionRepository repository, ILogger<SubscriptionController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get Products
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        //[ProducesResponseType(typeof(IEnumerable<RunViewModelDto>), 200)]
        public async Task<IActionResult> GetSubscriptions(CancellationToken cancellationToken)
        {
            try
            {
                var subscription = await _repository.GetSubscriptionsAsync(cancellationToken);
               

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving PrivateRuns");
            }
        }


        /// <summary>
        /// Create Subscription
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("CreateSubscription")]
        //[Authorize]
        public async Task<IActionResult> CreateSubscription([FromBody] Subscription subscription)
        {
            try
            {
                await _repository.InsertSubscription(subscription);


                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Adding Subscription ");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the joined run" });
            }
        }

        /// <summary>
        /// Get Subscriptions By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Subscription), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSubscriptionById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var subscription = await _repository.GetSubscriptionByIdAsync(id, cancellationToken);

                if (subscription == null)
                    return NotFound();

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Subscription {SubscriptionId}", id);
                return StatusCode(500, "An error occurred while retrieving the Subscription");
            }
        }

        /// <summary>
        /// Update products
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("UpdateSubscription")]
        //[Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSubscription(Subscription model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.SubscriptionId != model.SubscriptionId)
                return BadRequest("Subscriptions ID mismatch");

            try
            {
                var subscription = await _repository.GetSubscriptionByIdAsync(model.SubscriptionId, cancellationToken);

                if (subscription == null)
                    return NotFound($"Subscriptions with ID {model.SubscriptionId} not found");

            

                var success = await _repository.UpdateSubscriptionAsync(model, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update Subscriptions");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", model.SubscriptionId);
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }


        /// <summary>
        /// Get Subscriptions By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}/DeleteSubscriptionAsync")]
        public async Task<IActionResult> DeleteSubscriptionAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Subscription ID cannot be null or empty");
                }

                var result = await _repository.DeleteSubscriptionAsync(id, cancellationToken);

                if (!result)
                {
                    return NotFound($"Subscription with ID {id} not found");
                }

                return NoContent(); // 204 No Content is more appropriate for successful deletion
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Subscription {SubscriptionId}", id);
                return StatusCode(500, "An error occurred while deleting the Subscription");
            }
        }
    }
}

