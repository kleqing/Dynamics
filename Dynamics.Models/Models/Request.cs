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
		[Required (ErrorMessage = "Please enter the request description.")]
		public string Content { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? CreationDate { get; set; }
        [Required(ErrorMessage = "The title of the request is required.")]
        public string RequestTitle {  get; set; }
        [Required(ErrorMessage = "Please enter the request email address. This will be use to contact the requester.")]
        [DataType(DataType.EmailAddress)]
        public string? RequestEmail { get; set; }
        [Required(ErrorMessage = "Please enter the request phone number. This will be use to contact the requester.")]
        [DataType(DataType.PhoneNumber)]
        public string? RequestPhoneNumber { get; set; }
        [Required(ErrorMessage = "Please enter the request location. It is essential for the volunteer operation.")]
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
