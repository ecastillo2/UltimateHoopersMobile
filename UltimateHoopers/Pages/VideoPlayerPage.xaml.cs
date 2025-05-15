using Domain;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using Microsoft.Maui.Controls.Xaml;
using UltimateHoopers.ViewModels;

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

                // Set thumbnail image - use ImageSource.FromUri
                string thumbnailUrl = !string.IsNullOrWhiteSpace(_post.ThumbnailUrl) ?
                    _post.ThumbnailUrl : _post.PostFileURL;

                if (!string.IsNullOrWhiteSpace(thumbnailUrl))
                {
                    Debug.WriteLine($"Setting thumbnail: {thumbnailUrl}");
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

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Automatically start playing when the page appears
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Small delay to ensure the UI is ready
                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    OnPlayButtonTapped(this, EventArgs.Empty);
                    return false; // Don't repeat
                });
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
            // Add cache busting parameter
            string cacheBustParam = DateTime.Now.Ticks.ToString();
            string urlWithCacheBusting = videoUrl.Contains("?")
                ? $"{videoUrl}&cb={cacheBustParam}"
                : $"{videoUrl}?cb={cacheBustParam}";

            // Log the video URL being used
            Debug.WriteLine($"Using video URL with cache busting: {urlWithCacheBusting}");

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
            
            video.addEventListener('error', function(e) {
                console.log('Video error: ' + (e.target.error ? e.target.error.code : 'unknown'));
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

        // Rest of the code remains the same
    }
}