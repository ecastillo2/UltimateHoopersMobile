using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductRepository repository, ILogger<ProductController> logger)
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
        public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
        {
            try
            {
                var products = await _repository.GetProductsAsync(cancellationToken);
               

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving PrivateRuns");
            }
        }




        /// <summary>
        /// Get productss with cursor-based pagination for efficient scrolling
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<IList<Product>>), 200)]
        public async Task<IActionResult> GetProductsWithCursor(
            [FromQuery] string cursor = null,
            [FromQuery] int limit = 20,
            [FromQuery] string direction = "next",
            [FromQuery] string sortBy = "Points",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (products, nextCursor) = await _repository
                    .GetProductsWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                // Enrich each products with additional data
                foreach (var item in products)
                {
                    // Get additional products data using the products's ID
                    var privateRun = item;
                    
                }


                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cursor-based PrivateRuns");
                return StatusCode(500, "An error occurred while retrieving cursor-based PrivateRuns");
            }
        }

        /// <summary>
        /// Get products By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IList<Product>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _repository.GetProductByIdAsync(id, cancellationToken);

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
        /// Update products
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
        public async Task<IActionResult> UpdateProduct(string id, Product model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != model.ProductId)
                return BadRequest("products ID mismatch");

            try
            {
                var products = await _repository.GetProductByIdAsync(id, cancellationToken);

                if (products == null)
                    return NotFound($"products with ID {id} not found");

            

                var success = await _repository.UpdateProductAsync(model, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update products");

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

