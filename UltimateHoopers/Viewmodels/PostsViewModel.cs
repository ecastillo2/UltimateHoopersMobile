using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain;
using Microsoft.Maui.Controls;
using UltimateHoopers.Services;
using UltimateHoopers.Pages;

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
            Console.WriteLine("PostsViewModel created with postService: " + (postService != null));

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

                // Debug post data
                if (posts != null && posts.Count > 0)
                {
                    Console.WriteLine("First post details:");
                    Console.WriteLine($"  ID: {posts[0].PostId}");
                    Console.WriteLine($"  Username: {posts[0].UserName}");
                    Console.WriteLine($"  URL: {posts[0].PostFileURL}");
                    Console.WriteLine($"  Type: {posts[0].PostType}");
                }
                else
                {
                    Console.WriteLine("No posts returned from service or list is empty");
                }

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
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Posts.Add(post);
                            });
                            validPostCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing post: {ex.Message}");
                            skippedPostCount++;
                        }
                    }

                    Console.WriteLine($"Added {validPostCount} valid posts, skipped {skippedPostCount} invalid posts");
                    Console.WriteLine($"Final Posts collection count: {Posts.Count}");

                    // Force property changed notification for the collection
                    OnPropertyChanged(nameof(Posts));
                }
                else
                {
                    Console.WriteLine("No posts were returned from the service");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");

                // Use Application.Current.MainPage instead of Shell.Current.DisplayAlert
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Authentication Error", "Please log in to view posts", "OK");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading posts: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                });
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
            // Make sure PostFileURL is a valid URI
            if (!string.IsNullOrWhiteSpace(post.PostFileURL))
            {
                try
                {
                    // Validate the URL format
                    var uri = new Uri(post.PostFileURL);

                    // Ensure URL has a protocol (http or https)
                    if (!post.PostFileURL.StartsWith("http://") && !post.PostFileURL.StartsWith("https://"))
                    {
                        // Add https protocol if missing
                        post.PostFileURL = "https://" + post.PostFileURL.TrimStart('/');
                        Console.WriteLine($"Fixed URL by adding protocol: {post.PostFileURL}");
                    }
                }
                catch (UriFormatException ex)
                {
                    Console.WriteLine($"Invalid URL format: {post.PostFileURL}, Error: {ex.Message}");
                    // Don't clear the URL yet, let the converter handle it
                }
            }

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

            // Do the same validation for ThumbnailUrl if present
            if (!string.IsNullOrWhiteSpace(post.ThumbnailUrl))
            {
                try
                {
                    var uri = new Uri(post.ThumbnailUrl);

                    if (!post.ThumbnailUrl.StartsWith("http://") && !post.ThumbnailUrl.StartsWith("https://"))
                    {
                        post.ThumbnailUrl = "https://" + post.ThumbnailUrl.TrimStart('/');
                        Console.WriteLine($"Fixed thumbnail URL: {post.ThumbnailUrl}");
                    }
                }
                catch (UriFormatException ex)
                {
                    Console.WriteLine($"Invalid thumbnail URL: {post.ThumbnailUrl}, Error: {ex.Message}");
                }
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
                    await Application.Current.MainPage.DisplayAlert("Like Post",
                        post.LikedPost == true ? "Post liked!" : "Post unliked!",
                        "OK");
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to like post: {ex.Message}", "OK");
                });
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
                    await Application.Current.MainPage.DisplayAlert("Save Post",
                        post.SavedPost == true ? "Post saved!" : "Post unsaved!",
                        "OK");
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save post: {ex.Message}", "OK");
                });
            }
        }

        private async Task NavigateToComments(Post post)
        {
            // This would navigate to a comments page
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Comments", "Comments feature coming soon!", "OK");
            });
        }

        private async Task SharePost(Post post)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Share Post", "Share feature coming soon!", "OK");
            });
        }

        private async Task ShowPostOptions(Post post)
        {
            string result = await MainThread.InvokeOnMainThreadAsync<string>(async () =>
            {
                return await Application.Current.MainPage.DisplayActionSheet(
                    "Post Options",
                    "Cancel",
                    null,
                    "Report",
                    "Copy Link",
                    "Share to...",
                    "Hide");
            });

            // Handle the selected option
            switch (result)
            {
                case "Report":
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        await Application.Current.MainPage.DisplayAlert("Report", "Report feature coming soon!", "OK");
                    });
                    break;
                case "Copy Link":
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        await Application.Current.MainPage.DisplayAlert("Copy Link", "Link copied to clipboard", "OK");
                    });
                    break;
                case "Share to...":
                    await SharePost(post);
                    break;
                case "Hide":
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        await Application.Current.MainPage.DisplayAlert("Hide", "Post hidden", "OK");
                    });
                    break;
            }
        }

        public async Task PlayVideo(Post post)
        {
            try
            {
                if (post == null || string.IsNullOrEmpty(post.PostFileURL))
                {
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        await Application.Current.MainPage.DisplayAlert("Error", "Video URL is not available", "OK");
                    });
                    return;
                }

                Console.WriteLine($"Playing video: {post.PostFileURL}");

                // Navigate directly to the video player page
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    // We need to check what type of Page we're on for proper navigation
                    if (Application.Current.MainPage is Shell shell)
                    {
                        await shell.Navigation.PushModalAsync(new VideoPlayerPage(post));
                    }
                    else if (Application.Current.MainPage.Navigation != null)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(new VideoPlayerPage(post));
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Navigation not available", "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
                });
            }
        }
    }
}