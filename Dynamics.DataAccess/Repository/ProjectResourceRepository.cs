using System.Linq.Expressions;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository;

public class ProjectResourceRepository : IProjectResourceRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectResourceRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<ProjectResource?> GetAsync(Expression<Func<ProjectResource, bool>> predicate)
    {
        return await _context.ProjectResources.Where(predicate).FirstOrDefaultAsync();
    }

    //-----manage resource's statistic----------------
    public IQueryable<ProjectResource> GetAllProjectResourceQuery()
    {
        return _context.ProjectResources;
    }
    public async Task<bool> HandleResourceAutomatic(Guid transactionID, string donor)
    {
        //take list of resource type in ProjectResource table
        List<Guid> resourceTypes = _context.ProjectResources.Select(x => x.ResourceID).Distinct().ToList();
        //get transaction obj to take the resource name of transaction
        if (!string.IsNullOrEmpty(donor) && donor.Equals("User"))
        {
            var transactionObj = await _context.UserToProjectTransactionHistories.Include(x => x.ProjectResource).ThenInclude(x => x.Project)
                .Where(x => x.TransactionID.Equals(transactionID)).FirstOrDefaultAsync();

            if (transactionObj != null)
            {
                //check if the resource id of transaction is in the list of resource, if yes, update the quantity of resource
                if (transactionObj.ProjectResourceID != null && resourceTypes.Contains(transactionObj.ProjectResourceID))
                {
                    //find the obj resource in ProjectResource table
                    var resourceObj = transactionObj.ProjectResource;
                    if (resourceObj != null)
                    {
                        //update quantity of obj resource
                        resourceObj.Quantity += transactionObj.Amount;
                        _context.ProjectResources.Update(resourceObj);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
            }
        }
        else if (!string.IsNullOrEmpty(donor) && donor.Equals("Organization"))
        {
            var transactionObj =
                await _context.OrganizationToProjectTransactionHistory.Include(x => x.ProjectResource).ThenInclude(x => x.Project)
                .Include(x => x.OrganizationResource).ThenInclude(x => x.Organization)
                .FirstOrDefaultAsync(x =>
                    x.TransactionID.Equals(transactionID));
            if (transactionObj != null)
            {
                //check if the resource name of transaction is in the list of resource type, if yes, update the quantity of resource
                if (transactionObj.ProjectResourceID.HasValue && resourceTypes.Contains(transactionObj.ProjectResourceID.Value))
                {
                    //find the obj resource in ProjectResource table
                    var resourceObj = transactionObj.ProjectResource;
                    if (resourceObj != null)
                    {
                        //update quantity of obj resource
                        resourceObj.Quantity += transactionObj.Amount;
                        _context.ProjectResources.Update(resourceObj);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public async Task<IEnumerable<ProjectResource?>> FilterProjectResourceAsync(
        Expression<Func<ProjectResource, bool>> filter)
    {
        IQueryable<ProjectResource?> projectResourceList = _context.ProjectResources.Where(filter);
        return await projectResourceList.ToListAsync();
    }

    public async Task<bool> AddResourceTypeAsync(ProjectResource entity)
    {
        if (entity != null)
        {
            entity.ResourceID = Guid.NewGuid();
            entity.Quantity = 0;
            _context.ProjectResources.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateResourceTypeAsync(ProjectResource entity)
    {
        if (entity == null)
            return false;

        var existingItems = await FilterProjectResourceAsync(u => entity.ResourceID.Equals(u.ResourceID));
        var existingItem = existingItems.FirstOrDefault();

        if (existingItem != null)
        {
            // Update the existing tracked entity properties
            existingItem.ResourceName = entity.ResourceName;
            existingItem.ExpectedQuantity = entity.ExpectedQuantity;
            existingItem.Unit = entity.Unit;
            _context.ProjectResources.Update(existingItem);
            // No need to set the state or call Update again, since existingItem is already tracked
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteResourceTypeAsync(Guid resourceID)
    {
        var deleteItem = await _context.ProjectResources.FirstOrDefaultAsync(x => x.ResourceID.Equals(resourceID));
        var existUserDonate =
            await _context.UserToProjectTransactionHistories.FirstOrDefaultAsync(x => x.ProjectResourceID.Equals(resourceID));
        var existOrgDonate =
            await _context.OrganizationToProjectTransactionHistory.FirstOrDefaultAsync(x =>
                x.ProjectResourceID.Equals(resourceID));
        if (deleteItem == null || existUserDonate != null || existOrgDonate != null||deleteItem.ResourceName.ToLower().Equals("money"))
        {
            return false;
        }

        _context.ProjectResources.Remove(deleteItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateAsync(ProjectResource entity)
    {
        var existingResource = _context.ProjectResources.FirstOrDefault(x => x.ResourceID.Equals(entity.ResourceID));
        if (existingResource != null)
        {
            existingResource.Quantity = entity.Quantity;
        }
        _context.ProjectResources.Update(entity);
        await _context.SaveChangesAsync();
    }
}