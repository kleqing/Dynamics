using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.Services
{
    public class OrganizationToProjectHistoryVMService : IOrganizationToProjectHistoryVMService
    {

        private readonly ApplicationDbContext _db;

        public OrganizationToProjectHistoryVMService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<OrganizationToProjectHistory>> GetAllOrganizationToProjectHistoryAsync(Guid organizationId)
        {
            var result = await _db.OrganizationToProjectTransactionHistory
                                  .Where(otp => otp.OrganizationResource.OrganizationID.Equals(organizationId))
                                  .OrderByDescending(otp => otp.Time)
                                  .ThenBy(otp => otp.Status)
                                  .Include(otp => otp.OrganizationResource)
                                  .Include(otp => otp.ProjectResource)
                                           .ThenInclude(pr => pr.Project)
                                   .Select(otp => new OrganizationToProjectHistory
                                   {
                                       TransactionID = otp.TransactionID,
                                       OrganizationResourceID = otp.OrganizationResourceID,
                                       ProjectResourceID = otp.ProjectResourceID,
                                       Status = otp.Status,
                                       Time = otp.Time,
                                       Amount = otp.Amount,
                                       Attachments = otp.Attachments,
                                       OrganizationResource = otp.OrganizationResource,
                                       ProjectResource = otp.ProjectResource,
                                       Message = otp.Message,

                                   })
                                   .ToListAsync();
            return result;
        }

        public async Task<List<OrganizationToProjectHistory>> GetAllOrganizationToProjectHistoryByAcceptingAsync(Guid organizationId)
        {
            var result = await _db.OrganizationToProjectTransactionHistory.Where(otp => otp.Status == 1)
                                  .Where(otp => otp.OrganizationResource.OrganizationID.Equals(organizationId))
                                  .OrderByDescending(otp => otp.Time)
                                  .Include(otp => otp.OrganizationResource)
                                  .Include(otp => otp.ProjectResource)
                                           .ThenInclude(pr => pr.Project)
                                   .Select(otp => new OrganizationToProjectHistory
                                   {
                                       TransactionID = otp.TransactionID,
                                       OrganizationResourceID = otp.OrganizationResourceID,
                                       ProjectResourceID = otp.ProjectResourceID,
                                       Status = otp.Status,
                                       Time = otp.Time,
                                       Amount = otp.Amount,
                                       OrganizationResource = otp.OrganizationResource,
                                       ProjectResource = otp.ProjectResource,
                                       Message = otp.Message,

                                   })
                                   .ToListAsync();
            return result;
        }

    }
}
