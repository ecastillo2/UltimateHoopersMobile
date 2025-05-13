using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebAPI.DTOs
{
    /// <summary>
    /// Authentication result
    /// </summary>
    public class AuthResultDto
    {
        /// <summary>
        /// User identifier
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// JWT authentication token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// User access level (e.g., Standard, Admin)
        /// </summary>
        public string AccessLevel { get; set; }

        /// <summary>
        /// Token expiration date
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

}