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


    public string CreatePaymentUrl(HttpContext context, VnPayRequestDto model)
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
        _vnpay.AddRequestData("vnp_TxnRef", model.TransactionID.ToString()); // Our transaction id
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

    public VnPayRequestDto InitVnPayRequestDto(HttpContext context, VnPayRequestDto payRequestDto)
    {
        if (payRequestDto.ResourceID == null)
            throw new Exception("PAYMENT: YOU FORGOT TO ASSIGN THE MONEY RESOURCE ID");
        if (payRequestDto.TargetId == null) throw new Exception("PAYMENT: TARGET ID IS NULL, PLS CHECK AGAIN");
        if (payRequestDto.TargetType == null)
            throw new Exception("PAYMENT: TARGET TYPE IS NULL, PLEASE CHOOSE EITHER ORGANIZATION OR PROJECT");
        // If not allocation, default id is user
        if (!payRequestDto.TargetType.Equals(MyConstants.Allocation))
        {
            User u = JsonConvert.DeserializeObject<User>(context.Session.GetString("user"));
            payRequestDto.FromID = u.UserID;
        }

        // Set up our request Dto:
        payRequestDto.TransactionID = Guid.NewGuid();
        payRequestDto.Time = DateTime.Now;
        payRequestDto.Message ??= $"Donated {payRequestDto.Amount} VND";
        payRequestDto.Status = 1; // Always has the status of accepted (If error we caught it before)
        return payRequestDto;
    }

    /**
     * Things to do
     * There are in total 3 types of donation, when an action is taken, add it to history and add it manually in its resource table
     * Target type: project - user to prj, organization: user to organization, allocation: organization to project
     * User to organization:
     *
     * User to project
     * Organization to project
     */
    public async Task AddTransactionToDatabaseAsync(VnPayRequestDto payRequestDto)
    {
        // payRequestDto.Amount /= 100; // Because of VNPay
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

    private async Task UpdateUserToProject(VnPayRequestDto payRequestDto)
    {
        // User to prj
        var result = _mapper.Map<UserToProjectTransactionHistory>(payRequestDto);
        // Things to map: FromID => UserID,  ResourceID => ProjectResourceId,
        // We don't need targetId because the transaction is linked with resource id
        result.UserID = payRequestDto.FromID;
        result.ProjectResourceID = payRequestDto.ResourceID;
        result.Time = DateOnly.FromDateTime(payRequestDto.Time); // Manual convert because of... Time
        // Insert to the history table
        await _userToPrj.AddUserDonateRequestAsync(result);
        // Use Huyen's donate resource method to handle
        await _userToPrj.AcceptUserDonateRequestAsync(result);
    }

    private async Task UpdateUserToOrganization(VnPayRequestDto payRequestDto)
    {
        var result = _mapper.Map<UserToOrganizationTransactionHistory>(payRequestDto);
        // Map stuff
        result.UserID = payRequestDto.FromID;
        result.Time = DateOnly.FromDateTime(payRequestDto.Time); // Manual convert because of... Time
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
     *
     */
    private async Task UpdateOrganizationToProject(VnPayRequestDto payRequestDto)
    {
        // org to prj
        var result = _mapper.Map<OrganizationToProjectHistory>(payRequestDto);
        result.Time = DateOnly.FromDateTime(payRequestDto.Time); // Manual convert because of... Time
        // No need to set the fromId here
        
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