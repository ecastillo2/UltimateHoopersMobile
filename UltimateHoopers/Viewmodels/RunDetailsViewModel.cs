using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using UltimateHoopers.Models;

namespace UltimateHoopers.ViewModels
{
    public class RunDetailsViewModel : BindableObject
    {
        private RunDto _run;
        private bool _isBusy;
        private bool _isUserJoined;
        private ObservableCollection<Player> _joinedPlayers;

        // Properties
        public RunDto Run
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

                // Ensure run has Players collection
                InitializePlayersCollection();

                // Update the joined players list
                RefreshJoinedPlayersList();
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

        public ObservableCollection<Player> JoinedPlayers
        {
            get => _joinedPlayers;
            set
            {
                _joinedPlayers = value;
                OnPropertyChanged();
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
        public ICommand ViewPlayerProfileCommand { get; }

        // Constructor
        public RunDetailsViewModel(RunDto run)
        {
            // Initialize joined players collection
            _joinedPlayers = new ObservableCollection<Player>();

            // Initialize commands
            JoinRunCommand = new Command(async () => await JoinRun());
            ShareRunCommand = new Command(async () => await ShareRun());
            ViewPlayerProfileCommand = new Command<Player>(async (player) => await ViewPlayerProfile(player));

            // Set the run and initialize players collection
            Run = run;

            // Check if user is already joined
            CheckUserJoined();

            // Make sure the joined players list is populated
            RefreshJoinedPlayersList();

            // Log the initial state
            Debug.WriteLine($"RunDetailsViewModel initialized with {JoinedPlayers?.Count ?? 0} joined players");
        }

        // Methods
        private void InitializePlayersCollection()
        {
            if (Run != null)
            {
                // Ensure Players collection is initialized
                if (Run.Players == null)
                {
                    Run.Players = new ObservableCollection<Player>();
                    Debug.WriteLine("Created new Players collection for Run");
                }

                // If Players collection is empty but CurrentPlayerCount > 0,
                // populate with sample players
                if (Run.Players.Count == 0 && Run.CurrentPlayerCount > 0)
                {
                    Debug.WriteLine($"Populating empty Players collection with sample data (CurrentPlayerCount: {Run.CurrentPlayerCount})");

                    // Add host as first player
                    if (!string.IsNullOrEmpty(Run.HostName))
                    {
                        Run.Players.Add(new Player
                        {
                            Id = Run.HostId ?? Guid.NewGuid().ToString(),
                            Name = Run.HostName,
                            Username = "@" + Run.HostName.ToLower().Replace(" ", ""),
                            ProfileImageUrl = "https://via.placeholder.com/200x200.png?text=" +
                                Uri.EscapeDataString(Run.HostName.Substring(0, 1)),
                            IsHost = true,
                            HasJoined = true
                        });

                        Debug.WriteLine($"Added host player: {Run.HostName}");
                    }

                    // Add sample players based on CurrentPlayerCount
                    string[] sampleNames = { "Michael", "Sarah", "Jason", "Tiffany", "Carlos", "Zoe", "David", "Maya", "Kevin", "Lisa" };
                    int playersToAdd = Math.Min(Run.CurrentPlayerCount - Run.Players.Count, sampleNames.Length);

                    for (int i = 0; i < playersToAdd; i++)
                    {
                        string playerName = sampleNames[i];
                        Run.Players.Add(new Player
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = playerName,
                            Username = "@" + playerName.ToLower(),
                            ProfileImageUrl = "https://via.placeholder.com/200x200.png?text=" +
                                Uri.EscapeDataString(playerName.Substring(0, 1)),
                            IsHost = false,
                            HasJoined = true
                        });

                        Debug.WriteLine($"Added sample player: {playerName}");
                    }

                    // Add more generic players if needed
                    int remainingPlayers = Run.CurrentPlayerCount - Run.Players.Count;
                    for (int i = 0; i < remainingPlayers; i++)
                    {
                        Run.Players.Add(new Player
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = $"Player {i + 1}",
                            Username = $"@player{i + 1}",
                            IsHost = false,
                            HasJoined = true
                        });

                        Debug.WriteLine($"Added generic player: Player {i + 1}");
                    }

                    // Notify property changed
                    OnPropertyChanged(nameof(Run));
                }

                Debug.WriteLine($"Players collection now has {Run.Players.Count} players");
            }
        }

        public void RefreshJoinedPlayersList()
        {
            try
            {
                // Clear the existing list
                if (JoinedPlayers == null)
                {
                    JoinedPlayers = new ObservableCollection<Player>();
                    Debug.WriteLine("Created new JoinedPlayers collection");
                }
                else
                {
                    JoinedPlayers.Clear();
                    Debug.WriteLine("Cleared existing JoinedPlayers collection");
                }

                // If Run and Players exist, filter for joined players
                if (Run?.Players != null && Run.Players.Count > 0)
                {
                    var joinedPlayers = Run.Players
                        .Where(p => p.HasJoined)
                        .ToList();

                    // Add to the observable collection
                    foreach (var player in joinedPlayers)
                    {
                        JoinedPlayers.Add(player);
                        Debug.WriteLine($"Added joined player to list: {player.Name}");
                    }

                    Debug.WriteLine($"Found {JoinedPlayers.Count} joined players");
                }
                else
                {
                    Debug.WriteLine("No players found in Run.Players collection");
                }

                // Notify property changed
                OnPropertyChanged(nameof(JoinedPlayers));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing joined players list: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public void RefreshRunDetails()
        {
            try
            {
                // In a real app, this would refresh the run details from a service
                Debug.WriteLine("RefreshRunDetails called");

                // Ensure Players collection is properly initialized
                InitializePlayersCollection();

                // Check if user is already joined
                CheckUserJoined();

                // Refresh the joined players list
                RefreshJoinedPlayersList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing run details: {ex.Message}");
            }
        }

        private void CheckUserJoined()
        {
            // In a real app, this would check if the current user is in the player list
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
                        Debug.WriteLine("Current user is already joined in this run");
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
                Debug.WriteLine("JoinRun method called");

                if (IsUserJoined)
                {
                    // Leave the run
                    bool confirmed = await Application.Current.MainPage.DisplayAlert(
                        "Leave Run",
                        $"Are you sure you want to leave this run at {Run.Name}?",
                        "Leave", "Cancel");

                    if (!confirmed)
                        return;

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
                            Debug.WriteLine($"Removed player {playerToRemove.Name} from run");
                        }
                    }

                    IsUserJoined = false;

                    // Refresh joined players list
                    RefreshJoinedPlayersList();

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

                        await Application.Current.MainPage.DisplayAlert(
                            "Payment",
                            "Payment processing coming soon!",
                            "OK");
                    }

                    // Create a player object for the current user
                    var player = new Player
                    {
                        Id = App.User?.UserId ?? Guid.NewGuid().ToString(),
                        Name = App.User?.UserName ?? "You",
                        Username = "@" + (App.User?.UserName ?? "you").ToLower().Replace(" ", ""),
                        ProfileImageUrl = App.User?.Profile?.ImageURL ?? "",
                        IsHost = false,
                        HasJoined = true
                    };

                    Run.Players.Add(player);
                    Run.CurrentPlayerCount++;
                    IsUserJoined = true;
                    Debug.WriteLine($"Added current user as player: {player.Name}");

                    // Refresh joined players list
                    RefreshJoinedPlayersList();

                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        $"You've joined the run at {Run.Name}! See you on the court!",
                        "OK");
                }

                OnPropertyChanged(nameof(Run));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error joining run: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

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
                Debug.WriteLine("ShareRun method called");

                string action = await Application.Current.MainPage.DisplayActionSheet(
                    "Share Run",
                    "Cancel",
                    null,
                    "Copy Link",
                    "Share via Message",
                    "Share to Social Media");

                if (action == "Copy Link")
                {
                    string url = $"https://ultimatehoopers.com/run/{Run.Id}";
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

        private async Task ViewPlayerProfile(Player player)
        {
            if (player == null)
                return;

            try
            {
                Debug.WriteLine($"ViewPlayerProfile called for player: {player.Name}");

                await Application.Current.MainPage.DisplayAlert(
                    "Player Profile",
                    $"Viewing profile for {player.Name} ({player.Username})",
                    "OK");

                // In a real app, you would navigate to the player's profile page
                // await Shell.Current.GoToAsync($"//PlayerProfilePage?id={player.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error viewing player profile: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Could not view player profile at this time.",
                    "OK");
            }
        }
    }
}