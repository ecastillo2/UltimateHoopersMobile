using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Product entity operations
    /// </summary>
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        public override async Task<Product> GetByIdAsync(object id)
        {
            string productId = id.ToString();
            return await _dbSet
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        /// <summary>
        /// Add new product
        /// </summary>
        public override async Task AddAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.ProductId))
                product.ProductId = Guid.NewGuid().ToString();

            // Set image URL
            product.ImageURL = $"https://uhblobstorageaccount.blob.core.windows.net/productimage/{product.ProductId}.webp";

            await base.AddAsync(product);
        }

        /// <summary>
        /// Update product
        /// </summary>
        public async Task UpdateProductAsync(Product product)
        {
            var existingProduct = await GetByIdAsync(product.ProductId);
            if (existingProduct == null)
                return;

            // Update properties
            existingProduct.Title = product.Title;
            existingProduct.Type = product.Type;
            existingProduct.Status = product.Status;
            existingProduct.Description = product.Description;
            existingProduct.Points = product.Points;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            existingProduct.Tag = product.Tag;
            existingProduct.ProductNumber = product.ProductNumber;

            _dbSet.Update(existingProduct);
            await SaveAsync();
        }
    }

    /// <summary>
    /// Interface for Product repository
    /// </summary>
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task UpdateProductAsync(Product product);
    }
}