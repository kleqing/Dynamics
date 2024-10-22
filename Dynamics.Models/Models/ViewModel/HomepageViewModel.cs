using Dynamics.Models.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class HomepageViewModel
{
    public List<RequestOverviewDto>? Requests { get; set; }
    public List<ProjectOverviewDto>? Projects { get; set; }
    // public List<OrganizationOverviewDto> Organizations { get; set; }
    public List<ProjectOverviewDto>? SuccessfulProjects { get; set; }
    public List<OrganizationOverviewDto>? Organizations { get; set; }
}