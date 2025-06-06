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
    /// Request Repository
    /// </summary>
    public class RequestRepository : IRequestRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<RequestRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;
        private readonly string _blobBaseUrl;

        /// <summary>
        /// Request Repository
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RequestRepository(ApplicationContext context, IConfiguration configuration, ILogger<RequestRepository> logger = null)
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
        public async Task<List<Request>> GetRequestsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Request
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
        /// Get Requests Paginated Async
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(List<Request> Requests, int TotalCount, int TotalPages)> GetRequestsPaginatedAsync(int page = 1,int pageSize = 20,CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            try
            {
                var totalCount = await _context.Request.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var requests = await _context.Request
                    .AsNoTracking()

                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (requests, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving paginated Clients");
                throw;
            }
        }

        /// <summary>
        /// Get Requests With CursorAsync
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(List<Request> Requests, string NextCursor)> GetRequestsWithCursorAsync(string cursor = null,int limit = 20,string direction = "next",string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                // Default query starting point
                IQueryable<Request> query = _context.Request.AsNoTracking();

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
                    var newCursorData = new RequestCursorData
                    {
                        Id = lastItem.RequestId,


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
        /// Get Request By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Request> GetRequestByIdAsync(string requestId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Request
                    .AsNoTracking()

                    .FirstOrDefaultAsync(p => p.RequestId == requestId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Request {RequestId}", requestId);
                throw;
            }
        }

        /// <summary>
        /// Get Request By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRequestAsync(string requestId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                _logger?.LogWarning("DeleteRequestAsync called with null or empty Requestd");
                return false;
            }

            try
            {
                // Find the product with related entities
                var request = await _context.Request
                    //.Include(p => p.Reviews) // Include related entities if any
                    //.Include(p => p.Categories)
                    .FirstOrDefaultAsync(p => p.RequestId == requestId, cancellationToken);

                if (request == null)
                {
                    _logger?.LogWarning("Product with ID {ProductId} not found for deletion", requestId);
                    return false;
                }

                // Delete related entities first (if cascade delete is not configured)
                //if (product.Reviews?.Any() == true)
                //{
                //    _context.Reviews.RemoveRange(product.Reviews);
                //}

                // Remove the product
                _context.Request.Remove(request);

                // Save all changes - EF Core automatically wraps this in a transaction
                var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

                if (rowsAffected > 0)
                {
                    _logger?.LogInformation("Successfully deleted Request {RequestId}", requestId);
                    return true;
                }
                else
                {
                    _logger?.LogWarning("No rows affected when deleting Request {RequestId}", requestId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting Request {RequestId}", requestId);
                throw;
            }
        }

        /// <summary>
        /// Insert Request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task InsertRequest(Request model, CancellationToken cancellationToken = default)
        {
            try
            {


                // Add to context and save
                await _context.Request.AddAsync(model);
                await SaveChangesAsync();

                _logger?.LogInformation("Successfully inserted Request with ID: {RequestId}", model.RequestId);
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
        public async Task<bool> UpdateRequestAsync(Request request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve the existing product from database
                var existingitem = await _context.Request
                    .FirstOrDefaultAsync(p => p.RequestId == request.RequestId, cancellationToken);

                if (existingitem == null)
                {
                    return false;
                }

                // Update only the fields you want to allow updates for
                existingitem.Status = request.Status;
              
                // Don't update sensitive fields like CreatedDate, CreatedBy, etc.
                // existingProduct.CreatedDate = product.CreatedDate; // DON'T update

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating Product {RequestId}", request.RequestId);
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

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
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

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// DisposeAsync
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// BatchUpdateProductsAsync
        /// </summary>
        /// <param name="products"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<int> BatchUpdateProductsAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetProductsAsync
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stream All Products Async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IAsyncEnumerable<Product> StreamAllProductsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }


        #endregion
    }

    /// <summary>
    /// Helper class for cursor-based pagination
    /// </summary>
    internal class RequestCursorData
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public string points { get; set; }
    }
}