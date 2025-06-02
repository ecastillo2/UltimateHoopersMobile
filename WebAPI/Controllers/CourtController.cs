using DataLayer.DAL.Interface;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourtController : ControllerBase
    {
        private readonly ICourtRepository _courtRepository;
        private readonly ILogger<CourtController> _logger;

        public CourtController(ICourtRepository courtRepository, ILogger<CourtController> logger)
        {
            _courtRepository = courtRepository ?? throw new ArgumentNullException(nameof(courtRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GetC ourts
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CourtViewModelDto>), 200)]
        public async Task<IActionResult> GetCourts(CancellationToken cancellationToken)
        {
            try
            {
                var courts = await _courtRepository.GetCourtsAsync(cancellationToken);
                var viewModels = courts.Select(p => new CourtViewModelDto(p));

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Courts");
                return StatusCode(500, "An error occurred while retrieving Courts");
            }
        }

        /// <summary>
        /// Get Courts Paginated
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResultDto<CourtViewModelDto>), 200)]
        public async Task<IActionResult> GetCourtsPaginated([FromQuery] int page = 1,[FromQuery] int pageSize = 20,CancellationToken cancellationToken = default)
        {
            try
            {
                var (courts, totalCount, totalPages) = await _courtRepository
                    .GetCourtsPaginatedAsync(page, pageSize, cancellationToken);

                var viewModels = courts.Select(p => new CourtViewModelDto(p)).ToList();

                var result = new PaginatedResultDto<CourtViewModelDto>
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
        /// Get Profiles With Cursor
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<CourtViewModelDto>), 200)]
        public async Task<IActionResult> GetProfilesWithCursor([FromQuery] string cursor = null,[FromQuery] int limit = 20,[FromQuery] string direction = "next",[FromQuery] string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                var (courts, nextCursor) = await _courtRepository
                    .GetCourtsWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                var viewModels = courts.Select(p => new CourtViewModelDto(p)).ToList();

                var result = new CursorPaginatedResultDto<CourtViewModelDto>
                {
                    Items = viewModels,
                    NextCursor = nextCursor,
                    HasMore = !string.IsNullOrEmpty(nextCursor),
                    Direction = direction,
                    SortBy = sortBy
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cursor-based profiles");
                return StatusCode(500, "An error occurred while retrieving cursor-based profiles");
            }
        }

        /// <summary>
        /// Get Court By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CourtDetailViewModelDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCourtById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var court = await _courtRepository.GetCourtByIdAsync(id, cancellationToken);

                if (court == null)
                    return NotFound();

                return Ok(court);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile {ProfileId}", id);
                return StatusCode(500, "An error occurred while retrieving the profile");
            }
        }


        /// <summary>
        /// UpdateC ourt
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCourt(string id, CourtUpdateModelDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != model.CourtId)
                return BadRequest("Court ID mismatch");

            try
            {
                var court = await _courtRepository.GetCourtByIdAsync(id, cancellationToken);

                if (court == null)
                    return NotFound($"Court with ID {id} not found");

                // Update Court properties from model
                model.UpdateCourt(court);

                var success = await _courtRepository.UpdateCourtAsync(court, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update Court");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", id);
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }

    }
}

