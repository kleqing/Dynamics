using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class ManageProjectMemberVM
{
    public IEnumerable<Dynamics.Models.Models.ProjectMember> ProjectMembers { get; set; }
    public PaginationRequestDto PaginationRequestDto { get; set; }
}