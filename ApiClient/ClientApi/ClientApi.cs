using Domain;
using Domain.DtoModel;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Implementation of Client API client with organized CRUD operations
    /// </summary>
    public class ClientApi : IClientApi
    {
        #region Fields and Constructor

        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ClientApi(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://ultimatehoopersapi.azurewebsites.net";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #endregion

        #region CREATE Operations

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="model">Client model to create</param>
        /// <param name="accessToken">Bearer token for authentication</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>HTTP response message</returns>
        public async Task<HttpResponseMessage> CreateClientAsync(Client model, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthorizationHeader(accessToken);

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Client/CreateClient", jsonContent, cancellationToken);
                response.EnsureSuccessStatusCode();

                return response;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error during client creation: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON serialization error during client creation: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region READ Operations

        /// <summary>
        /// Get all clients
        /// </summary>
        /// <param name="accessToken">Bearer token for authentication</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of clients</returns>
        public async Task<List<Client>> GetClientsAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthorizationHeader(accessToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Client/GetClients", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<Client>>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error during clients retrieval: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error during clients retrieval: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a specific client by ID
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="accessToken">Bearer token for authentication</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Client object</returns>
        public async Task<Client> GetClientByIdAsync(string clientId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthorizationHeader(accessToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Client/{clientId}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<Client>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error during client retrieval: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error during client retrieval: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get clients with cursor-based pagination
        /// </summary>
        /// <param name="cursor">Pagination cursor</param>
        /// <param name="limit">Number of items to retrieve</param>
        /// <param name="direction">Pagination direction (next/previous)</param>
        /// <param name="sortBy">Sort field</param>
        /// <param name="accessToken">Bearer token for authentication</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated result with client details</returns>
        public async Task<CursorPaginatedResultDto<ClientDetailViewModelDto>> GetClientsWithCursorAsync(string cursor = null,int limit = 20,string direction = "next",string sortBy = "Points",string accessToken = null,CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthorizationHeader(accessToken);

                var queryParams = BuildQueryParameters(cursor, limit, direction, sortBy);
                var queryString = string.Join("&", queryParams);
                var requestUrl = $"{_baseUrl}/api/Client/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<ClientDetailViewModelDto>>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error during paginated clients retrieval: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error during paginated clients retrieval: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get courts associated with a specific client
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="accessToken">Bearer token for authentication</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of courts</returns>
        public async Task<List<Court>> GetClientCourtsAsync(string clientId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthorizationHeader(accessToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Client/{clientId}/courts", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<Court>>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error during client courts retrieval: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error during client courts retrieval: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update an existing client
        /// </summary>
        /// <param name="model">Updated client model</param>
        /// <param name="accessToken">Bearer token for authentication</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateClientAsync(Client model, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthorizationHeader(accessToken);

                var content = JsonSerializer.Serialize(model, _jsonOptions);
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/api/Client/UpdateClient", httpContent, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error during client update: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON serialization error during client update: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete a client by ID
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="accessToken">Bearer token for authentication</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DeleteClientAsync(string clientId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthorizationHeader(accessToken);

                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/Client/DeleteClient?runId={clientId}", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error during client deletion: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Set authorization header for HTTP client
        /// </summary>
        /// <param name="accessToken">Bearer token</param>
        private void SetAuthorizationHeader(string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        /// <summary>
        /// Build query parameters for cursor pagination
        /// </summary>
        /// <param name="cursor">Pagination cursor</param>
        /// <param name="limit">Number of items</param>
        /// <param name="direction">Pagination direction</param>
        /// <param name="sortBy">Sort field</param>
        /// <returns>List of query parameters</returns>
        private static List<string> BuildQueryParameters(string cursor, int limit, string direction, string sortBy)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(cursor))
                queryParams.Add($"cursor={Uri.EscapeDataString(cursor)}");

            queryParams.Add($"limit={limit}");
            queryParams.Add($"direction={Uri.EscapeDataString(direction)}");
            queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

            return queryParams;
        }

        #endregion
    }
}