using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for Product repository operations with consistent cancellation token support
    /// </summary>
    public interface IProductRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all Products
        /// </summary>
        Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Products with offset-based pagination
        /// </summary>
        Task<(List<Product> Products, int TotalCount, int TotalPages)> GetProductsPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Products with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "Points", "PlayerNumber", "Status")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Courts and the next cursor value</returns>
        Task<(List<Product> Products, string NextCursor)> GetProductsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all PrivateRuns for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<Product> StreamAllProductsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default);


        /// <summary>
        /// Get Product by ID
        /// </summary>
        Task<Product> GetProductByIdAsync(
            string privateRunId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert Product
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InsertProduct(Product product,CancellationToken cancellationToken = default);



        /// <summary>
        /// Update Product Async
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateProductAsync(Product product,CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple Run at once
        /// </summary>
        Task<int> BatchUpdateProductsAsync(
            IEnumerable<Product> product,
            CancellationToken cancellationToken = default);


        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}