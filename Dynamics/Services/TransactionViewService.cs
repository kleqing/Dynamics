using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Dto;

namespace Dynamics.Services;

public class TransactionViewService : ITransactionViewService
{
    private readonly ApplicationDbContext _context;

    public TransactionViewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserTransactionDto>> GetUserToOrganizationTransactionDTOsAsync(Expression<Func<UserToOrganizationTransactionHistory, bool>> filter)
    {
        var result = _context.UserToOrganizationTransactionHistories
            .Where(filter)
            .Select(ut => new UserTransactionDto
            {
                TransactionID = ut.TransactionID,
                User = ut.User,
                Amount = ut.Amount,
                Message = ut.Message,
                Status = ut.Status,
                ResourceName = ut.OrganizationResource.ResourceName,
                Target = "Organization - " + ut.OrganizationResource.Organization.OrganizationName,
                Time = ut.Time,
                Unit = ut.OrganizationResource.Unit,
                Type = "organization"
            });
        return await result.ToListAsync();
    }

    public async Task<List<UserTransactionDto>> GetUserToProjectTransactionDTOsAsync(Expression<Func<UserToProjectTransactionHistory, bool>> predicate)
    {
        var result = _context.UserToProjectTransactionHistories
            .Where(predicate)
            .Select(ut => new UserTransactionDto
            {
                TransactionID = ut.TransactionID,
                User = ut.User,
                Amount = ut.Amount,
                Message = ut.Message,
                Status = ut.Status,
                ResourceName = ut.ProjectResource.ResourceName,
                Target = "Project - " + ut.ProjectResource.Project.ProjectName,
                Time = ut.Time,
                Unit = ut.ProjectResource.Unit,
                Type = "project"
            });
        return await result.ToListAsync();
    }

    public async Task<List<OrganizationTransactionDto>> GetUserToOrganizationTransactionDtosAsync(IQueryable<UserToOrganizationTransactionHistory> query)
    {
        var organizationTransactionDtos = query
            .Include(x => x.User)
            .Select(uto => new OrganizationTransactionDto
        {
            TransactionID = uto.TransactionID,
            Name = uto.User.UserFullName,
            Unit = uto.OrganizationResource.Unit,
            Amount = uto.Amount,
            Message = uto.Message,
            ResourceName = uto.OrganizationResource.ResourceName,
            Status = uto.Status,
            Time = uto.Time,
            Attachments = uto.Attachments,
            Type = "UserToOrg",
        });

        return await organizationTransactionDtos.ToListAsync();
    }

    public async Task<List<OrganizationTransactionDto>> GetOrganizationToProjectTransactionDtosAsync(IQueryable<OrganizationToProjectHistory> query)
    {
        var organizationTransactionDtos = query
            .Include(x => x.ProjectResource)
                .ThenInclude(pr => pr.Project)
            .Select(uto => new OrganizationTransactionDto
        {
            TransactionID = uto.TransactionID,
            Name = uto.ProjectResource.Project.ProjectName,
            Project = uto.ProjectResource.Project,
            Unit = uto.OrganizationResource.Unit,
            ResourceName = uto.OrganizationResource.ResourceName,
            Amount = uto.Amount,
            Message = uto.Message,
            Status = uto.Status,
            Time = uto.Time,
            Attachments = uto.Attachments,
            Type = "OrgToPrj",
        });

        return await organizationTransactionDtos.ToListAsync();
    }
}