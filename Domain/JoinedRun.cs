using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain
{
    /// <summary>
    /// Represents a joined run, which is a player's participation in a basketball run/game
    /// </summary>
    public class JoinedRun
    {
        /// <summary>
        /// Initializes a new instance of the JoinedRun class
        /// </summary>
        [JsonConstructor]
        public JoinedRun() { }

        /// <summary>
        /// Initializes a new instance of the JoinedRun class with profiles
        /// </summary>
        /// <param name="profiles">List of profiles joined to the run</param>
        
        public JoinedRun(List<Profile> profiles)
        {
            JoinedRunProfiles = profiles;
        }

        #region Database Fields (Table Properties)

        /// <summary>
        /// Gets or sets the joined run ID (Primary Key)
        /// </summary>
        [Key]
        public string? JoinedRunId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID of the player
        /// </summary>
        public string? ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the run ID
        /// </summary>
        public string? RunId { get; set; }

        /// <summary>
        /// Gets or sets the date when the player was invited
        /// </summary>
        public DateTime? InvitedDate { get; set; }

        /// <summary>
        /// Gets or sets the acceptance status of the invite
        /// Valid values: "Accepted", "Declined", "Undecided", "Accepted / Pending", "Refund"
        /// </summary>
        public string? AcceptedInvite { get; set; }

        /// <summary>
        /// Gets or sets the type of the joined run
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player is present at the run
        /// </summary>
        public bool? Present { get; set; }

        /// <summary>
        /// Gets or sets the squad ID if the player is part of a squad
        /// </summary>
        public string? SquadId { get; set; }

        #endregion

        #region Navigation Properties (Not Mapped to Database)

        /// <summary>
        /// Gets or sets the list of profiles for the joined run (not mapped to database)
        /// </summary>
        [NotMapped]
        public List<Profile>? JoinedRunProfiles { get; set; }

        /// <summary>
        /// Gets or sets the associated run (not mapped to database)
        /// </summary>
        [NotMapped]
        public Run? Run { get; set; }

        /// <summary>
        /// Gets or sets the associated profile (not mapped to database)
        /// </summary>
        [NotMapped]
        public Profile? Profile { get; set; }

        /// <summary>
        /// Gets or sets the associated order (not mapped to database)
        /// </summary>
        [NotMapped]
        public Order? Order { get; set; }

        #endregion

        #region Computed Properties (Not Mapped to Database)

        /// <summary>
        /// Gets a value indicating whether the player has accepted the invite (computed property)
        /// </summary>
        [NotMapped]
        public bool HasAccepted => AcceptedInvite == "Accepted";

        /// <summary>
        /// Gets a value indicating whether the player has declined the invite (computed property)
        /// </summary>
        [NotMapped]
        public bool HasDeclined => AcceptedInvite == "Declined";

        /// <summary>
        /// Gets a value indicating whether the player's status is undecided (computed property)
        /// </summary>
        [NotMapped]
        public bool IsUndecided => AcceptedInvite == "Undecided" || string.IsNullOrEmpty(AcceptedInvite);

        /// <summary>
        /// Gets a value indicating whether the player is pending acceptance (computed property)
        /// </summary>
        [NotMapped]
        public bool IsPending => AcceptedInvite == "Accepted / Pending";

        /// <summary>
        /// Gets a value indicating whether the player has requested a refund (computed property)
        /// </summary>
        [NotMapped]
        public bool HasRequestedRefund => AcceptedInvite == "Refund";

        /// <summary>
        /// Gets a formatted relative time since the invitation (computed property)
        /// </summary>
        [NotMapped]
        public string? RelativeTime { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates the invitation status and sets the present flag if accepted
        /// </summary>
        /// <param name="status">The new invitation status</param>
        public void UpdateInvitationStatus(string status)
        {
            AcceptedInvite = status;

            // If accepting the invitation, default to not present (will be set during actual run)
            if (status == "Accepted")
            {
                Present = false;
            }
        }

        /// <summary>
        /// Marks the player as present for the run
        /// </summary>
        public void MarkAsPresent()
        {
            if (HasAccepted)
            {
                Present = true;
            }
        }

        /// <summary>
        /// Marks the player as absent from the run
        /// </summary>
        public void MarkAsAbsent()
        {
            Present = false;
        }

        /// <summary>
        /// Gets the display text for the current invitation status
        /// </summary>
        /// <returns>User-friendly status text</returns>
        public string GetStatusDisplayText()
        {
            return AcceptedInvite switch
            {
                "Accepted" => "Accepted",
                "Declined" => "Declined",
                "Accepted / Pending" => "Pending Payment",
                "Refund" => "Refund Requested",
                "Undecided" => "Pending Response",
                null => "Pending Response",
                _ => AcceptedInvite
            };
        }

        /// <summary>
        /// Determines if the player can be marked as present
        /// </summary>
        /// <returns>True if the player can be marked as present</returns>
        public bool CanMarkAsPresent()
        {
            return HasAccepted || IsPending;
        }

        #endregion
    }
}