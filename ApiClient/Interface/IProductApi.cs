using Domain;
using Domain.DtoModel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UltimateHoopers.Models;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Interface for Run API operations
    /// </summary>
    public interface IProductApi
    {
        /// <summary>
        /// Get all Runs
        /// </summary>
        Task<List<Product>> GetProductsAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Product DetailViewModelDto
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<CursorPaginatedResultDto<ProductDetailViewModelDto>> GetProductsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            string accessToken = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Product By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Product> GetProductByIdAsync(string runId, string accessToken, CancellationToken cancellationToken = default);

      
        /// <summary>
        /// Create Client Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Product> CreateProductAsync(Product product, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Client Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateProductAsync(Product product, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete Product Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> DeleteProductAsync(string productId, string accessToken, CancellationToken cancellationToken = default);

        
    }
}