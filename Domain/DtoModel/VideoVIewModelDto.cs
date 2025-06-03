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
    public class VideoViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public VideoViewModelDto() { }

        // Existing constructor for mapping from Profile
        public VideoViewModelDto(Video video)
        {
            video.VideoId = VideoId;
            video.ClientId = ClientId;
            video.VideoURL = VideoURL;
            video.VideoName = VideoName;
            video.Status = Status;
            video.VideoDate = VideoDate;
           
        }

        public string? VideoId { get; set; }
        public string? ClientId { get; set; }
        public string? VideoURL { get; set; }
        public string? VideoName { get; set; }
        public string? Status { get; set; }
        public DateTime? VideoDate { get; set; }

    }
}
