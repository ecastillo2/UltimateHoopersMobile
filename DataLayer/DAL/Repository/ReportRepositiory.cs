using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.Context;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataLayer.DAL.Repository
{
    /// <summary>
    /// Implementation of the Court repository with optimized query methods
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<ReportRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;

        public ReportRepository(ApplicationContext context, IConfiguration configuration, ILogger<ReportRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }


        public async IAsyncEnumerable<ReportDto> StreamAllCountsAsync(
     [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var courtCounts = await _context.Court
               .AsNoTracking()
               .CountAsync(cancellationToken);

            var productCounts = await _context.Product
               .AsNoTracking()
               .CountAsync(cancellationToken);

            var clientCounts = await _context.Client
               .AsNoTracking()
               .CountAsync(cancellationToken);

            yield return new ReportDto
            {
                CourtsCount = courtCounts,
                ProductsCount = productCounts,
                ClientsCount = clientCounts,
            };
        }

     

        #region IDisposable and IAsyncDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

      

        #endregion
    }

   
}