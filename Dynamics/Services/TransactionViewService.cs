using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Dto;
using Dynamics.Models.Models.DTO;
using Dynamics.Utility;

namespace Dynamics.Services;

public class TransactionViewService : ITransactionViewService
{
    private readonly ApplicationDbContext _context;
    private readonly ISearchService _searchService;

    public TransactionViewService(ApplicationDbContext context, ISearchService searchService)
    {
        _context = context;
        _searchService = searchService;
    }
    

    public async Task<List<UserTransactionDto>> GetUserToOrganizationTransactionDTOs(
        IQueryable<UserToOrganizationTransactionHistory> query = null)
    {
        var userTransactionDtosQueryable = query.Select(ut => new UserTransactionDto
        {
            TransactionID = ut.TransactionID,
            Amount = ut.Amount,
            Message = ut.Message,
            Status = ut.Status,
            ResourceName = ut.OrganizationResource.ResourceName,
            Name = "Organization - " + ut.OrganizationResource.Organization.OrganizationName,
            Time = ut.Time,
            Unit = ut.OrganizationResource.Unit,
            Attachments = ut.Attachments,
            Type = "userToOrg",
        });
        return await userTransactionDtosQueryable.ToListAsync();
    }

    public async Task<List<UserTransactionDto>> GetUserToProjectTransactionDTOs(
        IQueryable<UserToProjectTransactionHistory> query)
    {
        var result = query.Select(ut => new UserTransactionDto
        {
            TransactionID = ut.TransactionID,
            Amount = ut.Amount,
            Message = ut.Message,
            Status = ut.Status,
            ResourceName = ut.ProjectResource.ResourceName,
            Name = "Project - " + ut.ProjectResource.Project.ProjectName, // Project name
            Time = ut.Time,
            Unit = ut.ProjectResource.Unit,
            Attachments = ut.Attachments,
            Type = "userToPrj"
        });
        return await result.ToListAsync();
    }

    public async Task<List<UserTransactionDto>> GetOrganizationToProjectTransactionDTOs(
        IQueryable<OrganizationToProjectHistory> query)
    {
        var result = query.Select(opt => new UserTransactionDto
        {
            TransactionID = opt.TransactionID,
            Amount = opt.Amount,
            Message = opt.Message,
            Status = opt.Status,
            ResourceName = opt.ProjectResource.ResourceName,
            Name = opt.OrganizationResource.Organization.OrganizationName, // Organization name
            Time = opt.Time,
            Unit = opt.ProjectResource.Unit,
            Attachments = opt.Attachments,
            Type = "orgToPrj"
        });
        return await result.ToListAsync();
    }
    
    /**
     * Things that are different:
     * Query: user to prj && user to org
     * The kind of filter (prj only or org only)
     */
    public async Task<List<UserTransactionDto>> SetupUserTransactionDtosWithSearchParams(SearchRequestDto searchOptions, IQueryable<UserToProjectTransactionHistory> userToPrjQueryable,
        IQueryable<UserToOrganizationTransactionHistory> userToOrgQueryable)
    {
        // Building up queries with search
        userToOrgQueryable = _searchService.GetUserToOrgQueryWithSearchParams(searchOptions, userToOrgQueryable);
        userToPrjQueryable = _searchService.GetUserToPrjQueryWithSearchParams(searchOptions, userToPrjQueryable);
        // Preparing the dtos
        var userToOrgTransactionDtos = new List<UserTransactionDto>();
        var userToPrjTransactionDtos = new List<UserTransactionDto>();
        // Check if user only want organization or project and only execute on that one
        if (searchOptions.Filter != null &&
            searchOptions.Filter.Equals(MyConstants.Organization, StringComparison.OrdinalIgnoreCase))
        {
            userToOrgTransactionDtos =
                await GetUserToOrganizationTransactionDTOs(userToOrgQueryable);
        }
        else if (searchOptions.Filter != null &&
                 searchOptions.Filter.Equals(MyConstants.Project, StringComparison.OrdinalIgnoreCase))
        {
            userToPrjTransactionDtos = await GetUserToProjectTransactionDTOs(userToPrjQueryable);
        }
        else
        {
            // Get both for display if no filter for organization / project selected
            userToOrgTransactionDtos = await GetUserToOrganizationTransactionDTOs(userToOrgQueryable);
            userToPrjTransactionDtos = await GetUserToProjectTransactionDTOs(userToPrjQueryable);
        }

        // Merge into a list and display
        var total = new List<UserTransactionDto>();
        total.AddRange(userToOrgTransactionDtos);
        total.AddRange(userToPrjTransactionDtos);
        return total.OrderByDescending(utd => utd.Time).ToList();
    }
    /**
     * Things that are different:
     * Query: org to prj && user to prj
     * The kind of filter (user only or org only)
     */
    public async Task<List<UserTransactionDto>> SetupProjectTransactionDtosWithSearchParams(SearchRequestDto searchOptions, IQueryable<UserToProjectTransactionHistory> userToPrjQueryable,
        IQueryable<OrganizationToProjectHistory> orgToPrjQueryable)
    {
        // Building up queries with search
        orgToPrjQueryable = _searchService.GetOrgToPrjQueryWithSearchParams(searchOptions, orgToPrjQueryable);
        userToPrjQueryable = _searchService.GetUserToPrjQueryWithSearchParams(searchOptions, userToPrjQueryable);
        // Preparing the dtos
        var userToOrgTransactionDtos = new List<UserTransactionDto>();
        var userToPrjTransactionDtos = new List<UserTransactionDto>();
        // Check if user only want organization or project and only execute on that one
        if (searchOptions.Filter != null &&
            searchOptions.Filter.Equals(MyConstants.Organization, StringComparison.OrdinalIgnoreCase))
        {
            userToOrgTransactionDtos =
                await GetOrganizationToProjectTransactionDTOs(orgToPrjQueryable);
        }
        else if (searchOptions.Filter != null &&
                 searchOptions.Filter.Equals(MyConstants.User, StringComparison.OrdinalIgnoreCase))
        {
            userToPrjTransactionDtos = await GetUserToProjectTransactionDTOs(userToPrjQueryable);
        }
        else
        {
            // Get both for display if no filter for organization / project selected
            userToOrgTransactionDtos = await GetOrganizationToProjectTransactionDTOs(orgToPrjQueryable);
            userToPrjTransactionDtos = await GetUserToProjectTransactionDTOs(userToPrjQueryable);
        }

        // Merge into a list and display
        var total = new List<UserTransactionDto>();
        total.AddRange(userToOrgTransactionDtos);
        total.AddRange(userToPrjTransactionDtos);
        return total.OrderByDescending(utd => utd.Time).ToList();
    }
    /**
     * Things that are different:
     * Query: org to prj && user to org
     * The kind of filter (user only or prj only)
     */
    public async Task<List<UserTransactionDto>> SetupOrganizationTransactionDtosWithSearchParams(SearchRequestDto searchOptions, IQueryable<OrganizationToProjectHistory> organizationToProjectQueryable,
        IQueryable<UserToOrganizationTransactionHistory> userToOrgQueryable)
    {
        // Building up queries with search
        userToOrgQueryable = _searchService.GetUserToOrgQueryWithSearchParams(searchOptions, userToOrgQueryable);
        organizationToProjectQueryable = _searchService.GetOrgToPrjQueryWithSearchParams(searchOptions, organizationToProjectQueryable);
        // Preparing the dtos
        var userToOrgTransactionDtos = new List<UserTransactionDto>();
        var userToPrjTransactionDtos = new List<UserTransactionDto>();
        // Check if user only want organization or project and only execute on that one
        if (searchOptions.Filter != null &&
            searchOptions.Filter.Equals(MyConstants.User, StringComparison.OrdinalIgnoreCase))
        {
            userToOrgTransactionDtos =
                await GetUserToOrganizationTransactionDTOs(userToOrgQueryable);
        }
        else if (searchOptions.Filter != null &&
                 searchOptions.Filter.Equals(MyConstants.Project, StringComparison.OrdinalIgnoreCase))
        {
            userToPrjTransactionDtos = await GetOrganizationToProjectTransactionDTOs(organizationToProjectQueryable);
        }
        else
        {
            // Get both for display if no filter for organization / project selected
            userToOrgTransactionDtos = await GetUserToOrganizationTransactionDTOs(userToOrgQueryable);
            userToPrjTransactionDtos = await GetOrganizationToProjectTransactionDTOs(organizationToProjectQueryable);
        }

        // Merge into a list and display
        var total = new List<UserTransactionDto>();
        total.AddRange(userToOrgTransactionDtos);
        total.AddRange(userToPrjTransactionDtos);
        return total.OrderByDescending(utd => utd.Time).ToList();
    }
}