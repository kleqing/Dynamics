using System.ComponentModel.DataAnnotations;
using Dynamics.Models.Models;
using Microsoft.SqlServer.Server;

namespace Dynamics.Models.Dto;

public class OrganizationOverviewDto
{
    public Guid OrganizationID { get; set; }
    public User OrganizationLeader { get; set; }
    public string OrganizationName { get; set; }
    public string? OrganizationAddress { get; set; }
    public string? OrganizationDescription { get; set; }
    public string? OrganizationPictures { get; set; }
    public int OrganizationMemberCount { get; set; }
    public int OrganizationStatus { get; set; }
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly StartTime { get; set; }
    public DateOnly ShutdownDay { get; set; }
    public virtual ICollection<OrganizationMember> OrganizationMember { get; set; }

}