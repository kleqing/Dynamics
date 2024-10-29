using Dynamics.Models.Models;
using Dynamics.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamics.DataAccess.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        public AdminRepository(ApplicationDbContext db, UserManager<User> userManager
        , IUserRepository userRepository)
        {
            _db = db;
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

        // Change organization status
        public async Task<int> ChangeOrganizationStatus(Guid id)
        {
            var org = await GetOrganization(c => c.OrganizationID == id);
            if (org != null)
            {
                switch (org.OrganizationStatus)
                {
                    case -1: // Cancel
                        org.OrganizationStatus = 1; // Change to active
                        break;
                    case 0: // Pending
                        org.OrganizationStatus = 1; // Change to active
                        break;
                    case 1: // Active
                        org.OrganizationStatus = -1; // Change to cancel
                        break;
                }
                _db.Organizations.Update(org); // Update to db
                await _db.SaveChangesAsync();
                return org.OrganizationStatus;
            }
            return org.OrganizationStatus;
        }

        public async Task<List<Organization>> GetTop5Organization()
        {
            var TopOrganizations = await _db.Projects
                .GroupBy(pm => pm.OrganizationID)                      // Group OrganizationID
                .Select(g => new
                {
                    OrganizationID = g.Key,                     // Get OrganizationID
                    ProjectCount = g.Count()                   // Count if organization is in project or not
                })
                .OrderBy(x => x.ProjectCount) // Order by project count (asc)
                .Take(5) // Take the top 5
                .ToListAsync();

            // Get the detailed information of the top 5 users
            var orgID = TopOrganizations.Select(x => x.OrganizationID).ToList();
            var organization = await _db.Organizations
                .Where(u => orgID.Contains(u.OrganizationID))
                .ToListAsync();

            foreach (var org in organization)
            {
                org.ProjectCount = TopOrganizations.FirstOrDefault(x => x.OrganizationID == org.OrganizationID)?.ProjectCount ?? 0; // Based on ID, get the project count
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

        // Change request status, function is same as 
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


        // Get choosen request information
        public async Task<Request> GetRequestInfo(Expression<Func<Request, bool>> expression)
        {
            return await _db.Requests.Where(expression)
                .Select(
                r => new Request
                {
                    RequestID = r.RequestID,
                    RequestTitle = r.RequestTitle,
                    Content = r.Content,
                    Location = r.Location,
                    RequestEmail = r.RequestEmail,
                    RequestPhoneNumber = r.RequestPhoneNumber,
                    CreationDate = r.CreationDate,
                    Attachment = r.Attachment
                })
                .FirstOrDefaultAsync();
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
                .Where(u => userIds.Contains(u.Id))  // Find users in the top list
                .ToListAsync();

            // Add the project count to each user (manually map the project count)
            foreach (var user in users)
            {
                user.ProjectCount = topUsers.FirstOrDefault(x => x.UserID == user.Id)?.ProjectCount ?? 0;
            }

            return users;
        }

        public async Task<bool> BanUserById(Guid id)
        {
            var user = await GetUser(u => id == u.Id);
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

        // Change user role in both Auth and Main database
        public async Task ChangeUserRole(Guid id)
        {
            var authUser = await _userManager.FindByIdAsync(id.ToString());
            var businessUser = await GetUser(u => u.Id == id);

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

        public async Task<bool> BanProject(Guid id) // Ban project using ajax
        {
            var project = await GetProjects(r => r.ProjectID == id);
            if (project != null)
            {
                project.isBanned = !project.isBanned;
                project.ProjectStatus = project.isBanned ? -1 : 1; // If project is banned, change status to -1 (cancel). Otherwise, change to 1 (active)
                await _db.SaveChangesAsync();
                return project.isBanned;
            }
            return project.isBanned;
        }

        public async Task<Project?> GetProjectInfo(Expression<Func<Project, bool>> filter)
        {
            var list = await _db.Projects.Where(filter).Include(p => p.ProjectMember).ThenInclude(u => u.User).FirstOrDefaultAsync();
            if (list == null)
            {
                return null;
            }
            return list;
        }

        // ---------------------------------------
        // Report
        public async Task<List<Report>> ViewReport()
        {
            return await _db.Reports.Include(u => u.Reporter).ToListAsync();
        }

        // ---------------------------------------
        // Payment
        
        // User to project transaction history
        public async Task<List<UserToProjectTransactionHistory>> ViewUserToProjectTransactionInHistory(Expression<Func<UserToProjectTransactionHistory, bool>> filter)
        {
            var list =  await _db.UserToProjectTransactionHistories.Where(filter).Include(u => u.User)
                .Include(r => r.ProjectResource)
                .ToListAsync();
            if (list == null)
            {
                return null;
            }

            return list;
        }
        
        // Organization to project transaction history
        public async Task<List<OrganizationToProjectHistory>> ViewOrganizationToProjectTransactionHistory(Expression<Func<OrganizationToProjectHistory, bool>> filter)
        {
            var list = await _db.OrganizationToProjectTransactionHistory.Where(filter).Include(o => o.OrganizationResource)
                .ThenInclude(or => or.Organization)
                .Include(p => p.ProjectResource)
                .ThenInclude(pr => pr.Project)
                .ToListAsync();
            if (list == null)
            {
                return null;
            }
            return list;

        }

        public async Task<List<UserToOrganizationTransactionHistory>> ViewUserToOrganizationTransactionHistory(
            Expression<Func<UserToOrganizationTransactionHistory, bool>> filter)
        {
            var list = await _db.UserToOrganizationTransactionHistories.Where(filter)
                .Include(u => u.User)
                .Include(or => or.OrganizationResource)
                .ThenInclude(o => o.Organization)
                .ToListAsync();
            if (list == null)
            {
                return null;
            }

            return list;
        }
    }
}
