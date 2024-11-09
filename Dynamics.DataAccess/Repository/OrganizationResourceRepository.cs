using System.Linq.Expressions;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository;

public class OrganizationResourceRepository : IOrganizationResourceRepository
{
    private readonly ApplicationDbContext _context;

    public OrganizationResourceRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public Task<List<OrganizationResource>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<OrganizationResource?> GetAsync(Expression<Func<OrganizationResource, bool>> predicate)
    {
        return await _context.OrganizationResources.FirstOrDefaultAsync(predicate);
    }

    public Task<bool> CreateAsync(OrganizationResource organizationResource)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(OrganizationResource organizationResource)
    {
        var existing = await GetAsync(or => or.ResourceID == organizationResource.ResourceID);
        _context.Entry(existing).CurrentValues.SetValues(organizationResource);
        await _context.SaveChangesAsync();
    }

    public Task<OrganizationResource> DeleteAsync(Expression<Func<OrganizationResource, bool>> predicate)
    {
        throw new NotImplementedException();
    }
}