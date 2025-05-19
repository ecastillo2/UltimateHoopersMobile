using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UltimateHoopers.ViewModels
{
    public class NotificationsViewModel : BindableObject
    {
        private bool _isRefreshing;
        private ObservableCollection<NotificationItem> _notifications = new ObservableCollection<NotificationItem>();

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

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand NotificationTappedCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }

        public NotificationsViewModel()
        {
            // Initialize commands
            RefreshCommand = new Command(async () => await RefreshNotifications());
            NotificationTappedCommand = new Command<NotificationItem>(async (notification) => await OnNotificationTapped(notification));
            MarkAllAsReadCommand = new Command(async () => await MarkAllAsRead());
        }

        public async Task LoadNotificationsAsync(string filter = "All")
        {
            if (IsRefreshing)
                return;

            try
            {
                IsRefreshing = true;
                await Task.Delay(600); // Simulate network call

                // Clear existing notifications
                Notifications.Clear();

                // In a real app, you would call your API to get notifications
                // var apiNotifications = await _notificationService.GetNotificationsAsync(filter);

                // For now, we'll create sample notifications
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notifications: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load notifications", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task RefreshNotifications()
        {
            await LoadNotificationsAsync("All");
        }

        private async Task OnNotificationTapped(NotificationItem notification)
        {
            if (notification == null)
                return;

            try
            {
                // Mark as read
                notification.IsUnread = false;

                // Update UI
                var index = Notifications.IndexOf(notification);
                if (index >= 0)
                {
                    Notifications[index] = notification;
                }

                // In a real app, you would call your API to mark the notification as read
                // await _notificationService.MarkAsReadAsync(notification.Id);

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
            // Navigate based on notification type and data
            switch (notification.Type)
            {
                case "game_invite":
                    // Navigate to the game details page
                    await Application.Current.MainPage.DisplayAlert("Game Invite",
                        $"Navigating to game invite: {notification.Title}", "OK");
                    // await Shell.Current.GoToAsync($"//GameDetailsPage?id={notification.EntityId}");
                    break;

                case "friend_request":
                    // Navigate to the player profile page
                    await Application.Current.MainPage.DisplayAlert("Friend Request",
                        $"Navigating to profile: {notification.Title}", "OK");
                    // await Shell.Current.GoToAsync($"//PlayerProfilePage?id={notification.EntityId}");
                    break;

                case "post_like":
                case "post_comment":
                    // Navigate to the post details
                    await Application.Current.MainPage.DisplayAlert("Post Interaction",
                        $"Navigating to post: {notification.Title}", "OK");
                    // await Shell.Current.GoToAsync($"//PostDetailsPage?id={notification.EntityId}");
                    break;

                case "system":
                    // Just show the notification details
                    await Application.Current.MainPage.DisplayAlert(notification.Title,
                        notification.Message, "OK");
                    break;

                default:
                    // Default action
                    await Application.Current.MainPage.DisplayAlert("Notification",
                        notification.Message, "OK");
                    break;
            }
        }

        private async Task MarkAllAsRead()
        {
            try
            {
                // Mark all notifications as read
                foreach (var notification in Notifications)
                {
                    notification.IsUnread = false;
                }

                // Update the collection to refresh the UI
                var tempNotifications = new ObservableCollection<NotificationItem>(Notifications);
                Notifications.Clear();
                foreach (var notification in tempNotifications)
                {
                    Notifications.Add(notification);
                }

                // In a real app, you would call your API to mark all notifications as read
                // await _notificationService.MarkAllAsReadAsync();

                await Application.Current.MainPage.DisplayAlert("Success", "All notifications marked as read", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking all as read: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to mark notifications as read", "OK");
            }
        }

        private List<NotificationItem> GetSampleNotifications()
        {
            // Create a list of sample notifications
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
                },
                new NotificationItem
                {
                    Id = "4",
                    Title = "Game Result",
                    Message = "Your team won the game at Central Park! Final score: 21-15",
                    TimeAgo = "Yesterday",
                    IsUnread = false,
                    Type = "game_result",
                    Category = "Games",
                    EntityId = "game101",
                    IconText = "🏆",
                    IconBackground = Colors.Gold
                },
                new NotificationItem
                {
                    Id = "5",
                    Title = "Profile Views",
                    Message = "5 people viewed your profile this week",
                    TimeAgo = "2 days ago",
                    IsUnread = false,
                    Type = "profile_views",
                    Category = "Activity",
                    EntityId = "stats101",
                    IconText = "👁️",
                    IconBackground = Colors.Purple
                },
                new NotificationItem
                {
                    Id = "6",
                    Title = "Post Like",
                    Message = "John and 3 others liked your highlight video",
                    TimeAgo = "3 days ago",
                    IsUnread = false,
                    Type = "post_like",
                    Category = "Activity",
                    EntityId = "post202",
                    IconText = "❤️",
                    IconBackground = Colors.Red
                },
                new NotificationItem
                {
                    Id = "7",
                    Title = "System Update",
                    Message = "Ultimate Hoopers app has been updated to version 2.0",
                    TimeAgo = "5 days ago",
                    IsUnread = false,
                    Type = "system",
                    Category = "Activity",
                    EntityId = "system303",
                    IconText = "ℹ️",
                    IconBackground = Colors.Gray
                }
            };
        }
    }

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