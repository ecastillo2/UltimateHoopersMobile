using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiClient.Core
{
    /// <summary>
    /// Provides a base implementation for API clients with common functionality
    /// </summary>
    public abstract class BaseApiClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        protected readonly JsonSerializerOptions _jsonOptions;

        protected BaseApiClient(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Performs a GET request to the specified endpoint
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="token">Optional authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The deserialized response</returns>
        protected async Task<T> GetAsync<T>(string endpoint, string token = null, CancellationToken cancellationToken = default)
        {
            try
            {
                PrepareRequest(token);
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error during GET request to {Endpoint}", endpoint);
                throw new ApiException("An error occurred while processing the request", ex);
            }
        }

        /// <summary>
        /// Performs a POST request to the specified endpoint
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="data">The data to send</param>
        /// <param name="token">Optional authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The deserialized response</returns>
        protected async Task<T> PostAsync<T>(string endpoint, object data, string token = null, CancellationToken cancellationToken = default)
        {
            try
            {
                PrepareRequest(token);
                var content = new StringContent(
                    JsonSerializer.Serialize(data, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error during POST request to {Endpoint}", endpoint);
                throw new ApiException("An error occurred while processing the request", ex);
            }
        }

        /// <summary>
        /// Performs a PUT request to the specified endpoint
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="data">The data to send</param>
        /// <param name="token">Optional authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The deserialized response</returns>
        protected async Task<T> PutAsync<T>(string endpoint, object data, string token = null, CancellationToken cancellationToken = default)
        {
            try
            {
                PrepareRequest(token);
                var content = new StringContent(
                    JsonSerializer.Serialize(data, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error during PUT request to {Endpoint}", endpoint);
                throw new ApiException("An error occurred while processing the request", ex);
            }
        }

        /// <summary>
        /// Performs a DELETE request to the specified endpoint
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="token">Optional authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The deserialized response</returns>
        protected async Task<T> DeleteAsync<T>(string endpoint, string token = null, CancellationToken cancellationToken = default)
        {
            try
            {
                PrepareRequest(token);
                var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error during DELETE request to {Endpoint}", endpoint);
                throw new ApiException("An error occurred while processing the request", ex);
            }
        }

        /// <summary>
        /// Prepares the HTTP request headers
        /// </summary>
        /// <param name="token">Authentication token</param>
        private void PrepareRequest(string token)
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Remove existing Authorization header if it exists
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
            }

            // Add Authorization header if token is provided
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
        }

        /// <summary>
        /// Handles the HTTP response
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to</typeparam>
        /// <param name="response">The HTTP response</param>
        /// <returns>The deserialized response</returns>
        private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API returned non-success status code {StatusCode} with content: {Content}",
                    response.StatusCode, content);

                throw new ApiException(response.StatusCode, content);
            }

            if (string.IsNullOrEmpty(content))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing response: {Content}", content);
                throw new ApiException("Failed to deserialize response", ex);
            }
        }
    }
}