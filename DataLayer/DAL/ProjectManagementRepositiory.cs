using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL
{
    public class ProjectManagementRepository : IProjectManagementRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       
        public ProjectManagementRepository(HUDBContext context)
        {
            this._context = context;

        }

        /// <summary>
        /// Get ProjectManagement By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<ProjectManagement> GetProjectManagementById(string ProjectManagementId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.ProjectManagement
                                       where model.ProjectManagementId == ProjectManagementId
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
        public async Task<List<ProjectManagement>> GetProjectManagements()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await context.ProjectManagement.ToListAsync();

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
        public async Task InsertProjectManagement(ProjectManagement model)
        {
            using (var context = _context)
            {
                try
                {

                 

                    await context.ProjectManagement.AddAsync(model);
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
        public async Task UpdateProjectManagement(ProjectManagement model)
        {
            using (var context = _context)
            {
                var existingItem = context.ProjectManagement.Where(s => s.ProjectManagementId == model.ProjectManagementId).FirstOrDefault<ProjectManagement>();

                if (existingItem != null)
                {
                    existingItem.Url = model.Url;
                    existingItem.Name = model.Name;
                    existingItem.Description = model.Description;
                   

                    context.ProjectManagement.Update(existingItem);
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
        public async Task DeleteProjectManagement(string ProjectManagementId)
        {
            using (var context = _context)
            {
                ProjectManagement obj = (from u in context.ProjectManagement
                                         where u.ProjectManagementId == ProjectManagementId
                                         select u).FirstOrDefault();

                _context.ProjectManagement.Remove(obj);
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
