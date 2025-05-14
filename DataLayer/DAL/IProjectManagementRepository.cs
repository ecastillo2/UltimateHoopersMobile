using Domain;

namespace DataLayer.DAL
{
    public interface IProjectManagementRepository : IDisposable
    {
        Task<List<ProjectManagement>> GetProjectManagements();
        Task<ProjectManagement> GetProjectManagementById(string ProjectManagementId);
        Task InsertProjectManagement(ProjectManagement model);
        Task DeleteProjectManagement(string ProjectManagementId); 
        Task UpdateProjectManagement(ProjectManagement model);
        Task<int> Save();

    }
}
