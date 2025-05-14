using Domain;

namespace DataLayer.DAL
{
    public interface ISettingRepository : IDisposable
    {
        
        Task UpdateSetting(Setting model);
        Task<int> Save();

    }
}
