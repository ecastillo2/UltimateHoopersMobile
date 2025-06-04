using Domain;

namespace DataLayer.DAL.Interface
{
    public interface IGameRepository : IDisposable
    {
        /// <summary>
        /// Get Games with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "CreatedDate", "GameId")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Games and the next cursor value</returns>
        Task<(List<Game> Games, string NextCursor)> GetGamesWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Games By ProfileId with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="profileId">The profile ID to filter games by</param>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "CreatedDate", "GameId")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Games and the next cursor value</returns>
        Task<(List<Game> Games, string NextCursor)> GetGamesByProfileIdWithCursorAsync(
            string profileId,
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Games By Client Id with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="clientId">The client ID to filter games by</param>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "CreatedDate", "GameId")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Games and the next cursor value</returns>
        Task<(List<Game> Games, string NextCursor)> GetGamesByClientIdWithCursorAsync(
            string clientId,
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all games
        /// </summary>
        /// <returns>List of all games with complete information</returns>
        Task<List<Game>> GetGames();

        /// <summary>
        /// Get game by ID
        /// </summary>
        /// <param name="gameId">The ID of the game to retrieve</param>
        /// <returns>The game with the specified ID, including winners and losers</returns>
        Task<Game> GetGameById(string gameId);

        /// <summary>
        /// Get games by client ID
        /// </summary>
        /// <param name="clientId">The client ID to filter games by</param>
        /// <returns>List of games with the specified client ID</returns>
        Task<List<Game>> GetGameByClientId(string clientId);

        /// <summary>
        /// Insert new game
        /// </summary>
        /// <param name="model">The game model to insert</param>
        /// <returns>Task representing the async operation</returns>
        Task InsertGame(Game model);

        /// <summary>
        /// Update existing game
        /// </summary>
        /// <param name="model">The game model with updated information</param>
        /// <returns>Task representing the async operation</returns>
        Task UpdateGame(Game model);

        /// <summary>
        /// Delete game by ID
        /// </summary>
        /// <param name="gameId">The ID of the game to delete</param>
        /// <returns>Task representing the async operation</returns>
        Task DeleteGame(string gameId);

        /// <summary>
        /// Get game history
        /// </summary>
        /// <returns>List of games with player information</returns>
        Task<List<Game>> GetGameHistory();

        /// <summary>
        /// Get games by profile ID
        /// </summary>
        /// <param name="profileId">The profile ID to get games for</param>
        /// <returns>List of games the profile participated in</returns>
        Task<List<Game>> GetGamesByProfileId(string profileId);

        /// <summary>
        /// Save changes to database
        /// </summary>
        /// <returns>Number of entities saved</returns>
        Task<int> Save();
    }
}