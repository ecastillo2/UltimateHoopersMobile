using Domain;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace UltimateHoopers.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPlayerPage : ContentPage
    {
        private readonly Post _post;
        private bool _isVideoLoaded = false;

        // Default constructor for design time and XAML previews
        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        // Constructor with post parameter
        public VideoPlayerPage(Post post)
        {
            InitializeComponent();
            _post = post ?? throw new ArgumentNullException(nameof(post));

            try
            {
                // Set caption if available
                if (!string.IsNullOrWhiteSpace(_post.Caption))
                {
                    captionLabel.Text = _post.Caption;
                }
                else if (!string.IsNullOrWhiteSpace(_post.UserName))
                {
                    captionLabel.Text = $"Video by {_post.UserName}";
                }
                else
                {
                    captionLabel.Text = "Video";
                }

                // Set thumbnail image
                string thumbnailUrl = !string.IsNullOrWhiteSpace(_post.ThumbnailUrl) ?
                    _post.ThumbnailUrl : _post.PostFileURL;

                if (!string.IsNullOrWhiteSpace(thumbnailUrl))
                {
                    Debug.WriteLine($"Setting thumbnail: {thumbnailUrl}");
                    thumbnailImage.Source = ImageSource.FromUri(new Uri(thumbnailUrl));
                }

                // Initially show the fallback grid with thumbnail and play button
                fallbackGrid.IsVisible = true;
                videoWebView.IsVisible = false;
                loadingIndicator.IsVisible = false;

                // Debug output
                Debug.WriteLine($"VideoPlayerPage created for post: {_post.PostId}");
                Debug.WriteLine($"PostFileURL: {_post.PostFileURL}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in VideoPlayerPage constructor: {ex.Message}");
                DisplayAlert("Error", $"Error loading video: {ex.Message}", "OK");
            }
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

                // Show loading indicator, hide play button
                playButtonFrame.IsVisible = false;
                loadingIndicator.IsVisible = true;

                string videoUrl = _post.PostFileURL;
                Debug.WriteLine($"Loading video URL: {videoUrl}");

                // Create an enhanced HTML wrapper for the video
                string videoHtml = GetVideoHtml(videoUrl);

                // Load the HTML into the WebView
                videoWebView.Source = new HtmlWebViewSource
                {
                    Html = videoHtml
                };

                // Show the WebView
                videoWebView.IsVisible = true;
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

        private string GetVideoHtml(string videoUrl)
        {
            string cacheBustParam = DateTime.Now.Ticks.ToString();
            string urlWithCacheBusting = videoUrl.Contains("?")
                ? $"{videoUrl}&cb={cacheBustParam}"
                : $"{videoUrl}?cb={cacheBustParam}";

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
        <video id='videoPlayer' controls autoplay playsinline>
            <source src='" + urlWithCacheBusting + @"' type='video/mp4'>
            Your browser does not support HTML5 video.
        </video>
    </div>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            console.log('Video player loaded');
            var video = document.getElementById('videoPlayer');
            
            video.addEventListener('canplay', function() {
                console.log('Video can play');
                window.location.href = 'maui-callback://videoCanPlay';
            });
            
            video.addEventListener('error', function() {
                console.log('Video error');
                window.location.href = 'maui-callback://videoError';
            });
            
            video.addEventListener('playing', function() {
                console.log('Video playing');
                window.location.href = 'maui-callback://videoPlaying';
            });
            
            // Force load
            video.load();
            
            // Try autoplay
            setTimeout(function() {
                var playPromise = video.play();
                if (playPromise !== undefined) {
                    playPromise.catch(function(error) {
                        console.log('Autoplay prevented:', error);
                    });
                }
            }, 1000);
        });
    </script>
</body>
</html>";
        }

        // WebView navigation event handlers
        private void VideoWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("maui-callback://"))
            {
                e.Cancel = true; // Cancel the navigation

                if (e.Url == "maui-callback://videoCanPlay")
                {
                    Debug.WriteLine("Video can play callback received");
                }
                else if (e.Url == "maui-callback://videoError")
                {
                    Debug.WriteLine("Video error callback received");
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        bool openExternal = await DisplayAlert(
                            "Playback Issue",
                            "The video couldn't be played in the app. Would you like to open it in your browser?",
                            "Open in Browser",
                            "Cancel");

                        if (openExternal && _post != null && !string.IsNullOrWhiteSpace(_post.PostFileURL))
                        {
                            await Launcher.OpenAsync(new Uri(_post.PostFileURL));
                        }
                        else
                        {
                            // Reset UI if user cancels
                            fallbackGrid.IsVisible = true;
                            playButtonFrame.IsVisible = true;
                            loadingIndicator.IsVisible = false;
                        }
                    });
                }
                else if (e.Url == "maui-callback://videoPlaying")
                {
                    Debug.WriteLine("Video playing callback received");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Hide fallback once playing
                        fallbackGrid.IsVisible = false;
                        loadingIndicator.IsVisible = false;
                    });
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
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Device.StartTimer(TimeSpan.FromSeconds(3), () =>
                    {
                        // If the fallback is still visible, hide it
                        if (fallbackGrid.IsVisible)
                        {
                            fallbackGrid.IsVisible = false;
                            loadingIndicator.IsVisible = false;
                        }
                        return false; // Don't repeat
                    });
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

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private async void OnOpenInBrowserClicked(object sender, EventArgs e)
        {
            try
            {
                if (_post != null && !string.IsNullOrWhiteSpace(_post.PostFileURL))
                {
                    await Launcher.OpenAsync(new Uri(_post.PostFileURL));
                }
                else
                {
                    await DisplayAlert("Error", "Video URL is not available", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening in browser: {ex.Message}");
                await DisplayAlert("Error", $"Could not open browser: {ex.Message}", "OK");
            }
        }
    }
}