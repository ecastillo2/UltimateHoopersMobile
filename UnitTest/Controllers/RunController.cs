using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTest.Utils;
using WebAPI.Controllers;

namespace UnitTest.Controllers
{
    public class RunControllerTests
    {
        private readonly Mock<IRunRepository> _mockRepository;
        private readonly Mock<ILogger<RunController>> _mockLogger;
        private readonly RunController _controller;

        public RunControllerTests()
        {
            _mockRepository = new Mock<IRunRepository>();
            _mockLogger = new Mock<ILogger<RunController>>();
            _controller = new RunController(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetPrivateRuns_ReturnsOkResult_WithRuns()
        {
            // Arrange
            var runs = new List<Run>
            {
                new Run { RunId = "1", Name = "Morning Run", CourtId = "court-1" },
                new Run { RunId = "2", Name = "Evening Run", CourtId = "court-2" }
            };

            _mockRepository.Setup(repo => repo.GetRunsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(runs);

            // Act
            var result = await _controller.GetPrivateRuns(CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedRuns = okResult.Value.Should().BeAssignableTo<IEnumerable<RunViewModelDto>>().Subject;
            returnedRuns.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetRunById_ReturnsOkResult_WithRun_WhenRunExists()
        {
            // Arrange
            var runId = "1";
            var run = new Run { RunId = runId, Name = "Test Run", CourtId = "court-1" };

            _mockRepository.Setup(repo => repo.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(run);

            // Act
            var result = await _controller.GetRunById(runId, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedRun = okResult.Value.Should().BeOfType<Run>().Subject;
            returnedRun.RunId.Should().Be(runId);
        }

        [Fact]
        public async Task UpdateRun_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var runId = "1";
            var model = new RunUpdateModelDto
            {
                RunId = runId,
                CourtId = runId, // Note: The controller checks CourtId instead of RunId (potential bug?)
                //Name = "Updated Run"
            };

            var existingRun = new Run { RunId = runId, Name = "Original Run" };

            _mockRepository.Setup(repo => repo.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRun);
            _mockRepository.Setup(repo => repo.UpdateRunAsync(It.IsAny<Run>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdateRun(runId, model, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
    }
}