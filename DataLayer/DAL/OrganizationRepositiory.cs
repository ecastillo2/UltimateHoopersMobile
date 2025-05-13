using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL
{
    public class OrganizationRepository : IOrganizationRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       
        public OrganizationRepository(HUDBContext context)
        {
            this._context = context;
            
           
        }


        

        public async Task<Organization> GetOrganizationInfo()
        {
            using (var context = _context)
            {
                try
                {
                    // Use async method for database operations
                    var query = await context.Organization.FirstOrDefaultAsync();
                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return null; // Return null or handle the error appropriately
                }
            }
        }


        public async Task UpdateOrganization(Organization model)
        {
            using (var context = _context)
            {
                try
                {
                    // Check if the organization exists in the database
                    var organization = await context.Organization.FirstOrDefaultAsync(o => o.OrganizationId == model.OrganizationId);

                    if (organization != null)
                    {
                        // Update the fields of the existing organization with the values from the model
                        organization.CompanyName = model.CompanyName;
                        organization.InstagramURL = model.InstagramURL;
                        organization.FacebookURL = model.FacebookURL;
                        organization.TwitterURL = model.TwitterURL;
                        organization.YouTubeURL = model.YouTubeURL;
                        // Add other fields to update as needed

                        // Save changes to the database
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine("Organization not found.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
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
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        
    }
}
