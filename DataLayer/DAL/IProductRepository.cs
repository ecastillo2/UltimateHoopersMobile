using Domain;

namespace DataLayer.DAL
{
    public interface IProductRepository : IDisposable
    {
        Task<List<Product>> GetProducts();
        Task<Product> GetProductById(string ProductId);
        Task InsertProduct(Product model);
        Task UpdateProduct(Product model);
        Task DeleteProduct(string TagId); 
        Task<int> Save();

    }
}
