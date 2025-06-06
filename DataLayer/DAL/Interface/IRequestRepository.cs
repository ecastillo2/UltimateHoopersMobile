using System.Runtime.CompilerServices;
using Domain;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for Product repository operations with consistent cancellation token support
    /// </summary>
    public interface IRequestRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all Products
        /// </summary>
        Task<List<Request>> GetRequestsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Products with offset-based pagination
        /// </summary>
        Task<(List<Request> Requests, int TotalCount, int TotalPages)> GetRequestsPaginatedAsync(int page = 1,int pageSize = 20,CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Requests with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "Points", "PlayerNumber", "Status")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Courts and the next cursor value</returns>
        Task<(List<Request> Requests, string NextCursor)> GetRequestsWithCursorAsync(string cursor = null,int limit = 20,string direction = "next",string sortBy = "Points",CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all PrivateRuns for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<Request> StreamAllRequestsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Product by ID
        /// </summary>
        Task<Request> GetRequestByIdAsync(string requestId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert Product
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InsertRequest(Request request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Request by ID
        /// </summary>
        Task<bool> DeleteRequestAsync(string requestId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Request Async
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateRequestAsync(Request request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple Run at once
        /// </summary>
        Task<int> BatchUpdateRequestsAsync(IEnumerable<Request> request, CancellationToken cancellationToken = default);


        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}