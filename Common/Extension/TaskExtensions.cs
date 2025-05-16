using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Extensions
{
    /// <summary>
    /// Extension methods for Task objects to support cancellation
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Allows adding cancellation support to a task that doesn't have it built-in
        /// </summary>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            // Create a task completion source that will be completed when the task completes or is canceled
            var tcs = new TaskCompletionSource<bool>();

            // Register a callback with the cancellation token
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                // Create a task that will complete when the cancellation token is triggered
                var cancellationTask = tcs.Task;

                // Wait for either the original task or the cancellation to complete
                var completedTask = await Task.WhenAny(task, cancellationTask);

                // If the cancellation completed first, then cancel
                if (completedTask == cancellationTask)
                {
                    // Check if already canceled
                    cancellationToken.ThrowIfCancellationRequested();
                }

                // Return the result of the original task
                return await task;
            }
        }

        /// <summary>
        /// Allows adding cancellation support to a task that doesn't have it built-in
        /// </summary>
        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            // Create a task completion source that will be completed when the task completes or is canceled
            var tcs = new TaskCompletionSource<bool>();

            // Register a callback with the cancellation token
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                // Create a task that will complete when the cancellation token is triggered
                var cancellationTask = tcs.Task;

                // Wait for either the original task or the cancellation to complete
                var completedTask = await Task.WhenAny(task, cancellationTask);

                // If the cancellation completed first, then cancel
                if (completedTask == cancellationTask)
                {
                    // Check if already canceled
                    cancellationToken.ThrowIfCancellationRequested();
                }

                // Await the original task to propagate any exceptions
                await task;
            }
        }
    }
}