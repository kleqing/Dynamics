using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Models;

public class UserWalletTransaction
{
    [Key]
    public Guid TransactionId { get; set; }
    [Required]
    public int Amount { get; set; }
    // public Guid ToUserId { get; set; }
    [MaxLength(500)]
    public string? Message { get; set; }
    [Required]
    public string TransactionType { get; set; } // Is refund or not
    [Required]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
    public DateTime Time { get; set; }
    [Required]
    public Guid WalletId { get; set; }
    public virtual Wallet Wallet { get; set; }
}