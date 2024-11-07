using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace Dynamics.Areas.Admin.Models
{
	public class TransactionBase
	{
		public Guid WithdrawID { get; set; }
		public Guid ProjectResourceID { get; set; }
		public DateTime Time { get; set; }
		public string? Message { get; set; }
		public string Received { get; set; }
		public string Description { get; set; }
	}
}
