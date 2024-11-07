using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models.ViewModel
{
    public class ProjectVM
    {

        public Guid ProjectID { get; set; }
        public Guid OrganizationID { get; set; }
        public Guid? RequestID { get; set; }
        [Required]
        [Display(Name = "Project name")]
        public string ProjectName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string ProjectEmail { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string ProjectPhoneNumber { get; set; }

        // [Required(ErrorMessage = "The Project Address field is required *")]
        public string ProjectAddress { get; set; }
        public int ProjectStatus { get; set; }
        public string? Attachment { get; set; }
        [Required(ErrorMessage = "The project description field is required *")]
        [MaxLength(1000, ErrorMessage = "The project description cannot be longer than 1000 characters.")]
        public string ProjectDescription { get; set; }
        [DataType(DataType.Date)]
        public DateOnly? StartTime { get; set; }
        [DataType(DataType.Date)]
        public DateOnly? EndTime { get; set; }
        public string shutdownReanson { get; set; }
        public Guid LeaderID { get; set; }
        public OrganizationVM OrganizationVM { get; set; }
    }
}
