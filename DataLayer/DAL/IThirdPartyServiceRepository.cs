using Domain;

namespace DataLayer.DAL
{
    public interface IThirdPartyServiceRepository : IDisposable
    {
        Task<List<ThirdPartyService>> GetThirdPartyServices();
        Task<ThirdPartyService> GetThirdPartyServiceById(string ThirdPartyServiceId);
        Task InsertThirdPartyService(ThirdPartyService model);
        Task DeleteThirdPartyService(string ThirdPartyServiceId);
        Task UpdateThirdPartyService(ThirdPartyService model);
        Task<int> Save();

    }
}
