using Domain.DtoModel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for Report repository operations with consistent cancellation token support
    /// </summary>
    public interface IReportRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all counts as a single ReportDto object
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Single ReportDto with all counts</returns>
        Task<ReportDto> GetAllCountsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all counts (for scenarios where you need streaming)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of ReportDto</returns>
        IAsyncEnumerable<ReportDto> StreamAllCountsAsync(CancellationToken cancellationToken = default);
    }
}