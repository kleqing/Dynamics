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

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class PaymentsController : Controller
    {

        private readonly IAdminRepository _adminRepository;

        public PaymentsController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                var userTransactionToProject = await _adminRepository.ViewUserToProjectTransactionInHistory(u => true);

                var allTransaction = userTransactionToProject
                    .Where(p => p.ProjectResource.ResourceName == "Money")
                    .Select(p => new TransactionBase
                    {
                        TransactionID = p.TransactionID,
                        ProjectResourceID = p.ProjectResourceID,
                        Time = p.Time,
                        Message = p.ProjectResource.Project.ProjectDescription,
                        Received = p.ProjectResource.Project.ProjectName,
                        Description = p.ProjectResource.Project.ProjectDescription
                    })
                    .GroupBy(t => t.ProjectResourceID)
                    .Select(g => g.First())
                    .OrderByDescending(t => t.Time)
                    .ToList();

                var model = new Payment
                {
                    listTransaction = allTransaction
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
                    await _adminRepository.ViewUserToProjectTransactionInHistory(u => u.TransactionID == id);
                if (userToProject == null || !userToProject.Any())
                {
                    return NotFound();
                }

                var projectID = userToProject.FirstOrDefault().ProjectResource.Project.ProjectID;

                var listResource = await _adminRepository.ViewUserToProjectResource(p => p.ProjectID == projectID);

                var withDraws = await _adminRepository.ReviewWithdraw(p => p.ProjectID == projectID);

                var userToProjectdetail = userToProject.FirstOrDefault();

                var countReport = await _adminRepository.CountProjectReport("Project", projectID);

                ViewBag.CountReport = countReport;

                var model = new Payment
                {
                    listUserToProject = new List<UserToProjectTransactionHistory> { userToProjectdetail },
                    listUserDonateToProjectTable = listResource,
                    listWithdraws = withDraws
                };
                return View("Details", model);
            }
            else
            {
                    return RedirectToAction("Error", "Home");
                }
            }
    }
}
