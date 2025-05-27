using DataLayer.DAL;
using DataLayer.DAL.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebAPI.Extensions;
using WebAPI.Services;

namespace UnitTest.Extensions
{
    public class ServiceExtensionsTests
    {
        [Fact]
        public void AddDataServices_RegistersAllRepositories()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();

            configuration.Setup(c => c.GetConnectionString("UnderGroundhoopersDB"))
                .Returns("Server=test;Database=test;");

            // Act
            services.AddDataServices(configuration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            // Verify key services are registered
            serviceProvider.GetService<IUnitOfWork>().Should().NotBeNull();
            serviceProvider.GetService<IUserRepository>().Should().NotBeNull();
            serviceProvider.GetService<IProfileRepository>().Should().NotBeNull();
            serviceProvider.GetService<IPostRepository>().Should().NotBeNull();
        }

        [Fact]
        public void AddApplicationServices_RegistersAuthService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddApplicationServices();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IAuthService>().Should().NotBeNull();
        }

        [Fact]
        public void AddCorsPolicies_RegistersCorsService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddCorsPolicies();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var corsService = serviceProvider.GetService<ICorsService>();
            corsService.Should().NotBeNull();
        }
    }
}