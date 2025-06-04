using Domain;
using Domain.DtoModel;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Interface for Game API client operations
    /// </summary>
    public interface IGameApi
    {
        /// <summary>
        /// Get all games
        /// </summary>
        /// <param name="accessToken">Bearer access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of games</returns>
        Task<List<Game>> GetGamesAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get games with cursor-based pagination
        /// </summary>
        /// <param name="cursor">Cursor for pagination</param>
        /// <param name="limit">Number of items to return</param>
        /// <param name="direction">Direction of pagination (next/previous)</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="accessToken">Bearer access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated games result</returns>
        Task<CursorPaginatedResultDto<GameDetailViewModelDto>> GetGamesWithCursorAsync(
            string cursor = null, 
            int limit = 20, 
            string direction = "next", 
            string sortBy = "CreatedDate", 
            string accessToken = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get game by ID
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <param name="accessToken">Bearer access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Game details</returns>
        Task<Game> GetGameByIdAsync(string gameId, string accessToken, CancellationToken cancellationToken = default);

        

        /// <summary>
        /// Get games by client ID with cursor-based pagination
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="cursor">Cursor for pagination</param>
        /// <param name="limit">Number of items to return</param>
        /// <param name="direction">Direction of pagination (next/previous)</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="accessToken">Bearer access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated games result for the client</returns>
        Task<CursorPaginatedResultDto<Game>> GetGamesByClientIdWithCursorAsync(
            string clientId,
            string cursor = null, 
            int limit = 20, 
            string direction = "next", 
            string sortBy = "CreatedDate", 
            string accessToken = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get games by profile ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="accessToken">Bearer access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of games for the profile</returns>
        Task<List<Game>> GetGameByProfileIdAsync(string profileId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get games by profile ID with cursor-based pagination
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="cursor">Cursor for pagination</param>
        /// <param name="limit">Number of items to return</param>
        /// <param name="direction">Direction of pagination (next/previous)</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="accessToken">Bearer access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated games result for the profile</returns>
        Task<CursorPaginatedResultDto<Game>> GetGamesByProfileIdWithCursorAsync(
            string profileId,
            string cursor = null, 
            int limit = 20, 
            string direction = "next", 
            string sortBy = "CreatedDate", 
            string accessToken = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new game
        /// </summary>
        /// <param name="model">Game model to create</param>
        /// <param name="accessToken">Bearer access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>HTTP response message</returns>
        Task<HttpResponseMessage> CreateGameAsync(Game model, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing game
        /// </summary>
        /// <param name="model">Game model with updates</param>
        /// <param name="accessToken">Bearer access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateGameAsync(Game model, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a game
        /// </summary>
        /// <param name="gameId">Game ID to delete</param>
        /// <param name="accessToken">Bearer access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status and error message if any</returns>
        Task<(bool Success, string ErrorMessage)> DeleteGameAsync(string gameId, string accessToken, CancellationToken cancellationToken = default);
    }
}