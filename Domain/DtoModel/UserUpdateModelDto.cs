using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class UserUpdateModelDto
    {
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

        public void UpdateUser(User user)
        {
            user.Token = Token;
            user.Email = Email;
            user.PasswordHash = PasswordHash;
            user.SecurityStamp = SecurityStamp;
            user.PhoneNumber = PhoneNumber;
            user.LockoutEnd = LockoutEnd;
            user.LockoutEnabled = LockoutEnabled;
            user.AccessFailedCount = AccessFailedCount;
            user.FirstName = FirstName;
            user.LastName = LastName;
            user.LastLoginDate = LastLoginDate;
            user.Role = Role;
            user.AccessLevel = AccessLevel;
            user.Type = Type;
            user.Subscription = Subscription;
            user.ResetCode = ResetCode;
            user.ResetLink = ResetLink;
            user.Status = Status;
            user.SegId = SegId;
            user.SubId = SubId;
        }
    }
}
