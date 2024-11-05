using System.Linq.Expressions;
using Dynamics.Models.Models;

namespace Dynamics.Services;

public interface IUserWalletTransactionService
{
    Task<List<UserWalletTransaction>> GetUserWalletTransactionsAsync(Expression<Func<UserWalletTransaction, bool>>? expression = null);
    Task<UserWalletTransaction> AddNewTransactionAsync(UserWalletTransaction transaction);
}