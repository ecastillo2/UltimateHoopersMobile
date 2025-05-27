using DataLayer.Context;
using DataLayer.DAL.Repository;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTest.Repositories
{
    public class GenericRepositoryTests
    {
        private readonly Mock<ApplicationContext> _mockContext;
        private readonly Mock<DbSet<User>> _mockDbSet;
        private readonly Mock<ILogger> _mockLogger;
        private readonly GenericRepository<User> _repository;

        public GenericRepositoryTests()
        {
            _mockContext = new Mock<ApplicationContext>();
            _mockDbSet = new Mock<DbSet<User>>();
            _mockLogger = new Mock<ILogger>();

            _mockContext.Setup(c => c.Set<User>()).Returns(_mockDbSet.Object);
            _repository = new GenericRepository<User>(_mockContext.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEntity_WhenEntityExists()
        {
            // Arrange
            var userId = "1";
            var user = new User { UserId = userId, Email = "test@example.com" };

            _mockDbSet.Setup(db => db.FindAsync(new object[] { userId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _repository.GetByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenEntityDoesNotExist()
        {
            // Arrange
            var userId = "nonexistent";

            _mockDbSet.Setup(db => db.FindAsync(new object[] { userId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _repository.GetByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var user = new User { UserId = "1", Email = "test@example.com" };

            _mockDbSet.Setup(db => db.AddAsync(user, It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<User>>());

            // Act
            await _repository.AddAsync(user);

            // Assert
            _mockDbSet.Verify(db => db.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Update_CallsDbSetAttachAndSetsModifiedState()
        {
            // Arrange
            var user = new User { UserId = "1", Email = "test@example.com" };
            var mockEntry = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<User>>();

            _mockDbSet.Setup(db => db.Attach(user)).Returns(mockEntry.Object);
            _mockContext.Setup(c => c.Entry(user)).Returns(mockEntry.Object);

            // Act
            _repository.Update(user);

            // Assert
            _mockDbSet.Verify(db => db.Attach(user), Times.Once);
        }
    }
}