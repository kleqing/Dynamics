using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dynamics.DataAccess.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _accessor;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IProjectResourceRepository _projectResourceRepo;

        public ProjectRepository(ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IProjectMemberRepository projectMemberRepository,
            IProjectResourceRepository projectResourceRepository)
        {
            this._db = dbContext;
            this._accessor = httpContextAccessor;
            this._projectMemberRepo = projectMemberRepository;
            this._projectResourceRepo = projectResourceRepository;
        }

        //manage project profile


        //shut down
        public async Task<bool> ShutdownProjectAsync(ShutdownProjectVM entity)
        {
            var projectObj = _db.Projects.FirstOrDefault(x => x.ProjectID.Equals(entity.ProjectID));
            if (projectObj != null)
            {
                projectObj.ProjectStatus = -1;
                projectObj.ShutdownReason = entity.Reason;
                _db.Projects.Update(projectObj);
                await _db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> FinishProjectAsync(FinishProjectVM entity)
        {
            var projectObj = _db.Projects.FirstOrDefault(x => x.ProjectID.Equals(entity.ProjectID));
            if (projectObj != null)
            {
                projectObj.ProjectStatus = 2;
                projectObj.ReportFile = entity.ReportFile;
                _db.Projects.Update(projectObj);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Project>> GetAllAsync(Expression<Func<Project, bool>>? filter = null)
        {
            // We include these just in case we want to display all stuff
            if (filter != null)
            {
                return await _db.Projects.Include(pr => pr.ProjectResource)
                    .Where(filter)
                    .Include(pr => pr.ProjectMember).ThenInclude(u => u.User)
                    .AsSplitQuery()
                    .ToListAsync();
            }
            // Use split query if you are including a collection. tbh it is better to use a projection
            // Use split query if you are including a collection. tbh it is better to use a projection
            return await _db.Projects
                .Include(pr => pr.ProjectResource)
                .Include(pr => pr.ProjectMember).ThenInclude(u => u.User)
                .AsSplitQuery()
                .ToListAsync();
        }

        public IQueryable<Project> GetAllQueryable(Expression<Func<Project, bool>>? filter = null)
        {
            if (filter != null)
            {
                return _db.Projects.Include(pr => pr.ProjectResource)
                    .Where(filter)
                    .Include(pr => pr.ProjectMember)
                    .ThenInclude(pr => pr.User)
                    .Include(o => o.Organization)
                    .OrderByDescending(p => p.StartTime)
                    .AsQueryable()
                    .AsSplitQuery();
            }
            return _db.Projects.Include(pr => pr.ProjectResource)
                .Include(pr => pr.ProjectMember)
                .ThenInclude(pr => pr.User)
                .Include(o => o.Organization)
                .OrderByDescending(p => p.StartTime)
                .AsQueryable()
                .AsSplitQuery();
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            IQueryable<Project> projects = _db.Projects
                .Include(x => x.ProjectMember).ThenInclude(x => x.User)
                .Include(x => x.ProjectResource)
                .Include(x => x.Organization)
                .AsSplitQuery();
            return await projects.ToListAsync();
        }

        public Task<Project?> GetProjectAsync(Expression<Func<Project, bool>> filter)
        {
            var project = _db.Projects.Include(x => x.ProjectMember).ThenInclude(x => x.User)
                .Include(x => x.ProjectResource)
                .Include(x => x.Organization)
                .Include(x => x.Request).ThenInclude(x => x.User)
                .Include(x => x.History).Where(filter)
                .AsSplitQuery();
            return project.FirstOrDefaultAsync();
        }
        public async Task<bool> UpdateAsync(Project entity)
        {
            var existingItem = await GetProjectAsync(u => entity.ProjectID.Equals(u.ProjectID));
            if (existingItem == null)
            {
                return false;
            }
            existingItem.ProjectName = entity.ProjectName;
            existingItem.ProjectDescription = entity.ProjectDescription;
            existingItem.Attachment = entity.Attachment;
            existingItem.ProjectAddress = entity.ProjectAddress;
            existingItem.ProjectEmail = entity.ProjectEmail;
            existingItem.ProjectPhoneNumber = entity.ProjectPhoneNumber;
            existingItem.StartTime = entity.StartTime;
            existingItem.EndTime = entity.EndTime;
            existingItem.ProjectStatus = entity.ProjectStatus;
            _db.Projects.Update(existingItem);
            await _db.SaveChangesAsync();
            return true;
        }

        //Repo of Tuấn

        public async Task<bool> AddProjectAsync(Project project)
        {
            try
            {
                _db.Projects.Add(project);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<List<Project>> GetAllProjectsByOrganizationIDAsync(Expression<Func<Project, bool>> filter)
        {
            var projects = await _db.Projects.Where(filter).ToListAsync();
            return projects;
        }

        public async Task<bool> AddProjectMemberAsync(ProjectMember projectMember)
        {
            try
            {
                _db.ProjectMembers.Add(projectMember);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> AddProjectResourceAsync(ProjectResource projectResource)
        {
            try
            {
                _db.ProjectResources.Add(projectResource);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Project> GetProjectByProjectIDAsync(Expression<Func<Project, bool>> filter)
        {
            var project = await _db.Projects.Where(filter).FirstOrDefaultAsync();
            return project;
        }

        public async Task<List<ProjectResource>> GetAllResourceByProjectIDAsync(Expression<Func<ProjectResource, bool>> filter)
        {
            var projectRsources = await _db.ProjectResources.Where(filter).ToListAsync();
            return projectRsources;
        }

        public async Task<ProjectResource> GetProjectResourceByResourceIDAsync(Expression<Func<ProjectResource, bool>> filter)
        {
            var projectResource = await _db.ProjectResources.Where(filter).FirstOrDefaultAsync();
            return projectResource;
        }

        public async Task<bool> UpdateProjectResource(ProjectResource entity)
        {
            var projectResourceItem = await GetProjectResourceByResourceIDAsync(p => p.ResourceID.Equals(entity.ResourceID));

            if (projectResourceItem == null)
            {
                return false;
            }
            _db.Entry(projectResourceItem).CurrentValues.SetValues(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
