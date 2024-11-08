using Microsoft.AspNetCore.Mvc;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;

namespace Dynamics.Controllers;

public class WithdrawController : Controller
{
    private readonly IWithdrawRepository _withdrawRepository;

    public WithdrawController(IWithdrawRepository withdrawRepository)
    {
        _withdrawRepository = withdrawRepository;
    }
    [HttpPost]
    public async Task<JsonResult> CreateWithdraw(string projectid, string bankAccountNumber, string bankId, string message)
    {
        try
        {
            var newWithdraw = new Withdraw
            {
                WithdrawID = new Guid(),
                ProjectID = new Guid(projectid),
                BankAccountNumber = bankAccountNumber,
                BankName = bankId,
                Message = message,
                Time = DateTime.Now
            };

            await _withdrawRepository.AddAsync(newWithdraw);

            return Json(new { success = true, message = "Withdraw request created successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred: " + ex.Message });
        }
    }
}