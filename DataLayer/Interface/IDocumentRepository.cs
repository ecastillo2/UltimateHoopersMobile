using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for Document repository
    /// </summary>
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        // Add any document-specific repository methods here
    }
}