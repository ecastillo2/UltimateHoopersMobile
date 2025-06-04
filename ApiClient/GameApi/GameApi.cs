using Azure;
using Domain;
using Domain.DtoModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Implementation of Game API client
    /// </summary>
    public class GameApi : IGameApi
    {
        private readonly ILogger<GameApi> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Game Api Constructor
        /// </summary>
        /// <param name="httpClient">HTTP client instance</param>
        /// <param name="configuration">Configuration for API settings</param>
        /// <param name="logger">Logger instance</param>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
        public GameApi(HttpClient httpClient, IConfiguration configuration, ILogger<GameApi> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://ultimatehoopersapi.azurewebsites.net";
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Get all Games
        /// </summary>
        public async Task<List<Game>> GetGamesAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Game/GetGames", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<Game>>(content, _jsonOptions) ?? new List<Game>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all games");
                throw;
            }
        }

        /// <summary>
        /// Get games with cursor-based pagination
        /// </summary>
        public async Task<CursorPaginatedResultDto<GameDetailViewModelDto>> GetGamesWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "CreatedDate",
            string accessToken = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Set authentication header if token is provided
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Build query string
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(cursor))
                    queryParams.Add($"cursor={Uri.EscapeDataString(cursor)}");

                queryParams.Add($"limit={limit}");
                queryParams.Add($"direction={Uri.EscapeDataString(direction)}");
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

                var queryString = string.Join("&", queryParams);
                var requestUrl = $"{_baseUrl}/api/Game/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                // Make the request
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<GameDetailViewModelDto>>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request error while getting games with cursor");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error while getting games with cursor");
                throw;
            }
        }

        /// <summary>
        /// Get Game by ID
        /// </summary>
        public async Task<Game> GetGameByIdAsync(string gameId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId))
                    throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Game/{gameId}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<Game>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game by ID: {GameId}", gameId);
                throw;
            }
        }

        /// <summary>
        /// Get Games by Client ID
        /// </summary>
        public async Task<CursorPaginatedResultDto<GameDetailViewModelDto>> GetGameByClientIdAsync(string clientId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId))
                    throw new ArgumentException("Client ID cannot be null or empty", nameof(clientId));

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Game/{clientId}/GetGameByClientIdAsync", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<GameDetailViewModelDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting games by client ID: {ClientId}", clientId);
                throw;
            }
        }

        /// <summary>
        /// Get Games by Client ID with cursor-based pagination
        /// </summary>
        public async Task<CursorPaginatedResultDto<ProductDetailViewModelDto>> GetGamesByClientIdWithCursorAsync(
            string clientId,
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "CreatedDate",
            string accessToken = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId))
                    throw new ArgumentException("Client ID cannot be null or empty", nameof(clientId));

                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                }

                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(cursor))
                    queryParams.Add($"cursor={Uri.EscapeDataString(cursor)}");

                queryParams.Add($"limit={limit}");
                queryParams.Add($"direction={Uri.EscapeDataString(direction)}");
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

                var queryString = string.Join("&", queryParams);
                var requestUrl = $"{_baseUrl}/api/Game/{clientId}/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<ProductDetailViewModelDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting games by client ID with cursor: {ClientId}", clientId);
                throw;
            }
        }

        /// <summary>
        /// Get Games by Profile ID
        /// </summary>
        public async Task<List<Game>> GetGameByProfileIdAsync(string profileId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId))
                    throw new ArgumentException("Profile ID cannot be null or empty", nameof(profileId));

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Game/{profileId}/GetGameByProfileIdAsync", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<Game>>(content, _jsonOptions) ?? new List<Game>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting games by profile ID: {ProfileId}", profileId);
                throw;
            }
        }

        /// <summary>
        /// Get Games by Profile ID with cursor-based pagination
        /// </summary>
        public async Task<CursorPaginatedResultDto<Game>> GetGamesByProfileIdWithCursorAsync(
            string profileId,
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "CreatedDate",
            string accessToken = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId))
                    throw new ArgumentException("Profile ID cannot be null or empty", nameof(profileId));

                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                }

                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(cursor))
                    queryParams.Add($"cursor={Uri.EscapeDataString(cursor)}");

                queryParams.Add($"limit={limit}");
                queryParams.Add($"direction={Uri.EscapeDataString(direction)}");
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

                var queryString = string.Join("&", queryParams);
                var requestUrl = $"{_baseUrl}/api/Game/{profileId}/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<Game>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting games by profile ID with cursor: {ProfileId}", profileId);
                throw;
            }
        }

        /// <summary>
        /// Create a new Game
        /// </summary>
        public async Task<HttpResponseMessage> CreateGameAsync(Game model, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Game/CreateGame", jsonContent, cancellationToken);
                response.EnsureSuccessStatusCode();

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating game");
                throw;
            }
        }

        /// <summary>
        /// Update an existing Game
        /// </summary>
        public async Task<bool> UpdateGameAsync(Game model, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/api/Game/UpdateGame", jsonContent, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game");
                throw;
            }
        }

        /// <summary>
        /// Delete a Game
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteGameAsync(string gameId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gameId))
                    return (false, "Game ID cannot be null or empty");

                if (string.IsNullOrWhiteSpace(accessToken))
                    return (false, "Access token cannot be null or empty");

                using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/api/Game/{gameId}/DeleteGameAsync");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return (true, string.Empty);
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => (false, $"Game {gameId} not found"),
                    HttpStatusCode.Unauthorized => (false, "Unauthorized - invalid access token"),
                    HttpStatusCode.Forbidden => (false, "Forbidden - insufficient permissions"),
                    _ => (false, $"Failed to delete game. Status: {response.StatusCode}, Error: {errorContent}")
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while deleting game {GameId}", gameId);
                return (false, $"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout occurred while deleting game {GameId}", gameId);
                return (false, "Request timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting game {GameId}", gameId);
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        
        Task<CursorPaginatedResultDto<Game>> IGameApi.GetGamesByClientIdWithCursorAsync(string clientId, string cursor, int limit, string direction, string sortBy, string accessToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

  
}