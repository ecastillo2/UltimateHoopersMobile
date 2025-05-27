using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading;
using UnitTest.Utils;
using WebAPI.Controllers;

namespace UnitTest.Controllers
{
    public class PostControllerTests
    {
        private readonly Mock<IPostRepository> _mockRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly PostController _controller;

        public PostControllerTests()
        {
            _mockRepository = new Mock<IPostRepository>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup controller with mock repository and configuration
            _controller = new PostController((DataLayer.Context.ApplicationContext)_mockRepository.Object, _mockConfiguration.Object);

            // Setup HTTP context with headers for TimeZone
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["TimeZone"] = "America/New_York";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task GetPostsWithCursor_ReturnsOkResult_WithPaginatedPosts()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post { PostId = "1", Caption = "Post 1", ProfileId = "user1" },
                new Post { PostId = "2", Caption = "Post 2", ProfileId = "user2" }
            };

            string nextCursor = "next-page-token";
            bool hasMore = true;

            _mockRepository.Setup(repo => repo.GetPostsWithCursorAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((posts, nextCursor, hasMore));

            // Act
            var result = await _controller.GetPostsWithCursor(null, 10, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedData = okResult.Value.Should().BeOfType<CursorPaginatedResultDto<Post>>().Subject;

            returnedData.Items.Should().HaveCount(2);
            returnedData.NextCursor.Should().Be(nextCursor);
            returnedData.HasMore.Should().BeTrue();
        }

        [Fact]
        public async Task GetPost_ReturnsOkResult_WithPost_WhenPostExists()
        {
            // Arrange
            var postId = "1";
            var post = new Post
            {
                PostId = postId,
                Caption = "Test Post",
                ProfileId = "user1",
                PostComments = new List<PostComment>(),
                Likes = 10
            };

            _mockRepository.Setup(repo => repo.GetPostById(postId, It.IsAny<string>()))
                .ReturnsAsync(post);

            // Act
            var result = await _controller.GetPost(postId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPost = okResult.Value.Should().BeOfType<Post>().Subject;

            returnedPost.PostId.Should().Be(postId);
            returnedPost.Caption.Should().Be("Test Post");
            returnedPost.Likes.Should().Be(10);
        }

        [Fact]
        public async Task GetPost_ReturnsNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            var postId = "nonexistent";

            _mockRepository.Setup(repo => repo.GetPostById(postId, It.IsAny<string>()))
                .ReturnsAsync((Post)null);

            // Act
            var result = await _controller.GetPost(postId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreatePost_ReturnsCreatedAtAction_WithCreatedPost()
        {
            // Arrange
            var post = new Post
            {
                PostId = "new-post-id",
                Caption = "New Post",
                ProfileId = "user1"
            };

            _mockRepository.Setup(repo => repo.InsertPost(It.IsAny<Post>()))
                .Returns(Task.CompletedTask);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.CreatePost(post);

            // Assert
            var createdAtResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtResult.ActionName.Should().Be(nameof(PostController.GetPost));
            createdAtResult.RouteValues["id"].Should().Be(post.PostId);

            var returnedPost = createdAtResult.Value.Should().BeOfType<Post>().Subject;
            returnedPost.PostId.Should().Be(post.PostId);
            returnedPost.Caption.Should().Be("New Post");

            // Verify that InvalidateCache was called
            _mockRepository.Verify(repo => repo.InvalidateCache(), Times.Once);
        }

        [Fact]
        public async Task UpdatePost_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var postId = "1";
            var post = new Post
            {
                PostId = postId,
                Caption = "Updated Post",
                ProfileId = "user1"
            };

            _mockRepository.Setup(repo => repo.UpdatePost(It.IsAny<Post>()))
                .Returns(Task.CompletedTask);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdatePost(postId, post);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify that UpdatePost was called with the correct post
            _mockRepository.Verify(repo => repo.UpdatePost(It.Is<Post>(p =>
                p.PostId == postId &&
                p.Caption == "Updated Post")), Times.Once);

            // Verify that InvalidateCache was called
            _mockRepository.Verify(repo => repo.InvalidateCache(), Times.Once);
        }

        [Fact]
        public async Task UpdatePost_ReturnsBadRequest_WhenIdsMismatch()
        {
            // Arrange
            var postId = "1";
            var post = new Post { PostId = "different-id" };

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UpdatePost(postId, post);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeletePost_ReturnsNoContent_WhenDeleteSucceeds()
        {
            // Arrange
            var postId = "1";

            _mockRepository.Setup(repo => repo.DeletePost(postId))
                .Returns(Task.CompletedTask);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.DeletePost(postId);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Verify that DeletePost was called with the correct id
            _mockRepository.Verify(repo => repo.DeletePost(postId), Times.Once);

            // Verify that InvalidateCache was called
            _mockRepository.Verify(repo => repo.InvalidateCache(), Times.Once);
        }

        [Fact]
        public async Task LikePost_ReturnsOkResult_WithSuccessMessage()
        {
            // Arrange
            var postId = "1";
            var profileId = "user1";

            _mockRepository.Setup(repo => repo.LikePostAsync(postId, profileId))
                .Returns(Task.CompletedTask);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.LikePost(postId, profileId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<object>().Subject;

            // Verify that LikePostAsync was called with correct parameters
            _mockRepository.Verify(repo => repo.LikePostAsync(postId, profileId), Times.Once);
        }

        [Fact]
        public async Task UnlikePost_ReturnsOkResult_WithSuccessMessage()
        {
            // Arrange
            var postId = "1";
            var profileId = "user1";

            _mockRepository.Setup(repo => repo.UnlikePostAsync(postId, profileId))
                .Returns(Task.CompletedTask);

            // Add controller context for authorization
            _controller.ControllerContext = TestUtilities.CreateControllerContext();

            // Act
            var result = await _controller.UnlikePost(postId, profileId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<object>().Subject;

            // Verify that UnlikePostAsync was called with correct parameters
            _mockRepository.Verify(repo => repo.UnlikePostAsync(postId, profileId), Times.Once);
        }
    }
}