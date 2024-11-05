using System.Globalization;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Utility;

namespace Dynamics.Services;

public class SearchService : ISearchService
{
    /**
     * Build base query based on user filter
     * Then apply a general search param (Meaning what is the thing it will search for base on the search param)
     * Ex: Filter money, query: ABC => Search for transaction with name | resource | message containing abc and the filter is ...
     */
    public IQueryable<UserToOrganizationTransactionHistory> GetUserToOrgQueryWithSearchParams(
        SearchRequestDto searchRequestDto,
        IQueryable<UserToOrganizationTransactionHistory> query)
    {
        // Use enum for clarity, you can use other like constants
        TransactionSearchOptions options;
        // First, build the base query
        if (Enum.TryParse(searchRequestDto.Filter, true, out options))
        {
            switch (options)
            {
                // Only return transaction with money
                case TransactionSearchOptions.Money:
                {
                    query = query.Where(uto =>
                        uto.OrganizationResource.ResourceName.Equals(SearchOptionsConstants.Money));
                    break;
                }
                case TransactionSearchOptions.Resource:
                {
                    query = query.Where(uto =>
                        !uto.OrganizationResource.ResourceName.Equals(SearchOptionsConstants.Money));
                    break;
                }
                case TransactionSearchOptions.Accepted:
                {
                    query = query.Where(uto => uto.Status == 1);
                    break;
                }
                case TransactionSearchOptions.Pending:
                {
                    query = query.Where(uto => uto.Status == 0);
                    break;
                }
                case TransactionSearchOptions.Denied:
                {
                    query = query.Where(uto => uto.Status == -1);
                    break;
                }
            }
        }

        // Add the general search params here
        query = AddGeneralDefaultSearchParamForUserToOrg(query, searchRequestDto.Query);
        // Check if date filter is also included
        var dateFrom = searchRequestDto.DateFrom;
        var dateTo = searchRequestDto.DateTo;
        // This means that if dateFrom is null, the whole statement return true, which means no filter applied
        // Else if dateFrom is not null, the statement return the right side, which is a filter (Basically it returns the value of the "correct statement")
        query = query.Where(uto => (dateFrom == null || uto.Time >= dateFrom) &&
                                   (dateTo == null || uto.Time <= dateTo))
            .OrderByDescending(uto => uto.Time); // Order by time as descending
        return query;
    }

    public IQueryable<UserToProjectTransactionHistory> GetUserToPrjQueryWithSearchParams(
        SearchRequestDto searchRequestDto, IQueryable<UserToProjectTransactionHistory> query)
    {
        // Use enum for clarity, you can use other like constants
        TransactionSearchOptions options;
        // First, build the base query
        if (Enum.TryParse(searchRequestDto.Filter, true, out options))
        {
            switch (options)
            {
                // Only return transaction with money
                case TransactionSearchOptions.Money:
                {
                    query = query.Where(uto =>
                        uto.ProjectResource.ResourceName.Equals(SearchOptionsConstants.Money));
                    break;
                }
                case TransactionSearchOptions.Resource:
                {
                    query = query.Where(uto =>
                        !uto.ProjectResource.ResourceName.Equals(SearchOptionsConstants.Money));
                    break;
                }
                case TransactionSearchOptions.Accepted:
                {
                    query = query.Where(uto => uto.Status == 1);
                    break;
                }
                case TransactionSearchOptions.Pending:
                {
                    query = query.Where(uto => uto.Status == 0);
                    break;
                }
                case TransactionSearchOptions.Denied:
                {
                    query = query.Where(uto => uto.Status == -1);
                    break;
                }
            }
        }

        // Add the general search params here
        query = AddGeneralDefaultSearchParamForUserToPrj(query, searchRequestDto.Query);
        // Check if date filter is also included
        var dateFrom = searchRequestDto.DateFrom;
        var dateTo = searchRequestDto.DateTo;
        return query.Where(uto => (dateFrom == null || uto.Time >= dateFrom) &&
                                  (dateTo == null || uto.Time <= dateTo))
            .OrderByDescending(uto => uto.Time); // Order by time as descending
    }

    public IQueryable<OrganizationToProjectHistory> GetOrgToPrjQueryWithSearchParams(SearchRequestDto searchRequestDto,
        IQueryable<OrganizationToProjectHistory> query)
    {
        // Use enum for clarity, you can use other like constants
        TransactionSearchOptions options;
        // First, build the base query
        if (Enum.TryParse(searchRequestDto.Filter, true, out options))
        {
            switch (options)
            {
                // Only return transaction with money
                case TransactionSearchOptions.Money:
                {
                    query = query.Where(uto =>
                        uto.ProjectResource.ResourceName.Equals(SearchOptionsConstants.Money));
                    break;
                }
                case TransactionSearchOptions.Resource:
                {
                    query = query.Where(uto =>
                        !uto.ProjectResource.ResourceName.Equals(SearchOptionsConstants.Money));
                    break;
                }
                case TransactionSearchOptions.Accepted:
                {
                    query = query.Where(uto => uto.Status == 1);
                    break;
                }
                case TransactionSearchOptions.Pending:
                {
                    query = query.Where(uto => uto.Status == 0);
                    break;
                }
                case TransactionSearchOptions.Denied:
                {
                    query = query.Where(uto => uto.Status == -1);
                    break;
                }
            }
        }

        // Add the general search params here
        query = AddGeneralDefaultSearchParamForOrgToPrj(query, searchRequestDto.Query);
        // Check if date filter is also included
        var dateFrom = searchRequestDto.DateFrom;
        var dateTo = searchRequestDto.DateTo;
        return query.Where(uto => (dateFrom == null || uto.Time >= dateFrom) &&
                                  (dateTo == null || uto.Time <= dateTo))
            .OrderByDescending(uto => uto.Time); // Order by time as descending
    }

    public IQueryable<UserWalletTransaction> GetUserWalletTransactionWithSearchParams(SearchRequestDto searchRequestDto,
        IQueryable<UserWalletTransaction> query)
    {
        // Use enum for clarity, you can use other like constants
        var options = searchRequestDto.Filter;
        // First, build the base query
        if (!string.IsNullOrEmpty(options))
        {
            query = query.Where(uwt => uwt.TransactionType.ToLower().Equals(options.ToLower()));
        }
        // Add the general search params here
        query = AddGeneralDefaultSearchParamForUserWalletTransaction(query, searchRequestDto.Query);
        // Check if date filter is also included
        var dateFrom = searchRequestDto.DateFrom.HasValue
            ? searchRequestDto.DateFrom.Value.ToDateTime(TimeOnly.MinValue)
            : (DateTime?)null;
        var dateTo = searchRequestDto.DateTo.HasValue
            ? searchRequestDto.DateTo.Value.ToDateTime(TimeOnly.MinValue)
            : (DateTime?)null;
        return query.Where(uto => (dateFrom == null || uto.Time >= dateFrom) &&
                                  (dateTo == null || uto.Time <= dateTo))
            .OrderByDescending(uto => uto.Time); // Order by time as descending
    }

    /**
     * Add a general search param for all kind of query based on user query
     * Should search for resource name, organization name, message of the transaction
     * && symbols mean that if the message is not null the message contain the q
     */
    private IQueryable<UserToOrganizationTransactionHistory> AddGeneralDefaultSearchParamForUserToOrg(
        IQueryable<UserToOrganizationTransactionHistory> query, string? userQuery)
    {
        if (userQuery == null) return query;
        var q = userQuery.ToLower();
        return query.Where(uto => (uto.Message != null && uto.Message.ToLower().Contains(q))
                                  || uto.OrganizationResource.ResourceName.ToLower().Contains(userQuery)
                                  || uto.OrganizationResource.Organization.OrganizationName.ToLower()
                                      .Contains(userQuery));
    }

    /**
     * Add a general search param for all kind of query based on user query
     * Should search for resource name, project name, message of the transaction
     */
    private IQueryable<UserToProjectTransactionHistory> AddGeneralDefaultSearchParamForUserToPrj(
        IQueryable<UserToProjectTransactionHistory> query, string? userQuery)
    {
        if (userQuery == null) return query;
        var q = userQuery.ToLower();
        return query.Where(utp =>
            utp.Message != null && utp.Message.ToLower().Contains(q)
            || utp.ProjectResource.ResourceName.ToLower().Contains(userQuery)
            || utp.ProjectResource.Project.ProjectName.ToLower().Contains(userQuery));
    }

    /**
     * Add a general search param for Organization To Project transaction
     */
    private IQueryable<OrganizationToProjectHistory> AddGeneralDefaultSearchParamForOrgToPrj(
        IQueryable<OrganizationToProjectHistory> query, string? userQuery)
    {
        if (userQuery == null) return query;
        var q = userQuery.ToLower();
        return query.Where(utp =>
            utp.Message != null && utp.Message.ToLower().Contains(q)
            || utp.ProjectResource.ResourceName.ToLower().Contains(userQuery)
            || utp.OrganizationResource.Organization.OrganizationName.ToLower().Contains(userQuery));
    }

    private IQueryable<UserWalletTransaction> AddGeneralDefaultSearchParamForUserWalletTransaction(
        IQueryable<UserWalletTransaction> query, string? userQuery)
    {
        if (userQuery == null) return query;
        var q = userQuery.ToLower();
        return query.Where(uwt =>
            uwt.Message != null && uwt.Message.ToLower().Contains(q));
    }
}