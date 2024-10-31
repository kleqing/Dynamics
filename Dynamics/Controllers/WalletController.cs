using System.Security.Cryptography.X509Certificates;
using Aspose.Cells;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using ILogger = Serilog.ILogger;

namespace Dynamics.Controllers;

[Authorize]
public class WalletController : Controller
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletController> _logger;
    private readonly IVnPayService _vnPayService;
    private readonly IUserWalletTransactionService _userWalletTransactionService;
    private readonly IUserWalletTransactionRepository _userWalletTransactionRepository;
    private readonly IPagination _pagination;
    private readonly ISearchService _searchService;

    public WalletController(IWalletService walletService, ILogger<WalletController> logger, IVnPayService vnPayService,
        IUserWalletTransactionService userWalletTransactionService,
        IUserWalletTransactionRepository userWalletTransactionRepository, IPagination pagination,
        ISearchService searchService)
    {
        _walletService = walletService;
        _logger = logger;
        _vnPayService = vnPayService;
        _userWalletTransactionService = userWalletTransactionService;
        _userWalletTransactionRepository = userWalletTransactionRepository;
        _pagination = pagination;
        _searchService = searchService;
    }

    /**
     * 9704198526191432198
     * NGUYEN VAN A
     * 07/15
     */
    // View wallet and history
    [Authorize]
    public async Task<IActionResult> Index(SearchRequestDto searchRequestDto,
        PaginationRequestDto paginationRequestDto)
    {
        // Check if user has a wallet or not
        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) return Unauthorized();
        UserWalletVM walletVM = new UserWalletVM();
        try
        {
            // Create wallet if user does not have one
            var userWallet = await _walletService.FindUserWalletByIdAsync(user.Id) ??
                             await _walletService.CreateEmptyWalletAsync(user.Id);
            var transactionsQueryable =
                _userWalletTransactionRepository.GetUserWalletTransactionsQueryable(uwt =>
                    uwt.WalletId == userWallet.WalletId);
            var transactionsSearchParams =
                _searchService.GetUserWalletTransactionWithSearchParams(searchRequestDto, transactionsQueryable);
            var paginated = await _pagination.PaginateAsync(transactionsSearchParams, HttpContext, paginationRequestDto,
                searchRequestDto);
            var dtos = _walletService.MapToListUserWalletTransactionVMs(paginated);
            walletVM = new UserWalletVM
            {
                Wallet = userWallet,
                PaginationRequestDto = paginationRequestDto,
                SearchRequestDto = searchRequestDto,
                UserWalletTransactionsVM = dtos,
            };
            // Get and display list of user transactions
            return View(walletVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        return View(walletVM);
    }

    // Top up page
    [HttpGet]
    public IActionResult TopUp()
    {
        return View();
    }

    public IActionResult TopUpAndPay(int amount, string? returnUrl, PayRequestDto? payRequestDto, int payAmount)
    {
        if (returnUrl != null) 
        {
            HttpContext.Session.SetString("paymentRedirect", returnUrl); // For redirect
        }
        else
        {
            // Setup redirect to the page
            HttpContext.Session.SetString("paymentRedirect", Url.Action("Index", "Wallet", null, Request.Scheme));
        }

        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) throw new UnauthorizedAccessException();
        var createPaymentDto = new VnPayCreatePaymentDto
        {
            FromId = user.Id,
            Amount = amount,
            Message = $"Top up {amount} VND to {user.UserName} wallet.",
            Time = DateTime.Now,
            TransactionId = Guid.NewGuid()
        };
        // Setup session to resolve later
        HttpContext.Session.Set("payment", createPaymentDto);
        // Check if the donation is both top up and payment
        if (payRequestDto != null)
        {
            payRequestDto.Amount = payAmount;
            HttpContext.Session.Set("donateRequest", payRequestDto);
        }
        return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, createPaymentDto));
    }

    // Handle VN Payment, update user wallet
    [HttpPost]
    public IActionResult TopUp(int amount, string? returnUrl)
    {
        if (returnUrl != null) 
        {
            HttpContext.Session.SetString("paymentRedirect", returnUrl); // For redirect
        }
        else
        {
            // Setup redirect to the page
            HttpContext.Session.SetString("paymentRedirect", Url.Action("Index", "Wallet", null, Request.Scheme));
        }

        var user = HttpContext.Session.GetCurrentUser();
        if (user == null) throw new UnauthorizedAccessException();
        var createPaymentDto = new VnPayCreatePaymentDto
        {
            FromId = user.Id,
            Amount = amount,
            Message = $"Top up {amount} VND to {user.UserName} wallet.",
            Time = DateTime.Now,
            TransactionId = Guid.NewGuid()
        };
        // Setup session to resolve later
        HttpContext.Session.Set("payment", createPaymentDto);
        return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, createPaymentDto));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CheckIfWalletIsEnough([FromBody] CheckWalletDto checkWalletDto)
    {
        Wallet? userWallet = await _walletService.FindUserWalletByIdAsync(checkWalletDto.UserId);
        // This should not happen
        if (userWallet == null)
        {
            return new JsonResult(new
            {
                Status = "not_found",
                Message = "User wallet not found, redirect to index",
                checkWalletDto.UserId,
                checkWalletDto.AmountToDonate,
            });
        }

        if (userWallet.Amount - checkWalletDto.AmountToDonate < 0)
        {
            return new JsonResult(new
            {
                Status = "not_enough_money",
                checkWalletDto.UserId,
                checkWalletDto.AmountToDonate,
                CurrentBalance = userWallet.Amount,
            });
        }

        return new JsonResult(new
        {
            Status = "ok",
            checkWalletDto.UserId,
            checkWalletDto.AmountToDonate,
            CurrentBalance = userWallet.Amount,
        });
    }

    public async Task<IActionResult> SpendDynamicsMoney(PayRequestDto payRequestDto, string returnUrl = "~/")
    {
        try
        {
            // See if there is a payrequestDto in session
            var temp = HttpContext.Session.Get<PayRequestDto>("donateRequest");
            if (temp != null)
            {
                payRequestDto = temp;
                // CLEAR OR ELSE DISASTER
                HttpContext.Session.Remove("donateRequest");
            }
            returnUrl = HttpContext.Session.GetString("paymentRedirect") ?? returnUrl;
            
            // If you don't have a wallet, just go to user -> your wallet
            Wallet? userWallet = await _walletService.FindUserWalletByIdAsync(payRequestDto.FromID);
            // If the donation type is not allocation (from user to prj / org)
            if (!payRequestDto.TargetType.Equals(MyConstants.Allocation, StringComparison.OrdinalIgnoreCase))
            {
                userWallet = await _walletService.SpendWalletAsync(payRequestDto.FromID, payRequestDto.Amount);
            }

            payRequestDto = _walletService.SetupPayRequestDto(payRequestDto);
            await _walletService.AddTransactionToDatabaseAsync(payRequestDto);
            TempData[MyConstants.Success] = "Transaction success!";
            TempData[MyConstants.Subtitle] = "New balance: " + userWallet.Amount + "VND";
        }
        catch (Exception e)
        {
            TempData[MyConstants.Error] = "Transaction failed!";
            TempData[MyConstants.Subtitle] = e.Message;
        }

        return Redirect(returnUrl);
    }

    [HttpPost]
    public IActionResult RefundCoins(string refundCoinVM)
    {
        return View();
    }
}