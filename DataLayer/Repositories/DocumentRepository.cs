using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Document entity operations
    /// </summary>
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get document by ID
        /// </summary>
        public override async Task<Document> GetByIdAsync(object id)
        {
            string documentId = id.ToString();
            return await _dbSet.FirstOrDefaultAsync(d => d.DocumentId == documentId);
        }

        /// <summary>
        /// Add new document
        /// </summary>
        public override async Task AddAsync(Document document)
        {
            if (string.IsNullOrEmpty(document.DocumentId))
                document.DocumentId = Guid.NewGuid().ToString();

            document.CreatedDate = DateTime.Now;

            await base.AddAsync(document);
        }
    }
}