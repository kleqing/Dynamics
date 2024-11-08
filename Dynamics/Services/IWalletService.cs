using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;

namespace Dynamics.Services;

public interface IWalletService
{
    Task<Wallet> CreateEmptyWalletAsync(Guid userId);
    Task<Wallet> CreateWalletWithAmountAsync(Guid userId, int amount);
    Task<Wallet?> FindUserWalletByIdAsync(Guid userId);
    Task UpdateWalletAsync(Wallet wallet, int? newAmount = null);
    Task<Wallet> TopUpWalletAsync(Guid userId, int amount, string? msg = null, Guid? transactionId = null);
    List<UserWalletTransactionVM> MapToListUserWalletTransactionVMs(List<UserWalletTransaction> userWalletTransactions);
    /**
     * Pay from user wallet / organization money resource
     */
    public Task AddTransactionToDatabaseAsync(PayRequestDto payRequestDto);
    /**
     * Setup the payrequest only
     */
    public PayRequestDto SetupPayRequestDto(PayRequestDto payRequestDto);

    /**
     * Use this to spend money in wallet
     */
    Task<Wallet> SpendWalletAsync(Guid userId, int amount, string? msg = null, Guid? transactionId = null);
    Task RefundProjectWalletAsync(Project project);
    Task RefundOrganizationWalletAsync(Organization organization);
}