using Microsoft.Maui.Controls;

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
                FontAttributes = FontAttributes.Bold
            };
            titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));
            titleLabel.SetDynamicResource(Label.TextColorProperty, "PrimaryColor");

            var subtitleLabel = new Label
            {
                FontSize = 16
            };
            subtitleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Subtitle), source: this));
            subtitleLabel.SetDynamicResource(Label.TextColorProperty, "SecondaryTextColor");

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
                Margin = new Thickness(0, 0, 10, 0)
            };
            profileFrame.SetDynamicResource(Frame.BorderColorProperty, "BorderColor");
            profileFrame.SetDynamicResource(Frame.BackgroundColorProperty, "CardBackgroundColor");

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
                HasShadow = true
            };
            menuFrame.SetDynamicResource(Frame.BorderColorProperty, "BorderColor");
            menuFrame.SetDynamicResource(Frame.BackgroundColorProperty, "CardBackgroundColor");

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