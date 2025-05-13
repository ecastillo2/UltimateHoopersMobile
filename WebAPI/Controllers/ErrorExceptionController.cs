using Microsoft.AspNetCore.Mvc;
using Domain;
using Microsoft.AspNetCore.Authorization;
using DataLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Error Exception Controller
    /// </summary>
    [Route("api/[controller]")]
    public class ErrorExceptionController : Controller
    {
        private readonly IErrorExceptionRepository _repository;

        /// <summary>
        /// Error Exception Controller
        /// </summary>
        /// <param name="repository">Error exception repository</param>
        public ErrorExceptionController(IErrorExceptionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Get Error Exceptions
        /// </summary>
        /// <returns>List of error exceptions</returns>
        [HttpGet("GetErrorExceptions")]
        [Authorize]
        public async Task<List<ErrorException>> GetErrorExceptions()
        {
            return await _repository.GetErrorExceptions();
        }

        /// <summary>
        /// Get Error Exception ById
        /// </summary>
        /// <param name="errorExceptionId">Error exception ID</param>
        /// <returns>Error exception</returns>
        [HttpGet("GetErrorExceptionById")]
        [Authorize]
        public async Task<ErrorException> GetErrorExceptionById(string errorExceptionId)
        {
            try
            {
                return await _repository.GetErrorExceptionById(errorExceptionId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Create Error Exception
        /// </summary>
        /// <param name="errorException">Error exception to create</param>
        /// <returns>Task</returns>
        [HttpPost("CreateErrorException")]
        //[Authorize]
        public async Task<IActionResult> CreateErrorException([FromBody] ErrorException errorException)
        {
            try
            {
                await _repository.InsertErrorException(errorException);
                return Ok(new { message = "Error exception created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the error exception", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Error Exception
        /// </summary>
        /// <param name="errorExceptionId">Error exception ID</param>
        /// <returns>Result</returns>
        [HttpDelete("DeleteErrorException")]
        [Authorize]
        public async Task<IActionResult> DeleteErrorException(string errorExceptionId)
        {
            try
            {
                await _repository.DeleteErrorException(errorExceptionId);
                return Ok(new { message = "Error exception deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the error exception", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Error Exception
        /// </summary>
        /// <param name="errorException">Error exception to update</param>
        /// <returns>Result</returns>
        [HttpPost("UpdateErrorException")]
        [Authorize]
        public async Task<IActionResult> UpdateErrorException([FromBody] ErrorException errorException)
        {
            try
            {
                await _repository.UpdateErrorException(errorException);
                return Ok(new { message = "Error exception updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the error exception", error = ex.Message });
            }
        }
    }
}