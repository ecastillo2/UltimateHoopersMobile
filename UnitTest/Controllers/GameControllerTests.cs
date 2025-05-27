using DataLayer.DAL.Interface;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTest.Utils;
using WebAPI.Controllers;

namespace UnitTest.Controllers
{
    public class GameControllerTests
    {
        private readonly Mock<IGameRepository> _mockRepository;
        private readonly Mock<ILogger<GameController>> _mockLogger;
        private readonly GameController _controller;

        public GameControllerTests()
        {
            _mockRepository = new Mock<IGameRepository>();
            _mockLogger = new Mock<ILogger<GameController>>();
            _controller = new GameController(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetGames_ReturnsOkResult_WithGames()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { GameId = "1", RunId = "run-1", GameNumber = "G001" },
                new Game { GameId = "2", RunId = "run-2", GameNumber = "G002" }
            };

            _mockRepository.Setup(repo => repo.GetGames())
                .ReturnsAsync(games);

            // Act
            var result = await _controller.GetGames();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedGames = okResult.Value.Should().BeAssignableTo<List<Game>>().Subject;
            returnedGames.Should().HaveCount(2);
            returnedGames[0].GameId.Should().Be("1");
            returnedGames[0].GameNumber.Should().Be("G001");
        }

        [Fact]
        public async Task GetGameById_ReturnsOkResult_WithGame_WhenGameExists()
        {
            // Arrange
            var gameId = "1";
            var game = new Game
            {
                GameId = gameId,
                RunId = "run-1",
                GameNumber = "G001",
                ProfileList = new List<Profile>(),
                Run = new Run()
            };

            _mockRepository.Setup(repo => repo.GetGameById(gameId))
                .ReturnsAsync(game);

            // Act
            var result = await _controller.GetGameById(gameId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedGame = okResult.Value.Should().BeOfType<Game>().Subject;
            returnedGame.GameId.Should().Be(gameId);
            returnedGame.GameNumber.Should().Be("G001");
        }

        [Fact]
        public async Task GetGameById_ReturnsNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            var gameId = "nonexistent";

            _mockRepository.Setup(repo => repo.GetGameById(gameId))
                .ReturnsAsync((Game)null);

            // Act
            var result = await _controller.GetGameById(gameId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task GetGamesByProfileId_ReturnsOkResult_WithGames()
        {
            // Arrange
            var profileId = "profile-1";
            var games = new List<Game>
            {
                new Game { GameId = "1", RunId = "run-1", GameNumber = "G001" },
                new Game { GameId = "2", RunId = "run-2", GameNumber = "G002" }
            };

            _mockRepository.Setup(repo => repo.GetGamesByProfileId(profileId))
                .ReturnsAsync(games);

            // Act
            var result = await _controller.GetGamesByProfileId(profileId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedGames = okResult.Value.Should().BeAssignableTo<List<Game>>().Subject;
            returnedGames.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateGame_ReturnsCreatedAtAction_WithCreatedGame()
        {
            // Arrange
            var game = new Game
            {
                GameId = "new-game",
                RunId = "run-1",
                GameNumber = "G003"
            };

            _mockRepository.Setup(repo => repo.InsertGame(It.IsAny<Game>()))
                .Returns(Task.CompletedTask);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.CreateGame(game);

            // Assert
            var createdAtResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtResult.ActionName.Should().Be(nameof(GameController.GetGameById));
            createdAtResult.RouteValues["gameId"].Should().Be(game.GameId);

            var returnedGame = createdAtResult.Value.Should().BeOfType<Game>().Subject;
            returnedGame.GameId.Should().Be(game.GameId);
            returnedGame.GameNumber.Should().Be("G003");

            // Verify that InsertGame was called with the game
            _mockRepository.Verify(repo => repo.InsertGame(It.Is<Game>(g =>
                g.GameId == game.GameId &&
                g.RunId == game.RunId)), Times.Once);
        }

        [Fact]
        public async Task CreateGame_ReturnsBadRequest_WhenGameIsNull()
        {
            // Arrange
            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.CreateGame(null);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task UpdateGame_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var gameId = "1";
            var game = new Game
            {
                GameId = gameId,
                RunId = "run-1",
                GameNumber = "G001",
                Status = "Updated"
            };

            _mockRepository.Setup(repo => repo.GetGameById(gameId))
                .ReturnsAsync(game);

            _mockRepository.Setup(repo => repo.UpdateGame(It.IsAny<Game>()))
                .Returns(Task.CompletedTask);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdateGame(game);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify that UpdateGame was called with the game
            _mockRepository.Verify(repo => repo.UpdateGame(It.Is<Game>(g =>
                g.GameId == gameId &&
                g.Status == "Updated")), Times.Once);
        }

        [Fact]
        public async Task UpdateGame_ReturnsBadRequest_WhenGameIsNull()
        {
            // Arrange
            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdateGame(null);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task UpdateGame_ReturnsNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            var gameId = "nonexistent";
            var game = new Game { GameId = gameId };

            _mockRepository.Setup(repo => repo.GetGameById(gameId))
                .ReturnsAsync((Game)null);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdateGame(game);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task DeleteGame_ReturnsNoContent_WhenDeleteSucceeds()
        {
            // Arrange
            var gameId = "1";
            var game = new Game { GameId = gameId };

            _mockRepository.Setup(repo => repo.GetGameById(gameId))
                .ReturnsAsync(game);

            _mockRepository.Setup(repo => repo.DeleteGame(gameId))
                .Returns(Task.CompletedTask);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.DeleteGame(gameId);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify that DeleteGame was called with the game id
            _mockRepository.Verify(repo => repo.DeleteGame(gameId), Times.Once);
        }

        [Fact]
        public async Task DeleteGame_ReturnsNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            var gameId = "nonexistent";

            _mockRepository.Setup(repo => repo.GetGameById(gameId))
                .ReturnsAsync((Game)null);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.DeleteGame(gameId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task GetGameHistory_ReturnsOkResult_WithGameHistory()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { GameId = "1", RunId = "run-1", GameNumber = "G001" },
                new Game { GameId = "2", RunId = "run-2", GameNumber = "G002" }
            };

            _mockRepository.Setup(repo => repo.GetGameHistory())
                .ReturnsAsync(games);

            // Act
            var result = await _controller.GetGameHistory();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedGames = okResult.Value.Should().BeAssignableTo<List<Game>>().Subject;
            returnedGames.Should().HaveCount(2);
        }
    }
}