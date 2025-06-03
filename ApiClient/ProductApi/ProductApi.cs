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
    /// Implementation of Product API client
    /// </summary>
    public class ProductApi : IProductApi
    {
        private readonly ILogger<ProductApi> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;


        /// <summary>
        /// Product Api Constructor
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ProductApi(HttpClient httpClient, IConfiguration configuration, ILogger<ProductApi> logger)
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
        /// Get all Products
        /// </summary>
        public async Task<List<Product>> GetProductsAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Product/GetProducts", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Product>>(content, _jsonOptions);
        }

        /// <summary>
        /// Get Product by ID
        /// </summary>
        public async Task<Product> GetProductByIdAsync(string productId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Use the correct route format that matches the [HttpGet("{id}")] attribute
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Product/{productId}", cancellationToken);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Product>(content, _jsonOptions);
        }



        /// <summary>
        /// Create a new Product
        /// </summary>
        public async Task<HttpResponseMessage> CreateProductAsync(Product model, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/Product/CreateProduct", jsonContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            //var result = JsonSerializer.Deserialize<Response>(content, _jsonOptions);

            return response;
        }

        /// <summary>
        /// Update an existing Product
        /// </summary>
        public async Task<bool> UpdateProductAsync(Product model, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/Product/UpdateProduct", jsonContent, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete a Run
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteProductAsync(string productId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                    return (false, "Product ID cannot be null or empty");

                if (string.IsNullOrWhiteSpace(accessToken))
                    return (false, "Access token cannot be null or empty");

                using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/api/Product/{productId}/DeleteProductAsync");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return (true, string.Empty);
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => (false, $"Product {productId} not found"),
                    HttpStatusCode.Unauthorized => (false, "Unauthorized - invalid access token"),
                    HttpStatusCode.Forbidden => (false, "Forbidden - insufficient permissions"),
                    _ => (false, $"Failed to delete product. Status: {response.StatusCode}, Error: {errorContent}")
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while deleting product {ProductId}", productId);
                return (false, $"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout occurred while deleting product {ProductId}", productId);
                return (false, "Request timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting product {ProductId}", productId);
                return (false, $"Unexpected error: {ex.Message}");
            }
        }


        public async Task<CursorPaginatedResultDto<ProductDetailViewModelDto>> GetProductsWithCursorAsync(string cursor = null, int limit = 20, string direction = "next", string sortBy = "Points", string accessToken = null, CancellationToken cancellationToken = default)
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
                var requestUrl = $"{_baseUrl}/api/Product/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                // Make the request
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<ProductDetailViewModelDto>>(content, _jsonOptions);
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