using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamics.Models.Models
{
    public class Organization
    {
        [Required]
        public Guid OrganizationID { get; set; }

        [Required(ErrorMessage = "The Organization Name field is required *")]
        [MaxLength(100, ErrorMessage = "Organization Name length cannot be longer than 100 characters.")]
		public string OrganizationName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string? OrganizationEmail { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string? OrganizationPhoneNumber { get; set; }
        public string? OrganizationAddress { get; set; }

        [Required(ErrorMessage = "The Organization Name field is required *")]
        public string OrganizationDescription { get; set; }
		public string? OrganizationPictures { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateOnly StartTime { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateOnly? ShutdownDay { get; set; }
        public bool isBanned { get; set; }
        [NotMapped]
        public int ProjectCount { get; set; }
        public virtual ICollection<Project> Project { get; set; }
		public virtual ICollection<OrganizationMember> OrganizationMember { get; set; }
		public virtual ICollection<OrganizationResource> OrganizationResource { get; set; }
	}
}