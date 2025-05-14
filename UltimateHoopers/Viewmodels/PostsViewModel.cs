using System;
using System.Collections.ObjectModel;
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

                // Clear current posts
                Posts.Clear();

                // Get posts from API
                var posts = await _postService.GetPostsAsync();

                // Add posts to collection
                if (posts != null)
                {
                    // Process the posts to add missing data if needed
                    foreach (var post in posts)
                    {
                        // Format relative time if it's not set
                        if (string.IsNullOrEmpty(post.RelativeTime) && !string.IsNullOrEmpty(post.PostedDate))
                        {
                            post.RelativeTime = FormatRelativeTime(post.PostedDate);
                        }

                        // Set default PostType if not specified
                        if (string.IsNullOrEmpty(post.PostType) && !string.IsNullOrEmpty(post.PostFileURL))
                        {
                            // Determine type based on file extension
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
                        }

                        // Ensure UserName is set
                        if (string.IsNullOrEmpty(post.UserName) && (!string.IsNullOrEmpty(post.FirstName) || !string.IsNullOrEmpty(post.LastName)))
                        {
                            post.UserName = $"{post.FirstName} {post.LastName}".Trim();
                        }

                        // Add to the collection
                        Posts.Add(post);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                await Shell.Current.DisplayAlert("Authentication Error", "Please log in to view posts", "OK");
                // Could navigate to login page here
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
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
            // In a real implementation, this would open a video player
            await Shell.Current.DisplayAlert("Play Video", "Video player coming soon!", "OK");
        }
    }
}