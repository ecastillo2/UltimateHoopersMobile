using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UltimateHoopers.Helpers;

namespace UltimateHoopers.Pages
{
    public partial class FAQPage : ContentPage
    {
        // List to store all FAQ items for filtering
        private List<FAQItem> _allFaqItems = new List<FAQItem>();
        private string _currentCategory = "All";

        public FAQPage()
        {
            InitializeComponent();

            // Initialize FAQ data
            LoadFAQData();

            // Display all FAQ items initially
            FilterFAQItems();
        }

        private void LoadFAQData()
        {
            // General Questions
            _allFaqItems.Add(new FAQItem
            {
                Category = "General",
                Question = "What is Ultimate Hoopers?",
                Answer = "Ultimate Hoopers is a mobile application designed to help basketball players find pickup games, connect with other players, track their stats, and share basketball-related content. It serves as a social platform specifically tailored for basketball enthusiasts."
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "General",
                Question = "Is Ultimate Hoopers free to use?",
                Answer = "The basic version of Ultimate Hoopers is free to download and use. However, some premium features may require a subscription or one-time payment. Check the app store for current pricing details."
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "General",
                Question = "Which platforms is Ultimate Hoopers available on?",
                Answer = "Ultimate Hoopers is currently available for iOS and Android devices. The app requires iOS 15.0+ or Android 8.0+ to run optimally."
            });

            // Account & Profile
            _allFaqItems.Add(new FAQItem
            {
                Category = "Account",
                Question = "How do I create an account?",
                Answer = "1. Download the Ultimate Hoopers app from your device's app store\n" +
                         "2. Open the app and tap \"Create Account\" on the login screen\n" +
                         "3. Enter your email, username, full name, and password\n" +
                         "4. Choose your account type (Player or Court Host)\n" +
                         "5. Complete any additional profile information\n" +
                         "6. Tap \"Create Account\" to finish"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Account",
                Question = "What's the difference between a Player account and a Court Host account?",
                Answer = "• Player Account: For individual basketball players looking to join games, track stats, and connect with other players.\n" +
                         "• Court Host Account: For court owners, gym managers, or event organizers who want to host games, create events, and manage court bookings."
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Account",
                Question = "How do I edit my profile?",
                Answer = "1. Log into your account\n" +
                         "2. Tap your profile icon in the bottom navigation bar or in the top-right corner\n" +
                         "3. Select \"Edit Profile\"\n" +
                         "4. Update your information and tap \"Save\" when finished"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Account",
                Question = "Can I change my username after creating an account?",
                Answer = "Yes, you can change your username once every 30 days. Go to Edit Profile and update your username field."
            });

            // Finding & Joining Games
            _allFaqItems.Add(new FAQItem
            {
                Category = "Games",
                Question = "How do I find pickup games near me?",
                Answer = "1. Navigate to the \"Find Runs\" tab in the bottom navigation\n" +
                         "2. The app will show nearby games based on your location\n" +
                         "3. Use filters to narrow down games by time, skill level, and distance\n" +
                         "4. Tap on a game to see more details or join"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Games",
                Question = "How do I join a game?",
                Answer = "After finding a game you'd like to join:\n" +
                         "1. Tap on the game listing to view details\n" +
                         "2. Tap the \"Join\" button\n" +
                         "3. Complete any required information (such as payment if it's a paid game)\n" +
                         "4. You'll receive a confirmation when successfully joined"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Games",
                Question = "What skill levels are available for games?",
                Answer = "Games are typically categorized as:\n" +
                         "• Beginner\n" +
                         "• Intermediate\n" +
                         "• Advanced\n" +
                         "• All Levels Welcome\n\n" +
                         "You can filter games by skill level to find matches appropriate for your abilities."
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Games",
                Question = "How do I create or host my own game?",
                Answer = "To host a game:\n" +
                         "1. Navigate to the \"Find Runs\" tab\n" +
                         "2. Tap the \"+\" button or \"Create Game\" option\n" +
                         "3. Enter game details including location, time, player limit, and skill level\n" +
                         "4. Set whether it's free or paid (and cost if applicable)\n" +
                         "5. Tap \"Create Game\" to publish your listing"
            });

            // Stats & Tracking
            _allFaqItems.Add(new FAQItem
            {
                Category = "Stats",
                Question = "How are my stats tracked in the app?",
                Answer = "Stats can be tracked in several ways:\n" +
                         "• Self-reporting after games\n" +
                         "• Court hosts or game organizers recording results\n" +
                         "• Team captains inputting team stats\n" +
                         "• Integration with certain scorekeeper apps"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Stats",
                Question = "What stats does Ultimate Hoopers track?",
                Answer = "The app tracks various basketball stats including:\n" +
                         "• Games played, wins, and losses\n" +
                         "• Win percentage\n" +
                         "• Points, assists, and rebounds per game\n" +
                         "• Shooting percentages (FG%, 3P%, FT%)\n" +
                         "• Advanced metrics for premium users"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Stats",
                Question = "How accurate are the stats?",
                Answer = "Stats accuracy depends on the reporting method. Self-reported stats rely on honest reporting, while stats recorded by hosts or through integrated scorekeeper apps tend to be more accurate."
            });

            // Posts & Social Features
            _allFaqItems.Add(new FAQItem
            {
                Category = "Posts",
                Question = "How do I share content on Ultimate Hoopers?",
                Answer = "1. Tap the \"+\" button in the bottom navigation or on the Posts page\n" +
                         "2. Choose to upload a photo, video, or text post\n" +
                         "3. Add a caption, hashtags, and tag other users if desired\n" +
                         "4. Tap \"Post\" to share with your followers"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Posts",
                Question = "What type of content can I share?",
                Answer = "You can share:\n" +
                         "• Photos and videos of games, skills, or basketball-related content\n" +
                         "• Text posts about basketball topics, game announcements, or questions\n" +
                         "• Highlights of your performance\n" +
                         "• Training tips and drills"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Posts",
                Question = "Can I control who sees my posts?",
                Answer = "Currently, posts are visible to all app users. Future updates may include private posting options and more granular privacy controls."
            });

            // Technical Support
            _allFaqItems.Add(new FAQItem
            {
                Category = "Support",
                Question = "How do I report a bug or technical issue?",
                Answer = "To report issues:\n" +
                         "1. Go to your profile\n" +
                         "2. Tap \"Help & Support\"\n" +
                         "3. Select \"Report a Problem\"\n" +
                         "4. Describe the issue in detail and submit"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Support",
                Question = "How can I recover my password?",
                Answer = "On the login screen:\n" +
                         "1. Tap \"Forgot Password\"\n" +
                         "2. Enter your email address\n" +
                         "3. Follow the instructions sent to your email to reset your password"
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Support",
                Question = "Does Ultimate Hoopers work offline?",
                Answer = "Some basic features work offline, but most functionality requires an internet connection. Stats and game information will sync once you're back online."
            });

            _allFaqItems.Add(new FAQItem
            {
                Category = "Support",
                Question = "How do I contact customer support?",
                Answer = "You can contact support by:\n" +
                         "• Using the \"Help & Support\" section in your profile\n" +
                         "• Emailing support@ultimatehoopers.com\n" +
                         "• Using the in-app chat support (available during business hours)"
            });
        }

        private void FilterFAQItems(string searchText = "")
        {
            // Clear current items
            FAQItemsContainer.Clear();

            // Filter by category and search text
            var filteredItems = _allFaqItems
                .Where(item =>
                    (_currentCategory == "All" || item.Category == _currentCategory) &&
                    (string.IsNullOrEmpty(searchText) ||
                     item.Question.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                     item.Answer.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Add the filtered items to the container
            foreach (var faqItem in filteredItems)
            {
                var faqView = CreateFAQView(faqItem);
                FAQItemsContainer.Add(faqView);
            }

            // If no results, show a message
            if (filteredItems.Count == 0)
            {
                FAQItemsContainer.Add(new Label
                {
                    Text = "No results found. Try adjusting your search or category filter.",
                    TextColor = (Color)Application.Current.Resources["SecondaryTextColor"],
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20)
                });
            }
        }

        private Frame CreateFAQView(FAQItem faqItem)
        {
            // Main container frame for the FAQ item
            var frame = new Frame
            {
                BorderColor = (Color)Application.Current.Resources["BorderColor"],
                CornerRadius = 10,
                HasShadow = true,
                BackgroundColor = (Color)Application.Current.Resources["CardBackgroundColor"],
                Padding = new Thickness(0)
            };

            // Create the content layout
            var contentLayout = new VerticalStackLayout
            {
                Spacing = 0
            };

            // Question header
            var questionGrid = new Grid
            {
                Padding = new Thickness(20),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var questionLabel = new Label
            {
                Text = faqItem.Question,
                TextColor = (Color)Application.Current.Resources["PrimaryTextColor"],
                FontAttributes = FontAttributes.Bold,
                FontSize = 16
            };

            var toggleIcon = new Label
            {
                Text = "▼",
                FontSize = 16,
                TextColor = (Color)Application.Current.Resources["PrimaryColor"],
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };

            questionGrid.Add(questionLabel, 0, 0);
            questionGrid.Add(toggleIcon, 1, 0);

            // Answer container (initially collapsed)
            var answerContainer = new StackLayout
            {
                Padding = new Thickness(20, 0, 20, 20),
                IsVisible = false, // Initially collapsed
                Spacing = 10
            };

            var separator = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = (Color)Application.Current.Resources["BorderColor"],
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var answerLabel = new Label
            {
                Text = faqItem.Answer,
                TextColor = (Color)Application.Current.Resources["SecondaryTextColor"],
                FontSize = 14
            };

            answerContainer.Add(separator);
            answerContainer.Add(answerLabel);

            // Add components to the layout
            contentLayout.Add(questionGrid);
            contentLayout.Add(answerContainer);

            // Handle tap event to expand/collapse
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                // Toggle visibility
                answerContainer.IsVisible = !answerContainer.IsVisible;

                // Update the icon
                toggleIcon.Text = answerContainer.IsVisible ? "▲" : "▼";
            };

            questionGrid.GestureRecognizers.Add(tapGesture);

            // Set the content
            frame.Content = contentLayout;

            return frame;
        }

        private void OnCategorySelected(object sender, EventArgs e)
        {
            if (sender is not Frame categoryFrame)
                return;

            // Get the command parameter from the tap gesture
            var tapGesture = categoryFrame.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
            string category = tapGesture?.CommandParameter as string ?? "All";

            // Update current category
            _currentCategory = category;

            // Update UI to show selected category
            UpdateCategorySelection(categoryFrame);

            // Filter FAQ items based on selected category and current search text
            FilterFAQItems(SearchEntry.Text);
        }

        private void UpdateCategorySelection(Frame selectedFrame)
        {
            // Reset all category frames to unselected style
            foreach (var child in CategoryTabs.Children)
            {
                if (child is Frame frame)
                {
                    // Reset to unselected style
                    frame.BackgroundColor = (Color)Application.Current.Resources["CardBackgroundColor"];
                    frame.BorderColor = (Color)Application.Current.Resources["BorderColor"];

                    if (frame.Content is Label label)
                    {
                        label.TextColor = (Color)Application.Current.Resources["PrimaryTextColor"];
                        label.FontAttributes = FontAttributes.None;
                    }
                }
            }

            // Set selected frame to selected style
            selectedFrame.BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"];
            selectedFrame.BorderColor = (Color)Application.Current.Resources["PrimaryColor"];

            if (selectedFrame.Content is Label selectedLabel)
            {
                selectedLabel.TextColor = Colors.White;
                selectedLabel.FontAttributes = FontAttributes.Bold;
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            // Filter items based on search text and current category
            FilterFAQItems(e.NewTextValue);
        }

        // Navigation handlers
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("FAQPage: OnHomeClicked - using DirectNavigationHelper");
                await DirectNavigationHelper.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FAQPage: Error navigating to HomePage: {ex.Message}");
                await DisplayAlert("Navigation Error",
                    "Could not navigate to home page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnSupportNavigationClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Support", "Support page coming soon!", "OK");
        }

        private async void OnProfileNavigationClicked(object sender, EventArgs e)
        {
            // Navigate to profile page if available
            try
            {
                if (App.User != null && App.User.Profile != null)
                {
                    await Navigation.PushAsync(new UserProfilePage());
                }
                else
                {
                    await DisplayAlert("Profile", "Please log in to view your profile", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to profile: {ex.Message}");
                await DisplayAlert("Profile", "Profile navigation unavailable", "OK");
            }
        }

        private async void OnContactSupportClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Contact Support",
                "Our support team is available via email at support@ultimatehoopers.com or through in-app chat during business hours.",
                "OK");
        }

        private async void OnShareFirstPostClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Create Post", "Create Post feature coming soon!", "OK");
        }
    }

    // Model class for FAQ items
    public class FAQItem
    {
        public string Category { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}