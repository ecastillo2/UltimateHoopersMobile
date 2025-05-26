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
    public class RunService : IRunService
    {
        private readonly IRunApi _runApi;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RunService> _logger;
        private const string TOKEN_KEY = "auth_token";
        private readonly string _baseUrl;

        // Constructor with proper DI
        public RunService(HttpClient httpClient, IConfiguration configuration, ILogger<RunService> logger = null)
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
            _runApi = new RunApi(_httpClient, _configuration);

            LogInfo($"RunService initialized with base URL: {_baseUrl}");
        }

        // Simplified constructor for non-DI scenarios
        public RunService()
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
            _runApi = new RunApi(_httpClient, _configuration);

            LogInfo($"RunService initialized with base URL: {_baseUrl} (non-DI constructor)");
        }

        public async Task<Run> CreateRunAsync(Run run)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Implementation logic
                throw new NotImplementedException("CreateRunAsync not implemented yet");
            }
            catch (Exception ex)
            {
                LogError("Error creating Run", ex);
                throw;
            }
        }

        public async Task<bool> DeleteRunAsync(string runId)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Implementation logic
                throw new NotImplementedException("DeleteRunAsync not implemented yet");
            }
            catch (Exception ex)
            {
                LogError($"Error deleting Run {runId}", ex);
                throw;
            }
        }

        public async Task<Run> GetRunByIdAsync(string runId)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Implementation logic
                throw new NotImplementedException("GetRunByIdAsync not implemented yet");
            }
            catch (Exception ex)
            {
                LogError($"Error getting Run {runId}", ex);
                throw;
            }
        }

        public async Task<List<Run>> GetRunsAsync()
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
                    var paginatedResult = await _runApi.GetRunsWithCursorAsync(
                        cursor: null,
                        limit: 50, // Request a larger batch
                        direction: "next",
                        sortBy: "Date",
                        accessToken: token);

                    LogInfo($"API call completed. Result: {(paginatedResult != null ? "Success" : "Null")}");
                    LogInfo($"Items count: {paginatedResult?.Items?.Count ?? 0}");

                    // Convert items to Run objects
                    if (paginatedResult != null && paginatedResult.Items != null && paginatedResult.Items.Count > 0)
                    {
                        // Create a list to hold the converted posts
                        var run = new List<Run>();

                        foreach (var item in paginatedResult.Items)
                        {
                            try
                            {
                                // Map properties from the DTO to a new Run object
                                var runs = new Run
                                {
                                    RunId = item.RunId,
                                    CourtId = item.CourtId,
                                    ProfileId = item.ProfileId,
                                    Type = item.Type,
                                    Status = item.Status,
                                    SkillLevel = item.SkillLevel,
                                    PlayerLimit = item.PlayerLimit,
                                    Name = item.Name,
                                    ImageUrl = item.Court.ImageURL,
                                    Court = item.Court,
                                    JoinedRunList = item.JoinedRunProfileList,
                                    RunDate = item.RunDate,
                                    UserName = "test",
                                    StartTime =   item.StartTime,
                                    EndTime = item.EndTime,
                                    PlayerCount = item.JoinedRunProfileList != null ? item.JoinedRunProfileList.Count : 0

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
                    else
                    {
                        LogInfo("No posts returned from API, returning empty list");
                        return new List<Run>();
                    }
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

        public async Task<bool> UserJoinRunAsync(CreateJoinedRunDto joinedRun)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                var paginatedResult = await _runApi.UserJoinRunAsync(joinedRun,token);

                return paginatedResult;
            }
            catch (Exception ex)
            {
                LogError("Error creating Run", ex);
                throw;
            }
        }

        /// <summary>
        /// Leave a run that the user previously joined
        /// </summary>
        /// <param name="runId">The ID of the run to leave</param>
        /// <param name="profileId">The profile ID of the user</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> LeaveRunAsync(string runId, string profileId)
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

        /// <summary>
        /// Get all runs that the current user has joined
        /// </summary>
        /// <returns>List of runs the user has joined</returns>
        public async Task<List<Run>> GetUserJoinedRunsAsync()
        {
            try
            {
                // Get auth token
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Get current user's profile ID
                string profileId = App.User?.Profile?.ProfileId;
                if (string.IsNullOrEmpty(profileId))
                {
                    throw new InvalidOperationException("User profile ID not available");
                }

                // Set token in authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Get all runs first
                var allRuns = await GetRunsAsync();

                // Filter to only runs that the user has joined
                var joinedRuns = allRuns?.Where(run =>
                    run.JoinedRunList != null &&
                    run.JoinedRunList.Any(jr => jr.ProfileId == profileId)
                ).ToList();

                return joinedRuns ?? new List<Run>();
            }
            catch (Exception ex)
            {
                LogError("Error getting user joined runs", ex);
                throw;
            }
        }

        // Helper method to format relative time
        private string FormatRelativeTime(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return "Recently";

            if (DateTime.TryParse(dateStr, out DateTime date))
            {
                TimeSpan diff = DateTime.Now - date;

                if (diff.TotalMinutes < 1)
                    return "Just now";
                if (diff.TotalMinutes < 60)
                    return $"{(int)diff.TotalMinutes} minutes ago";
                if (diff.TotalHours < 24)
                    return $"{(int)diff.TotalHours} hours ago";
                if (diff.TotalDays < 7)
                    return $"{(int)diff.TotalDays} days ago";

                return date.ToString("MMM dd, yyyy");
            }

            return "Recently";
        }

        // Helper to ensure URL has a protocol
        private string FixUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url.TrimStart('/');
                LogInfo($"Fixed URL: {url}");
            }

            return url;
        }

        // Helper to determine post type from URL
        private string DeterminePostType(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "image"; // Default to image

            string lowercaseUrl = url.ToLower();

            // Check for common video extensions
            if (lowercaseUrl.EndsWith(".mp4") ||
                lowercaseUrl.EndsWith(".mov") ||
                lowercaseUrl.EndsWith(".avi") ||
                lowercaseUrl.EndsWith(".webm") ||
                lowercaseUrl.EndsWith(".mkv") ||
                lowercaseUrl.EndsWith(".mpg") ||
                lowercaseUrl.EndsWith(".mpeg") ||
                lowercaseUrl.Contains("video") ||
                lowercaseUrl.Contains("mp4") ||
                lowercaseUrl.Contains("commondatastorage.googleapis.com/gtv-videos-bucket"))
            {
                return "video";
            }

            // Default to image for all other formats
            return "image";
        }

        public async Task<bool> UpdateRunAsync(Run run)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Implementation logic
                throw new NotImplementedException("UpdateRunAsync not implemented yet");
            }
            catch (Exception ex)
            {
                LogError($"Error updating Run {run?.RunId}", ex);
                throw;
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