using System.Linq.Expressions;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;

namespace Dynamics.Services;

public interface ITransactionViewService
{
    /**
     * Get the transaction entity in dto for display
     */
    Task<List<UserTransactionDto>> GetUserToOrganizationTransactionDTOsAsync(
        Expression<Func<UserToOrganizationTransactionHistory, bool>> predicate);

    /**
    * Get the transaction entity in dto for display
    */
    Task<List<UserTransactionDto>> GetUserToProjectTransactionDTOsAsync(
        Expression<Func<UserToProjectTransactionHistory, bool>> predicate);

    Task<List<OrganizationTransactionDto>> GetUserToOrganizationTransactionDtosAsync(IQueryable<UserToOrganizationTransactionHistory> query);
    Task<List<OrganizationTransactionDto>> GetOrganizationToProjectTransactionDtosAsync(IQueryable<OrganizationToProjectHistory> query);
}