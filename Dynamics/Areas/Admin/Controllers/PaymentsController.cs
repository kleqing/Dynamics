using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Dynamics.DataAccess.Repository;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Dynamics.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class PaymentsController : Controller
    {

        private readonly IAdminRepository _adminRepository;
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly IEmailSender _emailSender;

        public PaymentsController(IAdminRepository adminRepository, IWithdrawRepository withdrawRepository, IEmailSender emailSender)
        {
            _adminRepository = adminRepository;
            _withdrawRepository = withdrawRepository;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                
                var viewWithdraw = await _adminRepository.ReviewWithdraw(w => true);
                
                var withdraw = viewWithdraw.Where(r => r.Project.ProjectResource.FirstOrDefault().ResourceName == "Money")
                    .Select(r => new TransactionBase
                    {
                        WithdrawID = r.WithdrawID,
                        ProjectResourceID = r.Project.ProjectResource.FirstOrDefault().UserToProjectTransactionHistory.FirstOrDefault().ProjectResourceID,
                        Message = r.Message,
                        Time = r.Time,
                        Received = r.Project.ProjectName,
                        Description = r.Project.ProjectDescription,
                        Status = r.Status
                    })
                    .GroupBy(t => t.ProjectResourceID)
                    .Select(g => g.First())
                    .OrderByDescending(r => r.Time)
                    .ToList();
                
                var model = new Payment
                {
                    viewwithdraw = withdraw
                };

                return View(model);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }


        public async Task<IActionResult> Details(Guid? id)
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                var userToProject =
                    await _adminRepository.ViewUserToProjectTransactionInHistory(u => u.ProjectResource.Project.Withdraw.FirstOrDefault().WithdrawID == id);

                var projectID = userToProject.FirstOrDefault().ProjectResource.Project.ProjectID;

                var listResource = await _adminRepository.ViewUserToProjectResource(p => p.ProjectID == projectID);

                var listWithDraws = await _adminRepository.ReviewWithdraw(p => p.ProjectID == projectID);
                var withDraw = await _withdrawRepository.GetWithdraw(w => w.ProjectID == projectID);

                var userToProjectdetail = userToProject.FirstOrDefault();

                var countReport = await _adminRepository.CountProjectReport("Project", projectID);

                ViewBag.CountReport = countReport;

                var model = new Payment
                {
                    listUserToProject = new List<UserToProjectTransactionHistory> { userToProjectdetail },
                    listUserDonateToProjectTable = listResource,
                    listWithdraws = listWithDraws,
                    Withdraw = withDraw
                };

                return View("Details", model);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmWithdraw(Guid id)
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                var withdraw = await _withdrawRepository.GetWithdraw(w => w.WithdrawID == id);
                await _adminRepository.ChangeWithdrawStatus(id);
                await _emailSender.SendEmailAsync(withdraw.Project.ProjectEmail, "Withdraw success.",
                    $"Please check your bank account, the withdraw request has been approved. Thank you!");
            }
            return Json(new { success = true });
        }
        /*[HttpPost]
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
        }*/
    }
}
