using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.DtoModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using WebAPI.ApiClients;

namespace UltimateHoopers.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileApi _profileApi;
        private readonly ILogger<ProfileService> _logger;
        private const string TOKEN_KEY = "auth_token";

        // Constructor with proper DI
        public ProfileService(HttpClient httpClient, IConfiguration configuration, ILogger<ProfileService> logger = null)
        {
            // Create ProfileApi with the provided dependencies
            _profileApi = new ProfileApi(httpClient, configuration);
            _logger = logger;
        }

        // Simplified constructor for non-DI scenarios
        public ProfileService()
        {
            // Create ProfileApi with default dependencies
            var httpClient = new HttpClient();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ApiSettings:BaseUrl"] = "https://ultimatehoopersapi.azurewebsites.net/"
                })
                .Build();

            _profileApi = new ProfileApi(httpClient, configuration);
        }

        public async Task<Profile> GetProfileByIdAsync(string profileId)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                return await _profileApi.GetProfileByIdAsync(profileId, token);
            }
            catch (Exception ex)
            {
                LogError($"Error getting Profile {profileId}", ex);
                throw;
            }
        }

        public async Task<List<Profile>> GetProfilesAsync()
        {
            try
            {
                // Get token (first from App state, then from secure storage)
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Call the API with the retrieved token
                var profiles = await _profileApi.GetProfilesAsync(token);
                return profiles;
            }
            catch (Exception ex)
            {
                LogError("Error getting profiles", ex);
                throw;
            }
        }

        public async Task<List<Profile>> GetProfilesWithCursor()
        {
            try
            {
                // Get token (first from App state, then from secure storage)
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Call the API with the retrieved token to get cursor-paginated results
                var paginatedResult = await _profileApi.GetProfilesWithCursorAsync(
                    cursor: null,
                    limit: 50, // Request a larger batch
                    direction: "next",
                    sortBy: "Points",
                    accessToken: token);

                // Convert ProfileViewModelDto to Profile objects
                if (paginatedResult != null && paginatedResult.Items != null)
                {
                    // Create a list to hold the converted profiles
                    var profiles = new List<Profile>();

                    foreach (var item in paginatedResult.Items)
                    {
                        // Map properties from the DTO to a new Profile object
                        var profile = new Profile
                        {
                            ProfileId = item.ProfileId,
                            //UserName = item.Username,
                            Position = item.Position,
                            PlayerNumber = item.PlayerNumber,
                            City = item.City,
                            Height = item.Height,
                            //ImageURL = item.ImageUrl,
                            //TotalGames = item.TotalGames?.ToString(),
                            //TotalWins = item.TotalWins?.ToString(),
                            //TotalLosses = item.TotalLosses?.ToString(),
                            //WinPercentage = item.WinPercentage,
                            Ranking = item.Ranking?.ToString(),
                            StarRating = item.StarRating?.ToString()
                        };

                        profiles.Add(profile);
                    }

                    return profiles;
                }

                // Return empty list if no results
                return new List<Profile>();
            }
            catch (Exception ex)
            {
                LogError("Error getting profiles with cursor", ex);

                // For development, provide mock data if API fails
#if DEBUG
                return CreateMockProfiles();
#else
                throw;
#endif
            }
        }

        public async Task<bool> UpdateProfileAsync(Profile profile)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                return await _profileApi.UpdateProfileAsync(profile, token);
            }
            catch (Exception ex)
            {
                LogError($"Error updating Profile {profile?.ProfileId}", ex);
                throw;
            }
        }

        private async Task<string> GetTokenAsync()
        {
            // First try to get the token from the App's global auth token
            var token = App.AuthToken;

            // If it's not available in the global App state, try to get it from secure storage
            if (string.IsNullOrEmpty(token))
            {
                token = await SecureStorage.GetAsync(TOKEN_KEY);
            }

            // For development, provide a fallback token
#if DEBUG
            if (string.IsNullOrEmpty(token))
            {
                token = "development-token";
            }
#endif

            return token;
        }

        private void LogError(string message, Exception ex)
        {
            if (_logger != null)
            {
                _logger.LogError(ex, message);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"{message}: {ex.Message}");
            }
        }

        // Mock data for development and testing
        private List<Profile> CreateMockProfiles()
        {
            return new List<Profile>
            {
                new Profile
                {
                    ProfileId = "1",
                    UserName = "mike_jordan",
                    Position = "Shooting Guard",
                    PlayerNumber = "23",
                    City = "Chicago",
                    Height = "6'6\"",
                    ImageURL = "https://images.unsplash.com/photo-1566492031773-4f4e44671857",
                    TotalGames = "82",
                    //TotalWins = "72",
                    //TotalLosses = "10",
                    WinPercentage = "87.8%",
                    Ranking = "1",
                    StarRating = "5.0"
                },
                new Profile
                {
                    ProfileId = "2",
                    UserName = "magic_johnson",
                    Position = "Point Guard",
                    PlayerNumber = "32",
                    City = "Los Angeles",
                    Height = "6'9\"",
                    ImageURL = "https://images.unsplash.com/photo-1518020382113-a7e8fc38eac9",
                    TotalGames = "78",
                    //TotalWins = "65",
                    //TotalLosses = "13",
                    WinPercentage = "83.3%",
                    Ranking = "2",
                    StarRating = "4.9"
                },
                new Profile
                {
                    ProfileId = "3",
                    UserName = "kobe_bean",
                    Position = "Shooting Guard",
                    PlayerNumber = "24",
                    City = "Los Angeles",
                    Height = "6'6\"",
                    ImageURL = "https://images.unsplash.com/photo-1494790108377-be9c29b29330",
                    TotalGames = "80",
                    //TotalWins = "68",
                    //TotalLosses = "12",
                    WinPercentage = "85.0%",
                    Ranking = "3",
                    StarRating = "4.8"
                }
            };
        }
    }
}