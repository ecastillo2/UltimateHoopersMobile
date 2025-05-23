using Domain;
using Domain.DtoModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UltimateHoopers.Models;
using WebAPI.ApiClients;

namespace UltimateHoopers.Services
{
    public class JoinedRunService : IJoinedRunService
    {
        private readonly IJoinedRunApi _joinedRunApi;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JoinedRunService> _logger;
        private const string TOKEN_KEY = "auth_token";
        private readonly string _baseUrl;

        // Constructor with proper DI
        public JoinedRunService(HttpClient httpClient, IConfiguration configuration, ILogger<JoinedRunService> logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;

            // Get base URL from configuration or use default
            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ultimatehoopersapi.azurewebsites.net/";

            // Set base address for HttpClient if not already set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                LogInfo($"Set HttpClient.BaseAddress to {_baseUrl}");
            }

            // Set default headers
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Create the API client last, so it has the properly configured HttpClient
            _joinedRunApi = new JoinedRunApi(_httpClient, _configuration);

            LogInfo($"RunService initialized with base URL: {_baseUrl}");
        }

        // Simplified constructor for non-DI scenarios
        public JoinedRunService()
        {
            _httpClient = new HttpClient();
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ApiSettings:BaseUrl"] = "https://ultimatehoopersapi.azurewebsites.net/"
                })
                .Build();

            _baseUrl = _configuration["ApiSettings:BaseUrl"];

            // Set base address and headers
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Create the API client after HttpClient is configured
            _joinedRunApi = new JoinedRunApi(_httpClient, _configuration);

            LogInfo($"RunService initialized with base URL: {_baseUrl} (non-DI constructor)");
        }


        public async Task<List<JoinedRun>> GetUserJoinedRunsAsync(string ProfileId)
        {
            try
            {
                LogInfo("GetJoinedRunsAsync called");

                // Get token (first from App state, then from secure storage)
                var token = await GetTokenAsync();
                LogInfo($"Token retrieved: {!string.IsNullOrEmpty(token)}");

                if (string.IsNullOrEmpty(token))
                {
                    LogError("No access token available", null);
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Call the API with the retrieved token
                LogInfo($"Calling API with token. Base URL: {_httpClient.BaseAddress}");

                // Additional debug logging for the actual request
                try
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    LogInfo($"Set Authorization header: Bearer {token.Substring(0, Math.Min(10, token.Length))}...");
                }
                catch (Exception ex)
                {
                    LogError("Error setting authorization header", ex);
                }

                try
                {
                    var paginatedResult = await _joinedRunApi.GetUserJoinedRunsAsync(ProfileId, token);

                    LogInfo($"API call completed. Result: {(paginatedResult != null ? "Success" : "Null")}");
                    

                  // Create a list to hold the converted posts
                        var run = new List<JoinedRun>();

                        foreach (var item in paginatedResult)
                        {
                            try
                            {
                                // Map properties from the DTO to a new Run object
                                var runs = new JoinedRun
                                {
                                    RunId = item.RunId,
                                    JoinedRunId = item.JoinedRunId,
                                    ProfileId = item.ProfileId,
                                    Type = item.Type,
                                    

                                };

                                // Fix URLs by ensuring they have a protocol
                                //privateRun.PostFileURL = FixUrl(privateRun.PostFileURL);
                                //privateRun.ThumbnailUrl = FixUrl(privateRun.ThumbnailUrl);
                                //privateRun.ProfileImageURL = FixUrl(privateRun.ProfileImageURL);

                                run.Add(runs);
                                //LogInfo($"Added PrivateRun: {privateRun.PrivateRunId}, URL: {privateRun.PostFileURL}, Type: {post.PostType}");
                            }
                            catch (Exception ex)
                            {
                                LogError($"Error mapping post {item.RunId}", ex);
                                // Continue with the next post instead of failing the entire process
                            }
                        }

                        LogInfo($"Returning {run.Count} posts from API");
                        return run;
                   
                }
                catch (Exception apiEx)
                {
                    LogError("Error calling API", apiEx);
                    // Fall through to return mock data in development mode
                    throw;
                }
            }
            catch (UnauthorizedAccessException)
            {
                LogError("Unauthorized access", null);
                throw; // Rethrow to be handled by the caller
            }
            catch (Exception ex)
            {
                LogError("Error getting posts", ex);

#if DEBUG
                // For development and testing, return mock data if API call fails
                LogInfo("Returning mock PrivateRuns for testing due to API error");
                return null;
#else
                throw;
#endif
            }
        }

        public async Task<bool> RemoveUserJoinRunAsync(string profileId, string runId )
        {
            try
            {
                // Get auth token
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Set token in authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // In a real implementation, you would call the API to leave the run
                // For now, just return true as a placeholder

                // TODO: Implement actual API call when endpoint is available
                // var response = await _runApi.UserLeaveRunAsync(runId, profileId, token);

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error leaving run {runId}", ex);

                // For development, return success to allow testing
#if DEBUG
                Debug.WriteLine("DEBUG MODE: Returning true for LeaveRunAsync despite error");
                return true;
#else
                return false;
#endif
            }
        }

        private async Task<string> GetTokenAsync()
        {
            try
            {
                // First try to get the token from the App's global auth token
                var token = App.AuthToken;
                LogInfo($"App.AuthToken: {!string.IsNullOrEmpty(token)}");

                // If it's not available in the global App state, try to get it from secure storage
                if (string.IsNullOrEmpty(token))
                {
                    token = await SecureStorage.GetAsync(TOKEN_KEY);
                    LogInfo($"SecureStorage token: {!string.IsNullOrEmpty(token)}");
                }

                // For development, provide a fallback token
#if DEBUG
                if (string.IsNullOrEmpty(token))
                {
                    token = "development-token";
                    LogInfo("Using development fallback token");
                }
#endif

                return token;
            }
            catch (Exception ex)
            {
                LogError("Error retrieving token", ex);
                return null;
            }
        }

        private void LogInfo(string message)
        {
            if (_logger != null)
            {
                _logger.LogInformation(message);
            }

            Debug.WriteLine($"[PostService] INFO: {message}");
        }

        private void LogError(string message, Exception ex)
        {
            if (_logger != null && ex != null)
            {
                _logger.LogError(ex, message);
            }
            else if (_logger != null)
            {
                _logger.LogError(message);
            }

            if (ex != null)
            {
                Debug.WriteLine($"[PostService] ERROR: {message}: {ex.Message}");
                Debug.WriteLine($"[PostService] Stack trace: {ex.StackTrace}");
            }
            else
            {
                Debug.WriteLine($"[PostService] ERROR: {message}");
            }
        }

        
    }
}