using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoRepository _repository;
        private readonly ILogger<VideoController> _logger;

        public VideoController(IVideoRepository repository, ILogger<VideoController> logger)
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
        public async Task<IActionResult> GetVideos(CancellationToken cancellationToken)
        {
            try
            {
                var videos = await _repository.GetVideosAsync(cancellationToken);
               

                return Ok(videos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Videos");
                return StatusCode(500, "An error occurred while retrieving Videos");
            }
        }

        /// <summary>
        /// Get products with cursor-based pagination for efficient scrolling
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<VideoViewModelDto>), 200)]
        public async Task<IActionResult> GetVideosWithCursor([FromQuery] string cursor = null,[FromQuery] int limit = 20,[FromQuery] string direction = "next",[FromQuery] string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                var (videos, nextCursor) = await _repository.GetVideosWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                // Create a list to hold our detailed profile view models
                var detailedViewModels = new List<VideoDetailViewModelDto>();


                // Enrich each profile with additional data
                foreach (var item in videos)
                {
                    // Get additional profile data using the profile's ID
                    var video = item;

                    // Create a detailed view model with all the additional data
                    var detailedViewModel = new VideoDetailViewModelDto()
                    {
                        Video = item,
                        
                    };

                    // Add to our list
                    detailedViewModels.Add(detailedViewModel);
                }



                var result = new CursorPaginatedResultDto<VideoDetailViewModelDto>
                {
                    Items = detailedViewModels,
                    NextCursor = nextCursor,
                    HasMore = !string.IsNullOrEmpty(nextCursor),
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
        /// Create Video
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        [HttpPost("CreateVideo")]
        //[Authorize]
        public async Task<IActionResult> CreateVideo([FromBody] Video video)
        {
            try
            {
                await _repository.InsertVideo(video);


                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Adding Video");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the Video" });
            }
        }

        /// <summary>
        /// Get Videos By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IList<Video>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetVideoById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var video = await _repository.GetVideoByIdAsync(id, cancellationToken);

                if (video == null)
                    return NotFound();

                return Ok(video);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Video {VideoId}", id);
                return StatusCode(500, "An error occurred while retrieving the Product");
            }
        }

        /// <summary>
        /// Update Videos
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("UpdateVideo")]
        //[Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateVideo(Video model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.VideoId != model.VideoId)
                return BadRequest("Videos ID mismatch");

            try
            {
                var videos = await _repository.GetVideoByIdAsync(model.VideoId, cancellationToken);

                if (videos == null)
                    return NotFound($"Videos with ID {model.VideoId} not found");

            

                var success = await _repository.UpdateVideoAsync(model, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update Videos");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", model.VideoId);
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }


        /// <summary>
        /// Get Videos By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}/DeleteVideoAsync")]
        
        public async Task<IActionResult> DeleteVideoAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Video ID cannot be null or empty");
                }

                var result = await _repository.DeleteVideoAsync(id, cancellationToken);

                if (!result)
                {
                    return NotFound($"Video with ID {id} not found");
                }

                return NoContent(); // 204 No Content is more appropriate for successful deletion
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Video {VideoId}", id);
                return StatusCode(500, "An error occurred while deleting the Video");
            }
        }
    }
}

