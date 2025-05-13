using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL
{
    public class ErrorExceptionRepository : IErrorExceptionRepository, IDisposable
    {
        private ErrorExceptionContext _context;

        public ErrorExceptionRepository(ErrorExceptionContext context)
        {
            this._context = context;

        }

        /// <summary>
        /// Get ErrorExceptions
        /// </summary>
        /// <returns></returns>
        public async Task<List<ErrorException>> GetErrorExceptions()
        {
            var network = _context.ErrorException.ToList();


            return network;
        }

        /// <summary>
        /// Get ErrorException By Id
        /// </summary>
        /// <param name="errorExceptionId"></param>
        /// <returns></returns>
        public async Task<ErrorException> GetErrorExceptionById(string errorExceptionId)
        {

			ErrorException model = (from u in _context.ErrorException
								   where u.ErrorExceptionId == errorExceptionId
								   select u).FirstOrDefault();

            return model;
        }

        /// <summary>
        /// Insert ErrorException
        /// </summary>
        /// <param name="errorException"></param>
        /// <returns></returns>
        public async Task InsertErrorException(ErrorException errorException)
        {
            try
            {
				errorException.ErrorExceptionId = Guid.NewGuid().ToString();
				errorException.CreatedDate = DateTime.Now;
               

				await _context.ErrorException.AddAsync(errorException);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
			await _context.SaveChangesAsync();
		}

        /// <summary>
        /// Update ErrorException
        /// </summary>
        /// <param name="errorException"></param>
        /// <returns></returns>
        public async Task UpdateErrorException(ErrorException errorException)
        {
            _context.Entry(errorException).State = EntityState.Modified;
			await _context.SaveChangesAsync();
		}

        /// <summary>
        /// Delete ErrorException
        /// </summary>
        /// <param name="errorExceptionId"></param>
        /// <returns></returns>
        public async Task DeleteErrorException(string errorExceptionId)
        {
			ErrorException contact = (from u in _context.ErrorException
									  where u.ErrorExceptionId == errorExceptionId
									  select u).FirstOrDefault();

            _context.ErrorException.Remove(contact);
			await _context.SaveChangesAsync();
		}

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public Task<int> Save()
        {
            return _context.SaveChangesAsync();
        }

    }
}
