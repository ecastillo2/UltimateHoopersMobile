using DataLayer.DAL.Interface;
using Domain;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Services;

namespace UnitTest.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IProfileRepository> _mockProfileRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockProfileRepository = new Mock<IProfileRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            // Setup configuration
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("ThisIsAVeryLongSecretKeyForTesting123456789");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(c => c["Jwt:ExpirationHours"]).Returns("24");

            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockProfileRepository.Object,
                _mockConfiguration.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Authenticate_ReturnsUser_WhenCredentialsAreValid()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new User
            {
                UserId = "1",
                Email = email,
                Status = "Active",
                AccessLevel = "Standard"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(repo => repo.VerifyPassword(user, password))
                .Returns(true);
            _mockUserRepository.Setup(repo => repo.UpdateLastLoginDateAsync(user.UserId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.Authenticate(null, email, password);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(user.UserId);
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Authenticate_ReturnsNull_WhenCredentialsAreInvalid()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";
            var user = new User
            {
                UserId = "1",
                Email = email,
                Status = "Active"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(repo => repo.VerifyPassword(user, password))
                .Returns(false);

            // Act
            var result = await _authService.Authenticate(null, email, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Authenticate_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "password123";

            _mockUserRepository.Setup(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.Authenticate(null, email, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Authenticate_ReturnsNull_WhenUserIsInactive()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new User
            {
                UserId = "1",
                Email = email,
                Status = "Inactive"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(repo => repo.VerifyPassword(user, password))
                .Returns(true);

            // Act
            var result = await _authService.Authenticate(null, email, password);

            // Assert
            result.Should().BeNull();
        }
    }
}