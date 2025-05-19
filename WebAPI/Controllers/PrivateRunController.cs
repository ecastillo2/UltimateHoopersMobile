using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.DAL;
using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrivateRunController : ControllerBase
    {
        private readonly IPrivateRunRepository _privateRunRepository;
        private readonly ILogger<PrivateRunController> _logger;

        public PrivateRunController(IPrivateRunRepository privateRunRepository, ILogger<PrivateRunController> logger)
        {
            _privateRunRepository = privateRunRepository ?? throw new ArgumentNullException(nameof(privateRunRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all PrivateRuns
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PrivateRunViewModelDto>), 200)]
        public async Task<IActionResult> GetPrivateRuns(CancellationToken cancellationToken)
        {
            try
            {
                var privateRuns = await _privateRunRepository.GetPrivateRunsAsync(cancellationToken);
                var viewModels = privateRuns.Select(p => new PrivateRunViewModelDto(p));

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving PrivateRuns");
            }
        }

        /// <summary>
        /// Get PrivateRuns with standard pagination
        /// </summary>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResultDto<PrivateRunViewModelDto>), 200)]
        public async Task<IActionResult> GetPrivateRunsPaginated(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (privateRuns, totalCount, totalPages) = await _privateRunRepository
                    .GetPrivateRunsPaginatedAsync(page, pageSize, cancellationToken);

                var viewModels = privateRuns.Select(p => new PrivateRunViewModelDto(p)).ToList();

                var result = new PaginatedResultDto<PrivateRunViewModelDto>
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
        /// Get profiles with cursor-based pagination for efficient scrolling
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<PrivateRunViewModelDto>), 200)]
        public async Task<IActionResult> GetPrivateRunsWithCursor(
            [FromQuery] string cursor = null,
            [FromQuery] int limit = 20,
            [FromQuery] string direction = "next",
            [FromQuery] string sortBy = "Points",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (privateRuns, nextCursor) = await _privateRunRepository
                    .GetPrivateRunsWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                var viewModels = privateRuns.Select(p => new PrivateRunViewModelDto(p)).ToList();

                var result = new CursorPaginatedResultDto<PrivateRunViewModelDto>
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
                _logger.LogError(ex, "Error retrieving cursor-based PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving cursor-based PrivateRuns");
            }
        }

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PrivateRunDetailViewModelDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPrivateRunById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var privateRun = await _privateRunRepository.GetPrivateRunByIdAsync(id, cancellationToken);

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
        /// Update PrivateRun
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePrivateRun(string id, PrivateRunUpdateModelDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != model.CourtId)
                return BadRequest("PrivateRun ID mismatch");

            try
            {
                var privateRun = await _privateRunRepository.GetPrivateRunByIdAsync(id, cancellationToken);

                if (privateRun == null)
                    return NotFound($"PrivateRun with ID {id} not found");

                // Update PrivateRun properties from model
                model.UpdatePrivateRun(privateRun);

                var success = await _privateRunRepository.UpdatePrivateRunAsync(privateRun, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update PrivateRun");

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

