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

            // Set thumbnail image
            if (!string.IsNullOrEmpty(_post.ThumbnailUrl))
            {
                thumbnailImage.Source = _post.ThumbnailUrl;
            }
            else if (!string.IsNullOrEmpty(_post.PostFileURL))
            {
                // If no thumbnail, try to use the post image as fallback
                thumbnailImage.Source = _post.PostFileURL;
            }
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        // Stubs for event handlers referenced in the original XAML
        // These do nothing now but prevent compile errors
        private void OnMediaOpened(object sender, EventArgs e)
        {
            // Stub method - not used in simplified version
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            // Stub method - not used in simplified version
        }

        private void OnMediaFailed(object sender, EventArgs e)
        {
            // Stub method - not used in simplified version
        }

        private void OnPositionChanged(object sender, EventArgs e)
        {
            // Stub method - not used in simplified version
        }

        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            // Stub method - not used in simplified version
        }
    }
}