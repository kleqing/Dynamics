using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models
{
    public class OrganizationToProjectHistory
    {
		public Guid TransactionID { get; set; }
        public Guid OrganizationResourceID { get; set; }
        public Guid? ProjectResourceID { get; set; }
        public int Status { get; set; }
        public int Amount { get; set; }
        [MaxLength(100, ErrorMessage = "Message cannot exceed 100 characters.")]
        public string? Message { get; set; }
		[DataType(DataType.DateTime)]
		public DateOnly Time { get; set; }

		// Can be null bc money don't need attachments
		public string? Attachments { get; set; } 
        public virtual OrganizationResource OrganizationResource { get; set; }
		public virtual ProjectResource ProjectResource { get; set; }
	}
}
