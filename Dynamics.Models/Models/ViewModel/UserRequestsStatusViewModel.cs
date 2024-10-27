using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class UserRequestsStatusViewModel
{
    public List<OrganizationMember> OrganizationJoinRequests { get; set; }
    public List<ProjectMember> ProjectJoinRequests { get; set; }
    public List<UserTransactionDto> ResourcesDonationRequests { get; set; }
    public PaginationRequestDto PaginationRequestDto { get; set; }
    public SearchRequestDto SearchRequestDto { get; set; }
    public readonly string[] FilterOptions = { "Filter", "Organization", "Project", "Pending", "Denied" };
}