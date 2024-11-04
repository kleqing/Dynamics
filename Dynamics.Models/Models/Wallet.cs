using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Models;

public class Wallet
{
    // Default wallet currency is VND (The same as real life VND)
    [Key]
    public Guid WalletId { get; set; }
    [Required]
    public int Amount { get; set; }
    [MaxLength(3, ErrorMessage = "Currency maximum length is 3")]
    public string Currency { get; set; } = "VND";
    [Required]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<UserWalletTransaction> WalletTransactions { get; set; }
}