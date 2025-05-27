using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using WebAPI;

namespace UnitTest.Integration
{
    public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly HttpClient _client;

        protected IntegrationTestBase(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        protected async Task<T> GetAsync<T>(string requestUri)
        {
            var response = await _client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<HttpResponseMessage> PostAsync<T>(string requestUri, T data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _client.PostAsync(requestUri, content);
        }
    }
}