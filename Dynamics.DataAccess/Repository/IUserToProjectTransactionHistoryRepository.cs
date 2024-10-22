using System.Linq.Expressions;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository;

public interface IUserToProjectTransactionHistoryRepository
{
    Task<List<UserToProjectTransactionHistory>> GetAllAsyncWithExpression(Expression<Func<UserToProjectTransactionHistory, bool>> filter);

    Task<UserToProjectTransactionHistory?> GetAsync(Expression<Func<UserToProjectTransactionHistory, bool>> filter);
    Task<UserToProjectTransactionHistory> DeleteTransactionByIdAsync(Guid id);
    //get user donate-huyen
    Task<List<UserToProjectTransactionHistory>> GetAllUserDonateAsync(
    Expression<Func<UserToProjectTransactionHistory, bool>> filter);
    //accept or deny transaction from user
    Task<bool> AddUserDonateRequestAsync(UserToProjectTransactionHistory? userDonate);
    Task<bool> AcceptUserDonateRequestAsync(UserToProjectTransactionHistory userDonate);
    Task<bool> DenyUserDonateRequestAsync(UserToProjectTransactionHistory userDonate);
}

