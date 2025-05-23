using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class ContactRepository : IContactRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private ApplicationContext _context;
       

        public ContactRepository(ApplicationContext context)
        {
            _context = context;

        }

        /// <summary>
        /// Get Courts
        /// </summary>
        /// <returns></returns>
        public async Task<List<Contact>> GetContacts()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await context.Contact.ToListAsync();

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
        /// Insert Contact
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertContact(Contact model)
        {
            using (var context = _context)
            {
                try
                {
                    model.ContactId = Guid.NewGuid().ToString();
                    model.CreatedDate = DateTime.Now.ToString();

                    await context.Contact.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }



        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<Contact> GetContactById(string ContactId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Contact
                                       where model.ContactId == ContactId
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
        /// Delete Contact
        /// </summary>
        /// <param name="ContactId"></param>
        /// <returns></returns>
        public async Task DeleteContact(string ContactId)
        {
            using (var context = _context)
            {
                Contact obj = (from u in context.Contact
                               where u.ContactId == ContactId
                               select u).FirstOrDefault();



                _context.Contact.Remove(obj);
                await Save();
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
