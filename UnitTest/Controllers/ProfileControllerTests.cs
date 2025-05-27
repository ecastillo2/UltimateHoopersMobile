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
    public class ProfileControllerTests
    {
        private readonly Mock<IProfileRepository> _mockRepository;
        private readonly Mock<ILogger<ProfileController>> _mockLogger;
        private readonly ProfileController _controller;

        public ProfileControllerTests()
        {
            _mockRepository = new Mock<IProfileRepository>();
            _mockLogger = new Mock<ILogger<ProfileController>>();
            _controller = new ProfileController(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetProfiles_ReturnsOkResult_WithProfiles()
        {
            // Arrange
            var profiles = new List<Profile>
            {
                new Profile { ProfileId = "1", UserName = "TestUser1" },
                new Profile { ProfileId = "2", UserName = "TestUser2" }
            };

            _mockRepository.Setup(repo => repo.GetProfilesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            // Act
            var result = await _controller.GetProfiles(CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProfiles = okResult.Value.Should().BeAssignableTo<IEnumerable<ProfileViewModelDto>>().Subject;
            returnedProfiles.Should().HaveCount(2);
            returnedProfiles.First().ProfileId.Should().Be("1");
            returnedProfiles.First().UserName.Should().Be("TestUser1");
        }

        [Fact]
        public async Task GetProfileById_ReturnsOkResult_WithProfile_WhenProfileExists()
        {
            // Arrange
            var profileId = "1";
            var profile = new Profile { ProfileId = profileId, UserName = "TestUser" };
            var settings = new Setting { SettingId = "1", ProfileId = profileId };
            var scoutingReport = new ScoutingReport { ScoutingReportId = "1", ProfileId = profileId };
            var gameStats = new GameStatistics { TotalGames = 10, WinPercentage = 70 };

            _mockRepository.Setup(repo => repo.GetProfileByIdAsync(profileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);
            _mockRepository.Setup(repo => repo.GetProfileSettingsAsync(profileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);
            _mockRepository.Setup(repo => repo.GetScoutingReportAsync(profileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(scoutingReport);
            _mockRepository.Setup(repo => repo.GetProfileGameStatisticsAsync(profileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(gameStats);

            // Act
            var result = await _controller.GetProfileById(profileId, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProfile = okResult.Value.Should().BeOfType<ProfileDetailViewModelDto>().Subject;
            returnedProfile.ProfileId.Should().Be(profileId);
            returnedProfile.UserName.Should().Be("TestUser");
            returnedProfile.Setting.Should().NotBeNull();
            returnedProfile.ScoutingReport.Should().NotBeNull();
            returnedProfile.GameStatistics.Should().NotBeNull();
            returnedProfile.GameStatistics.TotalGames.Should().Be(10);
        }

        [Fact]
        public async Task GetProfileById_ReturnsNotFound_WhenProfileDoesNotExist()
        {
            // Arrange
            var profileId = "nonexistent";

            _mockRepository.Setup(repo => repo.GetProfileByIdAsync(profileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Profile)null);

            // Act
            var result = await _controller.GetProfileById(profileId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateProfile_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var profileId = "1";
            var model = new ProfileUpdateModelDto
            {
                ProfileId = profileId,
                Bio = "Updated bio",
                Height = "6'2\"",
                Weight = "180"
            };

            var existingProfile = new Profile { ProfileId = profileId, UserName = "TestUser" };

            _mockRepository.Setup(repo => repo.GetProfileByIdAsync(profileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProfile);
            _mockRepository.Setup(repo => repo.UpdateProfileAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdateProfile(profileId, model, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify the profile was updated with the model values
            _mockRepository.Verify(repo => repo.UpdateProfileAsync(
                It.Is<Profile>(p =>
                    p.ProfileId == profileId &&
                    p.Bio == model.Bio &&
                    p.Height == model.Height &&
                    p.Weight == model.Weight),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task UpdateProfile_ReturnsBadRequest_WhenIdsMismatch()
        {
            // Arrange
            var profileId = "1";
            var model = new ProfileUpdateModelDto { ProfileId = "different-id" };

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdateProfile(profileId, model, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateProfile_ReturnsNotFound_WhenProfileDoesNotExist()
        {
            // Arrange
            var profileId = "nonexistent";
            var model = new ProfileUpdateModelDto { ProfileId = profileId };

            _mockRepository.Setup(repo => repo.GetProfileByIdAsync(profileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Profile)null);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdateProfile(profileId, model, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}