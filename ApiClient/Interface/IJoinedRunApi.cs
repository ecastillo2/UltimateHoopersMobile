using Domain;
using Domain.DtoModel;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// IJoinedRunApi
    /// </summary>
    public interface IJoinedRunApi
    {

        /// <summary>
        /// GetUserJoinedRunsAsync
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<JoinedRunDetailViewModelDto>> GetUserJoinedRunsAsync(string profileId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// GetJoinedRunProfilesByRunIdAsync
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Profile>> GetJoinedRunProfilesByRunIdAsync(string runId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// RemoveProfileJoinRunAsync
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RemoveProfileJoinRunAsync(string profileId, string runId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// AddProfileToJoinedRunAsync
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AddProfileToJoinedRunAsync(string profileId, string runId, string status, string accessToken, CancellationToken cancellationToken = default);


    }
}