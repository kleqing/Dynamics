using AutoMapper;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Utility;
using Newtonsoft.Json;
using ILogger = Serilog.ILogger;

namespace Dynamics.Services;

public class WalletService : IWalletService
{
    public IOrganizationToProjectTransactionHistoryRepository OrganizationToPrj { get; }
    private readonly IWalletRepository _walletRepository;
    private readonly IUserWalletTransactionService _userWalletTransactionService;
    private readonly IMapper _mapper;
    private readonly IUserToOrganizationTransactionHistoryRepository _userToOrg;
    private readonly IProjectResourceRepository _projectResourceRepo;
    private readonly IOrganizationResourceRepository _organizationResourceRepo;
    private readonly IOrganizationRepository _organizationRepo;
    private readonly IOrganizationToProjectTransactionHistoryRepository _orgToPrj;
    private readonly IUserToProjectTransactionHistoryRepository _userToPrj;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IWalletRepository walletRepository, IUserWalletTransactionService userWalletTransactionService,
        IMapper mapper,
        IUserToOrganizationTransactionHistoryRepository userToOrg,
        IOrganizationToProjectTransactionHistoryRepository organizationToPrj,
        IProjectResourceRepository projectResourceRepo, IOrganizationResourceRepository organizationResourceRepo,
        IOrganizationRepository organizationRepo, IOrganizationToProjectTransactionHistoryRepository orgToPrj,
        IUserToProjectTransactionHistoryRepository userToPrj, ILogger<WalletService> logger)
    {
        OrganizationToPrj = organizationToPrj;
        _walletRepository = walletRepository;
        _userWalletTransactionService = userWalletTransactionService;
        _mapper = mapper;
        _userToOrg = userToOrg;
        _projectResourceRepo = projectResourceRepo;
        _organizationResourceRepo = organizationResourceRepo;
        _organizationRepo = organizationRepo;
        _orgToPrj = orgToPrj;
        _userToPrj = userToPrj;
        _logger = logger;
    }

    /**
     * Use this user first visit the wallet page
     */
    public async Task<Wallet> CreateEmptyWalletAsync(Guid userId)
    {
        var wallet = new Wallet
        {
            Amount = 0,
            UserId = userId
        };
        return await _walletRepository.CreateWalletAsync(wallet);
    }

    /**
     * User this for one time action like donate to project without have to top up first. Make sure to update other table as well
     */
    public async Task<Wallet> CreateWalletWithAmountAsync(Guid userId, int amount)
    {
        var wallet = new Wallet
        {
            Amount = amount,
            UserId = userId
        };
        return await _walletRepository.CreateWalletAsync(wallet);
    }

    public async Task<Wallet?> FindUserWalletByIdAsync(Guid userId)
    {
        return await _walletRepository.GetWalletAsync(w => w.UserId == userId);
    }

    /**
     * WARNING: ONLY USE THIS IF THE WALLET YOU PASSED IN IS MAPPED FROM DTO, USE THE REPOSITORY FOR UPDATE IF THE WALLET IS RETRIED FROM DATABASE
     */
    public async Task UpdateWalletAsync(Wallet wallet, int? newAmount = null)
    {
        if (newAmount.HasValue) wallet.Amount = newAmount.Value;
        await _walletRepository.UpdateWalletAsync(wallet);
    }

    /**
     * Add new amount to user wallet <br/>
     * Then, add a transaction to user wallet transactions <br/>
     * Note that this method do not concern with user to prj / org transactions.
     */
    public async Task<Wallet> TopUpWalletAsync(Guid userId, int amount, string? msg = null, Guid? transactionId = null)
    {
        var userWallet = await FindUserWalletByIdAsync(userId);
        if (userWallet == null) throw new Exception("User wallet not found");
        userWallet.Amount += amount;
        await UpdateWalletAsync(userWallet);
        await _userWalletTransactionService.AddNewTransactionAsync(new UserWalletTransaction
        {
            TransactionId = transactionId ?? Guid.NewGuid(),
            WalletId = userWallet.WalletId,
            Amount = amount,
            Message = msg ?? $"Topped up {amount:N0} VND to Dynamics digital wallet",
            TransactionType = TransactionConstants.TopUp,
            Time = DateTime.Now,
        });
        return userWallet;
    }

    public async Task<Wallet> SpendWalletAsync(Guid userId, int amount, string? msg = null, Guid? transactionId = null)
    {
        var userWallet = await FindUserWalletByIdAsync(userId);
        if (userWallet == null) throw new Exception("User wallet not found");
        var newAmount = userWallet.Amount - amount;
        if (newAmount < 0) throw new Exception("Something went wrong please try again");
        userWallet.Amount -= amount;
        await UpdateWalletAsync(userWallet);
        await _userWalletTransactionService.AddNewTransactionAsync(new UserWalletTransaction
        {
            TransactionId = transactionId ?? Guid.NewGuid(),
            WalletId = userWallet.WalletId,
            Amount = amount,
            Message = msg ?? $"Spent {amount:N0} VND from Dynamics wallet",
            TransactionType = TransactionConstants.Donate,
            Time = DateTime.Now,
        });
        return userWallet;
    }

    public async Task RefundProjectWalletAsync(Project project)
    {
        var resource = await _projectResourceRepo.GetAsync(pr => pr.ProjectID == project.ProjectID && pr.ResourceName.Equals("Money"));
        var transList = await _userToPrj.GetAllAsyncWithExpression(utp => utp.ProjectResourceID == resource.ResourceID);
        foreach (var trans in transList)
        {
            var userWallet = await FindUserWalletByIdAsync(trans.UserID);
            if (userWallet == null) throw new Exception("User wallet not found");
            userWallet.Amount += trans.Amount;
            await UpdateWalletAsync(userWallet);
            await _userWalletTransactionService.AddNewTransactionAsync(new UserWalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = userWallet.WalletId,
                Amount = trans.Amount,
                Message = $"Refunded {trans.Amount:N0} VND to your Dynamics wallet because project {project.ProjectName} is shutting down",
                TransactionType = TransactionConstants.Refund,
                Time = DateTime.Now,
            });
        }
    }

    public async Task RefundOrganizationWalletAsync(Organization organization)
    {
        var resource =
            await _organizationResourceRepo.GetAsync(or =>
                or.OrganizationID == organization.OrganizationID && or.ResourceName.Equals("Money"));
        var transList = await _userToOrg.GetAllAsyncWithExpression(uto => uto.ResourceID == resource.ResourceID);
        var sortedTransList = transList.OrderByDescending(t => t.Time).ToList();
        var cumulativeAmount = 0;
        var resultList = new List<UserToOrganizationTransactionHistory>();
        var overflowAmount = 0;
        // only take the transactions that add up to the remaining amount in organization (sort from newest)
        foreach (var trans in sortedTransList)
        {
            if (cumulativeAmount + trans.Amount >= resource.Quantity)
            {
                var remaining = resource.Quantity - cumulativeAmount;
                trans.Amount = remaining;
                resultList.Add(trans);
                break;
            }

            cumulativeAmount += trans.Amount;
            resultList.Add(trans);
        }
        // refund for those transactions
        foreach (var trans in resultList)
        {
            var userWallet = await FindUserWalletByIdAsync(trans.UserID);
            if (userWallet == null) throw new Exception("User wallet not found");
            userWallet.Amount += trans.Amount;
            await UpdateWalletAsync(userWallet);
            await _userWalletTransactionService.AddNewTransactionAsync(new UserWalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = userWallet.WalletId,
                Amount = trans.Amount,
                Message = $"Refunded {trans.Amount:N0} VND to your Dynamics wallet because organization {organization.OrganizationName} is shutting down",
                TransactionType = TransactionConstants.Refund,
                Time = DateTime.Now,
            });
        }
    }

    public List<UserWalletTransactionVM> MapToListUserWalletTransactionVMs(
        List<UserWalletTransaction> userWalletTransactions)
    {
        var result = _mapper.Map<List<UserWalletTransactionVM>>(userWalletTransactions);
        return result;
    }

    public PayRequestDto SetupPayRequestDto(PayRequestDto payRequestDto)
    {
        if (payRequestDto.ResourceID.Equals(Guid.Empty))
            throw new Exception("PAYMENT: YOU FORGOT TO ASSIGN THE MONEY RESOURCE ID");
        if (payRequestDto.TargetId.Equals(Guid.Empty))
            throw new Exception("PAYMENT: TARGET ID IS NULL, PLS CHECK AGAIN");
        if (payRequestDto.TargetType == null)
            throw new Exception("PAYMENT: TARGET TYPE IS NULL, PLEASE CHOOSE EITHER ORGANIZATION OR PROJECT");
        // Set up our request Dto:
        payRequestDto.TransactionID = Guid.NewGuid();
        payRequestDto.Time = DateTime.Now;
        payRequestDto.Message ??= $"Donated {payRequestDto.Amount:N0} VND";
        payRequestDto.Status = 0; // Always has the status of accepted (If error we caught it before)
        return payRequestDto;
    }

    // /**
    //  * Things to do
    //  * There are in total 3 types of donation, when an action is taken, add it to history and add it manually in its resource table
    //  * Target type: project - user to prj, organization: user to organization, allocation: organization to project
    //  * User to organization:
    //  *
    //  * User to project
    //  * Organization to project
    //  */
    public async Task AddTransactionToDatabaseAsync(PayRequestDto payRequestDto)
    {
        try
        {
            if (payRequestDto.TargetType.Equals(MyConstants.Project))
            {
                await UpdateUserToProject(payRequestDto);
            }
            else if (payRequestDto.TargetType.Equals(MyConstants.Organization))
            {
                await UpdateUserToOrganization(payRequestDto);
            }
            else
            {
                await UpdateOrganizationToProject(payRequestDto);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task UpdateUserToProject(PayRequestDto payRequestDto)
    {
        // User to prj
        var result = _mapper.Map<UserToProjectTransactionHistory>(payRequestDto);
        // Things to map: FromID => UserID,  ResourceID => ProjectResourceId,
        // We don't need targetId because the transaction is linked with resource id
        result.UserID = payRequestDto.FromID;
        result.ProjectResourceID = payRequestDto.ResourceID;
        result.Time = DateOnly.FromDateTime(payRequestDto.Time); // Manual convert because of... Time
        result.Status = 1;
        // Insert to the history table
        await _userToPrj.AddUserDonateRequestAsync(result);
        // Use Huyen's donate resource method to handle
        await _userToPrj.AcceptUserDonateRequestAsync(result);
    }

    private async Task UpdateUserToOrganization(PayRequestDto payRequestDto)
    {
        var result = _mapper.Map<UserToOrganizationTransactionHistory>(payRequestDto);
        // Map stuff
        result.UserID = payRequestDto.FromID;
        result.Time = DateOnly.FromDateTime(payRequestDto.Time); // Manual convert because of... Time
        result.Status = 1;
        // Insert to history
        await _userToOrg.AddAsync(result);
        // Add the money to organization resource
        var targetResource = await _organizationResourceRepo.GetAsync(or => or.ResourceID == payRequestDto.ResourceID);
        if (targetResource == null) throw new Exception("THIS ORGANIZATION DON'T HAVE RESOURCE MONEY ?");
        targetResource.Quantity += payRequestDto.Amount;
        await _organizationResourceRepo.UpdateAsync(targetResource);
        // Done
    }

    /**
     * We get both Organization money resource and project as well to perform calculations on them
     */
    private async Task UpdateOrganizationToProject(PayRequestDto payRequestDto)
    {
        // org to prj
        var result = _mapper.Map<OrganizationToProjectHistory>(payRequestDto);
        result.Time = DateOnly.FromDateTime(payRequestDto.Time); // Manual convert because of... Time
        result.Status = 1;
        // No need to set the fromId here because it already has the org resource ID which links to the organization

        var projectMoneyResource = await _projectResourceRepo.GetAsync(pr =>
            pr.ProjectID == payRequestDto.TargetId &&
            pr.ResourceName.ToLower().Equals("money"));
        var organizationMoneyResource =
            await _organizationResourceRepo.GetAsync(or => or.ResourceID == payRequestDto.ResourceID);
        if (organizationMoneyResource == null)
            throw new Exception("PAYMENT: Organization resource not found (Is this one lacking money?)");
        if (projectMoneyResource == null)
            throw new Exception("PAYMENT: Project resource not found (Is this one lacking money?)");

        result.OrganizationResourceID = payRequestDto.ResourceID; // Pay request holds the organizationResource ID
        result.ProjectResourceID = projectMoneyResource.ResourceID; // The resourceId we searched for 

        // Insert to database:
        await _orgToPrj.AddAsync(result);

        // Update in project resource and organization resource
        // One gains, another one loss
        projectMoneyResource.Quantity += payRequestDto.Amount;
        organizationMoneyResource.Quantity -= payRequestDto.Amount;
        await _projectResourceRepo.UpdateResourceTypeAsync(projectMoneyResource);
        await _organizationResourceRepo.UpdateAsync(organizationMoneyResource);
        // Done
    }
}