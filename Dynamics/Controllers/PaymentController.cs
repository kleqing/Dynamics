using Dynamics.Models.Models;
using Dynamics.Models.Models.Dto;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ILogger = Serilog.ILogger;

namespace Dynamics.Controllers;

// For testing purposes
[Authorize]
public class PaymentController : Controller
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IVnPayService _vnPayService;

    public PaymentController(ILogger<PaymentController> logger, IVnPayService vnPayService)
    {
        _logger = logger;
        _vnPayService = vnPayService;
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
    public IActionResult Pay(VnPayRequestDto payRequestDto, string? returnUrl = "~/")
    {
        // This return URL will be used to redirect user to a specific page after they click on the payment success button
        if (returnUrl != null)
        {
            HttpContext.Session.SetString("paymentRedirect", returnUrl);
        }
        payRequestDto = _vnPayService.InitVnPayRequestDto(HttpContext, payRequestDto);
        // Set pay request dto to the session so that we will use it later
        HttpContext.Session.Set("payment", payRequestDto);
        // Redirect user to the website for payment
        return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, payRequestDto));
    }

    [Authorize]
    public async Task<IActionResult> PaymentCallBack()
    {
        // Get the query from the request (VnPay passed these for us) and put it in the response dto
        var responseDto = _vnPayService.ExtractPaymentResult(Request.Query);
        var requestDto = HttpContext.Session.Get<VnPayRequestDto>("payment");
        HttpContext.Session.Remove("payment"); // Remove when not needed anymore
        if (responseDto == null || responseDto.VnPayResponseCode != "00")
        {
            TempData["message"] = "Payment failed, Error code: " + responseDto.VnPayResponseCode;
            return RedirectToAction(nameof(PaymentFailure), responseDto);
        }

        // Things to create:
        // A Transaction with transaction id, project resource id, user id, status = 1, amount, message, time
        try
        {
            await _vnPayService.AddTransactionToDatabaseAsync(requestDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
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