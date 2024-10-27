using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class HomepageViewModel
{
    public List<RequestOverviewDto> Requests { get; set; } = new List<RequestOverviewDto>();
    public List<ProjectOverviewDto> Projects { get; set; } = new List<ProjectOverviewDto>();
    // public List<OrganizationOverviewDto> Organizations { get; set; }
    public List<ProjectOverviewDto> SuccessfulProjects { get; set; } = new List<ProjectOverviewDto>();
    public List<OrganizationOverviewDto> Organizations { get; set; } = new List<OrganizationOverviewDto>();
}