using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL
{
    public class CourtRepository : ICourtRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       
        public CourtRepository(HUDBContext context)
        {
            this._context = context;

        }

        /// <summary>
        /// Get Court By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<Court> GetCourtById(string CourtId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Court
                                       where model.CourtId == CourtId
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
        public async Task<List<Court>> GetCourts()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await context.Court.ToListAsync();

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
        public async Task InsertCourt(Court model)
        {
            using (var context = _context)
            {
                try
                {

                    string fileType = string.Empty;
                    fileType = ".webp";

                    model.ImageURL = "https://uhblobstorageaccount.blob.core.windows.net/courtimage/" + model.CourtId + fileType;
                    

                    await context.Court.AddAsync(model);
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
        public async Task UpdateCourt(Court model)
        {
            using (var context = _context)
            {
                var existingItem = context.Court.Where(s => s.CourtId == model.CourtId).FirstOrDefault<Court>();

                if (existingItem != null)
                {
                    existingItem.Name = model.Name;
                    existingItem.Latitude = model.Latitude;
                    existingItem.Longitude = model.Longitude;
                    existingItem.Address = model.Address;
                    existingItem.Status = model.Status;
                    existingItem.NumberOfCourts = model.NumberOfCourts;
                    existingItem.RentalCostPerHour = model.RentalCostPerHour;
                    existingItem.Url = model.Url;
                    existingItem.CourtSize = model.CourtSize;
                    existingItem.CourtNumber = model.CourtNumber;

                    context.Court.Update(existingItem);
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
        public async Task DeleteCourt(string CourtId)
        {
            using (var context = _context)
            {
                Court obj = (from u in context.Court
                             where u.CourtId == CourtId
                             select u).FirstOrDefault();

                _context.Court.Remove(obj);
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
