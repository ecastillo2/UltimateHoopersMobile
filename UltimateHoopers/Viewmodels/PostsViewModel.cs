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
        private bool _isLoading = false;

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

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
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
            Debug.WriteLine("PostsViewModel created with postService: " + (postService != null));

            // Initialize commands
            RefreshCommand = new Command(async () => await LoadPostsAsync());
            LikePostCommand = new Command<Post>(async (post) => await LikePost(post));
            SavePostCommand = new Command<Post>(async (post) => await SavePost(post));
            CommentPostCommand = new Command<Post>(async (post) => await NavigateToComments(post));
            SharePostCommand = new Command<Post>(async (post) => await SharePost(post));
            ViewCommentsCommand = new Command<Post>(async (post) => await NavigateToComments(post));
            PostOptionsCommand = new Command<Post>(async (post) => await ShowPostOptions(post));
            PlayVideoCommand = new Command<Post>(async (post) => await PlayVideo(post));

            // Load posts automatically when ViewModel is created
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadPostsAsync();
            });
        }

        public async Task LoadPostsAsync()
        {
            if (IsRefreshing || IsLoading)
                return;

            try
            {
                IsRefreshing = true;
                IsLoading = true;
                Debug.WriteLine("Loading posts...");

                // Clear current posts
                Posts.Clear();

                // Get posts from API
                var posts = await _postService.GetPostsAsync();
                Debug.WriteLine($"Received {posts?.Count ?? 0} posts from service");

                // Debug post data
                if (posts != null && posts.Count > 0)
                {
                    Debug.WriteLine("First post details:");
                    Debug.WriteLine($"  ID: {posts[0].PostId}");
                    Debug.WriteLine($"  Username: {posts[0].UserName}");
                    Debug.WriteLine($"  URL: {posts[0].PostFileURL}");
                    Debug.WriteLine($"  Type: {posts[0].PostType}");
                }
                else
                {
                    Debug.WriteLine("No posts returned from service or list is empty");

                    // Show feedback to the user
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "No Posts Found",
                            "There are no posts available at this time. Please try again later.",
                            "OK");
                    });

                    return;
                }

                // Add posts to collection with robust null handling
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
                                Debug.WriteLine("Skipping null post");
                                skippedPostCount++;
                                continue;
                            }

                            // For posts with missing video/image URLs, try to find them in other properties
                            if (string.IsNullOrWhiteSpace(post.PostFileURL))
                            {
                                // Try to repair the post by checking other properties
                                if (!string.IsNullOrWhiteSpace(post.PostText))
                                {
                                    Debug.WriteLine($"Post {post.PostId} has no PostFileURL but has PostText");
                                    post.PostType = "text"; // Mark as text post
                                }
                                else if (!string.IsNullOrWhiteSpace(post.ThumbnailUrl))
                                {
                                    Debug.WriteLine($"Post {post.PostId} has no PostFileURL but has ThumbnailUrl. Using as PostFileURL.");
                                    post.PostFileURL = post.ThumbnailUrl;
                                    post.PostType = "image";
                                }
                                else
                                {
                                    Debug.WriteLine($"Skipping post {post.PostId}: Missing critical media URL");
                                    skippedPostCount++;
                                    continue;
                                }
                            }

                            // Sanitize the post by ensuring all fields have valid values
                            SanitizePost(post);

                            // Log the post after sanitization
                            Debug.WriteLine($"Adding sanitized post: {post.PostId}, Type: {post.PostType}, URL: {post.PostFileURL}");

                            // Add to the collection on the UI thread
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Posts.Add(post);
                            });
                            validPostCount++;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing post: {ex.Message}");
                            skippedPostCount++;
                        }
                    }

                    Debug.WriteLine($"Added {validPostCount} valid posts, skipped {skippedPostCount} invalid posts");
                    Debug.WriteLine($"Final Posts collection count: {Posts.Count}");

                    // Show user feedback if many posts were skipped
                    if (validPostCount == 0 && skippedPostCount > 0)
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Post Display Issue",
                                "We found posts but couldn't display them. This may be due to formatting issues. Our team is working on a fix.",
                                "OK");
                        });
                    }

                    // Force property changed notification for the collection
                    OnPropertyChanged(nameof(Posts));
                }
                else
                {
                    Debug.WriteLine("No posts were returned from the service");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"Authentication error: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var result = await Application.Current.MainPage.DisplayAlert(
                        "Authentication Error",
                        "Your session has expired. Please log in again.",
                        "Log in", "Cancel");

                    if (result)
                    {
                        // Navigate to login page
                        Application.Current.MainPage = new LoginPage();
                    }
                });
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Network error: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Network Error",
                        "Could not connect to the server. Please check your internet connection and try again.",
                        "OK");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading posts: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Something went wrong while loading posts. Please try again later.",
                        "OK");
                });
            }
            finally
            {
                IsRefreshing = false;
                IsLoading = false;
            }
        }

        // Helper to log all details of a post for debugging
        private void LogPostDetails(Post post)
        {
            if (post == null)
            {
                Debug.WriteLine("Post is null");
                return;
            }

            Debug.WriteLine("=== Raw Post Details ===");
            Debug.WriteLine($"PostId: {post.PostId ?? "null"}");
            Debug.WriteLine($"UserName: {post.UserName ?? "null"}");
            Debug.WriteLine($"Caption: {post.Caption ?? "null"}");
            Debug.WriteLine($"PostFileURL: {post.PostFileURL ?? "null"}");
            Debug.WriteLine($"ThumbnailUrl: {post.ThumbnailUrl ?? "null"}");
            Debug.WriteLine($"ProfileImageURL: {post.ProfileImageURL ?? "null"}");
            Debug.WriteLine($"PostType: {post.PostType ?? "null"}");
            Debug.WriteLine($"Likes: {post.Likes?.ToString() ?? "null"}");
            Debug.WriteLine($"LikedPost: {post.LikedPost?.ToString() ?? "null"}");
            Debug.WriteLine($"SavedPost: {post.SavedPost?.ToString() ?? "null"}");
            Debug.WriteLine($"PostCommentCount: {post.PostCommentCount?.ToString() ?? "null"}");
            Debug.WriteLine($"RelativeTime: {post.RelativeTime ?? "null"}");
            Debug.WriteLine("=======================");
        }

        // Helper to ensure all post fields have valid values
        private void SanitizePost(Post post)
        {
            try
            {
                // Required fields
                post.PostId = string.IsNullOrWhiteSpace(post.PostId) ? Guid.NewGuid().ToString() : post.PostId;

                // Handle null PostFileURL - critical field
                if (string.IsNullOrWhiteSpace(post.PostFileURL))
                {
                    Debug.WriteLine($"Warning: Post {post.PostId} has null/empty PostFileURL. Setting a placeholder.");
                    post.PostFileURL = "https://via.placeholder.com/300";
                    post.PostType = "image"; // Default to image for empty URLs
                }
                else
                {
                    // Ensure URL has a protocol (http or https)
                    if (!post.PostFileURL.StartsWith("http://") && !post.PostFileURL.StartsWith("https://"))
                    {
                        // Add https protocol if missing
                        post.PostFileURL = "https://" + post.PostFileURL.TrimStart('/');
                        Debug.WriteLine($"Fixed URL by adding protocol: {post.PostFileURL}");
                    }
                }

                // If PostType is null or empty, detect it from the URL
                if (string.IsNullOrWhiteSpace(post.PostType))
                {
                    post.PostType = DeterminePostType(post.PostFileURL);
                    Debug.WriteLine($"Auto-detected post type: {post.PostType} for URL: {post.PostFileURL}");
                }

                // For video posts, ensure there's a thumbnail URL
                if (post.PostType?.ToLower() == "video" && string.IsNullOrWhiteSpace(post.ThumbnailUrl))
                {
                    // Set a default thumbnail or generate one from video
                    Debug.WriteLine($"Video post {post.PostId} has no thumbnail URL. Setting a placeholder.");
                    post.ThumbnailUrl = "https://via.placeholder.com/300/333333/FFFFFF?text=Video";
                }
                else if (!string.IsNullOrWhiteSpace(post.ThumbnailUrl))
                {
                    // Ensure thumbnail URL has a protocol
                    if (!post.ThumbnailUrl.StartsWith("http://") && !post.ThumbnailUrl.StartsWith("https://"))
                    {
                        post.ThumbnailUrl = "https://" + post.ThumbnailUrl.TrimStart('/');
                        Debug.WriteLine($"Fixed thumbnail URL: {post.ThumbnailUrl}");
                    }
                }

                // Ensure profile image URL has a protocol if it exists
                if (!string.IsNullOrWhiteSpace(post.ProfileImageURL))
                {
                    if (!post.ProfileImageURL.StartsWith("http://") && !post.ProfileImageURL.StartsWith("https://"))
                    {
                        post.ProfileImageURL = "https://" + post.ProfileImageURL.TrimStart('/');
                        Debug.WriteLine($"Fixed profile image URL: {post.ProfileImageURL}");
                    }
                }
                else
                {
                    // If no profile image is available, try to use the thumbnail as fallback
                    if (!string.IsNullOrWhiteSpace(post.ThumbnailUrl))
                    {
                        post.ProfileImageURL = post.ThumbnailUrl;
                    }
                }

                // Set default values for other fields
                post.UserName = post.UserName ?? "Anonymous";
                post.Caption = post.Caption ?? "";
                post.RelativeTime = post.RelativeTime ?? "Recently";
                post.Likes = post.Likes ?? 0;
                post.LikedPost = post.LikedPost ?? false;
                post.SavedPost = post.SavedPost ?? false;
                post.PostCommentCount = post.PostCommentCount ?? 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during post sanitization: {ex.Message}");
                // Don't rethrow - try to continue with the post as-is
            }
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
                OnPropertyChanged(nameof(Posts));

                // In a real app, you would update the like status on the server
                // await _postService.LikePostAsync(post.PostId, post.LikedPost.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error liking post: {ex.Message}");

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
                OnPropertyChanged(nameof(Posts));

                // In a real app, you would update the save status on the server
                // await _postService.SavePostAsync(post.PostId, post.SavedPost.Value);

                string message = post.SavedPost == true ? "Post saved" : "Post unsaved";
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Save Post", message, "OK");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving post: {ex.Message}");

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
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing post options: {ex.Message}");
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

                Debug.WriteLine($"Playing video: {post.PostFileURL}");

                // Navigate directly to the video player page
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error navigating to video player: {ex.Message}");
                        await Application.Current.MainPage.DisplayAlert("Error", $"Navigation error: {ex.Message}", "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing video: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
                });
            }
        }
    }
}