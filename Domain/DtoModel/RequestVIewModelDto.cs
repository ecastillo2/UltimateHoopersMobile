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
    public class RequestViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public RequestViewModelDto() { }

        // Existing constructor for mapping from Profile
        public RequestViewModelDto(Request request)
        {
            request.RequestId = RequestId;
            request.RunId = RunId;
            request.ProfileId = ProfileId;
            request.Status = Status;
            request.CreatedDate = CreatedDate;
     


        }

        public string? RequestId { get; set; }
        public string? RunId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
