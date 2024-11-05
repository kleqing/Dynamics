using System.Security.Cryptography;
using AutoMapper;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Utility;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;

namespace Dynamics.Services;

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _configuration;
    private readonly IUserToProjectTransactionHistoryRepository _userToPrj;
    private readonly IUserToOrganizationTransactionHistoryRepository _userToOrg;
    private readonly IOrganizationToProjectTransactionHistoryRepository _organizationToPrj;
    private readonly IMapper _mapper;
    private readonly IProjectResourceRepository _projectResourceRepo;
    private readonly IOrganizationResourceRepository _organizationResourceRepo;
    private readonly IOrganizationRepository _organizationRepo;
    private readonly IOrganizationToProjectTransactionHistoryRepository _orgToPrj;
    private readonly VnPayLibrary _vnpay;

    public VnPayService(IConfiguration configuration, IUserToProjectTransactionHistoryRepository userToPrj,
        IUserToOrganizationTransactionHistoryRepository userToOrg,
        IOrganizationToProjectTransactionHistoryRepository organizationToPrj, IMapper mapper,
        IProjectResourceRepository projectResourceRepo, IOrganizationResourceRepository organizationResourceRepo,
        IOrganizationRepository organizationRepo, IOrganizationToProjectTransactionHistoryRepository orgToPrj)
    {
        _configuration = configuration;
        _userToPrj = userToPrj;
        _userToOrg = userToOrg;
        _organizationToPrj = organizationToPrj;
        _mapper = mapper;
        _projectResourceRepo = projectResourceRepo;
        _organizationResourceRepo = organizationResourceRepo;
        _organizationRepo = organizationRepo;
        _orgToPrj = orgToPrj;
        _vnpay = new VnPayLibrary();
    }
    
    public string CreatePaymentUrl(HttpContext context, VnPayCreatePaymentDto model)
    {
        // Setup configuration
        string vnp_Url = _configuration["VnPay:VnPayUrl"];
        string vnp_TmnCode = _configuration["VnPay:vnp_TmnCode"];
        string vnp_HashSecret = _configuration["VnPay:vnp_HashSecret"];
        if (vnp_Url == null || vnp_TmnCode == null || vnp_HashSecret == null)
        {
            throw new Exception("PAYMENT: VnPay Url and VnPay Request are not found in appsettings.json.");
        }

        string vnp_Version = _configuration["VnPay:vnp_Version"];
        string vnp_Command = _configuration["VnPay:vnp_Command"];
        string vnp_locale = _configuration["VnPay:vnp_Locale"];
        string vnp_CurrCode = _configuration["VnPay:vnp_CurrCode"];
        string vnp_OrderType = _configuration["VnPay:vnp_OrderType"];
        string returnUrl = _configuration["VnPay:Return_Url"];
        _vnpay.AddRequestData("vnp_Version", vnp_Version);
        _vnpay.AddRequestData("vnp_Command", vnp_Command);
        _vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode); // Merchant code
        _vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString());
        _vnpay.AddRequestData("vnp_CreateDate",
            model.Time.ToString("yyyyMMddHHmmss")); // Be careful as this one is date time, not date only
        _vnpay.AddRequestData("vnp_CurrCode", vnp_CurrCode);
        // var ip = context.Connection.RemoteIpAddress.ToString();
        var ip = "13.160.92.202"; // We should include guest's ip here but whatevers
        _vnpay.AddRequestData("vnp_IpAddr", ip);
        if (!string.IsNullOrEmpty(vnp_locale))
        {
            _vnpay.AddRequestData("vnp_Locale", vnp_locale);
        }
        else
        {
            _vnpay.AddRequestData("vnp_Locale", "vn");
        }

        _vnpay.AddRequestData("vnp_OrderInfo", model.Message); // Our message
        _vnpay.AddRequestData("vnp_OrderType", vnp_OrderType); // Indicate money for VNPay
        _vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
        _vnpay.AddRequestData("vnp_TxnRef", model.TransactionId.ToString()); // Our transaction id
        var paymentUrl = _vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        return paymentUrl;
    }

    public VnPayResponseDto ExtractPaymentResult(IQueryCollection collection)
    {
        var vnpay = new VnPayLibrary();
        foreach (var (key, value) in collection)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value.ToString());
            }
        }

        // check valid first
        var vnp_ResponseCode =
            collection.FirstOrDefault(key => key.Key == "vnp_ResponseCode")
                .Value; // Get response code to determine the status
        // Check if the payment is NOT correct
        if (IsValidPayment(collection))
        {
            return new VnPayResponseDto()
            {
                Success = false,
                VnPayResponseCode = vnp_ResponseCode.ToString(),
            };
        }

        // These are what we need for our transaction
        var transactionID = (vnpay.GetResponseData("vnp_TxnRef")); // Our transaction id
        var vnp_OrderInfo = collection.FirstOrDefault(key => key.Key == "vnp_OrderInfo").Value;
        var vnp_Amount =
            int.Parse(collection.FirstOrDefault(key => key.Key == "vnp_Amount").Value) /
            100; // Minus 100 bc VnPay * 100 before

        // Parse date
        var vnp_PayDate = collection.FirstOrDefault(key => key.Key == "vnp_PayDate").Value.ToString();
        DateTime date = DateTime.ParseExact(vnp_PayDate, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);

        return new VnPayResponseDto()
        {
            TransactionID = new Guid(transactionID),
            Success = true,
            Message = vnp_OrderInfo.ToString(), // Should never be null
            VnPayResponseCode = vnp_ResponseCode.ToString(),
            Amount = vnp_Amount,
            Time = date,
        };
    }

    private bool IsValidPayment(IQueryCollection collection)
    {
        var vnp_SecureHash = collection.FirstOrDefault(key => key.Key == "vnp_SecureHash").Value;
        string vnp_HashSecret = _configuration["VnPay:vnp_HashSecret"];
        bool checkSignature = _vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
        return checkSignature;
    }
}