using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models
{
    public class UserToProjectTransactionHistory
    {
		
		public Guid TransactionID { get; set; }
        public Guid ProjectResourceID { get; set; }
        public Guid UserID { get; set; }
		public int Status { get; set; }
		[Required]
		public int Amount { get; set; }

        [MaxLength(100, ErrorMessage = "Message cannot exceed 100 characters.")]
        public string? Message { get; set; }
        [DataType(DataType.Date)]
        public DateOnly Time { get; set; }
        // Can be null bc money don't need attachments
        public string? Attachments { get; set; } 
        public virtual User User { get; set; }
		public virtual ProjectResource ProjectResource { get; set; }
	}
}
