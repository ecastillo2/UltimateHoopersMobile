using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class SettingRepository : ISettingRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private ApplicationContext _context;
       
        public SettingRepository(ApplicationContext context)
        {
            _context = context;

        }

       
        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateSetting(Setting model)
        {
            using (var context = _context)
            {
                var existingItem = context.Setting.Where(s => s.ProfileId == model.ProfileId).FirstOrDefault<Setting>();

                if (existingItem != null)
                {
                    existingItem.AllowEmailNotification = model.AllowEmailNotification;
                    existingItem.AllowComments = model.AllowComments;
                 

                    context.Setting.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

       

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
