using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class UserHistoryViewModel
{
    public List<UserTransactionDto> UserTransactions { get; set; }
    public SearchRequestDto SearchRequestDto { get; set; }
    public PaginationRequestDto PaginationRequestDto { get; set; }
    /**
     * The value is the one that will get sent
     */
    public Dictionary<string, string> FilterOptions { get; set; } = new()
    {
        { "Filter", string.Empty },
        { "Show all", "All" },
        { "Only resource donations", "Resource" },
        { "Only money donations", "Money" },
        { "Only organization donations", "Organization" },
        { "Only project donations", "Project" },
    };
    // public readonly string[] FilterOptions = { "Filter", "Resource", "Money", "Organization", "Project" };
}