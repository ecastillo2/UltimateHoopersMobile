using Domain;
using Domain.DtoModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Implementation of PrivateRun API client
    /// </summary>
    public class PrivateRunApi : IPrivateRunApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public PrivateRunApi(HttpClient httpClient, IConfiguration configuration)
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
        /// Get all PrivateRuns
        /// </summary>
        public async Task<List<PrivateRun>> GetPrivateRunsAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/PrivateRun/GetPrivateRuns", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<PrivateRun>>(content, _jsonOptions);
        }



        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        public async Task<PrivateRun> GetPrivateRunByIdAsync(string privateRunId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/PrivateRun/GetPrivateRunById?privateRunId={privateRunId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<PrivateRun>(content, _jsonOptions);
        }

        /// <summary>
        /// Create a new PrivateRun
        /// </summary>
        public async Task<PrivateRun> CreatePrivateRunAsync(PrivateRun privateRun, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(privateRun, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/PrivateRun/CreatePrivateRun", jsonContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<PrivateRun>(content, _jsonOptions);
        }

        /// <summary>
        /// Update an existing PrivateRun
        /// </summary>
        public async Task<bool> UpdatePrivateRunAsync(PrivateRun privateRun, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(privateRun, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/PrivateRun/UpdatePrivateRun", jsonContent, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete a PrivateRun
        /// </summary>
        public async Task<bool> DeletePrivateRunAsync(string privateRunId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/PrivateRun/DeletePrivateRun?privateRunId={privateRunId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }



        public async Task<CursorPaginatedResultDto<PrivateRunViewModelDto>> GetPrivateRunsWithCursorAsync(string cursor = null, int limit = 20, string direction = "next", string sortBy = "Points", string accessToken = null, CancellationToken cancellationToken = default)
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
                var requestUrl = $"{_baseUrl}/api/PrivateRun/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                // Make the request
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<PrivateRunViewModelDto>>(content, _jsonOptions);
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
    }
}