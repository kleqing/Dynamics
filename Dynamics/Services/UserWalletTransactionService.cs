using System.Linq.Expressions;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;

namespace Dynamics.Services;

public class UserWalletTransactionService : IUserWalletTransactionService
{
    private readonly IUserWalletTransactionRepository _userWalletTransactionRepository;

    public UserWalletTransactionService(IUserWalletTransactionRepository userWalletTransactionRepository)
    {
        _userWalletTransactionRepository = userWalletTransactionRepository;
    }

    public async Task<List<UserWalletTransaction>> GetUserWalletTransactionsAsync(
        Expression<Func<UserWalletTransaction, bool>>? expression = null)
    {
        return await _userWalletTransactionRepository.GetAllTransactionsAsync(expression);
    }

    public async Task<UserWalletTransaction> AddNewTransactionAsync(UserWalletTransaction transaction)
    {
        var uwt = await _userWalletTransactionRepository.AddNewTransactionAsync(transaction);
        return uwt;
    }
}