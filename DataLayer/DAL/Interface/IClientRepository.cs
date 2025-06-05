using System.Runtime.CompilerServices;
using Domain;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for PrivateRun repository operations with consistent cancellation token support
    /// </summary>
    public interface IClientRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all PrivateRuns
        /// </summary>
        Task<List<Client>> GetClientsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRuns with offset-based pagination
        /// </summary>
        Task<(List<Client> Clients, int TotalCount, int TotalPages)> GetClientsPaginatedAsync(int page = 1,int pageSize = 20,CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRuns with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "Points", "PlayerNumber", "Status")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Courts and the next cursor value</returns>
        Task<(List<Client> Clients, string NextCursor)> GetClientsWithCursorAsync(string cursor = null,int limit = 20,string direction = "next",string sortBy = "Points",CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all PrivateRuns for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<Client> StreamAllClientsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<Client> GetClientByIdAsync(string privateRunId,CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert Client
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InsertClient(Client client, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<List<Court>> GetCourtByClientIdAsync(string clientId,CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Client Courts Async
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Court>> GetClientCourtsAsync(string clientId,CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a profile
        /// </summary>
        Task<bool> UpdateClientAsync(Client privateRun,CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple Run at once
        /// </summary>
        Task<int> BatchUpdateClientsAsync(IEnumerable<Client> PrivateRuns,CancellationToken cancellationToken = default);

        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}