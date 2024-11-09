using System.ComponentModel.DataAnnotations;
using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class UserWalletVM
{
    public Wallet Wallet { get; set; }
    public List<UserWalletTransactionVM> UserWalletTransactionsVM { get; set; } = null;
    public SearchRequestDto SearchRequestDto { get; set; }
    public PaginationRequestDto PaginationRequestDto { get; set; }
    public Dictionary<string, string>? FilterOptions { get; set; } = new()
    {
        { "Filter", string.Empty },
        { "Show all", "" },
        { "Only top up transactions", "topup" },
        { "Only donate transactions", "donate" },
        { "Only refund transactions", "refund" },
    };
}

public class UserWalletTransactionVM
{
    public int Amount { get; set; }
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
    public DateTime Time { get; set; }
    public string TransactionType { get; set; } // topup, donate, refund
    public string? Message { get; set; }
}