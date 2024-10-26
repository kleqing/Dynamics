using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class ManageOrganizationTransactionHistoryVM
{
    public IEnumerable<OrganizationTransactionDto> Transactions { get; set; }
    public SearchRequestDto SearchRequestDto { get; set; }
    public PaginationRequestDto PaginationRequestDto { get; set; }
    public readonly string[] FilterOptions = { "Filter", "Resource", "Money", "User", "Project", "Accepted", "Denied", "Pending" };
}