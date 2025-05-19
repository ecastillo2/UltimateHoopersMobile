using Microsoft.Maui.Controls;
using System;

namespace UltimateHoopers.Services
{
    public class NotificationSettings : BindableObject
    {
        private bool _enablePushNotifications = true;
        public bool EnablePushNotifications
        {
            get => _enablePushNotifications;
            set
            {
                _enablePushNotifications = value;
                OnPropertyChanged();
            }
        }

        private bool _enableEmailNotifications = false;
        public bool EnableEmailNotifications
        {
            get => _enableEmailNotifications;
            set
            {
                _enableEmailNotifications = value;
                OnPropertyChanged();
            }
        }

        private bool _gameInvitations = true;
        public bool GameInvitations
        {
            get => _gameInvitations;
            set
            {
                _gameInvitations = value;
                OnPropertyChanged();
            }
        }

        private bool _gameReminders = true;
        public bool GameReminders
        {
            get => _gameReminders;
            set
            {
                _gameReminders = value;
                OnPropertyChanged();
            }
        }

        private bool _friendRequests = true;
        public bool FriendRequests
        {
            get => _friendRequests;
            set
            {
                _friendRequests = value;
                OnPropertyChanged();
            }
        }

        private bool _postInteractions = true;
        public bool PostInteractions
        {
            get => _postInteractions;
            set
            {
                _postInteractions = value;
                OnPropertyChanged();
            }
        }

        private bool _systemUpdates = true;
        public bool SystemUpdates
        {
            get => _systemUpdates;
            set
            {
                _systemUpdates = value;
                OnPropertyChanged();
            }
        }

        private bool _quietHoursEnabled = false;
        public bool QuietHoursEnabled
        {
            get => _quietHoursEnabled;
            set
            {
                _quietHoursEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _quietHoursStart = "22:00";
        public string QuietHoursStart
        {
            get => _quietHoursStart;
            set
            {
                _quietHoursStart = value;
                OnPropertyChanged();
                // Also update the TimeSpan property when string is updated
                OnPropertyChanged(nameof(QuietHoursStartTime));
            }
        }

        private string _quietHoursEnd = "08:00";
        public string QuietHoursEnd
        {
            get => _quietHoursEnd;
            set
            {
                _quietHoursEnd = value;
                OnPropertyChanged();
                // Also update the TimeSpan property when string is updated
                OnPropertyChanged(nameof(QuietHoursEndTime));
            }
        }

        // TimeSpan properties for easier binding to TimePicker - these do not have backing fields
        // to avoid ambiguity with string properties
        public TimeSpan QuietHoursStartTime
        {
            get
            {
                if (TimeSpan.TryParse(_quietHoursStart, out TimeSpan result))
                {
                    return result;
                }
                return new TimeSpan(22, 0, 0); // Default 10:00 PM
            }
            set
            {
                QuietHoursStart = value.ToString(@"hh\:mm");
            }
        }

        public TimeSpan QuietHoursEndTime
        {
            get
            {
                if (TimeSpan.TryParse(_quietHoursEnd, out TimeSpan result))
                {
                    return result;
                }
                return new TimeSpan(8, 0, 0); // Default 8:00 AM
            }
            set
            {
                QuietHoursEnd = value.ToString(@"hh\:mm");
            }
        }
    }
}