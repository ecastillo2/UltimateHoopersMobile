using Domain;
using Domain.DtoModel;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Interface for ISubscription API operations
    /// </summary>
    public interface ISubscriptionApi
    {
        /// <summary>
        /// Get all ISubscriptions
        /// </summary>
        Task<List<Subscription>> GetSubscriptionsAsync(string accessToken, CancellationToken cancellationToken = default);



        /// <summary>
        /// Get Subscription By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Subscription> GetSubscriptionByIdAsync(string subscriptionId, string accessToken, CancellationToken cancellationToken = default);

      
        /// <summary>
        /// Create Client Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> CreateSubscriptionAsync(Subscription subscription, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Client Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateSubscriptionAsync(Subscription subscription, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete Subscription Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(bool Success, string ErrorMessage)> DeleteSubscriptionAsync(string subscriptionId, string accessToken, CancellationToken cancellationToken = default);

        
    }
}