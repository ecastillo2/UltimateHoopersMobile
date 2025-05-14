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
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LockoutEnd { get; set; }
        public string? LockoutEnabled { get; set; }
        public string? AccessFailedCount { get; set; }
        public string? Country { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? SignUpDate { get; set; }
        public string? LastLoginDate { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
        public string? AccessLevel { get; set; }
        public string? Type { get; set; }
        public string? Location { get; set; }
        public string? Subscription { get; set; }
        public string? ResetCode { get; set; }
        public string? ResetLink { get; set; }
        public string? Address { get; set; }
        public string? Status { get; set; }
        public string? SegId { get; set; }
        public string? SubId { get; set; }
        
        public string? ProfileId { get; set; }
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
        [JsonIgnore]
        public List<PrivateRun>? PrivateRun { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<PrivateRunInvite>? PrivateRunInvite { get; set; }
        [NotMapped]
        public DateTime? TokenExpiration { get; set; }
    }
}