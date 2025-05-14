using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Domain;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace UltimateHoopers.Pages
{
    public partial class VideoPlayerPage : ContentPage
    {
        private bool _isUserSeeking = false;
        private readonly Post _post;

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

            // Set caption if available
            if (!string.IsNullOrEmpty(_post.Caption))
            {
                captionLabel.Text = _post.Caption;
            }
            else
            {
                captionLabel.Text = $"Video by {_post.UserName}";
            }

            // Set video source
            if (!string.IsNullOrEmpty(_post.PostFileURL))
            {
                try
                {
                    mediaElement.Source = MediaSource.FromUri(_post.PostFileURL);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting video source: {ex.Message}");
                    DisplayAlert("Error", "Could not load video", "OK");
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Auto-play when page appears
                mediaElement.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting playback: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            try
            {
                // Stop playback when leaving the page
                mediaElement.Pause();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error pausing playback: {ex.Message}");
            }
        }

        private void OnMediaOpened(object sender, EventArgs e)
        {
            progressSlider.Maximum = mediaElement.Duration.TotalSeconds;
            totalTimeLabel.Text = FormatTimeSpan(mediaElement.Duration);
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            mediaElement.Position = TimeSpan.Zero;
            mediaElement.Play();
        }

        private void OnMediaFailed(object sender, MediaFailedEventArgs e)
        {
            DisplayAlert("Error", $"Failed to load video: {e.ErrorMessage}", "OK");
        }

        private void OnPositionChanged(object sender, MediaPositionChangedEventArgs e)
        {
            if (!_isUserSeeking)
            {
                progressSlider.Value = e.Position.TotalSeconds;
                currentTimeLabel.Text = FormatTimeSpan(e.Position);
            }
        }

        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (Math.Abs(e.NewValue - e.OldValue) > 1)
            {
                _isUserSeeking = true;
                mediaElement.Position = TimeSpan.FromSeconds(e.NewValue);
                currentTimeLabel.Text = FormatTimeSpan(mediaElement.Position);
                _isUserSeeking = false;
            }
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.Hours > 0)
            {
                return $"{timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                return $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
            }
        }
    }
}