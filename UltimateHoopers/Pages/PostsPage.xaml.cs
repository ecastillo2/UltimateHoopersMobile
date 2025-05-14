using Domain;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
//using UltimateHoopers.Models;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class PostsPage : ContentPage
    {
        //private readonly IPostService _postService;
        private readonly ObservableCollection<Post> _posts = new ObservableCollection<Post>();

        
        private readonly PostService _postService = new PostService();

        public PostsPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in InitializeComponent: {ex.Message}");
                throw; // Re-throw to see the actual error
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPostsAsync();
        }

        private async Task LoadPostsAsync()
        {
            try
            {
                var posts = await _postService.GetPostsAsync();

                if (posts.Count > 0)
                {
                    _posts.Clear();
                    foreach (var post in posts)
                    {
                        _posts.Add(post);
                    }

                    // If you had a ListView or CollectionView, you would set its ItemsSource here
                    // PostsList.ItemsSource = _posts;

                    await DisplayAlert("Posts Loaded", $"Successfully loaded {posts.Count} posts", "OK");
                }
                else
                {
                    await DisplayAlert("No Posts", "No posts were found", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnProfileClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Profile", "Profile feature coming soon!", "OK");
        }

        private void OnMenuClicked(object sender, TappedEventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }

        private async void OnHomeClicked(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("//HomePage");
        }

        private async void OnExploreClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Explore", "Explore feature coming soon!", "OK");
        }

        private async void OnCreatePostClicked(object sender, TappedEventArgs e)
        {
            try
            {
                var newPost = new Post
                {
                    PostId = Guid.NewGuid().ToString(),
                    Caption = "New post created from mobile app",
                    Type = "Text",
                    Status = "Active",
                    PostedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var createdPost = await _postService.CreatePostAsync(newPost);

                if (createdPost != null)
                {
                    await DisplayAlert("Success", "Post created successfully!", "OK");
                    await LoadPostsAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Failed to create post", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnActivityClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Activity", "Activity feed feature coming soon!", "OK");
        }

        private async void OnScheduleClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Schedule", "Schedule feature coming soon!", "OK");
        }

        private async void OnSettingsClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Settings", "Settings feature coming soon!", "OK");
        }
    }
}