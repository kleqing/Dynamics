using System.Linq.Expressions;
using Dynamics.Models.Models;

namespace Dynamics.DataAccess.Repository;

public interface IOrganizationMemberRepository
{
    Task<List<OrganizationMember>> GetAllAsync(Expression<Func<OrganizationMember, bool>>? predicate = null);
    Task<OrganizationMember?> GetAsync(Expression<Func<OrganizationMember, bool>> predicate);
    Task<bool> CreateAsync(OrganizationMember project);
    Task<bool> UpdateAsync(OrganizationMember project);
    Task<OrganizationMember> DeleteAsync(Expression<Func<OrganizationMember, bool>> predicate);
}

