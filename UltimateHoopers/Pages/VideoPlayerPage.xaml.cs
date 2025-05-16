using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using UltimateHoopers.Controls;

namespace UltimateHoopers.Pages
{
    public partial class VideoPlayerPage : ContentPage
    {
        private readonly Domain.Post _post;
        private bool _isVideoLoaded = false;
        private bool _isMuted = false; // Default to unmuted for better fullscreen experience

        // Default constructor for design time and XAML previews
        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        // Constructor with post parameter
        public VideoPlayerPage(Domain.Post post)
        {
            InitializeComponent();
            _post = post ?? throw new ArgumentNullException(nameof(post));

            try
            {
                // Set binding context for social interactions
                BindingContext = _post;

                // Set caption if available
                if (!string.IsNullOrWhiteSpace(_post.Caption))
                {
                    captionLabel.Text = _post.Caption;
                }
                else if (!string.IsNullOrWhiteSpace(_post.UserName))
                {
                    captionLabel.Text = "Video by " + _post.UserName;
                }
                else
                {
                    captionLabel.Text = "Video";
                }

                // Set thumbnail image - prioritize ThumbnailUrl if available, otherwise use PostFileURL
                SetThumbnailImage();

                // Initially show the fallback grid with thumbnail and play button
                fallbackGrid.IsVisible = true;
                videoWebView.IsVisible = false;
                loadingIndicator.IsVisible = false;

                // Show volume button with appropriate icon
                UpdateVolumeButtonIcon();

                // Debug output
                Debug.WriteLine($"VideoPlayerPage created for post: {_post.PostId}");
                Debug.WriteLine($"PostFileURL: {_post.PostFileURL}");
                Debug.WriteLine($"ThumbnailUrl: {_post.ThumbnailUrl ?? "Not available"}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in VideoPlayerPage constructor: {ex.Message}");
                DisplayAlert("Error", $"Error loading video: {ex.Message}", "OK");
            }
        }

        private void SetThumbnailImage()
        {
            try
            {
                // Choose the thumbnail source - prefer ThumbnailUrl if available
                string thumbnailUrl = !string.IsNullOrWhiteSpace(_post.ThumbnailUrl) ?
                    _post.ThumbnailUrl : _post.PostFileURL;

                if (!string.IsNullOrWhiteSpace(thumbnailUrl))
                {
                    Debug.WriteLine($"Setting thumbnail: {thumbnailUrl}");

                    // Make sure the URL has a valid protocol
                    if (!thumbnailUrl.StartsWith("http://") && !thumbnailUrl.StartsWith("https://"))
                    {
                        thumbnailUrl = "https://" + thumbnailUrl.TrimStart('/');
                        Debug.WriteLine($"Fixed thumbnail URL with protocol: {thumbnailUrl}");
                    }

                    try
                    {
                        thumbnailImage.Source = ImageSource.FromUri(new Uri(thumbnailUrl));
                    }
                    catch (UriFormatException ex)
                    {
                        Debug.WriteLine($"Invalid thumbnail URI: {ex.Message}");
                        // Fall back to default image
                        thumbnailImage.Source = "dotnet_bot.png";
                    }
                }
                else
                {
                    thumbnailImage.Source = "dotnet_bot.png";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting thumbnail: {ex.Message}");
                thumbnailImage.Source = "dotnet_bot.png"; // Fallback to default image
            }
        }

        // Override this method to help with audio playback on appearing
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Automatically start playing when the page appears
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Small delay to ensure the UI is ready
                await Task.Delay(500);

                // Start playback
                OnPlayButtonTapped(this, EventArgs.Empty);

                // After another short delay, try to ensure audio is properly initialized
                await Task.Delay(1000);
                TryResumeAudioContext();
            });
        }

        // Handle the play button tap event
        private void OnPlayButtonTapped(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Play button tapped");

                // Only proceed if we have a valid video URL
                if (string.IsNullOrWhiteSpace(_post?.PostFileURL))
                {
                    Debug.WriteLine("PostFileURL is empty");
                    DisplayAlert("Error", "Video URL is not available", "OK");
                    return;
                }

                // Validate URL format
                if (!Uri.TryCreate(_post.PostFileURL, UriKind.Absolute, out Uri videoUri))
                {
                    Debug.WriteLine($"Invalid video URL format: {_post.PostFileURL}");
                    DisplayAlert("Error", "Invalid video URL format", "OK");
                    return;
                }

                // Show loading indicator, hide play button
                playButtonFrame.IsVisible = false;
                loadingIndicator.IsVisible = true;

                string videoUrl = _post.PostFileURL;
                Debug.WriteLine($"Loading video URL: {videoUrl}");

                // Ensure URL has a protocol
                if (!videoUrl.StartsWith("http://") && !videoUrl.StartsWith("https://"))
                {
                    videoUrl = "https://" + videoUrl.TrimStart('/');
                    Debug.WriteLine($"Fixed video URL with protocol: {videoUrl}");
                }

                // Create an enhanced HTML wrapper for the video
                string videoHtml = GetVideoHtml(videoUrl);

                // Load the HTML into the WebView
                videoWebView.Source = new HtmlWebViewSource
                {
                    Html = videoHtml
                };

                // Show the WebView
                videoWebView.IsVisible = true;
                Debug.WriteLine("WebView is now visible with video source set");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing video: {ex.Message}");
                DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");

                // Reset UI states
                playButtonFrame.IsVisible = true;
                loadingIndicator.IsVisible = false;
            }
        }

        // Toggle mute state
        // Toggle mute state
        private void OnVolumeButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Toggle mute state
                _isMuted = !_isMuted;
                Debug.WriteLine($"Volume button clicked. New mute state: {_isMuted}");

                // Update the volume button icon immediately for UI feedback
                UpdateVolumeButtonIcon();

                // Update the video mute state if it's loaded
                if (_isVideoLoaded && videoWebView.Handler != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () => {
                        try
                        {
                            // Execute stronger JavaScript to control volume
                            // This directly targets the video element and forces volume to 1.0 when unmuted
                            string js = @"
                        (function() {
                            try {
                                var video = document.getElementById('videoPlayer');
                                if (video) {
                                    // Set muted property
                                    video.muted = " + (_isMuted ? "true" : "false") + @";
                                    
                                    // When unmuting, explicitly set volume to 1
                                    " + (!_isMuted ? "video.volume = 1.0;" : "") + @"
                                    
                                    // Try to wake up audio context if needed
                                    if (!video.muted) {
                                        // Create a user gesture interaction to help browsers enable sound
                                        var context = new (window.AudioContext || window.webkitAudioContext)();
                                        context.resume().then(() => console.log('AudioContext resumed'));
                                        
                                        // Force play to re-engage audio
                                        var playPromise = video.play();
                                        if (playPromise !== undefined) {
                                            playPromise.catch(e => {
                                                console.log('Play failed after unmute: ' + e);
                                                // Try once more with user interaction context
                                                video.play();
                                            });
                                        }
                                    }
                                    
                                    console.log('Volume and mute settings applied. Muted: ' + video.muted + ', Volume: ' + video.volume);
                                    return {muted: video.muted, volume: video.volume};
                                } else {
                                    console.error('Video element not found');
                                    return {error: 'Video element not found'};
                                }
                            } catch(e) {
                                console.error('Error in volume control JS: ' + e);
                                return {error: e.toString()};
                            }
                        })();
                    ";

                            string result = await videoWebView.EvaluateJavaScriptAsync(js);
                            Debug.WriteLine($"JavaScript volume control result: {result}");

                            // If unmuting, try a second approach after a short delay to ensure it takes effect
                            if (!_isMuted)
                            {
                                await Task.Delay(300);
                                await videoWebView.EvaluateJavaScriptAsync(
                                    "var video = document.getElementById('videoPlayer'); " +
                                    "if(video) { video.volume = 1.0; video.muted = false; }");
                            }
                        }
                        catch (Exception jsEx)
                        {
                            Debug.WriteLine($"JavaScript evaluation error: {jsEx.Message}");

                            // If JavaScript fails, try reloading the video with the new mute state
                            if (!string.IsNullOrEmpty(_post?.PostFileURL))
                            {
                                Debug.WriteLine("Reloading video with new mute state after JavaScript error");
                                videoWebView.Source = new HtmlWebViewSource { Html = GetVideoHtml(_post.PostFileURL) };
                            }
                        }
                    });
                }
                else if (!_isVideoLoaded && fallbackGrid.IsVisible)
                {
                    // If video isn't loaded yet, we'll apply the mute state when it loads
                    Debug.WriteLine("Video not loaded yet, mute state will be applied when it loads");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling mute: {ex.Message}");
            }
        }

        // Update the volume button icon based on current mute state
        private void UpdateVolumeButtonIcon()
        {
            if (volumeButton != null && volumeIcon != null)
            {
                volumeIcon.Text = _isMuted ? "🔇" : "🔊";
                Debug.WriteLine($"Volume icon updated to: {volumeIcon.Text}");
            }
        }

        // HTML template for video player
        private string GetVideoHtml(string videoUrl)
        {
            // Add cache busting parameter
            string cacheBustParam = DateTime.Now.Ticks.ToString();
            string urlWithCacheBusting = videoUrl.Contains("?")
                ? $"{videoUrl}&cb={cacheBustParam}"
                : $"{videoUrl}?cb={cacheBustParam}";

            // Log the video URL being used
            Debug.WriteLine($"Using video URL with cache busting: {urlWithCacheBusting}");
            Debug.WriteLine($"Initial muted state: {_isMuted}");

            return @"<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <style>
        body {
            margin: 0;
            padding: 0;
            background-color: #000;
            height: 100vh;
            width: 100vw;
            display: flex;
            justify-content: center;
            align-items: center;
            overflow: hidden;
        }
        .video-container {
            width: 100%;
            height: 100%;
            display: flex;
            justify-content: center;
            align-items: center;
        }
        video {
            width: 100%;
            height: 100%;
            max-height: 100vh;
            object-fit: contain;
        }
    </style>
</head>
<body>
    <div class='video-container'>
        <video id='videoPlayer' controls autoplay playsinline" + (_isMuted ? " muted" : "") + @">
            <source src='" + urlWithCacheBusting + @"' type='video/mp4'>
            Your browser does not support HTML5 video.
        </video>
    </div>
    <script>
        // Wait for document to be fully loaded
        document.addEventListener('DOMContentLoaded', function() {
            console.log('Video player HTML initialized');
            var video = document.getElementById('videoPlayer');
            
            if (video) {
                // Configure audio settings
                video.muted = " + (_isMuted ? "true" : "false") + @";
                video.volume = 1.0; // Set volume to maximum
                
                // Log settings for debugging
                console.log('Initial muted state: ' + video.muted);
                console.log('Initial volume: ' + video.volume);
                
                // Enable audio context to help with audio playback
                try {
                    window.addEventListener('click', function() {
                        // Create audio context on first user interaction
                        var audioCtx = new (window.AudioContext || window.webkitAudioContext)();
                        if (audioCtx.state === 'suspended') {
                            audioCtx.resume().then(function() {
                                console.log('AudioContext resumed successfully');
                            });
                        }
                    }, { once: true });
                } catch(e) {
                    console.log('AudioContext not supported: ' + e);
                }
                
                // Monitor volume changes for debugging
                video.addEventListener('volumechange', function() {
                    console.log('Volume changed. Muted: ' + video.muted + ', Volume: ' + video.volume);
                });
                
                // Setup other event listeners
                video.addEventListener('canplay', function() {
                    console.log('Video can play now');
                    window.location.href = 'maui-callback://videoCanPlay';
                });
                
                video.addEventListener('error', function(e) {
                    console.log('Video error: ' + (e.target.error ? e.target.error.code : 'unknown'));
                    window.location.href = 'maui-callback://videoError';
                });
                
                video.addEventListener('playing', function() {
                    console.log('Video now playing');
                    window.location.href = 'maui-callback://videoPlaying';
                    
                    // Set volume after start playing
                    video.muted = " + (_isMuted ? "true" : "false") + @";
                    video.volume = 1.0;
                });
                
                // Force load the video
                video.load();
                
                // Try to autoplay with sound if not muted
                setTimeout(function() {
                    console.log('Attempting to play video...');
                    
                    // Add this code to help with audio permissions
                    if (!" + (_isMuted ? "true" : "false") + @") {
                        // If not muted, try to wake audio context
                        try {
                            var audioCtx = new (window.AudioContext || window.webkitAudioContext)();
                            audioCtx.resume();
                        } catch(e) {}
                    }
                    
                    var playPromise = video.play();
                    
                    if (playPromise !== undefined) {
                        playPromise.then(function() {
                            console.log('Autoplay started successfully');
                            // Set volume again after successful play
                            video.volume = 1.0;
                            video.muted = " + (_isMuted ? "true" : "false") + @";
                        }).catch(function(error) {
                            console.log('Autoplay prevented: ' + error);
                            
                            // If autoplay fails, add a play button that will help trigger play with user gesture
                            if (!document.getElementById('manual-play-button')) {
                                var playButton = document.createElement('button');
                                playButton.id = 'manual-play-button';
                                playButton.innerText = 'Play Video';
                                playButton.style.position = 'absolute';
                                playButton.style.top = '50%';
                                playButton.style.left = '50%';
                                playButton.style.transform = 'translate(-50%, -50%)';
                                playButton.style.zIndex = '1000';
                                playButton.style.padding = '10px 20px';
                                playButton.style.backgroundColor = '#512BD4';
                                playButton.style.color = 'white';
                                playButton.style.border = 'none';
                                playButton.style.borderRadius = '4px';
                                playButton.style.cursor = 'pointer';
                                
                                playButton.onclick = function() {
                                    video.play()
                                        .then(function() {
                                            console.log('Video played via button click');
                                            video.volume = 1.0;
                                            video.muted = " + (_isMuted ? "true" : "false") + @";
                                            playButton.style.display = 'none';
                                        })
                                        .catch(function(e) {
                                            console.log('Play via button still failed: ' + e);
                                        });
                                };
                                
                                document.body.appendChild(playButton);
                            }
                        });
                    }
                }, 500);
            } else {
                console.error('Video element not found!');
            }
        });
    </script>
</body>
</html>";
        }

        private async void TryResumeAudioContext()
        {
            try
            {
                // Execute JavaScript to try to wake up any audio context
                if (videoWebView.Handler != null)
                {
                    string result = await videoWebView.EvaluateJavaScriptAsync(@"
                (function() {
                    try {
                        var video = document.getElementById('videoPlayer');
                        if (video) {
                            // Create and resume audio context
                            var context = new (window.AudioContext || window.webkitAudioContext)();
                            context.resume().then(() => console.log('AudioContext resumed'));
                            
                            // Unmute and set volume if needed
                            if (!" + (_isMuted ? "true" : "false") + @") {
                                video.muted = false;
                                video.volume = 1.0;
                                
                                // Try play again
                                video.play().catch(e => console.log('Play error: ' + e));
                            }
                            return 'Audio context resumed';
                        }
                        return 'Video element not found';
                    } catch(e) {
                        return 'Error: ' + e;
                    }
                })();
            ");

                    Debug.WriteLine($"Audio context resume result: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resuming audio context: {ex.Message}");
            }
        }

        // WebView navigation event handlers
        private void VideoWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("maui-callback://"))
            {
                e.Cancel = true; // Cancel the navigation to prevent page from changing

                if (e.Url == "maui-callback://videoLoaded")
                {
                    Debug.WriteLine("Video loaded callback received");
                    // Data is loaded but video may not be playing yet
                }
                else if (e.Url == "maui-callback://videoCanPlay")
                {
                    Debug.WriteLine("Video can play callback received");
                    // Video is ready to play but hasn't necessarily started playing
                }
                else if (e.Url == "maui-callback://videoPlaying")
                {
                    Debug.WriteLine("Video playing callback received");

                    // Now that the video is actually playing, we can handle video element visibility
                    if (sender is WebView webView && webView.Parent is AutoPlayVideoElement videoElement)
                    {
                        // Tell the video element to handle the playing state
                        videoElement.HandleVideoPlaying();
                    }
                }
                else if (e.Url == "maui-callback://videoError")
                {
                    Debug.WriteLine("Video error callback received");
                    // Show error state in UI if needed
                }
                else if (e.Url == "maui-callback://videoWaiting")
                {
                    Debug.WriteLine("Video waiting/buffering callback received");
                    // Could show a buffering indicator here if needed
                }
            }
        }

        private void VideoWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Success)
            {
                Debug.WriteLine("WebView loaded successfully");
                _isVideoLoaded = true;

                // Add a delay before hiding the loading indicator
                // This gives the video player time to initialize
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1000);

                    // If the fallback is still visible, hide it
                    if (fallbackGrid.IsVisible)
                    {
                        fallbackGrid.IsVisible = false;
                        loadingIndicator.IsVisible = false;
                    }

                    // Apply mute state after successful load
                    try
                    {
                        string result = await videoWebView.EvaluateJavaScriptAsync(
                            $"console.log('Setting muted after navigation to {(_isMuted ? "true" : "false")}');" +
                            $"var video = document.getElementById('videoPlayer');" +
                            $"if(video) {{ " +
                            $"  video.volume = 1.0; " +
                            $"  video.muted = {(_isMuted ? "true" : "false")}; " +
                            $"  console.log('Muted: ' + video.muted + ', Volume: ' + video.volume); " +
                            $"  true; " +
                            $"}} else {{ " +
                            $"  console.log('Video element not found after navigation'); " +
                            $"  false; " +
                            $"}}");

                        Debug.WriteLine($"Applied mute state after navigation, result: {result}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error applying mute state after navigation: {ex.Message}");
                    }
                });
            }
            else
            {
                Debug.WriteLine($"WebView navigation failed: {e.Result}");
                _isVideoLoaded = false;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Show fallback on error
                    fallbackGrid.IsVisible = true;
                    playButtonFrame.IsVisible = true;
                    loadingIndicator.IsVisible = false;
                });
            }
        }

        // Close button handler
        private async void OnCloseClicked(object sender, EventArgs e)
        {
            try
            {
                // Clean up resources if needed
                Debug.WriteLine("Closing video player");

                // Use Navigation to go back/dismiss the modal
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error closing video player: {ex.Message}");
                // Fallback if navigation fails
                if (Application.Current != null)
                {
                    Application.Current.MainPage = new AppShell();
                }
            }
        }

        private void OnLikeClicked(object sender, EventArgs e)
        {
            if (_post != null)
            {
                // Toggle like state
                _post.LikedPost = !(_post.LikedPost ?? false);

                // Update like count
                if (_post.LikedPost == true)
                {
                    _post.Likes = (_post.Likes ?? 0) + 1;
                }
                else
                {
                    _post.Likes = Math.Max(0, (_post.Likes ?? 0) - 1);
                }

                // Force UI update
                OnPropertyChanged(nameof(_post.LikedPost));

                // You would call your API here to update the like status
                // await _postService.LikePostAsync(_post.PostId, _post.LikedPost ?? false);
            }
        }

        private async void OnCommentsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Comments", "Comments feature coming soon!", "OK");
        }

        private async void OnShareClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Share", "Share feature coming soon!", "OK");
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_post != null)
            {
                // Toggle save state
                _post.SavedPost = !(_post.SavedPost ?? false);

                // Force UI update
                OnPropertyChanged(nameof(_post.SavedPost));

                // You would call your API here to update the save status
                // await _postService.SavePostAsync(_post.PostId, _post.SavedPost ?? false);

                // Show confirmation
                string message = _post.SavedPost == true ? "Post saved to collection" : "Post removed from collection";
                await DisplayAlert("Saved", message, "OK");
            }
        }
    }
}