using Dynamics.Models.Models;
using Dynamics.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;

namespace Dynamics.DataAccess.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly AuthDbContext _authDbContext;

        public AdminRepository(ApplicationDbContext db, AuthDbContext authDbContext, UserManager<IdentityUser> userManager
        , IUserRepository userRepository)
        {
            _db = db;
            _authDbContext = authDbContext;
            this._userManager = userManager;
            _userRepository = userRepository;
        }

        // ---------------------------------------
        // Organization (View, Ban, Top 5)
        public async Task<Organization?> GetOrganization(Expression<Func<Organization, bool>> filter)
        {
            var org = await _db.Organizations.Where(filter).FirstOrDefaultAsync();
            if (org == null)
            {
                return null;
            }
            return org;
        }

        public async Task<bool> BanOrganizationById(Guid id)
        {
            var org = await GetOrganization(c => c.OrganizationID == id);
            if (org != null)
            {
                org.isBanned = !org.isBanned;
                await _db.SaveChangesAsync();
                return org.isBanned;
            }
            return org.isBanned;
        }

        public async Task<List<Organization>> GetTop5Organization()
        {
            var TopOrganizations = await _db.Projects
                .GroupBy(pm => pm.OrganizationID)                      // Group OrganizationID
                .Select(g => new
                {
                    OrganizationID = g.Key,
                    ProjectCount = g.Count()                   // Count if organization is in project or not
                })
                .OrderBy(x => x.ProjectCount)
                .Take(5)
                .ToListAsync();

            // Get the detailed information of the top 5 users
            var orgID = TopOrganizations.Select(x => x.OrganizationID).ToList();
            var organization = await _db.Organizations
                .Where(u => orgID.Contains(u.OrganizationID))
                .ToListAsync();

            foreach (var org in organization)
            {
                org.ProjectCount = TopOrganizations.FirstOrDefault(x => x.OrganizationID == org.OrganizationID)?.ProjectCount ?? 0;
            }
            return organization;
        }

        public async Task<List<Organization>> ViewOrganization()
        {
            var organizations = await _db.Organizations.ToListAsync();
            return organizations;
        }

        // ---------------------------------------
        // Request (View, Update)

        public async Task<List<Request>> ViewRequest()
        {
            var request = await _db.Requests.ToListAsync();
            return request;
        }

        public async Task<Request?> GetRequest(Expression<Func<Request, bool>> filter)
        {
            var request = await _db.Requests.Where(filter).FirstOrDefaultAsync();
            if (request == null)
            {
                return null;
            }
            return request;
        }

        public async Task<int> ChangeRequestStatus(Guid id)
        {
            var request = await GetRequest(r => r.RequestID == id);
            if (request != null)
            {
                switch (request.Status)
                {
                    case -1:
                        request.Status = 1;
                        break;
                    case 0:
                        request.Status = 1;
                        break;
                    case 1:
                        request.Status = -1;
                        break;
                }
                _db.Requests.Update(request);
                await _db.SaveChangesAsync();
                return request.Status;
            }
            return request.Status;
        }

        public async Task<Request> DeleteRequest(Guid id)
        {
            var request = await GetRequest(r => r.RequestID == id);
            if (request != null)
            {
                _db.Requests.Remove(request);
                await _db.SaveChangesAsync();
            }
            return request;
        }

        // ---------------------------------------
        // User (View, Ban, Top 5, allow access as admin)

        public async Task<List<User>> ViewUser()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<List<User>> GetTop5User()
        {
            // Group by UserID in ProjectMember to count the number of projects each user participates in
            var topUsers = await _db.ProjectMembers
                .GroupBy(pm => pm.UserID)    // Group by UserID
                .Select(g => new
                {
                    UserID = g.Key,
                    ProjectCount = g.Count()  // Count how many projects each user is in
                })
                .OrderByDescending(x => x.ProjectCount)  // Order by project count (desc)
                .Take(5)    // Take the top 5
                .ToListAsync();

            // Get the detailed information of the top 5 users
            var userIds = topUsers.Select(x => x.UserID).ToList();
            var users = await _db.Users
                .Where(u => userIds.Contains(u.UserID))  // Find users in the top list
                .ToListAsync();

            // Add the project count to each user (manually map the project count)
            foreach (var user in users)
            {
                user.ProjectCount = topUsers.FirstOrDefault(x => x.UserID == user.UserID)?.ProjectCount ?? 0;
            }

            return users;
        }

        public async Task<bool> BanUserById(Guid id)
        {
            var user = await GetUser(u => id == u.UserID);
            if (user != null)
            {
                user.isBanned = !user.isBanned;
                // If user is banned, remove admin role (change to user)
                if (user.isBanned)
                {
                    await _userRepository.AddToRoleAsync(id, RoleConstants.Banned);
                }
                else
                {
                    await _userRepository.AddToRoleAsync(id, RoleConstants.User);
                }
                await _db.SaveChangesAsync();
                return user.isBanned;  // Return ban value (true/false)
            }
            return user.isBanned;
        }

        public async Task<User?> GetUser(Expression<Func<User, bool>> filter)
        {
            var user = await _db.Users.Where(filter).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<string> GetUserRole(Guid id)
        {
            var authUser = await _userManager.FindByIdAsync(id.ToString());
            if (authUser == null) throw new Exception("GET ROLE FAILED: USER NOT FOUND");
            return _userManager.GetRolesAsync(authUser).GetAwaiter().GetResult().FirstOrDefault();
        }

        public async Task ChangeUserRole(Guid id)
        {
            var authUser = await _userManager.FindByIdAsync(id.ToString());
            var businessUser = await GetUser(u => u.UserID == id);

            var currentRoles = await _userManager.GetRolesAsync(authUser);
            string newRole = currentRoles.Contains(RoleConstants.Admin) ? RoleConstants.User : RoleConstants.Admin;

            // Remove current role and add the new one
            if (newRole == RoleConstants.Admin)
            {
                await _userManager.RemoveFromRoleAsync(authUser, RoleConstants.User);
                await _userManager.AddToRoleAsync(authUser, RoleConstants.Admin);
                businessUser.UserRole = RoleConstants.Admin;
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(authUser, RoleConstants.Admin);
                await _userManager.AddToRoleAsync(authUser, RoleConstants.User);
                businessUser.UserRole = RoleConstants.User;
            }

            await _db.SaveChangesAsync();
        }



        // ---------------------------------------
        // View Recent request (Recent item in dashoard page)
        public async Task<List<Request>> ViewRecentItem()
        {
            return await _db.Requests.Include(r => r.User).OrderByDescending(x => x.CreationDate).Take(7).ToListAsync();
        }

        // ---------------------------------------
        // Count (count number of user, organization, request, project in database)
        public async Task<int> CountUser()
        {
            return await _db.Users.CountAsync();
        }

        public async Task<int> CountOrganization()
        {
            return await _db.Organizations.CountAsync();
        }

        public async Task<int> CountRequest()
        {
            return await _db.Requests.CountAsync();
        }

        public async Task<int> CountProject()
        {
            return await _db.Projects.CountAsync();
        }

        // ---------------------------------------
        // Project

        public async Task<Project?> GetProjects(Expression<Func<Project, bool>> filter)
        {
            var project = await _db.Projects.Where(filter).FirstOrDefaultAsync();
            if (project == null)
            {
                return null;
            }
            return project;
        }

        public async Task<List<Project>> ViewProjects()
        {
            var project = await _db.Projects.ToListAsync();
            return project;
        }

        public async Task<bool> BanProject(Guid id)
        {
            var project = await GetProjects(r => r.ProjectID == id);
            if (project != null)
            {
                project.isBanned = !project.isBanned;
                project.ProjectStatus = project.isBanned ? -1 : 1;
                await _db.SaveChangesAsync();
                return project.isBanned;
            }
            return project.isBanned;
        }

        // ---------------------------------------
        // Report
        public async Task<List<Report>> ViewReport()
        {
            return await _db.Reports.Include(u => u.Reporter).ToListAsync();
        }

        // ---------------------------------------
    }
}
