using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
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
        /// Get products with cursor-based pagination for efficient scrolling
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<ProductViewModelDto>), 200)]
        public async Task<IActionResult> GetProductsWithCursor([FromQuery] string cursor = null,[FromQuery] int limit = 20,[FromQuery] string direction = "next",[FromQuery] string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                var (privateRuns, nextCursor) = await _repository
                    .GetProductsWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                // Create a list to hold our detailed profile view models
                var detailedViewModels = new List<ProductDetailViewModelDto>();


                // Enrich each profile with additional data
                foreach (var item in privateRuns)
                {
                    // Get additional profile data using the profile's ID
                    var privateRun = item;
                   



                    // Create a detailed view model with all the additional data
                    var detailedViewModel = new ProductDetailViewModelDto()
                    {
                        Product = item,
                        
                    };

                    // Add to our list
                    detailedViewModels.Add(detailedViewModel);
                }



                var result = new CursorPaginatedResultDto<ProductDetailViewModelDto>
                {
                    Items = detailedViewModels,
                    NextCursor = nextCursor,
                    //HasMore = !string.IsNullOrEmpty(nextCursor),
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
        /// Create Product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("CreateProduct")]
        //[Authorize]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            try
            {
                await _repository.InsertProduct(product);


                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Adding Product ");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the joined run" });
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
        [HttpPut("UpdateProduct")]
        //[Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(Product model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.ProductId != model.ProductId)
                return BadRequest("products ID mismatch");

            try
            {
                var products = await _repository.GetProductByIdAsync(model.ProductId, cancellationToken);

                if (products == null)
                    return NotFound($"products with ID {model.ProductId} not found");

            

                var success = await _repository.UpdateProductAsync(model, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update products");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", model.ProductId);
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }


        /// <summary>
        /// Get products By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}/DeleteProductAsync")]
        public async Task<IActionResult> DeleteProductAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Product ID cannot be null or empty");
                }

                var result = await _repository.DeleteProductAsync(id, cancellationToken);

                if (!result)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                return NoContent(); // 204 No Content is more appropriate for successful deletion
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Product {ProductId}", id);
                return StatusCode(500, "An error occurred while deleting the product");
            }
        }
    }
}

