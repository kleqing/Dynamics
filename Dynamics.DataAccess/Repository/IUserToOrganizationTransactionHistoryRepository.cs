using System.Linq.Expressions;
using Dynamics.Models.Models;

namespace Dynamics.DataAccess.Repository;

public interface IUserToOrganizationTransactionHistoryRepository
{
    Task<List<UserToOrganizationTransactionHistory>> GetAllAsync();
    Task<List<UserToOrganizationTransactionHistory>> GetAllAsyncWithExpression(Expression<Func<UserToOrganizationTransactionHistory, bool>> filter);

    Task<UserToOrganizationTransactionHistory?> GetAsync(Expression<Func<UserToOrganizationTransactionHistory, bool>> filter);
    Task AddAsync(UserToOrganizationTransactionHistory entity);
    Task<bool> Update(UserToOrganizationTransactionHistory entity);
    Task<UserToOrganizationTransactionHistory> DeleteTransactionByIdAsync(Guid id);

}