using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Microsoft.AspNetCore.Authorization;
using Domain;
using Swashbuckle.Swagger;

namespace API.Controllers
{
    /// <summary>
    /// Contact Controller
    /// </summary>
    [Route("api/[controller]")]
    public class ContactController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IContactRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Contact Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public ContactController(HUDBContext context, IConfiguration configuration)
        {
            this._configuration = configuration;
            this.repository = new ContactRepository(context);

        }

        /// <summary>
        /// Get Courts
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetContacts")]
        //[Authorize]
        public async Task<List<Domain.Contact>> GetContacts()
        {
            return await repository.GetContacts();

        }

        /// <summary>
        /// Create Contact
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        [HttpPost("CreateContact")]
        public async Task CreateContact([FromBody] Domain.Contact contact)
        {
            
            try
            {
                  await  repository.InsertContact(contact);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }

        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetContactById")]
        public async Task<Domain.Contact> GetContactById(string contactId)
        {
            try
            {
                return await repository.GetContactById(contactId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Delete Contact
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeleteContact")]
        public async Task<HttpResponseMessage> DeleteContact(string contactId)
        {
            try
            {
                await repository.DeleteContact(contactId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteContact");

                return await Task.FromResult(returnMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return await Task.FromResult(returnMessage);
        }
    }
}
