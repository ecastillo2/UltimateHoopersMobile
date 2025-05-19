using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    public class EntityRepository : IEntityRepository, IDisposable
    {
        private readonly HUDBContext _context;

        public EntityRepository(HUDBContext context)
        {
            this._context = context;
        }

        // CRUD operations
        public async Task<List<Entity>> GetEntities() { ... }
        public async Task<Entity> GetEntityById(string id) { ... }
        public async Task InsertEntity(Entity entity) { ... }
        public async Task UpdateEntity(Entity entity) { ... }
        public async Task DeleteEntity(string id) { ... }
        public async Task<int> Save() { ... }
        public void Dispose() { ... }
    }
}
