using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using UnitTest.Utils;
using WebAPI.Controllers;

namespace UnitTest.Controllers
{
    public class CourtControllerTests
    {
        private readonly Mock<ICourtRepository> _mockRepository;
        private readonly Mock<ILogger<CourtController>> _mockLogger;
        private readonly CourtController _controller;

        public CourtControllerTests()
        {
            _mockRepository = new Mock<ICourtRepository>();
            _mockLogger = new Mock<ILogger<CourtController>>();
            _controller = new CourtController(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCourts_ReturnsOkResult_WithCourts()
        {
            // Arrange
            var courts = new List<Court>
            {
                new Court { CourtId = "1", Name = "Court 1", Address = "123 Main St" },
                new Court { CourtId = "2", Name = "Court 2", Address = "456 Oak Ave" }
            };

            _mockRepository.Setup(repo => repo.GetCourtsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(courts);

            // Act
            var result = await _controller.GetCourts(CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCourts = okResult.Value.Should().BeAssignableTo<IEnumerable<CourtViewModelDto>>().Subject;
            returnedCourts.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCourtById_ReturnsOkResult_WithCourt_WhenCourtExists()
        {
            // Arrange
            var courtId = "1";
            var court = new Court { CourtId = courtId, Name = "Test Court", Address = "123 Test St" };

            _mockRepository.Setup(repo => repo.GetCourtByIdAsync(courtId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Act
            var result = await _controller.GetCourtById(courtId, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCourt = okResult.Value.Should().BeOfType<Court>().Subject;
            returnedCourt.CourtId.Should().Be(courtId);
        }

        [Fact]
        public async Task GetCourtById_ReturnsNotFound_WhenCourtDoesNotExist()
        {
            // Arrange
            var courtId = "nonexistent";

            _mockRepository.Setup(repo => repo.GetCourtByIdAsync(courtId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Court)null);

            // Act
            var result = await _controller.GetCourtById(courtId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateCourt_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var courtId = "1";
            var model = new CourtUpdateModelDto
            {
                CourtId = courtId,
                Name = "Updated Court",
                Address = "Updated Address"
            };

            var existingCourt = new Court { CourtId = courtId, Name = "Original Court" };

            _mockRepository.Setup(repo => repo.GetCourtByIdAsync(courtId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCourt);
            _mockRepository.Setup(repo => repo.UpdateCourtAsync(It.IsAny<Court>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdateCourt(courtId, model, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
    }
}