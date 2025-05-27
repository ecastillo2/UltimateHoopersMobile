using DataLayer.Context;
using DataLayer.DAL;
using DataLayer.DAL.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTest.Repositories
{
    public class UnitOfWorkTests
    {
        private readonly Mock<ApplicationContext> _mockContext;
        private readonly Mock<ILogger<UnitOfWork>> _mockLogger;
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            _mockContext = new Mock<ApplicationContext>();
            _mockLogger = new Mock<ILogger<UnitOfWork>>();
            _unitOfWork = new UnitOfWork(_mockContext.Object, _mockLogger.Object);
        }

        [Fact]
        public void User_ReturnsUserRepository()
        {
            // Act
            var userRepo = _unitOfWork.User;

            // Assert
            userRepo.Should().NotBeNull();
            userRepo.Should().BeOfType<UserRepository>();
        }

        [Fact]
        public void Profile_ReturnsProfileRepository()
        {
            // Act
            var profileRepo = _unitOfWork.Profile;

            // Assert
            profileRepo.Should().NotBeNull();
            profileRepo.Should().BeOfType<ProfileRepository>();
        }

        [Fact]
        public async Task BeginTransactionAsync_CallsContextBeginTransaction()
        {
            // Arrange
            var mockDatabase = new Mock<DatabaseFacade>(_mockContext.Object);
            var mockTransaction = new Mock<IDbContextTransaction>();

            _mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
            mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            // Act
            await _unitOfWork.BeginTransactionAsync();

            // Assert
            mockDatabase.Verify(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SaveChangesAsync_CallsContextSaveChanges()
        {
            // Arrange
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _unitOfWork.SaveChangesAsync();

            // Assert
            result.Should().Be(1);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}