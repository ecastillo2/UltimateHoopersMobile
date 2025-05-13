using Microsoft.Maui.Controls;

namespace UltimateHoopers.Controls
{
    public class BasePageWithMenu : ContentPage
    {
        protected Frame MenuPopup { get; private set; }

        public BasePageWithMenu()
        {
            // This constructor will be called by derived pages
            SetupMenuPopup();
        }

        private void SetupMenuPopup()
        {
            // Create the menu popup that will be shown when hamburger menu is clicked
            MenuPopup = new Frame
            {
                IsVisible = false,
                BackgroundColor = Colors.Transparent,
                Padding = 0,
                HasShadow = false
            };

            // Set Grid.Row and Grid.RowSpan in the page that uses this base class

            var grid = new Grid();

            // Semi-transparent background
            var overlay = new BoxView
            {
                Color = Colors.Black,
                Opacity = 0.5
            };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnCloseMenu;
            overlay.GestureRecognizers.Add(tapGestureRecognizer);

            grid.Add(overlay);

            // Menu panel
            var menuPanel = new Frame
            {
                HeightRequest = 400,
                WidthRequest = 250,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 70, 20, 0),
                BorderColor = Colors.LightGray,
                BackgroundColor = Colors.White,
                CornerRadius = 10,
                Padding = 0,
                HasShadow = true
            };

            var menuStack = new VerticalStackLayout
            {
                Spacing = 0
            };

            // Menu Header
            var headerGrid = new Grid
            {
                BackgroundColor = Colors.Purple, // Direct color assignment
                HeightRequest = 60,
                Padding = new Thickness(15)
            };

            headerGrid.Add(new Label
            {
                Text = "Menu",
                TextColor = Colors.White,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            });

            menuStack.Add(headerGrid);

            // Menu Items Stack
            var menuItemsStack = new VerticalStackLayout
            {
                Padding = 0,
                Spacing = 0
            };

            // Add common menu items
            menuItemsStack.Add(CreateMenuItem("👤", "My Profile", OnProfileMenuItemClicked));
            menuItemsStack.Add(CreateMenuItem("⚙️", "Settings", OnSettingsMenuItemClicked));
            menuItemsStack.Add(CreateMenuItem("🔔", "Notifications", OnNotificationsMenuItemClicked));
            menuItemsStack.Add(CreateMenuItem("❓", "Help & Support", OnHelpMenuItemClicked));
            menuItemsStack.Add(CreateMenuItem("🚪", "Logout", OnLogoutMenuItemClicked));

            menuStack.Add(menuItemsStack);
            menuPanel.Content = menuStack;
            grid.Add(menuPanel);

            MenuPopup.Content = grid;
        }

        private Frame CreateMenuItem(string icon, string title, EventHandler tappedHandler)
        {
            var menuItem = new Frame
            {
                BackgroundColor = Colors.Transparent,
                BorderColor = Colors.Transparent,
                Padding = new Thickness(15),
                HeightRequest = 60
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };

            grid.Add(new Label
            {
                Text = icon,
                FontSize = 20,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 15, 0)
            }, 0, 0);

            grid.Add(new Label
            {
                Text = title,
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.DarkGray
            }, 1, 0);

            menuItem.Content = grid;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += tappedHandler;
            menuItem.GestureRecognizers.Add(tapGesture);

            return menuItem;
        }

        // Menu handlers
        protected void OnMenuClicked(object sender, EventArgs e)
        {
            if (MenuPopup != null)
                MenuPopup.IsVisible = true;
        }

        protected void OnCloseMenu(object sender, EventArgs e)
        {
            if (MenuPopup != null)
                MenuPopup.IsVisible = false;
        }

        protected virtual async void OnProfileMenuItemClicked(object sender, EventArgs e)
        {
            if (MenuPopup != null)
                MenuPopup.IsVisible = false;
            await DisplayAlert("Profile", "Profile page coming soon!", "OK");
        }

        protected virtual async void OnSettingsMenuItemClicked(object sender, EventArgs e)
        {
            if (MenuPopup != null)
                MenuPopup.IsVisible = false;
            await DisplayAlert("Settings", "Settings page coming soon!", "OK");
        }

        protected virtual async void OnNotificationsMenuItemClicked(object sender, EventArgs e)
        {
            if (MenuPopup != null)
                MenuPopup.IsVisible = false;
            await DisplayAlert("Notifications", "Notifications page coming soon!", "OK");
        }

        protected virtual async void OnHelpMenuItemClicked(object sender, EventArgs e)
        {
            if (MenuPopup != null)
                MenuPopup.IsVisible = false;
            await DisplayAlert("Help & Support", "Help & Support page coming soon!", "OK");
        }

        protected virtual async void OnLogoutMenuItemClicked(object sender, EventArgs e)
        {
            if (MenuPopup != null)
                MenuPopup.IsVisible = false;
            bool answer = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (answer)
            {
                // Navigate back to login page
                Application.Current.MainPage = new Pages.LoginPage();
            }
        }
    }
}