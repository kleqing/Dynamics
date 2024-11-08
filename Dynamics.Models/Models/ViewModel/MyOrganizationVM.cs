using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class MyOrganizationVM
{
    public List<OrganizationOverviewDto> MyOrg { get; set; }
    public List<OrganizationOverviewDto> JoinedOrgs { get; set; }
}