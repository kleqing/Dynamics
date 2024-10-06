using System.Linq.Expressions;
using Dynamics.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.DataAccess.Repository;

public interface IOrganizationRepository
{
    // Task<IEnumerable<Organization>> GetAllAsync();
    // Task<Organization> GetAsync(Expression<Func<Organization, bool>> predicate);
    // Task<bool> CreateAsync(Organization project);
    // Task<bool> UpdateAsync(Organization project);
    // Task<Organization> DeleteAsync(Expression<Func<Organization, bool>> predicate);
    
    Task<List<Organization>> GetAllOrganizationsAsync();
    IQueryable<Organization> GetAll();
    Task<Organization> GetOrganizationUserLead(Guid userId);
    Task<Guid> GetOrgResourceIDCorresponding(Guid projectResourceID,Guid organizationUserLeadID);
}