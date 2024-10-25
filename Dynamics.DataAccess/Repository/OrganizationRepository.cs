using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dynamics.DataAccess.Repository
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationDbContext _db;

        public OrganizationRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        //Organization table

        public async Task<bool> AddOrganizationAsync(Organization organization)
        {
            try
            {
                _db.Organizations.Add(organization);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Organization?> GetOrganizationAsync(Expression<Func<Organization, bool>> filter)
        {
            var organization = await _db.Organizations.Where(filter).FirstOrDefaultAsync();
            return organization;
        }
        //public async Task<List<Organization>> GetListOrganizationsByUserIDAsync(Expression<Func<Organization, bool>> filter)
        //{
        //    var organizations = await _db.Organizations.Where(filter).ToListAsync();
        //    return organizations;
        //}

        public async Task<bool> UpdateOrganizationAsync(Organization organization)
        {
            var organizationItem = await GetOrganizationAsync(o => o.OrganizationID == organization.OrganizationID);

            if (organizationItem == null)
            {
                return false;
            }

            try
            {
                _db.Entry(organizationItem).CurrentValues.SetValues(organization);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        //Organization Member table
        //Member join organization
        public async Task<bool> AddOrganizationMemberSync(OrganizationMember organizationMember)
        {
            try
            {
                _db.OrganizationMember.Add(organizationMember);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<OrganizationMember?> GetOrganizationMemberAsync(
            Expression<Func<OrganizationMember, bool>> filter)
        {
            var organizationMember = await _db.OrganizationMember.Where(filter).FirstOrDefaultAsync();
            return organizationMember;
        }

        public async Task<bool> UpdateOrganizationMemberAsync(OrganizationMember entity)
        {
            var organizationItem = await GetOrganizationMemberAsync(om =>
                om.OrganizationID == entity.OrganizationID && om.UserID == entity.UserID);

            if (organizationItem == null)
            {
                return false;
            }

            _db.Entry(organizationItem).CurrentValues.SetValues(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        //Member out or remove 
        public async Task<bool> DeleteOrganizationMemberByOrganizationIDAndUserIDAsync(Guid organizationId, Guid userId)
        {
            var organizationMember =
                await GetOrganizationMemberAsync(om => om.OrganizationID == organizationId && om.UserID == userId);
            try
            {
                _db.OrganizationMember.Remove(organizationMember);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        //Organization Resource
        public async Task<bool> AddOrganizationResourceSync(OrganizationResource organizationResource)
        {
            try
            {
                _db.OrganizationResources.Add(organizationResource);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<OrganizationResource> GetOrganizationResourceByOrganizationIDAndResourceIDAsync(
            Guid organizationId, Guid resourceId)
        {
            var organizationResource = await _db.OrganizationResources
                .Where(or => or.OrganizationID == organizationId && or.ResourceID == resourceId).FirstOrDefaultAsync();
            return organizationResource;
        }

        public async Task<OrganizationResource> GetOrganizationResourceAsync(
            Expression<Func<OrganizationResource, bool>> filter)
        {
            var organizationResource = await _db.OrganizationResources.Where(filter).FirstOrDefaultAsync();
            return organizationResource;
        }

        public async Task<bool> UpdateOrganizationResourceAsync(OrganizationResource entity)
        {
            var organizationResourceItem =
                await GetOrganizationResourceByOrganizationIDAndResourceIDAsync(entity.OrganizationID,
                    entity.ResourceID);

            if (organizationResourceItem == null)
            {
                return false;
            }

            _db.Entry(organizationResourceItem).CurrentValues.SetValues(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOrganizationResourceAsync(Guid resourceId)
        {
            var organizationResource = await GetOrganizationResourceAsync(om => om.ResourceID.Equals(resourceId));
            try
            {
                _db.OrganizationResources.Remove(organizationResource);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<bool> AddUserToOrganizationTransactionHistoryASync(
            UserToOrganizationTransactionHistory transactionHistory)
        {
            try
            {
                _db.UserToOrganizationTransactionHistories.Add(transactionHistory);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<UserToOrganizationTransactionHistory>
            GetUserToOrganizationTransactionHistoryByTransactionIDAsync(
                Expression<Func<UserToOrganizationTransactionHistory, bool>> filter)
        {
            var userToOrganizationTransactionHistory =
                await _db.UserToOrganizationTransactionHistories.Where(filter).FirstOrDefaultAsync();
            return userToOrganizationTransactionHistory;
        }

        public async Task<bool> DeleteUserToOrganizationTransactionHistoryByTransactionIDAsync(Guid transactionID)
        {
            var userToOrganizationTransactionHistory =
                await GetUserToOrganizationTransactionHistoryByTransactionIDAsync(uto =>
                    uto.TransactionID == transactionID);
            try
            {
                _db.UserToOrganizationTransactionHistories.Remove(userToOrganizationTransactionHistory);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserToOrganizationTransactionHistoryAsync(
            UserToOrganizationTransactionHistory entity)
        {
            var userToOrganizationTransactionHistoryItem =
                await GetUserToOrganizationTransactionHistoryByTransactionIDAsync(uto =>
                    uto.TransactionID.Equals(entity.TransactionID));

            if (userToOrganizationTransactionHistoryItem == null)
            {
                return false;
            }

            _db.Entry(userToOrganizationTransactionHistoryItem).CurrentValues.SetValues(entity);
            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> AddOrganizationToProjectHistoryAsync(OrganizationToProjectHistory entity)
        {
            try
            {
                _db.OrganizationToProjectTransactionHistory.Add(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<OrganizationToProjectHistory> GetOrganizationToProjectHistoryAsync(
            Expression<Func<OrganizationToProjectHistory, bool>> filter)
        {
            var organizationToProjectHistory =
                await _db.OrganizationToProjectTransactionHistory.Where(filter).FirstOrDefaultAsync();
            return organizationToProjectHistory;
        }

        public async Task<bool> DeleteOrganizationToProjectHistoryAsync(Guid transactionId)
        {
            var organizationToProjectHistory =
                await GetOrganizationToProjectHistoryAsync(uto => uto.TransactionID.Equals(transactionId));
            try
            {
                _db.OrganizationToProjectTransactionHistory.Remove(organizationToProjectHistory);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //Repo of huyen

        public async Task<List<Organization>> GetAllOrganizationsAsync()
        {
            IQueryable<Organization> organizations = _db.Organizations.Include(x => x.OrganizationMember)
                .ThenInclude(x => x.User).Include(x => x.OrganizationResource);
            return await organizations.ToListAsync();
        }

        public IQueryable<Organization> GetAll(Expression<Func<Organization, bool>>? filter = null)
        {
            if (filter != null)
            {
                return _db.Organizations.Where(filter)
                    .Include(o => o.OrganizationMember)
                    .ThenInclude(om => om.User)
                    .OrderByDescending(o => o.StartTime)
                    .ThenBy(o => o.OrganizationID);
            }

            return _db.Organizations.Include(o => o.OrganizationMember)
                .ThenInclude(om => om.User)
                .OrderByDescending(o => o.StartTime)
                .ThenBy(o => o.OrganizationID);
        }

        public async Task<List<Organization>> GetAllOrganizationsWithExpressionAsync(
            Expression<Func<Organization, bool>>? filter = null)
        {
            if (filter != null)
            {
                return await _db.Organizations
                    .Where(filter)
                    .Include(o => o.OrganizationMember).ThenInclude(om => om.User).ToListAsync();
            }

            return await _db.Organizations.Include(o => o.OrganizationMember).ThenInclude(om => om.User).ToListAsync();
        }

        public async Task<Organization> GetOrganizationOfAUser(Guid userId)
        {
            var OrganizationObj = _db.OrganizationMember.Where(x => x.UserID.Equals(userId) && x.Status == 2)
                .Include("Organization").FirstOrDefaultAsync().Result;
            if (OrganizationObj != null)
            {
                return OrganizationObj.Organization;
            }

            return null;
        }

        public async Task<Organization> GetOrganizationUserLead(Guid userId)
        {
            var organizationMemberOfUser = _db.OrganizationMember.Where(x => x.UserID.Equals(userId) && x.Status == 2);
            if (organizationMemberOfUser == null)
            {
                return null;
            }

            var organizationUserLead = await organizationMemberOfUser.Include("Organization").FirstOrDefaultAsync();
            if (organizationUserLead == null)
            {
                return null;
            }

            return organizationUserLead.Organization;
        }

        public async Task<Guid> GetOrgResourceIDCorresponding(Guid projectResourceID, Guid organizationUserLeadID)
        {
            var projectResourceObj =
                await _db.ProjectResources.FirstOrDefaultAsync(x => x.ResourceID.Equals(projectResourceID));
            if (projectResourceObj == null)
            {
                return Guid.Empty;
            }

            var orgResourceObjCorresponding = await _db.OrganizationResources.FirstOrDefaultAsync(
                x => x.OrganizationID.Equals(organizationUserLeadID)
                     && x.ResourceName.ToLower().Trim().Equals(projectResourceObj.ResourceName.ToLower().Trim())
                     && x.Unit.ToLower().Trim().Equals(projectResourceObj.Unit.ToLower().Trim()));
            if (orgResourceObjCorresponding == null)
            {
                return Guid.Empty;
            }

            return orgResourceObjCorresponding.ResourceID;
        }
    }
}