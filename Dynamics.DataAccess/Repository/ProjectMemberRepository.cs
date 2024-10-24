using System.Linq.Expressions;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository;

public class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectMemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectMember>> GetAllAsync(Expression<Func<ProjectMember, bool>>? predicate = null)
    {
        if (predicate is null)
        {
            return await _context.ProjectMembers.Include(p => p.Project).ToListAsync();
        }
        return await _context.ProjectMembers.Where(predicate).Include(p => p.Project).ToListAsync();
    }

    public async Task<ProjectMember?> GetAsync(Expression<Func<ProjectMember, bool>>? predicate)
    {
        if (predicate is null)
        {
            return await _context.ProjectMembers.Include(pm => pm.User).Include(pm => pm.Project).FirstOrDefaultAsync();
        }
        return await _context.ProjectMembers.Where(predicate).Include(pm => pm.User).Include(pm => pm.Project).FirstOrDefaultAsync();
    }

    public Task<bool> CreateAsync(ProjectMember project)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateAsync(ProjectMember project)
    {
        _context.ProjectMembers.Update(project);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProjectMember> DeleteAsync(Expression<Func<ProjectMember, bool>> predicate)
    {
        var target = await GetAsync(predicate);
        if (target is null) return null;
        var final = _context.ProjectMembers.Remove(target);
        await _context.SaveChangesAsync();
        return final.Entity;
    }
    public List<ProjectMember> FilterProjectMember(Expression<Func<ProjectMember, bool>> filter)
    {
        IQueryable<ProjectMember> listProjectMember = _context.ProjectMembers.Include(x => x.User).Where(filter);
        if (listProjectMember != null)
        {
            return listProjectMember.ToList();
        }

        return null;
    }

    //------manage member request------------------

    public async Task<bool> AddJoinRequest(Guid memberID, Guid projectID)
    {
        _context.ChangeTracker.Clear();
        var newProjectMember = new ProjectMember();
        var res = await _context.ProjectMembers.AddAsync(newProjectMember);
        newProjectMember.ProjectID = projectID;
        newProjectMember.UserID = memberID;
        newProjectMember.Status = 0;
        await _context.SaveChangesAsync();
        return res != null;
    }

    public async Task<bool> AcceptJoinRequestAsync(Guid memberID, Guid projectID)
    {
        var memberObj =
            await _context.ProjectMembers.FirstOrDefaultAsync(x =>
                x.UserID.Equals(memberID) && x.ProjectID.Equals(projectID));
        if (memberObj != null)
        {
            memberObj.Status = 1;
            _context.ProjectMembers.Update(memberObj);
            _context.Entry(memberObj).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> DenyJoinRequestAsync(Guid memberID, Guid projectID)
    {
        var memberObj =
            await _context.ProjectMembers.FirstOrDefaultAsync(x =>
                x.UserID.Equals(memberID) && x.ProjectID.Equals(projectID));
        if (memberObj != null)
        {
            memberObj.Status = -1;
            _context.ProjectMembers.Update(memberObj);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}