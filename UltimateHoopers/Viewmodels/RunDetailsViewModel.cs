using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using UltimateHoopers.Models;

namespace UltimateHoopers.ViewModels
{
    public class RunDetailsViewModel : BindableObject
    {
        private Run _run;
        private bool _isBusy;
        private bool _isUserJoined;

        // Properties
        public Run Run
        {
            get => _run;
            set
            {
                _run = value;
                OnPropertyChanged();

                // Update dependent properties
                OnPropertyChanged(nameof(FormattedDate));
                OnPropertyChanged(nameof(GameTypeText));
                OnPropertyChanged(nameof(PrivacyText));
                OnPropertyChanged(nameof(CostText));
                OnPropertyChanged(nameof(JoinButtonText));
                OnPropertyChanged(nameof(CanJoin));
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanJoin));
            }
        }

        public bool IsUserJoined
        {
            get => _isUserJoined;
            set
            {
                _isUserJoined = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(JoinButtonText));
                OnPropertyChanged(nameof(CanJoin));
            }
        }

        // Computed properties
        public string FormattedDate => Run?.FormattedDate ?? string.Empty;

        public string GameTypeText => Run?.GameType ?? "5-on-5";

        public string PrivacyText => Run?.IsPublic == true ?
            "Public (anyone can join)" :
            "Private (invitation only)";

        public string CostText => Run?.Cost > 0 ?
            $"${Run.Cost:F2} per player" :
            "Free";

        public string JoinButtonText => IsUserJoined ?
            "Leave Run" :
            (Run?.IsFull == true ? "Join Waitlist" : "Join Run");

        public bool CanJoin => !IsBusy && Run != null;

        // Commands
        public ICommand JoinRunCommand { get; }
        public ICommand ShareRunCommand { get; }

        // Constructor
        public RunDetailsViewModel(Run run)
        {
            Run = run;

            // Initialize commands
            JoinRunCommand = new Command(async () => await JoinRun());
            ShareRunCommand = new Command(async () => await ShareRun());

            // Check if user is already joined
            CheckUserJoined();
        }

        // Methods
        public void RefreshRunDetails()
        {
            try
            {
                // In a real app, this would refresh the run details from a service

                // For now, just check if the user is joined
                CheckUserJoined();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing run details: {ex.Message}");
            }
        }

        private void CheckUserJoined()
        {
            // In a real app, this would check if the current user is in the player list

            // For demo purposes, we'll just set a default value
            IsUserJoined = false;

            // Check if any players match the current user ID
            string currentUserId = App.User?.UserId;
            if (!string.IsNullOrEmpty(currentUserId) && Run?.Players != null)
            {
                foreach (var player in Run.Players)
                {
                    if (player.Id == currentUserId)
                    {
                        IsUserJoined = true;
                        break;
                    }
                }
            }
        }

        private async Task JoinRun()
        {
            if (Run == null)
                return;

            try
            {
                IsBusy = true;

                if (IsUserJoined)
                {
                    // Leave the run
                    bool confirmed = await Application.Current.MainPage.DisplayAlert(
                        "Leave Run",
                        $"Are you sure you want to leave this run at {Run.Name}?",
                        "Leave", "Cancel");

                    if (!confirmed)
                        return;

                    // In a real app, this would call a service to leave the run
                    // For demo, just remove from player list and decrement count

                    // Find and remove current user
                    string currentUserId = App.User?.UserId;
                    if (!string.IsNullOrEmpty(currentUserId) && Run.Players != null)
                    {
                        Player playerToRemove = null;
                        foreach (var player in Run.Players)
                        {
                            if (player.Id == currentUserId)
                            {
                                playerToRemove = player;
                                break;
                            }
                        }

                        if (playerToRemove != null)
                        {
                            Run.Players.Remove(playerToRemove);
                            Run.CurrentPlayerCount--;
                        }
                    }

                    // Update state
                    IsUserJoined = false;

                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        $"You have left the run at {Run.Name}",
                        "OK");
                }
                else if (Run.IsFull)
                {
                    // Join waitlist
                    bool confirmed = await Application.Current.MainPage.DisplayAlert(
                        "Join Waitlist",
                        $"This run is currently full. Would you like to join the waitlist?",
                        "Join Waitlist", "Cancel");

                    if (!confirmed)
                        return;

                    // In a real app, this would call a service to join the waitlist

                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        $"You've been added to the waitlist for {Run.Name}. We'll notify you if a spot opens up!",
                        "OK");
                }
                else
                {
                    // Join the run
                    bool confirmed = await Application.Current.MainPage.DisplayAlert(
                        "Join Run",
                        $"Would you like to join this run at {Run.Name} on {Run.FormattedDate}?",
                        "Join", "Cancel");

                    if (!confirmed)
                        return;

                    // Check if the run has a cost
                    if (Run.Cost > 0)
                    {
                        bool payNow = await Application.Current.MainPage.DisplayAlert(
                            "Payment Required",
                            $"This run requires a payment of ${Run.Cost:F2}. Would you like to pay now?",
                            "Pay Now", "Cancel");

                        if (!payNow)
                            return;

                        // In a real app, this would navigate to a payment page
                        await Application.Current.MainPage.DisplayAlert(
                            "Payment",
                            "Payment processing coming soon!",
                            "OK");
                    }

                    // In a real app, this would call a service to join the run
                    // For demo, just add to player list and increment count

                    // Create a player object for the current user
                    var player = new Player
                    {
                        Id = App.User?.UserId ?? Guid.NewGuid().ToString(),
                        Name = App.User?.UserName ?? "You",
                        IsHost = false
                    };

                    // Add to the player list
                    Run.Players.Add(player);
                    Run.CurrentPlayerCount++;

                    // Update state
                    IsUserJoined = true;

                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        $"You've joined the run at {Run.Name}! See you on the court!",
                        "OK");
                }

                // Notify that the run has been updated
                OnPropertyChanged(nameof(Run));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error joining run: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "There was a problem processing your request. Please try again.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShareRun()
        {
            try
            {
                // In a real app, this would use the Share API to share the run

                string action = await Application.Current.MainPage.DisplayActionSheet(
                    "Share Run",
                    "Cancel",
                    null,
                    "Copy Link",
                    "Share via Message",
                    "Share to Social Media");

                if (action == "Copy Link")
                {
                    // Example URL format
                    string url = $"https://ultimatehoopers.com/run/{Run.Id}";

                    // In a real app, this would copy to clipboard
                    await Application.Current.MainPage.DisplayAlert(
                        "Link Copied",
                        $"Run link copied to clipboard: {url}",
                        "OK");
                }
                else if (action == "Share via Message" || action == "Share to Social Media")
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Share",
                        $"Sharing via {action} coming soon!",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sharing run: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "There was a problem sharing this run. Please try again.",
                    "OK");
            }
        }
    }
}