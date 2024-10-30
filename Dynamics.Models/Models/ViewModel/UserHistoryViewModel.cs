using Dynamics.Models.Dto;
namespace Dynamics.Models.Models.ViewModel;

public class UserHistoryViewModel
{
    public List<UserTransactionDto> UserTransactions { get; set; }
    public SearchRequestDto SearchRequestDto { get; set; }
    public PaginationRequestDto PaginationRequestDto { get; set; }
    public readonly string[] FilterOptions = { "Filter", "Resource", "Money", "Organization", "Project" };
}