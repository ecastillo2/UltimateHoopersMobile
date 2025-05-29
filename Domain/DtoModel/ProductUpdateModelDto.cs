using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class ProductUpdateModelDto
    {
        public string? ProductId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public int? Points { get; set; }
        public decimal? Price { get; set; }
        public string? Category { get; set; }
        public string? Tag { get; set; }

        public void UpdateProduct(Product product)
        {
            product.Title = Title;
            product.Description = Description;
            product.Type = Type;
            product.Status = Status;
            product.Points = Points;
            product.Price = Price;
            product.Category = Category;
            product.Tag = Tag;

        }
    }
}
