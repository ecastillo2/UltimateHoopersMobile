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
    /// Subscription Repository
    /// </summary>
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<SubscriptionRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;
        private readonly string _blobBaseUrl;

        /// <summary>
        /// Product Repository
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SubscriptionRepository(ApplicationContext context, IConfiguration configuration, ILogger<SubscriptionRepository> logger = null)
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
        public async Task<List<Subscription>> GetSubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Subscription
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
        /// Get Subscription By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Subscription> GetSubscriptionByIdAsync(string subscriptionId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Subscription
                    .AsNoTracking()

                    .FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        /// <summary>
        /// Get Product By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                _logger?.LogWarning("DeleteProductAsync called with null or empty productId");
                return false;
            }

            try
            {
                // Find the product with related entities
                var subscription = await _context.Subscription
                    //.Include(p => p.Reviews) // Include related entities if any
                    //.Include(p => p.Categories)
                    .FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId, cancellationToken);

                if (subscription == null)
                {
                    _logger?.LogWarning("Subscription with ID {SubscriptionId} not found for deletion", subscriptionId);
                    return false;
                }

                // Delete related entities first (if cascade delete is not configured)
                //if (product.Reviews?.Any() == true)
                //{
                //    _context.Reviews.RemoveRange(product.Reviews);
                //}

                // Remove the product
                _context.Subscription.Remove(subscription);

                // Save all changes - EF Core automatically wraps this in a transaction
                var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

                if (rowsAffected > 0)
                {
                    _logger?.LogInformation("Successfully deleted product {ProductId}", subscriptionId);
                    return true;
                }
                else
                {
                    _logger?.LogWarning("No rows affected when deleting product {ProductId}", subscriptionId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting product {ProductId}", subscriptionId);
                throw;
            }
        }

        /// <summary>
        /// Insert Subscription
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task InsertSubscription(Subscription model, CancellationToken cancellationToken = default)
        {
            try
            {

              

                // Add to context and save
                await _context.Subscription.AddAsync(model);
                await SaveChangesAsync();

                _logger?.LogInformation("Successfully inserted Subscription with ID: {SubscriptionId}", model.SubscriptionId);
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
        public async Task<bool> UpdateSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve the existing product from database
                var existingItem = await _context.Subscription
                    .FirstOrDefaultAsync(p => p.SubscriptionId == subscription.SubscriptionId, cancellationToken);

                if (existingItem == null)
                {
                    return false;
                }

                // Update only the fields you want to allow updates for
                existingItem.SubscriptionId = subscription.SubscriptionId;
                existingItem.Name = subscription.Name;
                existingItem.Price = subscription.Price;
                existingItem.PaymentFrequency = subscription.PaymentFrequency;
  

                // Don't update sensitive fields like CreatedDate, CreatedBy, etc.
                // existingProduct.CreatedDate = product.CreatedDate; // DON'T update

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating Product {SubscriptionId}", subscription.SubscriptionId);
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

   
}