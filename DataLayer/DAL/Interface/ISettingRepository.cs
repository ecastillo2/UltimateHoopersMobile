using Domain;

namespace DataLayer.DAL.Interface
{
    public interface ISettingRepository : IDisposable
    {
        
        Task UpdateSetting(Setting model);
        Task<int> Save();

    }
}
