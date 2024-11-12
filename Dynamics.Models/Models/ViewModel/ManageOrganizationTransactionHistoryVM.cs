using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class ManageOrganizationTransactionHistoryVM
{
    public IEnumerable<OrganizationTransactionDto> UserTransactions { get; set; }
    public IEnumerable<OrganizationTransactionDto> OrganizationTransactions { get; set; }
    // This one is for user table
    public SearchRequestDto UserSearchRequestDto { get; set; }
    public PaginationRequestDto UserPaginationRequestDto { get; set; }
    // This one is for organization table
    public SearchRequestDto OrganizationSearchRequestDto { get; set; }
    public PaginationRequestDto OrganizationPaginationRequestDto { get; set; }
    
    // Both will use the same filter
    public Dictionary<string, string> FilterOptions { get; set; } = new()
    {
        { "Show all", "All" },
        { "Only resource donations", "Resource" },
        { "Only money donations", "Money" },
        { "Only accepted donations", "Accepted" },
        { "Only denied donations", "Denied" },
    };
}