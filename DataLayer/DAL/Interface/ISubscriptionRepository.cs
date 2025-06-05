using Domain;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for Subscription repository operations with consistent cancellation token support
    /// </summary>
    public interface ISubscriptionRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all Subscriptions
        /// </summary>
        Task<List<Subscription>> GetSubscriptionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Subscription by ID
        /// </summary>
        Task<Subscription> GetSubscriptionByIdAsync(string subscriptionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert Subscription
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InsertSubscription(Subscription subscription, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Subscription by ID
        /// </summary>
        Task<bool> DeleteSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Subscription Async
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default);


        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}