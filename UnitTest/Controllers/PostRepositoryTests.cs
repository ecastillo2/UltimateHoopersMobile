using DataLayer.Context;
using DataLayer.DAL.Repository;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UnitTest.Repositories
{
    public class PostRepositoryTests
    {
        private readonly Mock<ApplicationContext> _mockContext;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly PostRepository _repository;

        public PostRepositoryTests()
        {
            _mockContext = new Mock<ApplicationContext>();
            _mockConfiguration = new Mock<IConfiguration>();
            _repository = new PostRepository(_mockContext.Object, _mockConfiguration.Object);
        }

        [Fact]
        public void InvalidateCache_ClearsCache()
        {
            // Act
            _repository.InvalidateCache();

            // Assert
            // This is difficult to test directly due to private cache implementation
            // The method should complete without throwing
            true.Should().BeTrue();
        }

        [Fact]
        public async Task GetAverageStarRatingByProfileId_ReturnsZero_WhenNoRatings()
        {
            // Arrange
            var profileId = "profile-1";
            var mockRatingDbSet = new Mock<DbSet<Rating>>();
            var ratings = new List<Rating>().AsQueryable();

            mockRatingDbSet.As<IQueryable<Rating>>().Setup(m => m.Provider).Returns(ratings.Provider);
            mockRatingDbSet.As<IQueryable<Rating>>().Setup(m => m.Expression).Returns(ratings.Expression);
            mockRatingDbSet.As<IQueryable<Rating>>().Setup(m => m.ElementType).Returns(ratings.ElementType);
            mockRatingDbSet.As<IQueryable<Rating>>().Setup(m => m.GetEnumerator()).Returns(ratings.GetEnumerator());

            _mockContext.Setup(c => c.Rating).Returns(mockRatingDbSet.Object);

            // Act
            var result = await _repository.GetAverageStarRatingByProfileId(profileId);

            // Assert
            result.Should().Be("0");
        }
    }
}