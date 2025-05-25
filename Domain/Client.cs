using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Client
    {
        [Key]
        public string? ClientId { get; set; }
        public string? ClientNumber { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}
