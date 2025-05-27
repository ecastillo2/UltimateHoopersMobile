using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class UserViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public UserViewModelDto() { }

        // Existing constructor for mapping from Profile
        public UserViewModelDto(User privateRun)
        {
            UserId = privateRun.UserId;
            ClientId = privateRun.ClientId;
            ProfileId = privateRun.ProfileId;
            Email = privateRun.Email;
            PhoneNumber = privateRun.PhoneNumber;
            LockoutEnd = privateRun.LockoutEnd;
            LockoutEnabled = privateRun.LockoutEnabled;
            AccessFailedCount = privateRun.AccessFailedCount;
            FirstName = privateRun.FirstName;
            LastName = privateRun.LastName;
            SignUpDate = privateRun.SignUpDate;
            LastLoginDate = privateRun.LastLoginDate;
            Role = privateRun.Role;
            Password = privateRun.Password;
            AccessLevel = privateRun.AccessLevel;
            Subscription = privateRun.Subscription;
            

        }

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
        public string? Country { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public DateTime? SignUpDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
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
        public Profile Profile { get; set; }
        public IList<JoinedRun> JoinedRunList { get; set; }
        [NotMapped]
        public string? ImageUrl { get; }
    
    }
}
