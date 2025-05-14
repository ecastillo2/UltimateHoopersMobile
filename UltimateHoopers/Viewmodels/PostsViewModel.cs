using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain;
using Microsoft.Maui.Controls;
using UltimateHoopers.Services;

namespace UltimateHoopers.ViewModels
{
    public class PostsViewModel : BindableObject
    {
        private readonly IPostService _postService;
        private bool _isRefreshing;

        public ObservableCollection<Post> Posts { get; } = new ObservableCollection<Post>();

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand LikePostCommand { get; }
        public ICommand SavePostCommand { get; }
        public ICommand CommentPostCommand { get; }
        public ICommand SharePostCommand { get; }
        public ICommand ViewCommentsCommand { get; }
        public ICommand PostOptionsCommand { get; }
        public ICommand PlayVideoCommand { get; }

        public PostsViewModel(IPostService postService)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));

            // Initialize commands
            RefreshCommand = new Command(async () => await LoadPostsAsync());
            LikePostCommand = new Command<Post>(async (post) => await LikePost(post));
            SavePostCommand = new Command<Post>(async (post) => await SavePost(post));
            CommentPostCommand = new Command<Post>(async (post) => await NavigateToComments(post));
            SharePostCommand = new Command<Post>(async (post) => await SharePost(post));
            ViewCommentsCommand = new Command<Post>(async (post) => await NavigateToComments(post));
            PostOptionsCommand = new Command<Post>(async (post) => await ShowPostOptions(post));
            PlayVideoCommand = new Command<Post>(async (post) => await PlayVideo(post));
        }

        public async Task LoadPostsAsync()
        {
            if (IsRefreshing)
                return;

            try
            {
                IsRefreshing = true;
                Console.WriteLine("Loading posts...");

                // Clear current posts
                Posts.Clear();

                // Get posts from API
                var posts = await _postService.GetPostsAsync();
                Console.WriteLine($"Received {posts?.Count ?? 0} posts from service");

                // Add posts to collection with super-robust null handling
                if (posts != null)
                {
                    int validPostCount = 0;
                    int skippedPostCount = 0;

                    // Process the posts to add missing data if needed
                    foreach (var post in posts)
                    {
                        try
                        {
                            // Debug log the raw post
                            LogPostDetails(post);

                            // Skip completely invalid posts (null or missing required field)
                            if (post == null)
                            {
                                Console.WriteLine("Skipping null post");
                                skippedPostCount++;
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(post.PostFileURL))
                            {
                                Console.WriteLine($"Skipping post {post.PostId}: Missing PostFileURL");
                                skippedPostCount++;
                                continue;
                            }

                            // Sanitize the post by ensuring all fields have valid values
                            SanitizePost(post);

                            // Log the post after sanitization
                            Console.WriteLine($"Adding sanitized post: {post.PostId}, Type: {post.PostType}, URL: {post.PostFileURL}");

                            // Add to the collection
                            Posts.Add(post);
                            validPostCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing post: {ex.Message}");
                            skippedPostCount++;
                        }
                    }

                    Console.WriteLine($"Added {validPostCount} valid posts, skipped {skippedPostCount} invalid posts");
                }
                else
                {
                    Console.WriteLine("No posts were returned from the service");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
                await Shell.Current.DisplayAlert("Authentication Error", "Please log in to view posts", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading posts: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        // Helper to log all details of a post for debugging
        private void LogPostDetails(Post post)
        {
            if (post == null)
            {
                Console.WriteLine("Post is null");
                return;
            }

            Console.WriteLine("=== Raw Post Details ===");
            Console.WriteLine($"PostId: {post.PostId ?? "null"}");
            Console.WriteLine($"UserName: {post.UserName ?? "null"}");
            Console.WriteLine($"FirstName: {post.FirstName ?? "null"}");
            Console.WriteLine($"LastName: {post.LastName ?? "null"}");
            Console.WriteLine($"Caption: {post.Caption ?? "null"}");
            Console.WriteLine($"PostFileURL: {post.PostFileURL ?? "null"}");
            Console.WriteLine($"ThumbnailUrl: {post.ThumbnailUrl ?? "null"}");
            Console.WriteLine($"ProfileImageURL: {post.ProfileImageURL ?? "null"}");
            Console.WriteLine($"PostType: {post.PostType ?? "null"}");
            Console.WriteLine($"Likes: {post.Likes?.ToString() ?? "null"}");
            Console.WriteLine($"LikedPost: {post.LikedPost?.ToString() ?? "null"}");
            Console.WriteLine($"SavedPost: {post.SavedPost?.ToString() ?? "null"}");
            Console.WriteLine($"PostCommentCount: {post.PostCommentCount?.ToString() ?? "null"}");
            Console.WriteLine($"RelativeTime: {post.RelativeTime ?? "null"}");
            Console.WriteLine($"PostedDate: {post.PostedDate ?? "null"}");
            Console.WriteLine("=======================");
        }

        // Helper to ensure all post fields have valid values
        private void SanitizePost(Post post)
        {
            // Required fields
            post.PostId = string.IsNullOrWhiteSpace(post.PostId) ? Guid.NewGuid().ToString() : post.PostId;

            // PostFileURL is already checked before this method is called

            // Set PostType based on file extension if not provided
            if (string.IsNullOrWhiteSpace(post.PostType))
            {
                var fileUrl = post.PostFileURL.ToLower();
                if (fileUrl.EndsWith(".mp4") || fileUrl.EndsWith(".mov") || fileUrl.EndsWith(".avi"))
                {
                    post.PostType = "video";
                }
                else if (fileUrl.EndsWith(".webp") || fileUrl.EndsWith(".jpg") ||
                         fileUrl.EndsWith(".jpeg") || fileUrl.EndsWith(".png") ||
                         fileUrl.EndsWith(".gif"))
                {
                    post.PostType = "image";
                }
                else
                {
                    // Default to image for unknown formats
                    post.PostType = "image";
                }
                Console.WriteLine($"Auto-detected post type: {post.PostType} for URL: {post.PostFileURL}");
            }

            // Optional fields with defaults
            post.UserName = string.IsNullOrWhiteSpace(post.UserName) ?
                (string.IsNullOrWhiteSpace(post.FirstName) && string.IsNullOrWhiteSpace(post.LastName) ?
                    "Anonymous User" :
                    $"{post.FirstName ?? ""} {post.LastName ?? ""}".Trim()) :
                post.UserName;

            post.Caption = post.Caption ?? ""; // Empty string instead of null
            post.RelativeTime = string.IsNullOrWhiteSpace(post.RelativeTime) ?
                (string.IsNullOrWhiteSpace(post.PostedDate) ?
                    "Recently" :
                    FormatRelativeTime(post.PostedDate)) :
                post.RelativeTime;

            // Numeric fields
            post.Likes = post.Likes ?? 0;
            post.LikedPost = post.LikedPost ?? false;
            post.SavedPost = post.SavedPost ?? false;
            post.PostCommentCount = post.PostCommentCount ?? 0;
        }

        private string FormatRelativeTime(string postedDateStr)
        {
            try
            {
                // Attempt to parse the posted date
                if (DateTime.TryParse(postedDateStr, out DateTime postedDate))
                {
                    var now = DateTime.Now;
                    var difference = now - postedDate;

                    if (difference.TotalMinutes < 1)
                        return "Just now";
                    if (difference.TotalMinutes < 60)
                        return $"{(int)difference.TotalMinutes}m ago";
                    if (difference.TotalHours < 24)
                        return $"{(int)difference.TotalHours}h ago";
                    if (difference.TotalDays < 7)
                        return $"{(int)difference.TotalDays}d ago";
                    if (difference.TotalDays < 30)
                        return $"{(int)(difference.TotalDays / 7)}w ago";
                    if (difference.TotalDays < 365)
                        return $"{(int)(difference.TotalDays / 30)}mo ago";

                    return $"{(int)(difference.TotalDays / 365)}y ago";
                }

                // If we couldn't parse the date, return the original string
                return postedDateStr;
            }
            catch
            {
                return postedDateStr;
            }
        }

        // Command handlers
        private async Task LikePost(Post post)
        {
            try
            {
                // Toggle the liked state locally for immediate feedback
                post.LikedPost = !(post.LikedPost ?? false);

                // Update like count
                if (post.LikedPost == true)
                {
                    post.Likes = (post.Likes ?? 0) + 1;
                }
                else
                {
                    post.Likes = Math.Max(0, (post.Likes ?? 0) - 1);
                }

                // Trigger UI update
                var index = Posts.IndexOf(post);
                if (index >= 0)
                {
                    Posts[index] = post;
                }

                // TODO: Implement API call to like/unlike post
                // await _postService.LikePostAsync(post.PostId, post.LikedPost ?? false);

                // For now, just show a message
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Like Post",
                        post.LikedPost == true ? "Post liked!" : "Post unliked!",
                        "OK");
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to like post: {ex.Message}", "OK");
            }
        }

        private async Task SavePost(Post post)
        {
            try
            {
                // Toggle the saved state locally
                post.SavedPost = !(post.SavedPost ?? false);

                // Trigger UI update
                var index = Posts.IndexOf(post);
                if (index >= 0)
                {
                    Posts[index] = post;
                }

                // TODO: Implement API call to save/unsave post
                // await _postService.SavePostAsync(post.PostId, post.SavedPost ?? false);

                // For now, just show a message
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Save Post",
                        post.SavedPost == true ? "Post saved!" : "Post unsaved!",
                        "OK");
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save post: {ex.Message}", "OK");
            }
        }

        private async Task NavigateToComments(Post post)
        {
            // This would navigate to a comments page
            await Shell.Current.DisplayAlert("Comments", "Comments feature coming soon!", "OK");
        }

        private async Task SharePost(Post post)
        {
            await Shell.Current.DisplayAlert("Share Post", "Share feature coming soon!", "OK");
        }

        private async Task ShowPostOptions(Post post)
        {
            string result = await Shell.Current.DisplayActionSheet(
                "Post Options",
                "Cancel",
                null,
                "Report",
                "Copy Link",
                "Share to...",
                "Hide");

            // Handle the selected option
            switch (result)
            {
                case "Report":
                    await Shell.Current.DisplayAlert("Report", "Report feature coming soon!", "OK");
                    break;
                case "Copy Link":
                    await Shell.Current.DisplayAlert("Copy Link", "Link copied to clipboard", "OK");
                    break;
                case "Share to...":
                    await SharePost(post);
                    break;
                case "Hide":
                    await Shell.Current.DisplayAlert("Hide", "Post hidden", "OK");
                    break;
            }
        }

        private async Task PlayVideo(Post post)
        {
            try
            {
                if (post == null || string.IsNullOrEmpty(post.PostFileURL))
                {
                    await Shell.Current.DisplayAlert("Error", "Video is not available", "OK");
                    return;
                }

                // Check if the video type is valid
                bool isVideoFile = post.PostFileURL.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                                post.PostFileURL.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                                post.PostFileURL.EndsWith(".webm", StringComparison.OrdinalIgnoreCase);

                if (!isVideoFile && post.PostType?.Equals("video", StringComparison.OrdinalIgnoreCase) != true)
                {
                    await Shell.Current.DisplayAlert("Error", "This is not a valid video file", "OK");
                    return;
                }

                // Navigate to the video player page
                await Shell.Current.Navigation.PushModalAsync(new Pages.VideoPlayerPage(post));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
            }
        }
    }
}