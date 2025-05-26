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
    public class ClientViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public ClientViewModelDto() { }

        // Existing constructor for mapping from Profile
        public ClientViewModelDto(Client privateRun)
        {
            ClientId = privateRun.ClientId;
            ClientNumber = privateRun.ClientNumber;
            Address = privateRun.Address;
            State = privateRun.State;
            City = privateRun.City;
            Zip = privateRun.Zip;
            PhoneNumber = privateRun.PhoneNumber;
            CreatedDate = privateRun.CreatedDate;
            

        }

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
