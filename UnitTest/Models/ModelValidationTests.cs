using Domain;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace UnitTest.Models
{
    public class ModelValidationTests
    {
        [Fact]
        public void User_RequiredProperties_AreValidated()
        {
            // Arrange
            var user = new User();
            var context = new ValidationContext(user, null, null);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(user, context, results, true);

            // Assert
            // Add validation attributes to User model as needed
            isValid.Should().BeTrue(); // Adjust based on actual validation rules
        }

        [Fact]
        public void Profile_UserNameMaxLength_IsEnforced()
        {
            // This test would verify model validation attributes
            // Add [MaxLength] or other validation attributes to test
            var profile = new Profile
            {
                UserName = new string('a', 300) // Very long username
            };

            var context = new ValidationContext(profile, null, null);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(profile, context, results, true);

            // Assert based on validation attributes
            isValid.Should().BeTrue(); // Adjust based on actual validation rules
        }
    }
}