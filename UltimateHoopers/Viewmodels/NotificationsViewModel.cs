using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UltimateHoopers.Services;

namespace UltimateHoopers.ViewModels
{
    public class NotificationsViewModel : BindableObject
    {
        private readonly INotificationService _notificationService;
        private bool _isRefreshing;
        private ObservableCollection<NotificationItem> _notifications = new ObservableCollection<NotificationItem>();
        private string _currentFilter = "All";
        private int _unreadCount = 0;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<NotificationItem> Notifications
        {
            get => _notifications;
            set
            {
                _notifications = value;
                OnPropertyChanged();
            }
        }

        public string CurrentFilter
        {
            get => _currentFilter;
            set
            {
                if (_currentFilter != value)
                {
                    _currentFilter = value;
                    OnPropertyChanged();

                    // Reload notifications with the new filter
                    LoadNotificationsAsync(_currentFilter).ConfigureAwait(false);
                }
            }
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                _unreadCount = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand NotificationTappedCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }
        public ICommand FilterCommand { get; }

        public NotificationsViewModel()
        {
            // Try to get the notification service from DI
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            _notificationService = serviceProvider.GetService<INotificationService>() ?? new NotificationService();

            // Initialize commands
            RefreshCommand = new Command(async () => await RefreshNotifications());
            NotificationTappedCommand = new Command<NotificationItem>(async (notification) => await OnNotificationTapped(notification));
            MarkAllAsReadCommand = new Command(async () => await MarkAllAsRead());
            FilterCommand = new Command<string>((filter) => CurrentFilter = filter);

            // Load notifications initially
            LoadNotificationsAsync("All").ConfigureAwait(false);
        }

        public async Task LoadNotificationsAsync(string filter = "All")
        {
            if (IsRefreshing)
                return;

            try
            {
                IsRefreshing = true;

                // Clear existing notifications
                Notifications.Clear();

                // Get notifications from the service
                List<NotificationDto> notificationDtos = await _notificationService.GetNotificationsAsync(filter);

                // Convert DTOs to view model items
                foreach (var dto in notificationDtos)
                {
                    // Create color from hex string
                    Color iconBgColor;
                    if (!string.IsNullOrEmpty(dto.IconBackgroundColor) &&
                        dto.IconBackgroundColor.StartsWith("#"))
                    {
                        try
                        {
                            iconBgColor = Color.FromArgb(dto.IconBackgroundColor);
                        }
                        catch
                        {
                            // Default to blue if parsing fails
                            iconBgColor = Colors.Blue;
                        }
                    }
                    else
                    {
                        iconBgColor = Colors.Blue;
                    }

                    var notification = new NotificationItem
                    {
                        Id = dto.Id,
                        Title = dto.Title,
                        Message = dto.Message,
                        TimeAgo = dto.TimeAgo,
                        IsUnread = !dto.IsRead,
                        Type = dto.Type,
                        Category = dto.Category,
                        EntityId = dto.EntityId,
                        IconText = dto.IconText,
                        IconBackground = iconBgColor,
                        AdditionalData = dto.AdditionalData
                    };

                    Notifications.Add(notification);
                }

                // Calculate unread count
                UpdateUnreadCount();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notifications: {ex.Message}");

                // If service fails, fall back to sample data
                var sampleNotifications = GetSampleNotifications();

                // Apply filter if needed
                if (filter != "All")
                {
                    sampleNotifications = sampleNotifications
                        .Where(n => n.Category.Equals(filter, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // Add notifications to the collection
                foreach (var notification in sampleNotifications)
                {
                    Notifications.Add(notification);
                }

                // Calculate unread count
                UpdateUnreadCount();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void UpdateUnreadCount()
        {
            UnreadCount = Notifications.Count(n => n.IsUnread);
        }

        private async Task RefreshNotifications()
        {
            await LoadNotificationsAsync(CurrentFilter);
        }

        private async Task OnNotificationTapped(NotificationItem notification)
        {
            if (notification == null)
                return;

            try
            {
                // Mark as read locally
                if (notification.IsUnread)
                {
                    notification.IsUnread = false;

                    // Update UI by replacing the item
                    var index = Notifications.IndexOf(notification);
                    if (index >= 0)
                    {
                        Notifications[index] = notification;
                    }

                    // Update unread count
                    UpdateUnreadCount();

                    // Mark as read on the server
                    await _notificationService.MarkAsReadAsync(notification.Id);
                }

                // Handle navigation based on notification type
                await HandleNotificationNavigation(notification);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling notification tap: {ex.Message}");
            }
        }

        private async Task HandleNotificationNavigation(NotificationItem notification)
        {
            try
            {
                // Navigate based on notification type and data
                switch (notification.Type.ToLowerInvariant())
                {
                    case "game_invite":
                        // Navigate to the game details page
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert("Game Invitation",
                                $"Navigating to game: {notification.EntityId}", "OK");
                            // In production, use: await Shell.Current.GoToAsync($"//GameDetailsPage?id={notification.EntityId}");
                        });
                        break;

                    case "friend_request":
                        // Navigate to the player profile page
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert("Friend Request",
                                $"Navigating to profile: {notification.EntityId}", "OK");
                            // In production, use: await Shell.Current.GoToAsync($"//PlayerProfilePage?id={notification.EntityId}");
                        });
                        break;

                    case "game_reminder":
                        // Navigate to the game details
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert("Game Reminder",
                                $"Your game is starting soon: {notification.Message}", "OK");
                            // In production, use: await Shell.Current.GoToAsync($"//GameDetailsPage?id={notification.EntityId}");
                        });
                        break;

                    case "game_result":
                        // Navigate to the game result
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert("Game Result",
                                notification.Message, "OK");
                            // In production, use: await Shell.Current.GoToAsync($"//GameDetailsPage?id={notification.EntityId}");
                        });
                        break;

                    case "post_like":
                    case "comment":
                        // Navigate to the post
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert("Post Interaction",
                                notification.Message, "OK");
                            // In production, use: await Shell.Current.GoToAsync($"//PostDetailsPage?id={notification.EntityId}");
                        });
                        break;

                    case "profile_views":
                    case "stats_update":
                        // Navigate to stats page
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert("Stats Update",
                                notification.Message, "OK");
                            // In production, use: await Shell.Current.GoToAsync($"//StatsPage");
                        });
                        break;

                    case "tournament":
                    case "training":
                        // Navigate to event details
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert("Event Details",
                                notification.Message, "OK");
                            // In production, use: await Shell.Current.GoToAsync($"//EventDetailsPage?id={notification.EntityId}");
                        });
                        break;

                    case "achievement":
                        // Show achievement details
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert("Achievement Unlocked",
                                notification.Message, "View Achievements");
                            // In production, use: await Shell.Current.GoToAsync($"//AchievementsPage");
                        });
                        break;

                    case "system":
                        // Just show the notification details
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert(notification.Title,
                                notification.Message, "OK");
                        });
                        break;

                    default:
                        // Default action
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert("Notification",
                                notification.Message, "OK");
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating from notification: {ex.Message}");

                // Fallback to just showing the notification content
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(notification.Title,
                        notification.Message, "OK");
                });
            }
        }

        private async Task MarkAllAsRead()
        {
            try
            {
                // Update all notifications locally
                foreach (var notification in Notifications.Where(n => n.IsUnread).ToList())
                {
                    notification.IsUnread = false;

                    // Update UI by replacing the item
                    var index = Notifications.IndexOf(notification);
                    if (index >= 0)
                    {
                        Notifications[index] = notification;
                    }
                }

                // Update unread count
                UpdateUnreadCount();

                // Call the service to mark all as read
                await _notificationService.MarkAllAsReadAsync();

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "All notifications marked as read", "OK");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking all as read: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to mark notifications as read", "OK");
                });
            }
        }

        // Fallback sample notifications (only used if service fails)
        private List<NotificationItem> GetSampleNotifications()
        {
            return new List<NotificationItem>
            {
                new NotificationItem
                {
                    Id = "1",
                    Title = "Game Invitation",
                    Message = "Michael invited you to play at Downtown Court this Saturday at 3 PM",
                    TimeAgo = "10 minutes ago",
                    IsUnread = true,
                    Type = "game_invite",
                    Category = "Games",
                    EntityId = "game123",
                    IconText = "🏀",
                    IconBackground = Colors.Green
                },
                new NotificationItem
                {
                    Id = "2",
                    Title = "Friend Request",
                    Message = "Sarah wants to connect with you on Ultimate Hoopers",
                    TimeAgo = "1 hour ago",
                    IsUnread = true,
                    Type = "friend_request",
                    Category = "Activity",
                    EntityId = "user456",
                    IconText = "👤",
                    IconBackground = Colors.Blue
                },
                new NotificationItem
                {
                    Id = "3",
                    Title = "Game Reminder",
                    Message = "Your game at Westside Court starts in 1 hour",
                    TimeAgo = "2 hours ago",
                    IsUnread = false,
                    Type = "game_reminder",
                    Category = "Games",
                    EntityId = "game789",
                    IconText = "⏰",
                    IconBackground = Colors.Orange
                }
            };
        }
    }

    // Custom notification item class for the ViewModel
    public class NotificationItem : BindableObject
    {
        // Basic notification properties
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string TimeAgo { get; set; }

        // UI properties
        private bool _isUnread;
        public bool IsUnread
        {
            get => _isUnread;
            set
            {
                _isUnread = value;
                OnPropertyChanged();
            }
        }

        public string IconText { get; set; } = "📩";
        public Color IconBackground { get; set; } = Colors.Blue;

        // Metadata for navigation and filtering
        public string Type { get; set; } = "general";
        public string Category { get; set; } = "All";
        public string EntityId { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();
    }
}