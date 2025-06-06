using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class ClientSignupModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Age Group")]
        public string AgeGroup { get; set; }

        [Display(Name = "Skill Level")]
        public string SkillLevel { get; set; }

        [Required(ErrorMessage = "You must accept the terms of service")]
        [Display(Name = "Accept Terms")]
        public bool AcceptTerms { get; set; }

        [Display(Name = "Subscribe to Newsletter")]
        public bool SubscribeNewsletter { get; set; }
    }

    public class AdminSignupModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Business email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Business Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Organization name is required")]
        [StringLength(100, ErrorMessage = "Organization name cannot exceed 100 characters")]
        [Display(Name = "Organization/Facility Name")]
        public string Organization { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Your Role")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Facility Type")]
        public string FacilityType { get; set; }

        [Required(ErrorMessage = "You must accept the terms of service")]
        [Display(Name = "Accept Terms")]
        public bool AcceptTerms { get; set; }

        [Display(Name = "Subscribe to Marketing")]
        public bool SubscribeMarketing { get; set; }
    }

    public class CreateUserModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string AccessLevel { get; set; }
        public string AgeGroup { get; set; }
        public string SkillLevel { get; set; }
        public string Organization { get; set; }
        public string Role { get; set; }
        public string FacilityType { get; set; }
        public bool AcceptTerms { get; set; }
        public bool SubscribeNewsletter { get; set; }
        public bool SubscribeMarketing { get; set; }
        public bool RequiresApproval { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}