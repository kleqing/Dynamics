using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace Dynamics.Models.Models
{
	public class Withdraw
	{
		public Guid WithdrawID { get; set; }
		public Guid ProjectID { get; set; }
		public int Status { get; set; }
		[MaxLength(500)]
		public string BankAccountNumber { get; set; }
		public string BankName { get; set; }
		public string? Message { get; set; }
		[Required]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
		public DateTime Time { get; set; } = DateTime.Now;
		public virtual Project Project { get; set; }
	}
}

