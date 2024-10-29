using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;

namespace Dynamics.Services;

public interface ISearchService
{
    /**
     * Get the transaction queries tailored to user search params
     */
    IQueryable<UserToOrganizationTransactionHistory> GetUserToOrgQueryWithSearchParams(
        SearchRequestDto searchRequestDto,
        IQueryable<UserToOrganizationTransactionHistory> query);

    IQueryable<UserToProjectTransactionHistory> GetUserToPrjQueryWithSearchParams(SearchRequestDto searchRequestDto,
        IQueryable<UserToProjectTransactionHistory> query);

    IQueryable<OrganizationToProjectHistory> GetOrgToPrjQueryWithSearchParams(SearchRequestDto searchRequestDto,
        IQueryable<OrganizationToProjectHistory> query);
}