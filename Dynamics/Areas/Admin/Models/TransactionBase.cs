using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace Dynamics.Areas.Admin.Models
{
	public class TransactionBase
	{
		public Guid TransactionID { get; set; }
		public DateOnly Time { get; set; }
		public string Message { get; set; }
		public string Type { get; set; }
		public string Sender { get; set; }
	}
}
