using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        [Key]
        public string? UserId { get; set; }
        public string? ClientId { get; set; }
        public string? ProfileId { get; set; }
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LockoutEnd { get; set; }
        public string? LockoutEnabled { get; set; }
        public string? AccessFailedCount { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? SignUpDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
        public string? AccessLevel { get; set; }
        public string? Type { get; set; }
        public string? Subscription { get; set; }
        public string? ResetCode { get; set; }
        public string? ResetLink { get; set; }
        public string? Status { get; set; }
        public string? SegId { get; set; }
        public string? SubId { get; set; }


        public bool? IsHost => AccountType == AccountType.Host;

        // Add the AccountType property
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AccountType AccountType { get; set; } = AccountType.Free;

        // Add IsHost convenience property
        
        
        [NotMapped]
        public List<User>? Followers { get; set; }

        [NotMapped]
        public string? UserName { get; set; }
       

        [NotMapped]
        public List<User>? Following { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<Post>? Posts { get; set; }

        [NotMapped]
        public Profile? Profile { get; set; }

        [NotMapped]
        public Client? Client { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<Run>? Run { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<JoinedRun>? JoinedRunList { get; set; }
        [NotMapped]
        public DateTime? TokenExpiration { get; set; }
    }

    // Define the AccountType enum
    public enum AccountType
    {
        Free,
        Host
    }
}