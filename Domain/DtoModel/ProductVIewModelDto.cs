using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class ProductViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public ProductViewModelDto() { }

        // Existing constructor for mapping from Profile
        public ProductViewModelDto(Product product)
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

        public string? ProductId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public int? Points { get; set; }
        public decimal? Price { get; set; }
        public string? Category { get; set; }
        public string? Tag { get; set; }

    }
}
