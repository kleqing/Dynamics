namespace Dynamics.Models.Dto;

public class VnPayCreatePaymentDto
{
    public Guid FromId { get; set; }
    public int Amount { get; set; }
    public DateTime Time{ get; set; }
    public Guid TransactionId { get; set; }
    public string Message { get; set; }
    
}