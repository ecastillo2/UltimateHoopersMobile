using System.Runtime.CompilerServices;
using DataLayer.Context;
using DataLayer.DAL.Interface;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataLayer.DAL.Repository
{
    /// <summary>
    /// Implementation of the PrivateRun repository with optimized query methods
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<ProductRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;
        private readonly string _blobBaseUrl;

        public ProductRepository(ApplicationContext context, IConfiguration configuration, ILogger<ProductRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _blobBaseUrl = configuration["BlobStorage:ImageBaseUrl"] ?? "https://ultimatehoopersapi.azurewebsites.net";
            _logger = logger;
        }

        /// <summary>
        /// Get Products Async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Product>> GetCProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Product
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving Runs");
                throw;
            }
        }

        /// <summary>
        /// Get Clients Paginated Async
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(List<Product> Products, int TotalCount, int TotalPages)> GetProductsPaginatedAsync(int page = 1,int pageSize = 20,CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            try
            {
                var totalCount = await _context.Product.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var products = await _context.Product
                    .AsNoTracking()

                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (products, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving paginated Clients");
                throw;
            }
        }

        /// <summary>
        /// Get Products With CursorAsync
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(List<Product> Products, string NextCursor)> GetProductsWithCursorAsync(string cursor = null,int limit = 20,string direction = "next",string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                // Default query starting point
                IQueryable<Product> query = _context.Product.AsNoTracking();

                // Parse the cursor if provided
                CursorData cursorData = null;
                if (!string.IsNullOrEmpty(cursor))
                {
                    try
                    {
                        // Decode and deserialize cursor
                        var decodedCursor = System.Text.Encoding.UTF8.GetString(
                            Convert.FromBase64String(cursor));
                        cursorData = System.Text.Json.JsonSerializer.Deserialize<CursorData>(decodedCursor);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Invalid cursor format. Starting from beginning");
                        // If cursor parsing fails, ignore and start from beginning
                        cursorData = null;
                    }
                }


                // Execute query with limit
                var privateRuns = await query.Take(limit + 1).ToListAsync(cancellationToken);

                // Check if we have a next page by fetching limit+1 items
                string nextCursor = null;
                if (privateRuns.Count > limit)
                {
                    // Remove the extra item we retrieved to check for "has next page"
                    var lastItem = privateRuns[limit];
                    privateRuns.RemoveAt(limit);

                    // Create cursor for next page based on last item properties
                    var newCursorData = new ProductCursorData
                    {
                        Id = lastItem.ProductId,


                    };

                    var serialized = System.Text.Json.JsonSerializer.Serialize(newCursorData);
                    nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serialized));
                }

                // If we requested previous direction and got results, we need to reverse the order
                if (direction.ToLowerInvariant() == "previous" && privateRuns.Any())
                {
                    privateRuns.Reverse();
                }

                return (privateRuns, nextCursor);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Runs with cursor");
                throw;
            }
        }

        /// <summary>
        /// Get Product By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Product> GetProductByIdAsync(string productId,CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Product
                    .AsNoTracking()

                    .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Product {ProductId}", productId);
                throw;
            }
        }



        /// <summary>
        /// Get Product By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteProductAsync(string productId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                _logger?.LogWarning("DeleteProductAsync called with null or empty productId");
                return false;
            }

            try
            {
                // Find the product with related entities
                var product = await _context.Product
                    //.Include(p => p.Reviews) // Include related entities if any
                    //.Include(p => p.Categories)
                    .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

                if (product == null)
                {
                    _logger?.LogWarning("Product with ID {ProductId} not found for deletion", productId);
                    return false;
                }

                // Delete related entities first (if cascade delete is not configured)
                //if (product.Reviews?.Any() == true)
                //{
                //    _context.Reviews.RemoveRange(product.Reviews);
                //}

                // Remove the product
                _context.Product.Remove(product);

                // Save all changes - EF Core automatically wraps this in a transaction
                var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

                if (rowsAffected > 0)
                {
                    _logger?.LogInformation("Successfully deleted product {ProductId}", productId);
                    return true;
                }
                else
                {
                    _logger?.LogWarning("No rows affected when deleting product {ProductId}", productId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting product {ProductId}", productId);
                throw;
            }
        }


        /// <summary>
        /// Insert Product
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task InsertProduct(Product model, CancellationToken cancellationToken = default)
        {
            try
            {

                model.ImageURL = $"{_blobBaseUrl}{model.ProductId}.webp";


                // Add to context and save
                await _context.Product.AddAsync(model);
                await SaveChangesAsync();

                _logger?.LogInformation("Successfully inserted Product with ID: {JoinedRunId}", model.ProductId);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Update Client Async
        /// </summary>
        /// <param name="privateRun"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> UpdateProductAsync(Product product,CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve the existing product from database
                var existingProduct = await _context.Product
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId, cancellationToken);

                if (existingProduct == null)
                {
                    return false;
                }

                // Update only the fields you want to allow updates for
                existingProduct.Title = product.Title;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Points = product.Points;
                existingProduct.Type = product.Type;
                existingProduct.Category = product.Category;
                existingProduct.Status = product.Status;
                existingProduct.ProductNumber = product.ProductNumber;
                existingProduct.Tag = product.Tag;

                // Don't update sensitive fields like CreatedDate, CreatedBy, etc.
                // existingProduct.CreatedDate = product.CreatedDate; // DON'T update

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating Product {ProductId}", product.ProductId);
                throw;
            }
        }

        /// <summary>
        /// Save Changes Async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        #region IDisposable and IAsyncDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public Task<int> BatchUpdateProductsAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

      

        public Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<Product> StreamAllProductsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }


        #endregion
    }

    /// <summary>
    /// Helper class for cursor-based pagination
    /// </summary>
    internal class ProductCursorData
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public string points { get; set; }
    }
}