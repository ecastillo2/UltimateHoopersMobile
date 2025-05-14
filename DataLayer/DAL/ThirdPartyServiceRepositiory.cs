using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL
{
    public class ThirdPartyServiceRepository : IThirdPartyServiceRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       
        public ThirdPartyServiceRepository(HUDBContext context)
        {
            this._context = context;

        }

        /// <summary>
        /// Get Court By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<ThirdPartyService> GetThirdPartyServiceById(string ThirdPartyServiceId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.ThirdPartyService
                                       where model.ThirdPartyServiceId == ThirdPartyServiceId
                                       select model).FirstOrDefaultAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Handle the exception or log it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Courts
        /// </summary>
        /// <returns></returns>
        public async Task<List<ThirdPartyService>> GetThirdPartyServices()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await context.ThirdPartyService.ToListAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Insert Tag
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertThirdPartyService(ThirdPartyService model)
        {
            using (var context = _context)
            {
                try
                {


                    await context.ThirdPartyService.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateThirdPartyService(ThirdPartyService model)
        {
            using (var context = _context)
            {
                var existingItem = context.ThirdPartyService.Where(s => s.ThirdPartyServiceId == model.ThirdPartyServiceId).FirstOrDefault<ThirdPartyService>();

                if (existingItem != null)
                {
                   
                    existingItem.Url = model.Url;
                    existingItem.Description = model.Description;
                    existingItem.Cost = model.Cost;
                    existingItem.PaymentFrequency = model.PaymentFrequency;
                    existingItem.Name = model.Name;
                    existingItem.Category = model.Category;
                    existingItem.Type = model.Type;

                    context.ThirdPartyService.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Delete Court
        /// </summary>
        /// <param name="CourtId"></param>
        /// <returns></returns>
        public async Task DeleteThirdPartyService(string ThirdPartyServiceId)
        {
            using (var context = _context)
            {
                ThirdPartyService obj = (from u in context.ThirdPartyService
                                         where u.ThirdPartyServiceId == ThirdPartyServiceId
                                         select u).FirstOrDefault();

                _context.ThirdPartyService.Remove(obj);
                await Save();
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
