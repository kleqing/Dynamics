using System.Linq.Expressions;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;

namespace Dynamics.Services;

public interface ITransactionViewService
{
    /**
     *  Execute the query user to org and map it to dtos
     */
    Task<List<UserTransactionDto>> GetUserToOrganizationTransactionDTOs(IQueryable<UserToOrganizationTransactionHistory> query);

    /**
    * Execute the query user to prj and map it to dtos
    */
    Task<List<UserTransactionDto>> GetUserToProjectTransactionDTOs(IQueryable<UserToProjectTransactionHistory> query);

    /**
     * Execute the query org to prj and map it to dtos
     */
    Task<List<UserTransactionDto>> GetTransactionProjectReceivedFromOrganizationDtosAsync(
     IQueryable<OrganizationToProjectHistory> query);
    /**
     * Setup for user related display transaction
     */
    Task<List<UserTransactionDto>> SetupUserTransactionDtosWithSearchParams(SearchRequestDto searchOptions,
        IQueryable<UserToProjectTransactionHistory> userToProjQuery,
        IQueryable<UserToOrganizationTransactionHistory> userToOrgQuery);

    Task<List<OrganizationTransactionDto>> SetupOrganizationTransactionDtosWithSearchParams(SearchRequestDto searchOptions,
     IQueryable<OrganizationToProjectHistory> organizationToProjectQueryable,
     IQueryable<UserToOrganizationTransactionHistory> userToOrgQueryable);
    // NEW method (Splitted from the upper one)
    Task<List<OrganizationTransactionDto>> SetupUserToOrgTransactionDtosWithSearchParams(
     SearchRequestDto searchOptions, IQueryable<UserToOrganizationTransactionHistory> userToOrgQueryable);

    Task<List<OrganizationTransactionDto>> SetupOrgToPrjTransactionDtosWithSearchParams(
     SearchRequestDto searchOptions, IQueryable<OrganizationToProjectHistory> organizationToProjectQueryable);
    
    
    Task<List<UserTransactionDto>> SetupProjectTransactionDtosWithSearchParams(SearchRequestDto searchOptions,
     IQueryable<UserToProjectTransactionHistory> userToPrjQueryable,
     IQueryable<OrganizationToProjectHistory> orgToPrjQueryable);
    
    Task<List<OrganizationTransactionDto>> GetTransactionOrganizationReceivedFromUserDtosAsync(IQueryable<UserToOrganizationTransactionHistory> query);
    Task<List<OrganizationTransactionDto>> GetOrganizationToProjectTransactionDtosAsync(IQueryable<OrganizationToProjectHistory> query);
    Task<List<UserTransactionDto>> GetTransactionProjectReceivedFromUserDtosAsync(
     IQueryable<UserToProjectTransactionHistory> query);

}