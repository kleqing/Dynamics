using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Dto;
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
            Name = "Organization - " +
                   ut.OrganizationResource.Organization.OrganizationName, // Target organization name
            Time = ut.Time,
            Unit = ut.OrganizationResource.Unit,
            Avatar = ut.User.UserAvatar,
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
            Name = "Project - " + ut.ProjectResource.Project.ProjectName, // Target project name
            Time = ut.Time,
            Unit = ut.ProjectResource.Unit,
            Attachments = ut.Attachments,
            Avatar = ut.User.UserAvatar,
            Type = "userToPrj"
        });
        return await result.ToListAsync();
    }

    public async Task<List<OrganizationTransactionDto>> GetTransactionOrganizationReceivedFromUserDtosAsync(
        IQueryable<UserToOrganizationTransactionHistory> query)
    {
        var organizationTransactionDtos = query
            .Include(x => x.User)
            .Select(uto => new OrganizationTransactionDto
            {
                TransactionID = uto.TransactionID,
                Name = uto.User.UserName, // The name of the user that org received
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

    public async Task<List<OrganizationTransactionDto>> GetOrganizationToProjectTransactionDtosAsync(
        IQueryable<OrganizationToProjectHistory> query)
    {
        var organizationTransactionDtos = query
            .Include(x => x.ProjectResource)
            .ThenInclude(pr => pr.Project)
            .Select(uto => new OrganizationTransactionDto
            {
                TransactionID = uto.TransactionID,
                Name = uto.ProjectResource.Project.ProjectName, // The name of the project organization sent to
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

    public async Task<List<UserTransactionDto>> GetTransactionProjectReceivedFromOrganizationDtosAsync(
        IQueryable<OrganizationToProjectHistory> query)
    {
        var result = query.Select(opt => new UserTransactionDto
        {
            TransactionID = opt.TransactionID,
            Amount = opt.Amount,
            Message = opt.Message,
            Status = opt.Status,
            ResourceName = opt.ProjectResource.ResourceName,
            Name = opt.OrganizationResource.Organization.OrganizationName, // Organization name who donated
            Time = opt.Time,
            Unit = opt.ProjectResource.Unit,
            Attachments = opt.Attachments,
            Type = "orgToPrj"
        });
        return await result.ToListAsync();
    }

    public async Task<List<UserTransactionDto>> GetTransactionProjectReceivedFromUserDtosAsync(
        IQueryable<UserToProjectTransactionHistory> query)
    {
        var result = query.Select(opt => new UserTransactionDto
        {
            TransactionID = opt.TransactionID,
            Amount = opt.Amount,
            Message = opt.Message,
            Status = opt.Status,
            ResourceName = opt.ProjectResource.ResourceName,
            Name = opt.User.UserName, // Username who donated
            Time = opt.Time,
            Unit = opt.ProjectResource.Unit,
            Attachments = opt.Attachments,
            Avatar = opt.User.UserAvatar,
            Type = "userToPrj"
        });
        return await result.ToListAsync();
    }

    /**
     * Things that are different:
     * Query: user to prj && user to org
     * The kind of filter (prj only or org only)
     */
    public async Task<List<UserTransactionDto>> SetupUserTransactionDtosWithSearchParams(SearchRequestDto searchOptions,
        IQueryable<UserToProjectTransactionHistory> userToPrjQueryable,
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
    public async Task<List<UserTransactionDto>> SetupProjectTransactionDtosWithSearchParams(
        SearchRequestDto searchOptions, IQueryable<UserToProjectTransactionHistory> userToPrjQueryable,
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
                await GetTransactionProjectReceivedFromOrganizationDtosAsync(orgToPrjQueryable);
        }
        else if (searchOptions.Filter != null &&
                 searchOptions.Filter.Equals(MyConstants.User, StringComparison.OrdinalIgnoreCase))
        {
            userToPrjTransactionDtos = await GetTransactionProjectReceivedFromUserDtosAsync(userToPrjQueryable);
        }
        else
        {
            // Get both for display if no filter for organization / project selected
            userToOrgTransactionDtos = await GetTransactionProjectReceivedFromOrganizationDtosAsync(orgToPrjQueryable);
            userToPrjTransactionDtos = await GetTransactionProjectReceivedFromUserDtosAsync(userToPrjQueryable);
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
    public async Task<List<OrganizationTransactionDto>> SetupOrganizationTransactionDtosWithSearchParams(
        SearchRequestDto searchOptions, IQueryable<OrganizationToProjectHistory> organizationToProjectQueryable,
        IQueryable<UserToOrganizationTransactionHistory> userToOrgQueryable)
    {
        // Building up queries with search
        userToOrgQueryable = _searchService.GetUserToOrgQueryWithSearchParams(searchOptions, userToOrgQueryable);
        organizationToProjectQueryable =
            _searchService.GetOrgToPrjQueryWithSearchParams(searchOptions, organizationToProjectQueryable);
        // Preparing the dtos
        var userToOrgTransactionDtos = new List<OrganizationTransactionDto>();
        var orgToPrjTransactionDtos = new List<OrganizationTransactionDto>();
        // Check if user only want organization or project and only execute on that one
        if (searchOptions.Filter != null &&
            searchOptions.Filter.Equals(MyConstants.User, StringComparison.OrdinalIgnoreCase))
        {
            userToOrgTransactionDtos =
                await GetTransactionOrganizationReceivedFromUserDtosAsync(userToOrgQueryable);
        }
        else if (searchOptions.Filter != null &&
                 searchOptions.Filter.Equals(MyConstants.Project, StringComparison.OrdinalIgnoreCase))
        {
            orgToPrjTransactionDtos =
                await GetOrganizationToProjectTransactionDtosAsync(organizationToProjectQueryable);
        }
        else
        {
            // Get both for display if no filter for organization / project selected
            userToOrgTransactionDtos = await GetTransactionOrganizationReceivedFromUserDtosAsync(userToOrgQueryable);
            orgToPrjTransactionDtos =
                await GetOrganizationToProjectTransactionDtosAsync(organizationToProjectQueryable);
        }

        // Merge into a list and display
        var total = new List<OrganizationTransactionDto>();
        total.AddRange(userToOrgTransactionDtos);
        total.AddRange(orgToPrjTransactionDtos);
        return total.OrderByDescending(utd => utd.Time).ToList();
    }

    public async Task<List<OrganizationTransactionDto>> SetupUserToOrgTransactionDtosWithSearchParams(
        SearchRequestDto searchOptions, IQueryable<UserToOrganizationTransactionHistory> userToOrgQueryable)
    {
        userToOrgQueryable = _searchService.GetUserToOrgQueryWithSearchParams(searchOptions, userToOrgQueryable);
        var userToOrgTransactionDtos = new List<OrganizationTransactionDto>();
        userToOrgTransactionDtos = await GetTransactionOrganizationReceivedFromUserDtosAsync(userToOrgQueryable);
        
        var total = new List<OrganizationTransactionDto>();
        total.AddRange(userToOrgTransactionDtos);
        return total.OrderByDescending(utd => utd.Time).ToList();
    }
    
    public async Task<List<OrganizationTransactionDto>> SetupOrgToPrjTransactionDtosWithSearchParams(
        SearchRequestDto searchOptions, IQueryable<OrganizationToProjectHistory> organizationToProjectQueryable)
    {
        organizationToProjectQueryable = _searchService.GetOrgToPrjQueryWithSearchParams(searchOptions, organizationToProjectQueryable);
        var userToOrgTransactionDtos = new List<OrganizationTransactionDto>();
        userToOrgTransactionDtos = await GetOrganizationToProjectTransactionDtosAsync(organizationToProjectQueryable);
        
        var total = new List<OrganizationTransactionDto>();
        total.AddRange(userToOrgTransactionDtos);
        return total.OrderByDescending(utd => utd.Time).ToList();
    }
}