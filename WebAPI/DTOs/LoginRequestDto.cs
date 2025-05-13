// File: WebAPI/DTOs/AuthDTOs.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    /// <summary>
    /// Login request data
    /// </summary>
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    /// <summary>
    /// Registration request data
    /// </summary>
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string UserName { get; set; }
    }

    /// <summary>
    /// Password reset request
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    /// <summary>
    /// Password reset using token
    /// </summary>
    public class ResetPasswordRequestDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }


}