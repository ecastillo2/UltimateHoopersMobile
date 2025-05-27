using DataLayer.Context;
using DataLayer.DAL.Repository;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTest.Repositories
{
    public class UserRepositoryTests
    {
        private readonly Mock<ApplicationContext> _mockContext;
        private readonly Mock<DbSet<User>> _mockDbSet;
        private readonly Mock<ILogger<UserRepository>> _mockLogger;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            _mockContext = new Mock<ApplicationContext>();
            _mockDbSet = new Mock<DbSet<User>>();
            _mockLogger = new Mock<ILogger<UserRepository>>();

            _mockContext.Setup(c => c.Set<User>()).Returns(_mockDbSet.Object);
            _repository = new UserRepository(_mockContext.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { UserId = "1", Email = email };
            var users = new List<User> { user }.AsQueryable();

            _mockDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            _mockDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            _mockDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            _mockDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            // Act
            var result = await _repository.GetUserByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(email);
        }

        [Fact]
        public async Task IsEmailAvailableAsync_ReturnsTrue_WhenEmailNotInUse()
        {
            // Arrange
            var email = "new@example.com";
            var users = new List<User>().AsQueryable();

            _mockDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            _mockDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            _mockDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            _mockDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            // Act
            var result = await _repository.IsEmailAvailableAsync(email);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_ReturnsTrue_ForValidPassword()
        {
            // Arrange
            var user = new User
            {
                UserId = "1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            };
            var password = "password123";

            // Act
            var result = _repository.VerifyPassword(user, password);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_ReturnsFalse_ForInvalidPassword()
        {
            // Arrange
            var user = new User
            {
                UserId = "1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            };
            var password = "wrongpassword";

            // Act
            var result = _repository.VerifyPassword(user, password);

            // Assert
            result.Should().BeFalse();
        }
    }
}