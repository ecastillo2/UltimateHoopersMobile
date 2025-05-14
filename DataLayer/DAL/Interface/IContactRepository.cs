using Domain;

namespace DataLayer.DAL
{
    public interface IContactRepository : IDisposable
    {
        Task<List<Contact>> GetContacts();
        Task<Contact> GetContactById(string ContactId);
        Task InsertContact(Contact model);
        Task DeleteContact(string NotificationId); 
        Task<int> Save();

    }
}
