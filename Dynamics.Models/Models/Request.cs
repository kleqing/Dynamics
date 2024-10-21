using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dynamics.Models.Models
{
    public class Request
    {
		public Guid RequestID { get; set; }
		public Guid UserID { get; set; }
		[Required]
		public string Content { get; set; }
        [DataType(DataType.Date)]
        public DateOnly? CreationDate { get; set; }
        [Required]
        public string RequestTitle {  get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? RequestEmail { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string? RequestPhoneNumber { get; set; }
        [Required]
        public string Location { get; set; }
		public string? Attachment { get; set; }
		public int? isEmergency { get; set; }
		public int Status { get; set; }
		[ValidateNever]
		public virtual User User { get; set; }
		[ValidateNever]
		public virtual Project Project { get; set; }
	}
}
