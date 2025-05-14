using Microsoft.AspNetCore.Mvc;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Repositories;
using System;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Court Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CourtController : ControllerBase
    {
        private readonly ICourtRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Court Controller Constructor
        /// </summary>
        /// <param name="repository">Court repository</param>
        /// <param name="configuration">Configuration</param>
        public CourtController(ICourtRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get All Courts
        /// </summary>
        /// <returns>List of courts</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Court>>> GetCourts()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get Court By Id
        /// </summary>
        /// <param name="id">Court ID</param>
        /// <returns>Court</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Court>> GetCourt(string id)
        {
            var court = await _repository.GetByIdAsync(id);

            if (court == null)
            {
                return NotFound();
            }

            return court;
        }

        /// <summary>
        /// Create Court
        /// </summary>
        /// <param name="court">Court to create</param>
        /// <returns>Created court</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Court>> CreateCourt(Court court)
        {
            try
            {
                if (string.IsNullOrEmpty(court.CourtId))
                {
                    court.CourtId = Guid.NewGuid().ToString();
                }

                // Set image URL using CourtId
                string fileType = ".webp";
                court.ImageURL = $"https://uhblobstorageaccount.blob.core.windows.net/courtimage/{court.CourtId}{fileType}";

                await _repository.AddAsync(court);
                await _repository.SaveAsync();

                return CreatedAtAction(nameof(GetCourt), new { id = court.CourtId }, court);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the court", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Court
        /// </summary>
        /// <param name="id">Court ID</param>
        /// <param name="court">Updated court data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCourt(string id, Court court)
        {
            if (id != court.CourtId)
            {
                return BadRequest();
            }

            try
            {
                await _repository.UpdateCourtAsync(court);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Check if court exists
                var existingCourt = await _repository.GetByIdAsync(id);
                if (existingCourt == null)
                {
                    return NotFound();
                }

                return StatusCode(500, new { message = "An error occurred while updating the court", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Court
        /// </summary>
        /// <param name="id">Court ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCourt(string id)
        {
            var court = await _repository.GetByIdAsync(id);
            if (court == null)
            {
                return NotFound();
            }

            try
            {
                await _repository.DeleteByIdAsync(id);
                await _repository.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the court", error = ex.Message });
            }
        }
    }
}