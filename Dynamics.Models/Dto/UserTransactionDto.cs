using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Models.ViewModel;

public class UserTransactionDto
{
    public Guid TransactionID { get; set; }
    public User User { get; set; }
    public string Unit { get; set; }
    public int Amount { get; set; }
    public string ResourceName { get; set; }
    public string? Message { get; set; }
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly Time { get; set; }
    public int Status { get; set; }
    public string Target { get; set; }
    public string Type { get; set; }
}