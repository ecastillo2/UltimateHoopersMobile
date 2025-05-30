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
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReportDto>), 200)]
        public async Task<IActionResult> StreamAllCountsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var counts =  _reportRepository.StreamAllCountsAsync(cancellationToken);
                

                return Ok(counts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Courts");
                return StatusCode(500, "An error occurred while retrieving Courts");
            }
        }

       





    }
}

