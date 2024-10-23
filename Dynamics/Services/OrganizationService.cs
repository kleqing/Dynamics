using AutoMapper;
using Dynamics.Models.Models;
using Dynamics.Models.Models.Dto;
using Dynamics.Models.Models.DTO;

namespace Dynamics.Services;

public class OrganizationService: IOrganizationService
{
    private readonly IMapper _mapper;

    public OrganizationService(IMapper mapper)
    {
        _mapper = mapper;
    }
    public OrganizationOverviewDto MapToOrganizationOverviewDto(Organization organization)
    {
        var resultDto = _mapper.Map<OrganizationOverviewDto>(organization);
        // Get the leader name of the organization
        var leaderUser = organization.OrganizationMember.FirstOrDefault(om => om.Status == 2 && om.OrganizationID == organization.OrganizationID);
        if (leaderUser == null) throw new Exception("NO LEADER FOUND!");
        resultDto.OrganizationLeader = leaderUser.User;
        return resultDto;
    }
    /**
     * The organization needs to include up to organization member to use this one
     */
    public List<OrganizationOverviewDto> MapToOrganizationOverviewDtoList(List<Organization> organizations)
    {
        var resultDtos = new List<OrganizationOverviewDto>();
        foreach (var organization in organizations)
        {
            var resultDto = _mapper.Map<OrganizationOverviewDto>(organization);
            // Get the leader name of the organization
            var leaderUser = organization.OrganizationMember.FirstOrDefault(om => om.Status == 2 && om.OrganizationID == organization.OrganizationID);
            if (leaderUser == null) throw new Exception("NO LEADER FOUND!");
            // Map the member count as well
            resultDto.OrganizationMemberCount = organization.OrganizationMember.Count(org => org.OrganizationID == organization.OrganizationID);
            resultDto.OrganizationLeader = leaderUser.User;
            // Get only the first address (the city)
            if (organization.OrganizationAddress != null)
            {
                var location = organization.OrganizationAddress.Split(",");
                var city = location[0];
                if (location.Length >= 4)
                {
                    city = location[3];
                }
                resultDto.OrganizationAddress = city;
            }
            
            resultDtos.Add(resultDto);
        }
        return resultDtos;
    }
}