using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.DataAccess.Repository
{
    public class OrganizationToProjectTransactionHistoryRepository:IOrganizationToProjectTransactionHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IProjectResourceRepository _projectResourceRepo;

        public OrganizationToProjectTransactionHistoryRepository(ApplicationDbContext context,
            IProjectResourceRepository projectResourceRepository)
        {
            _context = context;
            _projectResourceRepo = projectResourceRepository;
        }
        public Task<OrganizationToProjectHistory?> GetAsync(
      Expression<Func<OrganizationToProjectHistory, bool>> filter)
        {
            return _context.OrganizationToProjectTransactionHistory.Include(x=>x.OrganizationResource).ThenInclude(x=>x.Organization).Where(filter).FirstOrDefaultAsync();
        }

        public async Task<List<OrganizationToProjectHistory>> GetAllOrganizationDonateAsync(
    Expression<Func<OrganizationToProjectHistory, bool>> filter)
        {
            IQueryable<OrganizationToProjectHistory> listOrganizationDonate =
                _context.OrganizationToProjectTransactionHistory.Include(x => x.ProjectResource).ThenInclude(x => x.Project).Include(x => x.OrganizationResource).ThenInclude(X => X.Organization).Where(filter).OrderByDescending(x => x.Time);
            if (listOrganizationDonate != null)
            {
                return await listOrganizationDonate.ToListAsync();
            }

            return null;
        }

        public async Task AddAsync(OrganizationToProjectHistory transaction)
        {
            await _context.AddAsync(transaction);
        }

        public async Task<bool> AddOrgDonateRequestAsync(OrganizationToProjectHistory? orgDonate)
        {
            if (orgDonate != null)
            {
                orgDonate.TransactionID = Guid.NewGuid();
                if (orgDonate.Amount <= 0)
                {
                    orgDonate.Amount = 1;
                }
                orgDonate.Status = 0;
                orgDonate.Time = DateOnly.FromDateTime(DateTime.Now);
                await _context.OrganizationToProjectTransactionHistory.AddAsync(orgDonate);
                //find org resource
                var orgResource = await _context.OrganizationResources.FirstOrDefaultAsync(x => x.ResourceID == orgDonate.OrganizationResourceID);
                //update value org resource
                if (orgResource != null)
                {
                    orgResource.Quantity -= orgDonate.Amount;
                    _context.OrganizationResources.Update(orgResource);
                }
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }


        public async Task<bool> AcceptOrgDonateRequestAsync(OrganizationToProjectHistory transactionObj)
        {
            if (transactionObj != null)
            {
                //change status of transaction
                transactionObj.Status = 1;
                _context.OrganizationToProjectTransactionHistory.Update(transactionObj);
                await _context.SaveChangesAsync();

                //modify resource of project
                var addResourceAutomatic = _projectResourceRepo.HandleResourceAutomatic(transactionObj.TransactionID, "Organization");
                if (addResourceAutomatic.Result)
                    return true;
                return false;
            }

            return false;
        }

        public async Task<bool> DenyOrgDonateRequestAsync(OrganizationToProjectHistory transactionObj)
        {
           
            if (transactionObj != null)
            {
                //find orgresource
                var orgResource = await _context.OrganizationResources.FirstOrDefaultAsync(x => x.ResourceID == transactionObj.OrganizationResourceID);
                ;
                //update value orgresource
                if (orgResource != null)
                {
                    orgResource.Quantity += transactionObj.Amount;
                    _context.OrganizationResources.Update(orgResource);
                    transactionObj.Status = -1;
                    _context.OrganizationToProjectTransactionHistory.Update(transactionObj);
                    await _context.SaveChangesAsync();
                    return true;

                }
            }

            return false;
        }
    }
}
