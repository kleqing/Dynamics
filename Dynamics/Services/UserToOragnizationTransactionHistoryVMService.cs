using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dynamics.Services
{
    public class UserToOragnizationTransactionHistoryVMService : IUserToOragnizationTransactionHistoryVMService
    {
        private readonly ApplicationDbContext _db;

        public UserToOragnizationTransactionHistoryVMService(ApplicationDbContext db)
        {
            _db = db;
        }

        //for user request donate for Oragnization
        public async Task<List<UserToOrganizationTransactionHistory>> GetTransactionHistory(Guid organizationId)
        {
            var result = await _db.UserToOrganizationTransactionHistories.Where(uto => uto.Status == 0)
                                 .Where(uto => uto.OrganizationResource.OrganizationID.Equals(organizationId))
                                 .OrderByDescending(uto => uto.Time)
                                 .Include(uto => uto.User) 
                                 .Include(uto => uto.OrganizationResource)
                                 .ThenInclude(uto => uto.Organization)
                                  .Select(uto => new UserToOrganizationTransactionHistory
                                  {
                                      TransactionID = uto.TransactionID,
                                      ResourceID = uto.ResourceID,
                                      UserID = uto.UserID,
                                      Status = uto.Status,
                                      Amount = uto.Amount,
                                      Attachments = uto.Attachments,
                                      Message = uto.Message,
                                      Time = uto.Time,
                                      User = uto.User,
                                      OrganizationResource = uto.OrganizationResource,
                                  }).ToListAsync();
            return result;
        }


        //for Organization history
        public async Task<List<UserToOrganizationTransactionHistory>> GetTransactionHistoryIsAccept(Guid organizationId)
        {
            var result = await _db.UserToOrganizationTransactionHistories.Where(uto => uto.Status == 1)
                                 .Where(uto => uto.OrganizationResource.OrganizationID.Equals(organizationId))
                                 .OrderByDescending(uto => uto.Time)
                                 .Include(uto => uto.User)
                                 .Include(uto => uto.OrganizationResource)
                                        .ThenInclude(uto => uto.Organization)

                                  .Select(uto => new UserToOrganizationTransactionHistory
                                  {
                                      TransactionID = uto.TransactionID,
                                      ResourceID = uto.ResourceID,
                                      UserID = uto.UserID,
                                      Status = uto.Status,
                                      Amount = uto.Amount,
                                      Message = uto.Message,
                                      Time = uto.Time,
                                      User = uto.User,
                                      OrganizationResource = uto.OrganizationResource,
                                  })
                                  .ToListAsync();
            return result;
        }

        //for Donors
        public async Task<List<UserToOrganizationTransactionHistory>> GetTransactionHistoryByUserID(Guid userId)
        {
            var result = await _db.UserToOrganizationTransactionHistories
                                .Where(uto => uto.UserID.Equals(userId))
                                .OrderByDescending(uto => uto.Time)
                                .ThenBy(uto => uto.Status)
                                .Include(uto => uto.User)
                                .Include(uto => uto.OrganizationResource)
                                       .ThenInclude(uto => uto.Organization)

                                 .Select(uto => new UserToOrganizationTransactionHistory
                                 {
                                     TransactionID = uto.TransactionID,
                                     ResourceID = uto.ResourceID,
                                     UserID = uto.UserID,
                                     Status = uto.Status,
                                     Amount = uto.Amount,
                                     Attachments = uto.Attachments,
                                     Message = uto.Message,
                                     Time = uto.Time,
                                     User = uto.User,
                                     OrganizationResource = uto.OrganizationResource,
                                 })
                                 .ToListAsync();
            return result;
        }

    }
}
