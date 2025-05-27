using DataLayer.DAL;
using DataLayer.DAL.Interface;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using WebAPI.Controllers;

namespace UnitTest.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _controller;
        private readonly Mock<IUserRepository> _mockUserRepository;

        public UserControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _mockUserRepository = new Mock<IUserRepository>();

            // Setup the UnitOfWork to return the mocked repository
            _mockUnitOfWork.Setup(uow => uow.User).Returns(_mockUserRepository.Object);

            _controller = new UserController(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUsers_ReturnsOkResult_WithUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = "1", Email = "user1@example.com" },
                new User { UserId = "2", Email = "user2@example.com" }
            };

            _mockUserRepository.Setup(repo => repo.GetAllAsync(
                    null, null, "", It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<User>>().Subject;
            returnedUsers.Should().HaveCount(2);
            returnedUsers.First().UserId.Should().Be("1");
            returnedUsers.First().Email.Should().Be("user1@example.com");
        }

        [Fact]
        public async Task GetUser_ReturnsOkResult_WithUser_WhenUserExists()
        {
            // Arrange
            var userId = "1";
            var user = new User { UserId = userId, Email = "user1@example.com" };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUser = okResult.Value.Should().BeOfType<User>().Subject;
            returnedUser.UserId.Should().Be(userId);
            returnedUser.Email.Should().Be("user1@example.com");
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "nonexistent";

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateUser_ReturnsCreatedAtAction_WithCreatedUser()
        {
            // Arrange
            var password = "password123";
            var user = new User
            {
                Email = "newuser@example.com",
                FirstName = "New",
                LastName = "User"
            };

            var createdUser = new User
            {
                UserId = "new-user-id",
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            _mockUserRepository.Setup(repo => repo.IsEmailAvailableAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>(), password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(user, password);

            // Assert
            var createdAtResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtResult.ActionName.Should().Be(nameof(UserController.GetUser));
            createdAtResult.RouteValues["id"].Should().Be(createdUser.UserId);

            var returnedUser = createdAtResult.Value.Should().BeOfType<User>().Subject;
            returnedUser.UserId.Should().Be(createdUser.UserId);
            returnedUser.Email.Should().Be(createdUser.Email);

            // Password and PasswordHash should be null for security
            returnedUser.Password.Should().BeNull();
            returnedUser.PasswordHash.Should().BeNull();

            // Verify that transaction methods were called
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenEmailIsAlreadyInUse()
        {
            // Arrange
            var password = "password123";
            var user = new User { Email = "existing@example.com" };

            _mockUserRepository.Setup(repo => repo.IsEmailAvailableAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateUser(user, password);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<object>();

            // Verify that transaction was not started
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenPasswordIsMissing()
        {
            // Arrange
            string password = null;
            var user = new User { Email = "newuser@example.com" };

            // Act
            var result = await _controller.CreateUser(user, password);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var userId = "1";
            var user = new User
            {
                UserId = userId,
                Email = "user1@example.com",
                FirstName = "Updated",
                LastName = "User"
            };

            var existingUser = new User
            {
                UserId = userId,
                Email = "user1@example.com",
                FirstName = "Original",
                LastName = "User"
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateUser(userId, user);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify that User fields were updated correctly
            _mockUserRepository.Verify(repo => repo.Update(It.Is<User>(u =>
                u.UserId == userId &&
                u.FirstName == "Updated" &&
                u.LastName == "User")), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_ReturnsBadRequest_WhenIdsMismatch()
        {
            // Arrange
            var userId = "1";
            var user = new User { UserId = "different-id" };

            // Act
            var result = await _controller.UpdateUser(userId, user);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "nonexistent";
            var user = new User { UserId = userId };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.UpdateUser(userId, user);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ChangePassword_ReturnsNoContent_WhenChangeSucceeds()
        {
            // Arrange
            var userId = "1";
            var model = new ChangePasswordModel { CurrentPassword = "old-password", NewPassword = "new-password" };

            var user = new User { UserId = userId };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserRepository.Setup(repo => repo.VerifyPassword(user, model.CurrentPassword))
                .Returns(true);

            _mockUserRepository.Setup(repo => repo.ChangePasswordAsync(userId, model.NewPassword, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(userId, model);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify that ChangePasswordAsync was called with correct parameters
            _mockUserRepository.Verify(repo => repo.ChangePasswordAsync(userId, model.NewPassword, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenNewPasswordIsMissing()
        {
            // Arrange
            var userId = "1";
            var model = new ChangePasswordModel { CurrentPassword = "old-password", NewPassword = null };

            // Act
            var result = await _controller.ChangePassword(userId, model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ChangePassword_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "nonexistent";
            var model = new ChangePasswordModel { CurrentPassword = "old-password", NewPassword = "new-password" };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.ChangePassword(userId, model);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}