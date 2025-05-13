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
    /// Document Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IDocumentRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Document Controller
        /// </summary>
        /// <param name="repository">Document repository</param>
        /// <param name="configuration">Configuration</param>
        public DocumentController(IDocumentRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get Documents
        /// </summary>
        /// <returns>List of documents</returns>
        [HttpGet("GetDocuments")]
        public async Task<List<Document>> GetDocuments()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get Document By Id
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Document</returns>
        [HttpGet("GetDocumentById")]
        public async Task<Document> GetDocumentById(string documentId)
        {
            try
            {
                return await _repository.GetByIdAsync(documentId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Get Documents By Type
        /// </summary>
        /// <param name="type">Document type</param>
        /// <returns>List of documents</returns>
        [HttpGet("GetDocumentsByType")]
        public async Task<List<Document>> GetDocumentsByType(string type)
        {
            try
            {
                return await _repository.FindAsync(d => d.Type == type);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Create Document
        /// </summary>
        /// <param name="document">Document to create</param>
        /// <returns>Task</returns>
        [HttpPost("CreateDocument")]
        public async Task<IActionResult> CreateDocument([FromBody] Document document)
        {
            try
            {
                await _repository.AddAsync(document);
                await _repository.SaveAsync();
                return Ok(new { message = "Document created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the document", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Document
        /// </summary>
        /// <param name="document">Document to update</param>
        /// <returns>Result</returns>
        [HttpPost("UpdateDocument")]
        public async Task<IActionResult> UpdateDocument([FromBody] Document document)
        {
            try
            {
                _repository.Update(document);
                await _repository.SaveAsync();
                return Ok(new { message = "Document updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the document", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Result</returns>
        [HttpDelete("DeleteDocument")]
        public async Task<IActionResult> DeleteDocument(string documentId)
        {
            try
            {
                await _repository.DeleteByIdAsync(documentId);
                await _repository.SaveAsync();
                return Ok(new { message = "Document deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the document", error = ex.Message });
            }
        }
    }
}