using Domain;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiClient
{
    /// <summary>
    /// Generic HTTP client for making API requests
    /// </summary>
    public class WebClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the WebClient class
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        public WebClient(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClient = new HttpClient();

            // Get base URL from configuration
            _baseUrl = _configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrEmpty(_baseUrl))
            {
                _baseUrl = "https://ultimatehoopersapi.azurewebsites.net/";
            }

            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        /// <summary>
        /// Makes a GET request to the specified endpoint
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Deserialized response</returns>
        public async Task<T> GetAsync<T>(string endpoint, string token = null)
        {
            ConfigureHttpClient(token);

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GET request: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Makes a POST request to the specified endpoint
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Deserialized response</returns>
        public async Task<T> PostAsync<T>(string endpoint, object data, string token = null)
        {
            ConfigureHttpClient(token);

            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in POST request: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Makes a PUT request to the specified endpoint
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Deserialized response</returns>
        public async Task<T> PutAsync<T>(string endpoint, object data, string token = null)
        {
            ConfigureHttpClient(token);

            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PUT request: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Makes a DELETE request to the specified endpoint
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Deserialized response</returns>
        public async Task<T> DeleteAsync<T>(string endpoint, string token = null)
        {
            ConfigureHttpClient(token);

            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DELETE request: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Configures HTTP client with authentication token if provided
        /// </summary>
        /// <param name="token">Authentication token</param>
        private void ConfigureHttpClient(string token)
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        /// <summary>
        /// Handles HTTP response and deserializes content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="response">HTTP response</param>
        /// <returns>Deserialized response</returns>
        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"API request failed: {response.StatusCode}, {content}");
            }

            if (string.IsNullOrEmpty(content))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}