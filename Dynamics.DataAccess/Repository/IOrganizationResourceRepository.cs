using System.Linq.Expressions;
using Dynamics.Models.Models;

namespace Dynamics.DataAccess.Repository;

public interface IOrganizationResourceRepository
{
    Task<List<OrganizationResource>> GetAllAsync();
    Task<OrganizationResource?> GetAsync(Expression<Func<OrganizationResource, bool>> predicate);
    Task<bool> CreateAsync(OrganizationResource organizationResource);
    Task UpdateAsync(OrganizationResource organizationResource);
    Task<OrganizationResource> DeleteAsync(Expression<Func<OrganizationResource, bool>> predicate);
}