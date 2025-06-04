using Domain;
using Domain.DtoModel;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Interface for Run API operations
    /// </summary>
    public interface IVideoApi
    {
        /// <summary>
        /// Get all Runs
        /// </summary>
        Task<List<Video>> GetVideosAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Video DetailViewModelDto
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<CursorPaginatedResultDto<VideoDetailViewModelDto>> GetVideosWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            string accessToken = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Product By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Video> GetVideoByIdAsync(string videoId, string accessToken, CancellationToken cancellationToken = default);

      
        /// <summary>
        /// Create Client Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> CreateVideoAsync(Video video, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Video Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateVideoFileAsync(Video video, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete Vide oAsync
        /// </summary>
        /// <param name="VideoId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(bool Success, string ErrorMessage)> DeleteVideoFileAsync(string VideoId, string accessToken, CancellationToken cancellationToken = default);

        
    }
}