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
using UltimateHoopers.Models;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Implementation of Client API client
    /// </summary>
    public class UserApi : IUserApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserApi(HttpClient httpClient, IConfiguration configuration)
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
        /// Get all Clients
        /// </summary>
        public async Task<List<User>> GetUsersAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/User/GetUsers", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<User>>(content, _jsonOptions);
        }

        /// <summary>
        /// Get Run by ID
        /// </summary>
        public async Task<User> GetUserByIdAsync(string Id, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Use the correct route format that matches the [HttpGet("{id}")] attribute
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/User/{Id}", cancellationToken);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<User>(content, _jsonOptions);
        }

        /// <summary>
        /// Get Run by ID
        /// </summary>
        public async Task<Profile> GetProfileByUserId(string Id, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Use the correct route format that matches the [HttpGet("{id}")] attribute
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/User/{Id}/profile", cancellationToken);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Profile>(content, _jsonOptions);
        }



        /// <summary>
        /// Create a new Run
        /// </summary>
        public async Task<User> CreateUserAsync(User client, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(client, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/User/CreateUser", jsonContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<User>(content, _jsonOptions);
        }

       

        /// <summary>
        /// Delete a Run
        /// </summary>
        public async Task<bool> DeleteUserAsync(string Id, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/User/DeleteUser?Id={Id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }


        public async Task<CursorPaginatedResultDto<UserDetailViewModelDto>> GetUsersWithCursorAsync(string cursor = null, int limit = 20, string direction = "next", string sortBy = "Points", string accessToken = null, CancellationToken cancellationToken = default)
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
                var requestUrl = $"{_baseUrl}/api/User/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                // Make the request
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<UserDetailViewModelDto>>(content, _jsonOptions);
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

       
        public Task<bool> UpdateUserAsync(User run, string accessToken, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}