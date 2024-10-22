using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Models.Dto;

public class OrganizationOverviewDto
{
    public User OrganizationLeader { get; set; }
    public string OrganizationName { get; set; }
    public string? OrganizationAddress { get; set; }
    public string? OrganizationDescription { get; set; }
    public string? OrganizationPictures { get; set; }
    public int OrganizationMemberCount { get; set; }
}