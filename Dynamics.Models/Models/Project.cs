using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Models
{
    public class Project
    {
		public Guid ProjectID { get; set; }
		public Guid OrganizationID { get; set; }
        public Guid? RequestID { get; set; }

        [Required(ErrorMessage = "The Project Name field is required *")]
        [MaxLength(100, ErrorMessage = "Project Name length cannot be longer than 100 characters.")]
        public string ProjectName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string? ProjectEmail { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string? ProjectPhoneNumber { get; set; }

        [Required(ErrorMessage = "The Project Address field is required *")]
        public string ProjectAddress { get; set; }
        [Required]
        public int ProjectStatus { get; set; }
        [ValidateNever]
        public string? Attachment { get; set; }
        public string? ReportFile { get; set; }

        [Required(ErrorMessage = "The Project Description field is required *")]
        [MaxLength(1000, ErrorMessage = "Project Description cannot be longer than 1000 characters.")]
        public string ProjectDescription { get; set; }
        [DataType(DataType.Date)]
        public DateOnly? StartTime { get; set; }
        [DataType(DataType.Date)]
        public DateOnly? EndTime { get; set; }
        public string? ShutdownReason { get; set; }
        public bool isBanned { get; set; }
        public virtual Organization Organization { get; set; }
		public virtual Request Request { get; set; }
		public virtual ICollection<ProjectMember> ProjectMember { get; set; }
		public virtual ICollection<ProjectResource> ProjectResource { get; set; }
		public virtual ICollection<History> History { get; set; }
		public virtual ICollection<Withdraw> Withdraw { get; set; }
	}
}
