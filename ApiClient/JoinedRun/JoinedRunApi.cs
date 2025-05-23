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
    /// Implementation of Run API client
    /// </summary>
    public class JoinedRunApi : IJoinedRunApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public JoinedRunApi(HttpClient httpClient, IConfiguration configuration)
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
        /// Get Run by ID
        /// </summary>
        public async Task<List<JoinedRunDetailViewModelDto>> GetUserJoinedRunsAsync(string profileId, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/JoinedRun/GetUserJoinedRunsAsync/{profileId}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<JoinedRunDetailViewModelDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

       
        /// Delete a Run
        /// </summary>
        public async Task<bool> RemoveUserJoinRunAsync(string profileId, string runId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/JoinedRun/RemoveUserJoinRunAsync?profileId={profileId}&runId={runId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }


       
    }
}