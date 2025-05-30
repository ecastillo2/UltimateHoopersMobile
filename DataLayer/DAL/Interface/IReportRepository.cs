using Domain;
using Domain.DtoModel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for Court repository operations with consistent cancellation token support
    /// </summary>
    public interface IReportRepository : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Get all Courts
        /// </summary>
         IAsyncEnumerable<ReportDto> StreamAllCountsAsync(CancellationToken cancellationToken = default);

        
    }
}