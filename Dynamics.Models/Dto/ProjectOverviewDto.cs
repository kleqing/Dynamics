using Dynamics.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Dto;

public class ProjectOverviewDto
{
    public Guid ProjectID { get; set; }
    public User? ProjectLeader { get; set; }
    public string ProjectName { get; set; }
    public string? ProjectAddress { get; set; }
    public int ProjectMembers { get; set; }
    public int? ProjectProgress { get; set; }
    public int ProjectRaisedMoney { get; set; }
    public int ProjectStatus { get; set; }
    public Organization Organization { get; set; }
    public string? Attachment { get; set; }
    public string ProjectDescription { get; set; }
    [DataType(DataType.Date)]
    public DateOnly? StartTime { get; set; }
    [DataType(DataType.Date)]
    public DateOnly? EndTime { get; set; }
}