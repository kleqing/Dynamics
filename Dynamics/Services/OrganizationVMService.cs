using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamics.Services
{
    public class OrganizationVMService : IOrganizationVMService
    {
        private readonly ApplicationDbContext _db;

        public OrganizationVMService(ApplicationDbContext db)
        {
            _db = db;
        }

        //for Organization Details
        public async Task<OrganizationVM> GetOrganizationVMAsync(Expression<Func<Organization, bool>> filter)
        {
            var result = await _db.Organizations.Where(filter)
                                    .Include(o => o.OrganizationMember)
                                        .ThenInclude(om => om.User)
                                    .Include(o => o.Project)
                                        .ThenInclude(p => p.ProjectResource)
                                    .Include(o => o.OrganizationResource)
                                    .Select(o => new OrganizationVM
                                    {
                                        OrganizationID = o.OrganizationID,
                                        OrganizationStatus = o.OrganizationStatus,
                                        OrganizationName = o.OrganizationName,
                                        OrganizationEmail = o.OrganizationEmail,
                                        OrganizationPhoneNumber = o.OrganizationPhoneNumber,
                                        OrganizationAddress = o.OrganizationAddress,
                                        OrganizationDescription = o.OrganizationDescription,
                                        OrganizationPictures = o.OrganizationPictures,
                                        StartTime = o.StartTime,
                                        ShutdownDay = o.ShutdownDay,
                                        OrganizationMember = o.OrganizationMember.ToList(),
                                        OrganizationResource = o.OrganizationResource.ToList(),
                                        Project = o.Project.ToList(),
                                        CEO = o.OrganizationMember.Where(om => om.Status == 2).FirstOrDefault().User,
                                        Members = o.OrganizationMember.Where(om => om.Status > 0).Count(),
                                        Projects = o.Project.Count(),
                                        joinRequests = o.OrganizationMember.Where(om => om.Status == 0).Count(),
                                    }).FirstOrDefaultAsync();
            return result;
        }


        //for ALl Organization Page
        public async Task<List<OrganizationVM>> GetAllOrganizationVMsAsync()
        {
            var result = await _db.Organizations
                                    .Include(o => o.OrganizationMember)
                                    .ThenInclude(om => om.User)
                                    .Select( o => new OrganizationVM
                                    {
                                        OrganizationID = o.OrganizationID,//
                                        OrganizationName = o.OrganizationName,//
                                        OrganizationDescription = o.OrganizationDescription,//
                                        OrganizationPictures = o.OrganizationPictures,//
                                        StartTime = o.StartTime,//
                                        ShutdownDay = o.ShutdownDay,//
                                        OrganizationMember = o.OrganizationMember.ToList(),//
                                    }).ToListAsync();
            return result;
        }

        //For My Organization Page
        public async Task<List<OrganizationVM>> GetOrganizationVMsByUserIDAsync(Guid userId)
        {
            var result = await _db.Organizations
                                    .Where(o => o.OrganizationMember.Any(om => om.UserID.Equals(userId)))
                                    .Include(o => o.OrganizationMember)
                                    .ThenInclude(om => om.User)
                                    .Select(o => new OrganizationVM
                                    {
                                        OrganizationID = o.OrganizationID,
                                        OrganizationName = o.OrganizationName,
                                        OrganizationEmail = o.OrganizationEmail,
                                        OrganizationPhoneNumber = o.OrganizationPhoneNumber,
                                        OrganizationAddress = o.OrganizationAddress,
                                        OrganizationDescription = o.OrganizationDescription,
                                        OrganizationPictures = o.OrganizationPictures,
                                        StartTime = o.StartTime,
                                        ShutdownDay = o.ShutdownDay,
                                        OrganizationMember = o.OrganizationMember.Where(om => om.UserID.Equals(userId)).ToList(),
                                        CEO = o.OrganizationMember.Where(om => om.Status == 2).FirstOrDefault().User,
                                        Members = o.OrganizationMember.Where(om => om.Status > 0).Count(),
                                    }).ToListAsync();
            return result;
        }


        //for Organization of Ceo
        public async Task<OrganizationVM> GetOrganizationVMByUserIDAsync(Guid userId)
        {
            var result = await _db.Organizations
                                    .Where(o => o.OrganizationMember.Any(om => om.UserID.Equals(userId) && om.Status == 2))
                                    .Include(o => o.OrganizationMember)
                                    .ThenInclude(om => om.User)
                                    .Select(o => new OrganizationVM
                                    {
                                        OrganizationID = o.OrganizationID,
                                        OrganizationName = o.OrganizationName,
                                        OrganizationStatus = o.OrganizationStatus,
                                        OrganizationEmail = o.OrganizationEmail,
                                        OrganizationPhoneNumber = o.OrganizationPhoneNumber,
                                        OrganizationAddress = o.OrganizationAddress,
                                        OrganizationDescription = o.OrganizationDescription,
                                        OrganizationPictures = o.OrganizationPictures,
                                        StartTime = o.StartTime,
                                        ShutdownDay = o.ShutdownDay,
                                        OrganizationMember = o.OrganizationMember.ToList(),
                                        CEO = o.OrganizationMember.Where(om => om.Status == 2).FirstOrDefault().User,
                                        Members = o.OrganizationMember.Where(om => om.Status > 0).Count(),
                                    }).FirstOrDefaultAsync();
            return result;
        }
    }
}
