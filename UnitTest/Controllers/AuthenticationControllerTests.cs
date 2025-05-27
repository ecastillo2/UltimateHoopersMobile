using DataLayer.DAL.Interface;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using WebAPI.Controllers;
using WebAPI.Services;

namespace UnitTest.Controllers
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IProfileRepository> _mockProfileRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthenticationController _controller;

        public AuthenticationControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockProfileRepository = new Mock<IProfileRepository>();
            _mockConfiguration = new Mock<IConfiguration>();

            _controller = new AuthenticationController(
                _mockAuthService.Object,
                _mockProfileRepository.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task Authenticate_ReturnsOk_WithUser_WhenAuthenticationSucceeds()
        {
            // Arrange
            var user = new User
            {
                UserId = "test-user",
                Email = "test@example.com",
                ProfileId = "profile-1",
                Token = "generated-jwt-token"
            };

            var profile = new Profile
            {
                ProfileId = "profile-1",
                UserName = "TestUser"
            };

            _mockAuthService.Setup(service => service.Authenticate(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockProfileRepository.Setup(repo => repo.GetProfileByIdAsync(user.ProfileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            var loginModel = new User
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var result = await _controller.Authenticate(loginModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUser = okResult.Value.Should().BeOfType<User>().Subject;

            returnedUser.UserId.Should().Be(user.UserId);
            returnedUser.Email.Should().Be(user.Email);
            returnedUser.Token.Should().Be(user.Token);
            returnedUser.Profile.Should().NotBeNull();
            returnedUser.Profile.UserName.Should().Be("TestUser");

            // Password and PasswordHash should be removed for security
            returnedUser.Password.Should().BeNull();
            returnedUser.PasswordHash.Should().BeNull();
        }

        [Fact]
        public async Task Authenticate_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.Authenticate(null);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task Authenticate_ReturnsBadRequest_WhenCredentialsAreMissing()
        {
            // Arrange
            var loginModel = new User(); // Empty model with no credentials

            // Act
            var result = await _controller.Authenticate(loginModel);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task Authenticate_ReturnsBadRequest_WhenAuthenticationFails()
        {
            // Arrange
            _mockAuthService.Setup(service => service.Authenticate(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var loginModel = new User
            {
                Email = "test@example.com",
                Password = "wrong-password"
            };

            // Act
            var result = await _controller.Authenticate(loginModel);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task Authenticate_ReturnsOk_WithUser_WhenUsingToken()
        {
            // Arrange
            var user = new User
            {
                UserId = "test-user",
                Token = "valid-token",
                ProfileId = "profile-1"
            };

            _mockAuthService.Setup(service => service.Authenticate(
                    "valid-token",
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockProfileRepository.Setup(repo => repo.GetProfileByIdAsync(user.ProfileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Profile { ProfileId = user.ProfileId });

            var tokenModel = new User
            {
                Token = "valid-token"
            };

            // Act
            var result = await _controller.Authenticate(tokenModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUser = okResult.Value.Should().BeOfType<User>().Subject;

            returnedUser.UserId.Should().Be(user.UserId);
            returnedUser.Token.Should().Be(user.Token);
        }

        [Fact]
        public async Task Authenticate_ReturnsOk_EvenWhenProfileNotFound()
        {
            // Arrange
            var user = new User
            {
                UserId = "test-user",
                Email = "test@example.com",
                ProfileId = "nonexistent-profile",
                Token = "generated-jwt-token"
            };

            _mockAuthService.Setup(service => service.Authenticate(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockProfileRepository.Setup(repo => repo.GetProfileByIdAsync(user.ProfileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Profile)null);

            var loginModel = new User
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var result = await _controller.Authenticate(loginModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUser = okResult.Value.Should().BeOfType<User>().Subject;

            returnedUser.UserId.Should().Be(user.UserId);
            returnedUser.Email.Should().Be(user.Email);
            returnedUser.Token.Should().Be(user.Token);
            returnedUser.Profile.Should().BeNull(); // Profile should be null as it wasn't found
        }
    }
}