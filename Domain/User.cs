using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        [Key]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the password hash
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the access level (e.g., Admin, Staff, Standard)
        /// </summary>
        public string AccessLevel { get; set; }

        /// <summary>
        /// Gets or sets the status (e.g., Active, Inactive)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the subscription ID
        /// </summary>
        public string SubId { get; set; }

        /// <summary>
        /// Gets or sets the date of last login
        /// </summary>
        public string LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the JWT token (not stored in database)
        /// </summary>
        [NotMapped]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the associated profile
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
    }
}