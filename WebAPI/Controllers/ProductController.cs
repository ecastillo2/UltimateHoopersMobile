using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis;
using System.Net;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Product Controller
    /// </summary>
    [Route("api/[controller]")]
    
    public class ProductController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IProductRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Product Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public ProductController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.repository = new ProductRepository(context);

        }

        /// <summary>
        /// Retrieves a list of products from the repository.
        /// </summary>
        /// <returns>A list of <see cref="Product"/> objects.</returns>
        [HttpGet("GetProducts")]
        // Uncomment the line below to enable authorization for this endpoint
        // [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                // Fetch the list of products from the repository
                var products = await repository.GetProducts();

                // Return the products with an OK status code
                return Ok(products);
            }
            catch (Exception ex)
            {
                // Log the exception (replace with your preferred logging mechanism)
                // Example: _logger.LogError(ex, "An error occurred while retrieving products.");

                // Return a generic error message with a 500 status code
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving the products. Please try again later.",
                    Error = ex.Message // Consider removing this in production for security
                });
            }
        }


        /// <summary>
        /// Get Product By Id
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
      
        [HttpGet("GetProductById")]
        public async Task<Product> GetProductById(string productId)
        {
            try
            {
                return await repository.GetProductById(productId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create Product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
         [Authorize]
        [HttpPost("CreateProduct")]
        public async Task CreateProduct([FromBody] Product product)
        {
            
            try
            {
                  await  repository.InsertProduct(product);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Update Product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("UpdateProduct")]
        [Authorize]
        public async Task UpdateProduct([FromBody] Product product)
        {

            try
            {
                await repository.UpdateProduct(product);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Delete Product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteProduct")]
        [Authorize]
        public async Task<HttpResponseMessage> DeleteProduct(string productId)
        {
            try
            {
                await repository.DeleteProduct(productId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteProduct");

                return await Task.FromResult(returnMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return await Task.FromResult(returnMessage);
        }
    }
}
