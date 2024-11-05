using System.Linq.Expressions;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository;

public class OrganizationMemberRepository : IOrganizationMemberRepository
{
    private readonly ApplicationDbContext _context;

    public OrganizationMemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<OrganizationMember>> GetAllAsync(Expression<Func<OrganizationMember, bool>>? predicate = null)
    {
        if (predicate is null)
        {
            return await _context.OrganizationMember.Include(om => om.Organization).ToListAsync();
        }
        else
        {
            return await _context.OrganizationMember.Where(predicate).Include(om => om.Organization).ToListAsync();
        }
    }

    public async Task<OrganizationMember?> GetAsync(Expression<Func<OrganizationMember, bool>> predicate)
    {
        return await _context.OrganizationMember
            .Include(om => om.Organization)
            .FirstOrDefaultAsync(predicate);
    }

    public Task<bool> CreateAsync(OrganizationMember project)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(OrganizationMember project)
    {
        throw new NotImplementedException();
    }

    public async Task<OrganizationMember> DeleteAsync(Expression<Func<OrganizationMember, bool>> predicate)
    {
        var target = await GetAsync(predicate);
        if (target is null) return null;
        var final = _context.OrganizationMember.Remove(target);
        await _context.SaveChangesAsync();
        return final.Entity;
    }
}