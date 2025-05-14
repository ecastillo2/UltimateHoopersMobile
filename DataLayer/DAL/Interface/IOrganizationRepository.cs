using Domain;

namespace DataLayer.DAL
{
    public interface IOrganizationRepository : IDisposable
    {
        Task<Organization> GetOrganizationInfo();
        Task UpdateOrganization(Organization model);
        Task<int> Save();
    }
}
