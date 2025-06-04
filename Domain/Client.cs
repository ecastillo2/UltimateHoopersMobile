using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain
{
    public class Client
    {

        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public Client() { }
        // Existing constructor for mapping from ScoutingReport
        public Client(Client client)
        {
            ClientId = client.ClientId;
            ClientNumber = client.ClientNumber;
            Name = client.Name;
            Address = client.Address;
            State = client.State;
            City = client.City;
            Zip = client.Zip;
            PhoneNumber = client.PhoneNumber;
            CreatedDate = client.CreatedDate;
            
        }

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
        [NotMapped]
  
        public List<Court>? CourtList { get; set; }

        [NotMapped]

        public List<User>? UserList { get; set; }

    }
}
