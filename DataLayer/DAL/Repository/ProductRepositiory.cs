using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class ProductRepository : IProductRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       

        public ProductRepository(HUDBContext context)
        {
            _context = context;
            
           
        }


        /// <summary>
        /// Get Product By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<Product> GetProductById(string ProductId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Product
                                       where model.ProductId == ProductId
                                       select model).FirstOrDefaultAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Handle the exception or log it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Tags
        /// </summary>
        /// <returns></returns>
        public async Task<List<Product>> GetProducts()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await context.Product.ToListAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Insert Tag
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertProduct(Product model)
        {
            using (var context = _context)
            {
                try
                {
                   

                    model.ImageURL = "https://uhblobstorageaccount.blob.core.windows.net/productimage/" + model.ProductId + ".webp";

                    await context.Product.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateProduct(Product model)
        {
            using (var context = _context)
            {
                var existingItem = context.Product.Where(s => s.ProductId == model.ProductId).FirstOrDefault<Product>();

                if (existingItem != null)
                {
                    existingItem.Title = model.Title;
                    existingItem.Type = model.Type;
                    existingItem.Status = model.Status;
                    existingItem.Description = model.Description;
                    existingItem.Points = model.Points;
                    existingItem.Price = model.Price;
                    existingItem.Category = model.Category;

                    context.Product.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }


        /// <summary>
        /// DeleteTag
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task DeleteProduct(string ProductId)
        {
            using (var context = _context)
            {
                Product obj = (from u in context.Product
                               where u.ProductId == ProductId
                               select u).FirstOrDefault();



                _context.Product.Remove(obj);
                await Save();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }
         
       


    }
}
