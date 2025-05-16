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
    public class ProfileViewModel : BindableObject
    {
        private readonly IProfileService _profileService;
        private bool _isRefreshing;

        public ObservableCollection<Profile> Profiles { get; } = new ObservableCollection<Profile>();

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


        public ProfileViewModel(IProfileService profileService)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            Console.WriteLine("ProfileViewModel created with profileService: " + (profileService != null));

            // Initialize commands
            RefreshCommand = new Command(async () => await LoadProfilesAsync());
            //LikePostCommand = new Command<Post>(async (post) => await LikePost(post));
           
            CommentPostCommand = new Command<Post>(async (post) => await NavigateToComments(post));
           
            ViewCommentsCommand = new Command<Post>(async (post) => await NavigateToComments(post));
       
            
        }

        public async Task LoadProfilesAsync()
        {
            if (IsRefreshing)
                return;

            try
            {
                IsRefreshing = true;
                Console.WriteLine("Loading hoopers...");

                // Clear current profiles
                Profiles.Clear();

                // Get posts from API
                var profiles = await _profileService.GetProfilesAsync();
                Console.WriteLine($"Received {profiles?.Count ?? 0} posts from service");

                // Debug post data
                if (profiles != null && profiles.Count > 0)
                {
                    Console.WriteLine("First post details:");
                    //Console.WriteLine($"  ID: {profiles[0].PostId}");
                    //Console.WriteLine($"  Username: {profiles[0].UserName}");
                    //Console.WriteLine($"  URL: {profiles[0].PostFileURL}");
                    //Console.WriteLine($"  Type: {profiles[0].PostType}");
                }
                else
                {
                    Console.WriteLine("No posts returned from service or list is empty");
                }

                // Add posts to collection with robust null handling
                if (profiles != null)
                {
                    int validPostCount = 0;
                    int skippedPostCount = 0;

                    // Process the posts to add missing data if needed
                    foreach (var profile in profiles)
                    {
                        try
                        {
                            // Debug log the raw post
                            //LogProfileDetails(profile);

                            // Skip completely invalid posts (null or missing required field)
                            if (profile == null)
                            {
                                Console.WriteLine("Skipping null post");
                                skippedPostCount++;
                                continue;
                            }

                           

                            // Sanitize the post by ensuring all fields have valid values
                            //SanitizePost(profile);

                            // Log the post after sanitization
                            Console.WriteLine($"Adding sanitized post: {profile.ProfileId},  URL: {profile.ImageURL}");

                            // Add to the collection
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Profiles.Add(profile);
                            });
                            validPostCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing post: {ex.Message}");
                            skippedPostCount++;
                        }
                    }

                    Console.WriteLine($"Added {validPostCount} valid Profiles, skipped {skippedPostCount} invalid Profiles");
                    Console.WriteLine($"Final Profiles collection count: {Profiles.Count}");

                    // Force property changed notification for the collection
                    OnPropertyChanged(nameof(Profiles));
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
        private void LogProfilesDetails(Profile profiles)
        {
            if (profiles == null)
            {
                Console.WriteLine("Profile is null");
                return;
            }

            Console.WriteLine("=== Raw Profiles Details ===");
            Console.WriteLine($"ProfileId: {profiles.ProfileId ?? "null"}");
            //Console.WriteLine($"UserName: {post.UserName ?? "null"}");
            //Console.WriteLine($"FirstName: {post.FirstName ?? "null"}");
            //Console.WriteLine($"LastName: {post.LastName ?? "null"}");
            //Console.WriteLine($"Caption: {post.Caption ?? "null"}");
            //Console.WriteLine($"PostFileURL: {post.PostFileURL ?? "null"}");
            //Console.WriteLine($"ThumbnailUrl: {post.ThumbnailUrl ?? "null"}");
            //Console.WriteLine($"ProfileImageURL: {post.ProfileImageURL ?? "null"}");
            //Console.WriteLine($"PostType: {post.PostType ?? "null"}");
            //Console.WriteLine($"Likes: {post.Likes?.ToString() ?? "null"}");
            //Console.WriteLine($"LikedPost: {post.LikedPost?.ToString() ?? "null"}");
            //Console.WriteLine($"SavedPost: {post.SavedPost?.ToString() ?? "null"}");
            //Console.WriteLine($"PostCommentCount: {post.PostCommentCount?.ToString() ?? "null"}");
            //Console.WriteLine($"RelativeTime: {post.RelativeTime ?? "null"}");
            //Console.WriteLine($"PostedDate: {post.PostedDate ?? "null"}");
            //Console.WriteLine("=======================");
        }

        // Helper to ensure all post fields have valid values
        //private void SanitizePost(Post post)
        //{
        //    // Required fields
        //    post.PostId = string.IsNullOrWhiteSpace(post.PostId) ? Guid.NewGuid().ToString() : post.PostId;

        //    // Validate PostFileURL (already checked for null/empty before this method is called)
        //    if (!string.IsNullOrWhiteSpace(post.PostFileURL))
        //    {
        //        try
        //        {
        //            // Ensure URL has a protocol (http or https)
        //            if (!post.PostFileURL.StartsWith("http://") && !post.PostFileURL.StartsWith("https://"))
        //            {
        //                // Add https protocol if missing
        //                post.PostFileURL = "https://" + post.PostFileURL.TrimStart('/');
        //                Console.WriteLine($"Fixed URL by adding protocol: {post.PostFileURL}");
        //            }

        //            // Validate by creating a URI object (will throw if invalid)
        //            var uri = new Uri(post.PostFileURL);
        //        }
        //        catch (UriFormatException ex)
        //        {
        //            Console.WriteLine($"Invalid URL format: {post.PostFileURL}, Error: {ex.Message}");
        //            // If URL is invalid but not empty, don't clear it yet - let the converter handle it
        //        }
        //    }

        //    // Accurately determine PostType based on file extension
        //    if (string.IsNullOrWhiteSpace(post.PostType))
        //    {
        //        post.PostType = DeterminePostType(post.PostFileURL);
        //        Console.WriteLine($"Auto-detected post type: {post.PostType} for URL: {post.PostFileURL}");
        //    }

        //    // If PostType is already set but doesn't match the URL, prioritize the content type
        //    else if (post.PostType.ToLower() != DeterminePostType(post.PostFileURL))
        //    {
        //        string autoDetectedType = DeterminePostType(post.PostFileURL);
        //        Console.WriteLine($"PostType mismatch - Existing: {post.PostType}, Detected: {autoDetectedType} for URL: {post.PostFileURL}");

        //        // Only override if we can confidently determine it's a video from the URL
        //        if (autoDetectedType == "video" && IsVideoUrl(post.PostFileURL))
        //        {
        //            post.PostType = "video";
        //            Console.WriteLine($"Changed post type to video based on URL extension: {post.PostFileURL}");
        //        }
        //    }

        //    // Validate ThumbnailUrl if present (for video posts)
        //    if (post.PostType.ToLower() == "video")
        //    {
        //        // If it's a video but no thumbnail, use a logic to determine one
        //        if (string.IsNullOrWhiteSpace(post.ThumbnailUrl))
        //        {
        //            // You could set a default video thumbnail here if needed
        //            Console.WriteLine($"Video post {post.PostId} has no thumbnail URL");
        //        }
        //        else
        //        {
        //            // Validate and fix the thumbnail URL
        //            try
        //            {
        //                if (!post.ThumbnailUrl.StartsWith("http://") && !post.ThumbnailUrl.StartsWith("https://"))
        //                {
        //                    post.ThumbnailUrl = "https://" + post.ThumbnailUrl.TrimStart('/');
        //                    Console.WriteLine($"Fixed thumbnail URL: {post.ThumbnailUrl}");
        //                }

        //                // Validate by creating a URI object
        //                var uri = new Uri(post.ThumbnailUrl);
        //            }
        //            catch (UriFormatException ex)
        //            {
        //                Console.WriteLine($"Invalid thumbnail URL: {post.ThumbnailUrl}, Error: {ex.Message}");
        //            }
        //        }
        //    }

        //    // Fill other optional fields with defaults if missing
        //    post.UserName = string.IsNullOrWhiteSpace(post.UserName) ?
        //        (string.IsNullOrWhiteSpace(post.FirstName) && string.IsNullOrWhiteSpace(post.LastName) ?
        //            "Anonymous User" :
        //            $"{post.FirstName ?? ""} {post.LastName ?? ""}".Trim()) :
        //        post.UserName;

        //    post.Caption = post.Caption ?? ""; // Empty string instead of null
        //    post.RelativeTime = string.IsNullOrWhiteSpace(post.RelativeTime) ?
        //        (string.IsNullOrWhiteSpace(post.PostedDate) ?
        //            "Recently" :
        //            FormatRelativeTime(post.PostedDate)) :
        //        post.RelativeTime;

        //    // Numeric fields
        //    post.Likes = post.Likes ?? 0;
        //    post.LikedPost = post.LikedPost ?? false;
        //    post.SavedPost = post.SavedPost ?? false;
        //    post.PostCommentCount = post.PostCommentCount ?? 0;
        //}

        // Helper to determine post type from URL
        private string DeterminePostType(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "image"; // Default to image

            // Check for common video extensions
            if (IsVideoUrl(url))
                return "video";

            // Default to image for all other formats
            return "image";
        }

        // Helper to check if a URL points to a video
        private bool IsVideoUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            string lowercaseUrl = url.ToLower();
            return lowercaseUrl.EndsWith(".mp4") ||
                   lowercaseUrl.EndsWith(".mov") ||
                   lowercaseUrl.EndsWith(".avi") ||
                   lowercaseUrl.EndsWith(".webm") ||
                   lowercaseUrl.EndsWith(".mkv") ||
                   lowercaseUrl.Contains("video") ||
                   lowercaseUrl.Contains("mp4") ||
                   lowercaseUrl.Contains("youtu");
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
        //private async Task LikePost(Post post)
        //{
        //    try
        //    {
        //        // Toggle the liked state locally for immediate feedback
        //        post.LikedPost = !(post.LikedPost ?? false);

        //        // Update like count
        //        if (post.LikedPost == true)
        //        {
        //            post.Likes = (post.Likes ?? 0) + 1;
        //        }
        //        else
        //        {
        //            post.Likes = Math.Max(0, (post.Likes ?? 0) - 1);
        //        }

        //        // Trigger UI update
        //        var index = Posts.IndexOf(post);
        //        if (index >= 0)
        //        {
        //            Posts[index] = post;
        //        }

        //        // TODO: Implement API call to like/unlike post
        //        // await _postService.LikePostAsync(post.PostId, post.LikedPost ?? false);

        //        // For now, just show a message
        //        await MainThread.InvokeOnMainThreadAsync(async () =>
        //        {
        //            await Application.Current.MainPage.DisplayAlert("Like Post",
        //                post.LikedPost == true ? "Post liked!" : "Post unliked!",
        //                "OK");
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await MainThread.InvokeOnMainThreadAsync(async () =>
        //        {
        //            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to like post: {ex.Message}", "OK");
        //        });
        //    }
        //}

        

        private async Task NavigateToComments(Post post)
        {
            // This would navigate to a comments page
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Comments", "Comments feature coming soon!", "OK");
            });
        }

       
        
       
    }
}