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
    public class ClientControllerTests
    {
        private readonly Mock<IClientRepository> _mockRepository;
        private readonly Mock<ILogger<ClientController>> _mockLogger;
        private readonly ClientController _controller;

        public ClientControllerTests()
        {
            _mockRepository = new Mock<IClientRepository>();
            _mockLogger = new Mock<ILogger<ClientController>>();
            _controller = new ClientController(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetClients_ReturnsOkResult_WithClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                new Client { ClientId = "1", Name = "Client 1", Address = "123 Business St" },
                new Client { ClientId = "2", Name = "Client 2", Address = "456 Commerce Ave" }
            };

            _mockRepository.Setup(repo => repo.GetClientsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clients);

            // Act
            var result = await _controller.GetClients(CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedClients = okResult.Value.Should().BeAssignableTo<IEnumerable<ClientViewModelDto>>().Subject;
            returnedClients.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetClientById_ReturnsOkResult_WithClient_WhenClientExists()
        {
            // Arrange
            var clientId = "1";
            var client = new Client { ClientId = clientId, Name = "Test Client" };

            _mockRepository.Setup(repo => repo.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result = await _controller.GetClientById(clientId, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedClient = okResult.Value.Should().BeOfType<Client>().Subject;
            returnedClient.ClientId.Should().Be(clientId);
        }

        [Fact]
        public async Task UpdateClient_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var clientId = "1";
            var model = new ClientUpdateModelDto
            {
                ClientId = clientId,
                Name = "Updated Client",
                Address = "Updated Address"
            };

            var existingClient = new Client { ClientId = clientId, Name = "Original Client" };

            _mockRepository.Setup(repo => repo.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);
            _mockRepository.Setup(repo => repo.UpdateClientAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdateClient(clientId, model, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
    }
}
