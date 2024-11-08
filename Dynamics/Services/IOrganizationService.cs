using Dynamics.Models.Models;
using Dynamics.Models.Dto;


namespace Dynamics.Services;

public interface IOrganizationService
{
    OrganizationOverviewDto MapToOrganizationOverviewDto(Organization organization);
    List<OrganizationOverviewDto> MapToOrganizationOverviewDtoList(List<Organization> organizations);

}