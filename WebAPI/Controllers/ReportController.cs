using System;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.DAL.Interface;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository _reportRepository;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportRepository reportRepository, ILogger<ReportController> logger)
        {
            _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all counts for dashboard reporting
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ReportDto with all counts</returns>
        [HttpGet("StreamAllCountsAsync")]
        [ProducesResponseType(typeof(ReportDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> StreamAllCountsAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting report counts");

                var reportDto = await _reportRepository.GetAllCountsAsync(cancellationToken);

                if (!reportDto.IsDataComplete)
                {
                    _logger.LogWarning("Report data incomplete. Errors: {Errors}", string.Join(", ", reportDto.Errors));
                }

                return Ok(reportDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report counts");
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving report counts",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all counts (alternative endpoint name for clarity)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ReportDto with all counts</returns>
        [HttpGet("GetAllCounts")]
        [ProducesResponseType(typeof(ReportDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllCounts(CancellationToken cancellationToken)
        {
            return await StreamAllCountsAsync(cancellationToken);
        }

        /// <summary>
        /// Health check endpoint for the report service
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        [ProducesResponseType(200)]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}