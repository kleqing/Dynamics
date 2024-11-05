using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class UserRequestsStatusViewModel
{
    public List<OrganizationMember> OrganizationJoinRequests { get; set; }
    public List<ProjectMember> ProjectJoinRequests { get; set; }
    public List<UserTransactionDto> ResourcesDonationRequests { get; set; }

    public PaginationRequestDto PaginationRequestDto { get; set; }
    public SearchRequestDto SearchRequestDto { get; set; }
    public Dictionary<string, string> FilterOptions { get; set; } = new()
    {
        { "Filter", string.Empty },
        { "Show all", "All" },
        { "Only organization donations", "Organization" },
        { "Only project donations", "Project" },
        { "Only pending donations", "Pending" },
        { "Only denied donations", "Denied" },
    };
}