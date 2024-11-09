using System.Linq.Expressions;
using Dynamics.Models.Models;

namespace Dynamics.DataAccess.Repository;

public interface IUserWalletTransactionRepository
{
    Task<UserWalletTransaction?> GetTransactionAsync(Expression<Func<UserWalletTransaction, bool>>? predicate = null);
    Task<List<UserWalletTransaction>> GetAllTransactionsAsync(Expression<Func<UserWalletTransaction, bool>>? predicate = null);
    Task<UserWalletTransaction> AddNewTransactionAsync(UserWalletTransaction transaction);

    IQueryable<UserWalletTransaction> GetUserWalletTransactionsQueryable(
        Expression<Func<UserWalletTransaction, bool>>? filter = null);
    Task UpdateTransactionAsync(UserWalletTransaction transaction);
    // Task DeleteTransactionAsync(Guid transactionId);
}