using Domain;
using Domain.DtoModel;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace WebAPI.ApiClients
{
    public class ProfileApi : IProfileApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProfileApi(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://ultimatehoopersapi.azurewebsites.net";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<bool> DeleteProfileAsync(string profileId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                // Set authentication header
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Build request URL
                var requestUrl = $"{_baseUrl}/api/Profile/{profileId}";

                // Make the DELETE request
                var response = await _httpClient.DeleteAsync(requestUrl, cancellationToken);

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
                Console.WriteLine($"Error deleting profile: {ex.Message}");
                throw;
            }
        }

        public async Task<Profile> GetProfileByIdAsync(string profileId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                // Set authentication header
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Build request URL
                var requestUrl = $"{_baseUrl}/api/Profile/{profileId}";

                // Make the request
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var profile = JsonSerializer.Deserialize<Profile>(content, _jsonOptions);

                return profile;
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting profile: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateProfileAsync(Profile profile, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                // Set authentication header
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Build request URL
                var requestUrl = $"{_baseUrl}/api/Profile";

                // Serialize the profile object
                var content = JsonSerializer.Serialize(profile, _jsonOptions);
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

        public async Task<bool> UpdateScoutingReportAsync(ScoutingReport model, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                // Set authentication header
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Build request URL
                var requestUrl = $"{_baseUrl}/api/Profile/UpdateScoutingReport";

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

        public async Task<List<Profile>> GetProfilesAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                // Set authentication header
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Build request URL
                var requestUrl = $"{_baseUrl}/api/Profile";

                // Make the request
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var profiles = JsonSerializer.Deserialize<List<Profile>>(content, _jsonOptions);

                return profiles;
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting profiles: {ex.Message}");
                throw;
            }
        }

        public async Task<CursorPaginatedResultDto<ProfileViewModelDto>> GetProfilesWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            string accessToken = null,
            CancellationToken cancellationToken = default)
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
                var requestUrl = $"{_baseUrl}/api/Profile/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                // Make the request
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                return JsonSerializer.Deserialize<CursorPaginatedResultDto<ProfileViewModelDto>>(content, _jsonOptions);
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