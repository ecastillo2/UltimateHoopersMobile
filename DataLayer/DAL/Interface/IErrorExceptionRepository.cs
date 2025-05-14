using Domain;

namespace DataLayer.DAL
{
    public interface IErrorExceptionRepository : IDisposable
    {
        Task<List<ErrorException>> GetErrorExceptions();
        Task<ErrorException> GetErrorExceptionById(string errorExceptionId);
        Task InsertErrorException(ErrorException errorException);
        Task DeleteErrorException(string errorExceptionId);
        Task UpdateErrorException(ErrorException ErrorException);
        Task<int> Save();

    }
}
