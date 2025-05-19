using Domain;

namespace DataLayer.DAL.Interface
{
    public interface IPlayerCommentRepository : IDisposable
    {
        Task<List<PlayerComment>> GetPlayerComments();
        Task<PlayerComment> GetPlayerCommentById(string TagId);
        Task<List<PlayerComment>> GetPlayerCommentByProfileId(string ProfileId,string timezone);
        Task InsertPlayerComment(PlayerComment model);
        Task DeletePlayerComment(string PlayerCommentId); 
        Task<int> Save();

    }
}
