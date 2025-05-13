using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for ErrorException repository
    /// </summary>
    public interface IErrorExceptionRepository
    {
        Task<List<ErrorException>> GetErrorExceptions();
        Task<ErrorException> GetErrorExceptionById(string errorExceptionId);
        Task InsertErrorException(ErrorException errorException);
        Task UpdateErrorException(ErrorException errorException);
        Task DeleteErrorException(string errorExceptionId);
    }

    /// <summary>
    /// Repository for ErrorException entity operations
    /// </summary>
    public class ErrorExceptionRepository : IErrorExceptionRepository
    {
        private readonly ApplicationDbContext _context;

        public ErrorExceptionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Get all error exceptions
        /// </summary>
        public async Task<List<ErrorException>> GetErrorExceptions()
        {
            return await _context.ErrorExceptions.ToListAsync();
        }

        /// <summary>
        /// Get error exception by ID
        /// </summary>
        public async Task<ErrorException> GetErrorExceptionById(string errorExceptionId)
        {
            return await _context.ErrorExceptions.FirstOrDefaultAsync(e => e.ErrorExceptionId == errorExceptionId);
        }

        /// <summary>
        /// Insert new error exception
        /// </summary>
        public async Task InsertErrorException(ErrorException errorException)
        {
            if (string.IsNullOrEmpty(errorException.ErrorExceptionId))
                errorException.ErrorExceptionId = Guid.NewGuid().ToString();

            errorException.CreatedDate = DateTime.Now;

            await _context.ErrorExceptions.AddAsync(errorException);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Update error exception
        /// </summary>
        public async Task UpdateErrorException(ErrorException errorException)
        {
            var existingException = await GetErrorExceptionById(errorException.ErrorExceptionId);
            if (existingException == null)
                return;

            _context.Entry(existingException).CurrentValues.SetValues(errorException);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete error exception
        /// </summary>
        public async Task DeleteErrorException(string errorExceptionId)
        {
            var errorException = await GetErrorExceptionById(errorExceptionId);
            if (errorException != null)
            {
                _context.ErrorExceptions.Remove(errorException);
                await _context.SaveChangesAsync();
            }
        }
    }
}