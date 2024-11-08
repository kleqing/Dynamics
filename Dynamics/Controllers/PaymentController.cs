using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Dynamics.Controllers;

// For testing purposes
[Authorize]
public class PaymentController : Controller
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IVnPayService _vnPayService;
    private readonly IWalletService _walletService;

    public PaymentController(ILogger<PaymentController> logger, IVnPayService vnPayService, IWalletService walletService)
    {
        _logger = logger;
        _vnPayService = vnPayService;
        _walletService = walletService;
    }

    // GET (Only for testing purposes)
    public IActionResult Index()
    {
        return View();
    }
    /**
     * 9704198526191432198
     * NGUYEN VAN A
     * 07/15
     */
    [HttpPost]
    [Authorize]
    public IActionResult Pay(PayRequestDto payRequestDto, string? returnUrl = "~/")
    {
        throw new Exception("This method is abandoned, go to wallet controller instead");
        // This return URL will be used to redirect user to a specific page after they click on the payment success button
        if (returnUrl != null)
        {
            HttpContext.Session.SetString("paymentRedirect", returnUrl);
        }
        // payRequestDto = _vnPayService.InitVnPayRequestDto(HttpContext, payRequestDto);
        // Set pay request dto to the session so that we will use it later
        HttpContext.Session.Set("payment", payRequestDto);
        
        var paymentDto = new VnPayCreatePaymentDto
        {
            Amount = payRequestDto.Amount,
            Message = payRequestDto.Message ?? "No message",
            TransactionId = payRequestDto.TransactionID
        };
        // Redirect user to the website for payment
        return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, paymentDto));
    }

    // After payment 
    [Authorize]
    public async Task<IActionResult> PaymentCallBack()
    {
        // Get the query from the request (VnPay passed these for us) and put it in the response dto
        var responseDto = _vnPayService.ExtractPaymentResult(Request.Query);
        var requestDto = HttpContext.Session.Get<VnPayCreatePaymentDto>("payment");
        HttpContext.Session.Remove("payment"); // Remove when not needed anymore
        if (responseDto == null || responseDto.VnPayResponseCode != "00")
        {
            TempData["message"] = "Payment failed, Error code: " + responseDto.VnPayResponseCode;
            return RedirectToAction(nameof(PaymentFailure), responseDto);
        }

        await _walletService.TopUpWalletAsync(requestDto.FromId, responseDto.Amount, responseDto.Message, responseDto.TransactionID);
        var payRequestDto = HttpContext.Session.Get<PayRequestDto>("donateRequest");
        if (payRequestDto != null)
        {
            return RedirectToAction("SpendDynamicsMoney", "Wallet");
        }

        TempData["message"] = "Payment Successful";
        return RedirectToAction(nameof(PaymentSuccess), responseDto);
    }

    public IActionResult PaymentSuccess(VnPayResponseDto resp)
    {
        return View(resp);
    }

    public IActionResult PaymentFailure(VnPayResponseDto resp)
    {
        return View(resp);
    }
}