using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ProductDetailViewModelDto : ProductViewModelDto
    {
        public Product? Product { get; set; }
        public IList<Product>? ProductList { get; set; }
       


    }
}
