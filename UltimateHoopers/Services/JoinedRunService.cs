using Domain;
using Domain.DtoModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UltimateHoopers.Services
{
    public class JoinedRunService : IJoinedRunService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public JoinedRunService()
        {
            _httpClient = new HttpClient();
            _baseUrl = "https://ultimatehoopersapi.azurewebsites.net";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<JoinedRunDetailViewModelDto>> GetUserJoinedRunsAsync(string profileId)
        {
            try
            {
                Debug.WriteLine($"Fetching joined runs for profile: {profileId}");

                if (string.IsNullOrEmpty(profileId))
                {
                    Debug.WriteLine("Profile ID is null or empty - can't fetch joined runs");
                    return new List<JoinedRunDetailViewModelDto>();
                }

                // Ensure we have an auth token
                if (string.IsNullOrEmpty(App.AuthToken))
                {
                    Debug.WriteLine("Auth token is missing - can't fetch joined runs");
                    return GetMockJoinedRuns(profileId);
                }

                // Add the auth token to the request
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.AuthToken);

                // Make the API request
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/JoinedRun/GetUserJoinedRunsAsync/{profileId}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"API returned error status code: {response.StatusCode}");
                    // Return mock data as fallback
                    return GetMockJoinedRuns(profileId);
                }

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API response: {content}");

                var joinedRuns = JsonSerializer.Deserialize<List<JoinedRunDetailViewModelDto>>(content, _jsonOptions);

                if (joinedRuns == null || joinedRuns.Count == 0)
                {
                    Debug.WriteLine("No joined runs returned from API");
                    return GetMockJoinedRuns(profileId);
                }

                // Enrich each joined run with additional data
                foreach (var run in joinedRuns)
                {
                    // Ensure the Run object is initialized
                    if (run.Run == null)
                    {
                        Debug.WriteLine($"Run object is null for joined run {run.JoinedRunId}");
                        continue;
                    }

                    // Enrich with court data if available
                    if (run.Run.CourtId != null)
                    {
                        run.Run.Court = await GetCourtByIdAsync(run.Run.CourtId);
                    }
                }

                Debug.WriteLine($"Successfully loaded {joinedRuns.Count} joined runs");
                return joinedRuns;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching joined runs: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Return mock data on error
                return GetMockJoinedRuns(profileId);
            }
        }

        private async Task<Court> GetCourtByIdAsync(string courtId)
        {
            try
            {
                if (string.IsNullOrEmpty(courtId) || string.IsNullOrEmpty(App.AuthToken))
                {
                    return new Court
                    {
                        Name = "Basketball Court",
                        Address = "123 Main St",
                        City = "Atlanta",
                        State = "GA",
                        ImageURL = "https://placehold.co/600x400/png?text=Basketball+Court"
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.AuthToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Court/GetCourtById?courtId={courtId}");

                if (!response.IsSuccessStatusCode)
                {
                    return new Court
                    {
                        Name = "Basketball Court",
                        Address = "123 Main St",
                        City = "Atlanta",
                        State = "GA",
                        ImageURL = "https://placehold.co/600x400/png?text=Basketball+Court"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Court>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching court: {ex.Message}");
                return new Court
                {
                    Name = "Basketball Court",
                    Address = "123 Main St",
                    City = "Atlanta",
                    State = "GA",
                    ImageURL = "https://placehold.co/600x400/png?text=Basketball+Court"
                };
            }
        }

        private List<JoinedRunDetailViewModelDto> GetMockJoinedRuns(string profileId)
        {
            // Create some mock joined runs for development/fallback
            return new List<JoinedRunDetailViewModelDto>
            {
                new JoinedRunDetailViewModelDto
                {
                    JoinedRunId = "1",
                    ProfileId = profileId,
                    RunId = "run1",
                    Status = "Accepted",
                    InvitedDate = DateTime.Now,
                    Run = new Run
                    {
                        RunId = "run1",
                        Name = "Downtown Pickup Game",
                        RunDate = DateTime.Now.AddDays(1),
                        StartTime = DateTime.Now.AddDays(1).TimeOfDay,
                        EndTime = DateTime.Now.AddDays(1).TimeOfDay,
                        Description = "Competitive 5v5 full court runs",
                        PlayerLimit = 15,
                        Cost = 5.00m,
                        CourtId = "court1",
                        Court = new Court
                        {
                            CourtId = "court1",
                            Name = "Downtown Community Center",
                            Address = "123 Main Street",
                            City = "Atlanta",
                            State = "GA",
                            ImageURL = "https://placehold.co/600x400/png?text=Downtown+Court"
                        }
                    }
                },
                new JoinedRunDetailViewModelDto
                {
                    JoinedRunId = "2",
                    ProfileId = profileId,
                    RunId = "run2",
                    Status = "Accepted",
                    InvitedDate = DateTime.Now,
                    Run = new Run
                    {
                        RunId = "run2",
                        Name = "Morning Shootaround",
                        RunDate = DateTime.Now.AddDays(2),
                        StartTime = DateTime.Now.AddDays(1).TimeOfDay,
                        EndTime = DateTime.Now.AddDays(1).TimeOfDay,
                        Description = "Casual shooting and pickup games",
                        PlayerLimit = 12,
                        Cost = 0.00m,
                        CourtId = "court2",
                        Court = new Court
                        {
                            CourtId = "court2",
                            Name = "Westside Park",
                            Address = "456 Park Avenue",
                            City = "Atlanta",
                            State = "GA",
                            ImageURL = "https://placehold.co/600x400/png?text=Westside+Park"
                        }
                    }
                }
            };
        }

        public async Task<bool> RemoveUserJoinRunAsync(string profileId, string runId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(runId) || string.IsNullOrEmpty(App.AuthToken))
                {
                    Debug.WriteLine("Missing required parameters for RemoveUserJoinRunAsync");
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.AuthToken);

                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/JoinedRun/RemoveUserJoinRunAsync?profileId={profileId}&runId={runId}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing user from run: {ex.Message}");
                return false;
            }
        }
    }
}