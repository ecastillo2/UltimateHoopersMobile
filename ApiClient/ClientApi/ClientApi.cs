using Domain;
using Domain.DtoModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UltimateHoopers.Models;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Implementation of Client API client
    /// </summary>
    public class ClientApi : IClientApi
    {
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

        /// <summary>
        /// Get Clients Async
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Client>> GetClientsAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Client/GetClients", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Client>>(content, _jsonOptions);
        }

        /// <summary>
        /// Get Run by ID
        /// </summary>
        public async Task<Client> GetClientByIdAsync(string clientId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Use the correct route format that matches the [HttpGet("{id}")] attribute
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Client/{clientId}", cancellationToken);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Client>(content, _jsonOptions);
        }

        /// <summary>
        /// Get Run by ID
        /// </summary>
        public async Task<List<Court>> GetClientCourtsAsync(string clientId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Client/{clientId}/courts", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Court>>(content, _jsonOptions);
        }

        /// <summary>
        /// Create a new Run
        /// </summary>
        public async Task<Client> CreateClientAsync(Client client, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(client, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/Client/CreateClient", jsonContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Client>(content, _jsonOptions);
        }

        /// <summary>
        /// Update an existing Run
        /// </summary>
        public async Task<bool> UpdateRunAsync(Run run, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(run, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/Client/UpdateClient", jsonContent, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete a Run
        /// </summary>
        public async Task<bool> DeleteClientAsync(string clientId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/Client/DeleteClient?runId={clientId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }


        public async Task<CursorPaginatedResultDto<ClientDetailViewModelDto>> GetClientsWithCursorAsync(string cursor = null, int limit = 20, string direction = "next", string sortBy = "Points", string accessToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Set authentication header if token is provided
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Build query string
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(cursor))
                    queryParams.Add($"cursor={Uri.EscapeDataString(cursor)}");

                queryParams.Add($"limit={limit}");
                queryParams.Add($"direction={Uri.EscapeDataString(direction)}");
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

                var queryString = string.Join("&", queryParams);
                var requestUrl = $"{_baseUrl}/api/Client/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                // Make the request
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<ClientDetailViewModelDto>>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateClientAsync(Client model, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                // Set authentication header
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Build request URL
                var requestUrl = $"{_baseUrl}/api/Client/UpdateClient";

                // Serialize the profile object
                var content = JsonSerializer.Serialize(model, _jsonOptions);
                var httpContent = new StringContent(content, System.Text.Encoding.UTF8, "application/json");

                // Make the PUT request
                var response = await _httpClient.PutAsync(requestUrl, httpContent, cancellationToken);

                // Return true if successful, false otherwise
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                throw;
            }
        }
    }
}