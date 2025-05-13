using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class ProjectManagement
    {
        [Key]
        public string? ProjectManagementId { get; set; }
        public string? Url { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
       

    }
}
