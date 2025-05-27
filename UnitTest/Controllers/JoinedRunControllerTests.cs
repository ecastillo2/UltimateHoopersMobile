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
    public class JoinedRunControllerTests
    {
        private readonly Mock<IJoinedRunRepository> _mockRepository;
        private readonly Mock<ILogger<JoinedRunController>> _mockLogger;
        private readonly JoinedRunController _controller;

        public JoinedRunControllerTests()
        {
            _mockRepository = new Mock<IJoinedRunRepository>();
            _mockLogger = new Mock<ILogger<JoinedRunController>>();
            _controller = new JoinedRunController(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetJoinedRuns_ReturnsOkResult_WithJoinedRuns()
        {
            // Arrange
            var joinedRuns = new List<JoinedRun>
            {
                new JoinedRun { JoinedRunId = "1", ProfileId = "profile-1", RunId = "run-1" },
                new JoinedRun { JoinedRunId = "2", ProfileId = "profile-2", RunId = "run-2" }
            };

            _mockRepository.Setup(repo => repo.GetJoinedRuns())
                .ReturnsAsync(joinedRuns);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.GetJoinedRuns();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedJoinedRuns = okResult.Value.Should().BeAssignableTo<List<JoinedRun>>().Subject;
            returnedJoinedRuns.Should().HaveCount(2);
            returnedJoinedRuns[0].JoinedRunId.Should().Be("1");
            returnedJoinedRuns[0].ProfileId.Should().Be("profile-1");
        }

        [Fact]
        public async Task GetJoinedRunById_ReturnsOkResult_WithJoinedRun_WhenJoinedRunExists()
        {
            // Arrange
            var joinedRunId = "1";
            var joinedRun = new JoinedRun
            {
                JoinedRunId = joinedRunId,
                ProfileId = "profile-1",
                RunId = "run-1"
            };

            _mockRepository.Setup(repo => repo.GetJoinedRunById(joinedRunId))
                .ReturnsAsync(joinedRun);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.GetJoinedRunById(joinedRunId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedJoinedRun = okResult.Value.Should().BeOfType<JoinedRun>().Subject;
            returnedJoinedRun.JoinedRunId.Should().Be(joinedRunId);
            returnedJoinedRun.ProfileId.Should().Be("profile-1");
        }

        [Fact]
        public async Task GetJoinedRunById_ReturnsNotFound_WhenJoinedRunDoesNotExist()
        {
            // Arrange
            var joinedRunId = "nonexistent";

            _mockRepository.Setup(repo => repo.GetJoinedRunById(joinedRunId))
                .ReturnsAsync((JoinedRun)null);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.GetJoinedRunById(joinedRunId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task GetUserJoinedRunsAsync_ReturnsOkResult_WithJoinedRuns()
        {
            // Arrange
            var profileId = "profile-1";
            var joinedRuns = new List<JoinedRun>
            {
                new JoinedRun { JoinedRunId = "1", ProfileId = profileId, RunId = "run-1" },
                new JoinedRun { JoinedRunId = "2", ProfileId = profileId, RunId = "run-2" }
            };

            _mockRepository.Setup(repo => repo.GetJoinedRunsByProfileId(profileId))
                .ReturnsAsync(joinedRuns);

            foreach (var joinedRun in joinedRuns)
            {
                _mockRepository.Setup(repo => repo.GetRunById(joinedRun.RunId))
                    .ReturnsAsync(new Run { RunId = joinedRun.RunId });
            }

            // Act
            var result = await _controller.GetUserJoinedRunsAsync(profileId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<List<JoinedRunDetailViewModelDto>>().Subject;
            returnedResult.Should().HaveCount(2);
            returnedResult[0].JoinedRun.ProfileId.Should().Be(profileId);
            returnedResult[0].Run.Should().NotBeNull();
        }

        [Fact]
        public async Task IsProfileAlreadyInvitedToRun_ReturnsOkResult_WithBooleanResult()
        {
            // Arrange
            var profileId = "profile-1";
            var runId = "run-1";
            var isInvited = true;

            _mockRepository.Setup(repo => repo.IsProfileIdIdAlreadyInvitedToRunInJoinedRuns(profileId, runId))
                .ReturnsAsync(isInvited);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.IsProfileAlreadyInvitedToRun(profileId, runId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().Be(isInvited);
        }

        [Fact]
        public async Task IsProfileAlreadyInvitedToRun_ReturnsBadRequest_WhenParametersAreInvalid()
        {
            // Arrange
            string profileId = null;
            string runId = null;

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.IsProfileAlreadyInvitedToRun(profileId, runId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task RemoveUserJoinRunAsync_ReturnsOkResult_WhenRemovalSucceeds()
        {
            // Arrange
            var profileId = "profile-1";
            var runId = "run-1";

            _mockRepository.Setup(repo => repo.RemoveProfileFromRun(profileId, runId))
                .ReturnsAsync(true);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.RemoveUserJoinRunAsync(profileId, runId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var responseObj = okResult.Value.Should().BeAssignableTo<object>().Subject;

            // Verify that RemoveProfileFromRun was called with correct parameters
            _mockRepository.Verify(repo => repo.RemoveProfileFromRun(profileId, runId), Times.Once);
        }

        [Fact]
        public async Task RemoveUserJoinRunAsync_ReturnsNotFound_WhenProfileNotInRun()
        {
            // Arrange
            var profileId = "profile-1";
            var runId = "run-1";

            _mockRepository.Setup(repo => repo.RemoveProfileFromRun(profileId, runId))
                .ReturnsAsync(false);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.RemoveUserJoinRunAsync(profileId, runId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task RemoveUserJoinRunAsync_ReturnsBadRequest_WhenParametersAreInvalid()
        {
            // Arrange
            string profileId = null;
            string runId = null;

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.RemoveUserJoinRunAsync(profileId, runId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ClearJoinedRunByRun_ReturnsNoContent_WhenClearSucceeds()
        {
            // Arrange
            var runId = "run-1";

            _mockRepository.Setup(repo => repo.ClearJoinedRunByRun(runId))
                .Returns(Task.CompletedTask);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.ClearJoinedRunByRun(runId);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify that ClearJoinedRunByRun was called with correct parameters
            _mockRepository.Verify(repo => repo.ClearJoinedRunByRun(runId), Times.Once);
        }

        [Fact]
        public async Task ClearJoinedRunByRun_ReturnsBadRequest_WhenRunIdIsInvalid()
        {
            // Arrange
            string runId = null;

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.ClearJoinedRunByRun(runId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}