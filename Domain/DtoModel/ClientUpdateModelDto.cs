using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class ClientUpdateModelDto
    {
        public string? ClientId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public string? PhoneNumber { get; set; }

        public void UpdateClient(Client client)
        {
            client.Name = Name;
            client.Address = Address;
            client.State = State;
            client.City = City;
            client.Zip = Zip;
            client.PhoneNumber = PhoneNumber;
           
        }
    }
}
