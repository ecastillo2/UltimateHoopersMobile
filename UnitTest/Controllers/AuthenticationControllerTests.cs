using DataLayer.DAL.Interface;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using WebAPI.Controllers;
using WebAPI.Services;
using DataLayer.Context;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest.Controllers
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly ApplicationContext _context;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthenticationController _controller;

        public AuthenticationControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();

            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: "TestAuthDatabase")
                .Options;
            _context = new ApplicationContext(options);

            _mockConfiguration = new Mock<IConfiguration>();

            _controller = new AuthenticationController(
                _mockAuthService.Object,
                _context,
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

            // Add test data to in-memory database
            _context.Profile.Add(profile);
            await _context.SaveChangesAsync();

            _mockAuthService.Setup(service => service.Authenticate(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(user);

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

            var profile = new Profile { ProfileId = user.ProfileId };
            _context.Profile.Add(profile);
            await _context.SaveChangesAsync();

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

            // No profile added to context, so it will be null

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

        [Fact]
        public async Task Authenticate_ReturnsOk_WithClientData_WhenUserHasClient()
        {
            // Arrange
            var user = new User
            {
                UserId = "test-user",
                Email = "test@example.com",
                ProfileId = "profile-1",
                ClientId = "client-1",
                Token = "generated-jwt-token"
            };

            var profile = new Profile
            {
                ProfileId = "profile-1",
                UserName = "TestUser"
            };

            var client = new Client
            {
                ClientId = "client-1",
                Name = "Test Client"
            };

            var court = new Court
            {
                CourtId = "court-1",
                ClientId = "client-1",
                Name = "Test Court"
            };

            // Add test data to in-memory database
            _context.Profile.Add(profile);
            _context.Client.Add(client);
            _context.Court.Add(court);
            _context.User.Add(new User { UserId = "test-user-for-client", ClientId = "client-1" });
            await _context.SaveChangesAsync();

            _mockAuthService.Setup(service => service.Authenticate(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(user);

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

            returnedUser.Client.Should().NotBeNull();
            returnedUser.Client.ClientId.Should().Be("client-1");
            returnedUser.Client.CourtList.Should().NotBeNull();
            returnedUser.Client.CourtList.Should().ContainSingle();
            returnedUser.Client.CourtList.Should().Contain(c => c.CourtId == "court-1");
            returnedUser.Client.UserList.Should().NotBeNull();
            returnedUser.Client.UserList.Should().ContainSingle();
        }

        // Helper method to clean up the in-memory database after each test
        private void CleanDatabase()
        {
            _context.User.RemoveRange(_context.User);
            _context.Profile.RemoveRange(_context.Profile);
            _context.Client.RemoveRange(_context.Client);
            _context.Court.RemoveRange(_context.Court);
            _context.SaveChanges();
        }
    }
}