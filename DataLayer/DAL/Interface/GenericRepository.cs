using DataLayer.DAL.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly HUDBContext _context;

        public GenericRepository(HUDBContext context)
        {
            _context = context;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(string id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public virtual async Task DeleteAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            _context.Set<T>().Remove(entity);
        }
    }
}
