using System.Linq.Expressions;
using Dynamics.Models.Models;

namespace Dynamics.DataAccess.Repository;

public interface IWithdrawRepository
{
    Task AddAsync(Withdraw entity);
    Task<Withdraw?> GetWithdraw(Expression<Func<Withdraw, bool>> filer);
}