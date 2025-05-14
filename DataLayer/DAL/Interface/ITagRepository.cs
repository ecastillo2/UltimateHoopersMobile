using Domain;

namespace DataLayer.DAL
{
    public interface ITagRepository : IDisposable
    {
        Task<List<Tag>> GetTags();
        Task<Tag> GetTagById(string TagId);
        Task InsertTag(Tag model);
        Task DeleteTag(string TagId); 
        Task<int> Save();

    }
}
