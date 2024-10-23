using System.Linq.Expressions;
using Dynamics.Models.Models;

namespace Dynamics.DataAccess.Repository;

public interface IProjectMemberRepository
{
    Task<List<ProjectMember>> GetAllAsync(Expression<Func<ProjectMember, bool>>? predicate = null);
    Task<ProjectMember?> GetAsync(Expression<Func<ProjectMember, bool>> predicate);
    Task<bool> CreateAsync(ProjectMember project);
    Task<bool> UpdateAsync(ProjectMember project);
    Task<ProjectMember> DeleteAsync(Expression<Func<ProjectMember, bool>> predicate);
    List<ProjectMember> FilterProjectMember(Expression<Func<ProjectMember, bool>> filter);
    //manage member request-huyen
    Task<bool> AddJoinRequest(ProjectMember p);
    Task<bool> AcceptJoinRequestAsync(Guid memberID, Guid projectID);

    Task<bool> DenyJoinRequestAsync(Guid memberID, Guid projectID);
}