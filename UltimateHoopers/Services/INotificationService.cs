using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UltimateHoopers.Services
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotificationsAsync(string filter = "All");
        Task<bool> MarkAsReadAsync(string notificationId);
        Task<bool> MarkAllAsReadAsync();
        Task<int> GetUnreadCountAsync();
        Task<bool> UpdateNotificationSettingsAsync(NotificationSettings settings);
    }

    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;
        private const string TOKEN_KEY = "auth_token";
        private readonly string _baseUrl;

        // Constructor with proper DI
        public NotificationService(HttpClient httpClient, IConfiguration configuration, ILogger<NotificationService> logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;

            // Get base URL from configuration
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
        }

        // Simplified constructor for non-DI scenarios
        public NotificationService()
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
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(string filter = "All")
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Set token in authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Call the API - build the URL with the filter parameter
                string endpoint = $"api/notifications?filter={filter}";
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var notifications = JsonSerializer.Deserialize<List<NotificationDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return notifications ?? new List<NotificationDto>();
                }
                else
                {
                    LogError($"Error getting notifications: {response.StatusCode}", null);
                    return new List<NotificationDto>();
                }
            }
            catch (Exception ex)
            {
                LogError("Error getting notifications", ex);

                // For development, return mock data
#if DEBUG
                return GetMockNotifications(filter);
#else
                throw;
#endif
            }
        }

        public async Task<bool> MarkAsReadAsync(string notificationId)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Set token in authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Call the API
                string endpoint = $"api/notifications/{notificationId}/read";
                var response = await _httpClient.PutAsync(endpoint, null);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                LogError($"Error marking notification {notificationId} as read", ex);

                // For development, assume success
#if DEBUG
                return true;
#else
                throw;
#endif
            }
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Set token in authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Call the API
                string endpoint = "api/notifications/read-all";
                var response = await _httpClient.PutAsync(endpoint, null);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                LogError("Error marking all notifications as read", ex);

                // For development, assume success
#if DEBUG
                return true;
#else
                throw;
#endif
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Set token in authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Call the API
                string endpoint = "api/notifications/unread-count";
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (int.TryParse(content, out int count))
                    {
                        return count;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                LogError("Error getting unread count", ex);

                // For development, return a mock count
#if DEBUG
                return new Random().Next(1, 10);
#else
                return 0;
#endif
            }
        }

        public async Task<bool> UpdateNotificationSettingsAsync(NotificationSettings settings)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Set token in authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Serialize settings
                var json = JsonSerializer.Serialize(settings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Call the API
                string endpoint = "api/notifications/settings";
                var response = await _httpClient.PutAsync(endpoint, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                LogError("Error updating notification settings", ex);

                // For development, assume success
#if DEBUG
                return true;
#else
                throw;
#endif
            }
        }

        private async Task<string> GetTokenAsync()
        {
            try
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
            catch (Exception ex)
            {
                LogError("Error retrieving auth token", ex);
                return null;
            }
        }

        private void LogInfo(string message)
        {
            _logger?.LogInformation(message);
            Console.WriteLine($"[NotificationService] INFO: {message}");
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
                Console.WriteLine($"[NotificationService] ERROR: {message}: {ex.Message}");
            }
            else
            {
                Console.WriteLine($"[NotificationService] ERROR: {message}");
            }
        }

        private List<NotificationDto> GetMockNotifications(string filter = "All")
        {
            // Create a list of sample notifications for development
            var notifications = new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = "1",
                    Title = "Game Invitation",
                    Message = "Michael invited you to play at Downtown Court this Saturday at 3 PM",
                    CreatedAt = DateTime.Now.AddMinutes(-10),
                    IsRead = false,
                    Type = "game_invite",
                    Category = "Games",
                    EntityId = "game123",
                    IconText = "🏀",
                    IconBackgroundColor = "#4CAF50" // Green
                },
                new NotificationDto
                {
                    Id = "2",
                    Title = "Friend Request",
                    Message = "Sarah wants to connect with you on Ultimate Hoopers",
                    CreatedAt = DateTime.Now.AddHours(-1),
                    IsRead = false,
                    Type = "friend_request",
                    Category = "Activity",
                    EntityId = "user456",
                    IconText = "👤",
                    IconBackgroundColor = "#2196F3" // Blue
                },
                new NotificationDto
                {
                    Id = "3",
                    Title = "Game Reminder",
                    Message = "Your game at Westside Court starts in 1 hour",
                    CreatedAt = DateTime.Now.AddHours(-2),
                    IsRead = true,
                    Type = "game_reminder",
                    Category = "Games",
                    EntityId = "game789",
                    IconText = "⏰",
                    IconBackgroundColor = "#FF9800" // Orange
                },
                // Add more mock notifications as needed
            };

            // Filter if needed
            if (filter != "All")
            {
                notifications = notifications
                    .Where(n => n.Category.Equals(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return notifications;
        }
    }

    public class NotificationDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; } = "general";
        public string Category { get; set; } = "All";
        public string EntityId { get; set; }
        public string IconText { get; set; } = "📩";
        public string IconBackgroundColor { get; set; } = "#2196F3"; // Default blue
        public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();

        // Computed property for UI
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;

                if (timeSpan.TotalMinutes < 1)
                    return "Just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hours ago";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} days ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";

                return CreatedAt.ToString("MMM dd, yyyy");
            }
        }
    }

    public class NotificationSettings
    {
        public bool EnablePushNotifications { get; set; } = true;
        public bool EnableEmailNotifications { get; set; } = true;
        public bool GameInvitations { get; set; } = true;
        public bool GameReminders { get; set; } = true;
        public bool FriendRequests { get; set; } = true;
        public bool PostInteractions { get; set; } = true;
        public bool SystemUpdates { get; set; } = true;
        public string QuietHoursStart { get; set; } = "22:00"; // 10:00 PM
        public string QuietHoursEnd { get; set; } = "08:00"; // 8:00 AM
        public bool QuietHoursEnabled { get; set; } = false;
    }
}