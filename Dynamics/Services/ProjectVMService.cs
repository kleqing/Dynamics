using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dynamics.Services
{
   

    public class ProjectVMService : IProjectVMService
    {
        private readonly ApplicationDbContext _db;

        public ProjectVMService(ApplicationDbContext db) { _db = db; }

        public async Task<Project> GetProjectAsync(Expression<Func<Project, bool>> filter)
        {
            var result = await _db.Projects.Where(filter)
                .Include(p => p.ProjectResource)
                .Select(p => new Project
                {
                    ProjectID = p.ProjectID,
                    ProjectName = p.ProjectName,
                    ProjectDescription = p.ProjectDescription,
                    ProjectResource = p.ProjectResource.ToList(),
                })
                .FirstOrDefaultAsync();
            return result;
        }


    }
}
