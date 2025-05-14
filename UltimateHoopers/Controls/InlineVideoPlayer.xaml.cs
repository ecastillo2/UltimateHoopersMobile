using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace UltimateHoopers.Controls
{
    public partial class InlineVideoPlayer : ContentView
    {
        // Bindable properties
        public static readonly BindableProperty VideoUrlProperty =
            BindableProperty.Create(nameof(VideoUrl), typeof(string), typeof(InlineVideoPlayer), null,
                propertyChanged: OnVideoUrlChanged);

        public static readonly BindableProperty ThumbnailUrlProperty =
            BindableProperty.Create(nameof(ThumbnailUrl), typeof(string), typeof(InlineVideoPlayer), null,
                propertyChanged: OnThumbnailUrlChanged);

        // Properties
        public string VideoUrl
        {
            get => (string)GetValue(VideoUrlProperty);
            set => SetValue(VideoUrlProperty, value);
        }

        public string ThumbnailUrl
        {
            get => (string)GetValue(ThumbnailUrlProperty);
            set => SetValue(ThumbnailUrlProperty, value);
        }

        // Events
        public event EventHandler VideoStarted;
        public event EventHandler VideoError;

        // State tracking
        private bool _isPlaying = false;

        public InlineVideoPlayer()
        {
            InitializeComponent();

            // Set up tap gesture for play button
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnPlayTapped;
            thumbnailGrid.GestureRecognizers.Add(tapGesture);

            // Set up WebView events
            videoWebView.Navigated += VideoWebView_Navigated;
            videoWebView.Navigating += VideoWebView_Navigating;
        }

        // Property changed handlers
        private static void OnVideoUrlChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is InlineVideoPlayer player && newValue is string url)
            {
                // Reset player state when URL changes
                player.ResetPlayer();
            }
        }

        private static void OnThumbnailUrlChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is InlineVideoPlayer player && newValue is string url)
            {
                player.SetThumbnail(url);
            }
        }

        // Set thumbnail image
        private void SetThumbnail(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                thumbnailImage.Source = ImageSource.FromUri(new Uri(url));
            }
        }

        // Reset player to initial state
        public void ResetPlayer()
        {
            _isPlaying = false;
            thumbnailGrid.IsVisible = true;
            playButtonFrame.IsVisible = true;
            loadingIndicator.IsVisible = false;
            videoWebView.IsVisible = false;
            videoWebView.Source = null;
        }

        // Play button tap handler
        private void OnPlayTapped(object sender, EventArgs e)
        {
            if (_isPlaying) return;

            try
            {
                if (string.IsNullOrWhiteSpace(VideoUrl))
                {
                    Debug.WriteLine("Video URL is empty");
                    return;
                }

                // Show loading indicator
                playButtonFrame.IsVisible = false;
                loadingIndicator.IsVisible = true;
                loadingIndicator.IsRunning = true;

                // Create HTML for video
                string videoHtml = CreateVideoHtml(VideoUrl);

                // Load the video in WebView
                videoWebView.Source = new HtmlWebViewSource
                {
                    Html = videoHtml
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing inline video: {ex.Message}");
                ResetPlayer();
            }
        }

        // Create HTML for video player
        private string CreateVideoHtml(string videoUrl)
        {
            string cacheBustParam = DateTime.Now.Ticks.ToString();
            string urlWithCacheBusting = videoUrl.Contains("?")
                ? $"{videoUrl}&cb={cacheBustParam}"
                : $"{videoUrl}?cb={cacheBustParam}";

            return @"<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
    <style>
        body {
            margin: 0;
            padding: 0;
            background-color: #000;
            height: 100vh;
            width: 100vw;
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
            object-fit: cover;
        }
    </style>
</head>
<body>
    <div class='video-container'>
        <video id='videoPlayer' controls autoplay playsinline controlsList='nodownload'>
            <source src='" + urlWithCacheBusting + @"' type='video/mp4'>
            Your browser does not support HTML5 video.
        </video>
    </div>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            var video = document.getElementById('videoPlayer');
            
            video.addEventListener('canplay', function() {
                window.location.href = 'maui-callback://videoCanPlay';
            });
            
            video.addEventListener('playing', function() {
                window.location.href = 'maui-callback://videoPlaying';
            });
            
            video.addEventListener('error', function() {
                window.location.href = 'maui-callback://videoError';
            });
            
            // Load and try to play
            video.load();
            setTimeout(function() {
                video.play().catch(function(error) {
                    console.log('Autoplay prevented:', error);
                });
            }, 500);
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
                    Debug.WriteLine("Inline video can play");
                }
                else if (e.Url == "maui-callback://videoPlaying")
                {
                    Debug.WriteLine("Inline video playing");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _isPlaying = true;
                        thumbnailGrid.IsVisible = false;
                        videoWebView.IsVisible = true;
                        VideoStarted?.Invoke(this, EventArgs.Empty);
                    });
                }
                else if (e.Url == "maui-callback://videoError")
                {
                    Debug.WriteLine("Inline video error");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ResetPlayer();
                        VideoError?.Invoke(this, EventArgs.Empty);
                    });
                }
            }
        }

        private void VideoWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result != WebNavigationResult.Success)
            {
                Debug.WriteLine($"Inline video WebView navigation failed: {e.Result}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ResetPlayer();
                    VideoError?.Invoke(this, EventArgs.Empty);
                });
            }
        }

        // Public method to stop video
        public void StopVideo()
        {
            if (_isPlaying)
            {
                ResetPlayer();
            }
        }
    }
}