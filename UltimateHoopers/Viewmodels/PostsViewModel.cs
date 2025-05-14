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

        public ICommand RefreshCommand { get; }

        public PostsViewModel(IPostService postService)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
            RefreshCommand = new Command(async () => await LoadPostsAsync());
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
                if (posts != null && posts.Count > 0)
                {
                    foreach (var post in posts)
                    {
                        Posts.Add(post);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error loading posts: {ex.Message}");

                // You might want to show an alert here
                await Application.Current.MainPage.DisplayAlert("Error", "Unable to load posts. Please try again later.", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}