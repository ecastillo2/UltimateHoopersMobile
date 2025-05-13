using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace UltimateHoopers.Controls
{
    public class CustomToolbar : ContentView
    {
        // Events
        public event EventHandler MenuClicked;
        public event EventHandler ProfileClicked;

        // Bindable properties
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(CustomToolbar), string.Empty);

        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(CustomToolbar), string.Empty);

        // Properties
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public CustomToolbar()
        {
            BuildView();
        }

        private void BuildView()
        {
            // Create the toolbar layout
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Margin = new Thickness(0, 10, 0, 20)
            };

            // Title and subtitle
            var titleStack = new VerticalStackLayout();

            var titleLabel = new Label
            {
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Purple // Using direct color instead of resource
            };
            titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

            var subtitleLabel = new Label
            {
                FontSize = 16,
                TextColor = Colors.Gray // Using direct color instead of resource
            };
            subtitleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Subtitle), source: this));

            titleStack.Add(titleLabel);
            titleStack.Add(subtitleLabel);

            grid.Add(titleStack, 0, 0);

            // Profile button
            var profileFrame = new Frame
            {
                CornerRadius = 25,
                HeightRequest = 50,
                WidthRequest = 50,
                Padding = 0,
                HasShadow = true,
                Margin = new Thickness(0, 0, 10, 0),
                BorderColor = Colors.LightGray, // Using direct color instead of resource
                BackgroundColor = Colors.White // Using direct color instead of resource
            };

            var profileLabel = new Label
            {
                Text = "👤",
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            profileFrame.Content = profileLabel;

            var profileTap = new TapGestureRecognizer();
            profileTap.Tapped += (sender, args) => ProfileClicked?.Invoke(this, EventArgs.Empty);
            profileFrame.GestureRecognizers.Add(profileTap);

            grid.Add(profileFrame, 1, 0);

            // Menu button
            var menuFrame = new Frame
            {
                CornerRadius = 25,
                HeightRequest = 50,
                WidthRequest = 50,
                Padding = 0,
                HasShadow = true,
                BorderColor = Colors.LightGray, // Using direct color instead of resource
                BackgroundColor = Colors.White // Using direct color instead of resource
            };

            var menuLabel = new Label
            {
                Text = "☰",
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            menuFrame.Content = menuLabel;

            var menuTap = new TapGestureRecognizer();
            menuTap.Tapped += (sender, args) => MenuClicked?.Invoke(this, EventArgs.Empty);
            menuFrame.GestureRecognizers.Add(menuTap);

            grid.Add(menuFrame, 2, 0);

            // Set the content
            Content = grid;
        }
    }
}