using System.Linq.Expressions;
using Dynamics.Models.Models;

namespace Dynamics.DataAccess.Repository;

public interface IWalletRepository
{
    public Task<Wallet?> GetWalletAsync(Expression<Func<Wallet, bool>>? predicate = null);
    public Task<List<Wallet>> GetAllWalletAsync(Expression<Func<Wallet, bool>>? predicate = null);
    public Task<Wallet> CreateWalletAsync(Wallet wallet);
    /**
     * Only use this one if the wallet passed to the params is retrieved from database
     */
    public Task UpdateWalletAsync(Wallet wallet);

    public Task UpdateWalletAsync(Guid walletId, int amount, string? currency = null);

    public IQueryable<Wallet> GetWalletAsQueryable(Expression<Func<Wallet, bool>>? predicate = null);
}