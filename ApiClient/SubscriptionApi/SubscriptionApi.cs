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
    /// Implementation of Subscription API client
    /// </summary>
    public class SubscriptionApi : ISubscriptionApi
    {
        private readonly ILogger<SubscriptionApi> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;


        /// <summary>
        /// Subscription Api Constructor
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SubscriptionApi(HttpClient httpClient, IConfiguration configuration, ILogger<SubscriptionApi> logger)
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
        /// Get all Subscriptions
        /// </summary>
        public async Task<List<Subscription>> GetSubscriptionsAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Subscription/GetSubscriptions", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Subscription>>(content, _jsonOptions);
        }

        /// <summary>
        /// Get Subscription by ID
        /// </summary>
        public async Task<Subscription> GetSubscriptionByIdAsync(string subscriptionId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Use the correct route format that matches the [HttpGet("{id}")] attribute
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Subscription/{subscriptionId}", cancellationToken);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Subscription>(content, _jsonOptions);
        }

        /// <summary>
        /// Create a new Subscription
        /// </summary>
        public async Task<HttpResponseMessage> CreateSubscriptionAsync(Subscription model, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/Subscription/CreateSubscription", jsonContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            //var result = JsonSerializer.Deserialize<Response>(content, _jsonOptions);

            return response;
        }

        /// <summary>
        /// Update an existing Subscription
        /// </summary>
        public async Task<bool> UpdateSubscriptionAsync(Subscription model, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/Subscription/UpdateSubscription", jsonContent, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete a Run
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteSubscriptionAsync(string subscriptionId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subscriptionId))
                    return (false, "Subscription ID cannot be null or empty");

                if (string.IsNullOrWhiteSpace(accessToken))
                    return (false, "Access token cannot be null or empty");

                using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/api/Subscription/{subscriptionId}/DeleteSubscriptionAsync");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return (true, string.Empty);
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => (false, $"Subscription {subscriptionId} not found"),
                    HttpStatusCode.Unauthorized => (false, "Unauthorized - invalid access token"),
                    HttpStatusCode.Forbidden => (false, "Forbidden - insufficient permissions"),
                    _ => (false, $"Failed to delete product. Status: {response.StatusCode}, Error: {errorContent}")
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while deleting product {subscriptionId}", subscriptionId);
                return (false, $"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout occurred while deleting product {subscriptionId}", subscriptionId);
                return (false, "Request timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting product {subscriptionId}", subscriptionId);
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

       
    }
}