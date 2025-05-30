using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.Context;
using DataLayer.DAL.Interface;
using Domain.DtoModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataLayer.DAL.Repository
{
    /// <summary>
    /// Implementation of the Report repository with thread-safe operations
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

        /// <summary>
        /// Get all counts as a single ReportDto object - Thread safe sequential execution
        /// </summary>
        public async Task<ReportDto> GetAllCountsAsync(CancellationToken cancellationToken = default)
        {
            var reportDto = new ReportDto();
            var errors = new List<string>();

            try
            {
                // Execute sequentially to avoid DbContext threading issues
                reportDto.CourtsCount = await GetSafeCountAsync(
                    () => _context.Court.AsNoTracking().CountAsync(cancellationToken),
                    "Courts", errors);

                reportDto.ProductsCount = await GetSafeCountAsync(
                    () => _context.Product.AsNoTracking().CountAsync(cancellationToken),
                    "Products", errors);

                reportDto.ClientsCount = await GetSafeCountAsync(
                    () => _context.Client.AsNoTracking().CountAsync(cancellationToken),
                    "Clients", errors);

                reportDto.RunsCount = await GetSafeCountAsync(
                    () => _context.Run.Where(p => p.Status == "Active").AsNoTracking().CountAsync(cancellationToken),
                    "ActiveRuns", errors);

                reportDto.UsersCount = await GetSafeCountAsync(
                    () => _context.User.Where(p => p.Status == "Active").AsNoTracking().CountAsync(cancellationToken),
                    "ActiveUsers", errors);

                reportDto.ProfilesCount = await GetSafeCountAsync(
                    () => _context.Profile.Where(p => p.Status == "Active").AsNoTracking().CountAsync(cancellationToken),
                    "ActiveProfiles", errors);

                reportDto.OrdersCount = await GetSafeCountAsync(
                    () => _context.Order.AsNoTracking().CountAsync(cancellationToken),
                    "Orders", errors);

                reportDto.PostsCount = await GetSafeCountAsync(
                    () => _context.Post.Where(p => p.Status == "Active").AsNoTracking().CountAsync(cancellationToken),
                    "ActivePosts", errors);

                reportDto.IsDataComplete = errors.Count == 0;
                reportDto.Errors = errors;

                _logger?.LogInformation("Successfully retrieved report counts. Errors: {ErrorCount}", errors.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Critical error getting report counts");
                reportDto.IsDataComplete = false;
                reportDto.Errors.Add($"Critical error: {ex.Message}");
            }

            return reportDto;
        }

        /// <summary>
        /// Stream all counts - Thread safe sequential execution
        /// </summary>
        public async IAsyncEnumerable<ReportDto> StreamAllCountsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var reportDto = await GetAllCountsAsync(cancellationToken);
            yield return reportDto;
        }

        /// <summary>
        /// Helper method to safely execute count operations with error handling
        /// </summary>
        private async Task<int> GetSafeCountAsync(Func<Task<int>> countOperation, string entityName, List<string> errors)
        {
            try
            {
                return await countOperation();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error getting {entityName} count: {ex.Message}";
                _logger?.LogWarning(ex, "Error getting {EntityName} counts, using 0", entityName);
                errors.Add(errorMessage);
                return 0;
            }
        }

        #region IDisposable and IAsyncDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
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
                if (_context != null)
                {
                    await _context.DisposeAsync();
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}