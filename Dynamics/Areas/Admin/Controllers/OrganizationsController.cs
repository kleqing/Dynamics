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
using OfficeOpenXml;
using Aspose.Cells;
using Dynamics.Services;

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class OrganizationsController : Controller
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IWalletService _walletService;
        private readonly INotificationService _notificationService;

        public OrganizationsController(IAdminRepository adminRepository, IWalletService walletService, INotificationService notificationService)
        {
            _adminRepository = adminRepository;
            _walletService = walletService;
            _notificationService = notificationService;
        }

        // GET: Admin/Organizations
        // View list of organizations in the database
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                return View(await _adminRepository.ViewOrganization());
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Change organization status
        [HttpPost]
        public async Task<JsonResult> ChangeStatus(Guid id)
        {
            var result = await _adminRepository.ChangeOrganizationStatus(id);
            var org = await _adminRepository.GetOrganization(o => o.OrganizationID == id);
            if (result == -1) await _walletService.RefundOrganizationWalletAsync(org);
            var link = Url.Action(
                action: "Detail",
                controller: "Organization",
                values: new { area = "", organizationId = id }, // Set `area` to an empty string
                protocol: Request.Scheme
            );
            if (result == 1)
            {
                await _notificationService.AdminVerificationNotificationAsync(id, link, "ApproveOrg");
            }
            else if (result == -1)
            {
                await _notificationService.AdminVerificationNotificationAsync(id, link, "BanOrg");
            }
            return Json(new
            {
                Status = result
            });
        }

        // Export function, same as HomeController.cs
        public async Task<IActionResult> Export()
        {
            var listOrganization = await _adminRepository.ViewOrganization();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Organizations");

                worksheet.Cells[1, 1].Value = "List Organization";
                worksheet.Cells["A1:H1"].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells["A1:H1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[2, 1].Value = "Organization Name";
                worksheet.Cells[2, 2].Value = "Organization Description";
                worksheet.Cells[2, 3].Value = "Organization Email";
                worksheet.Cells[2, 4].Value = "Phone Number";
                worksheet.Cells[2, 5].Value = "Address";
                worksheet.Cells[2, 6].Value = "Start Time";
                worksheet.Cells[2, 7].Value = "Shutdown Day";
                worksheet.Cells[2, 8].Value = "Status";

                using (var range = worksheet.Cells["A2:H2"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                int recordIndex = 3;
                foreach (var organization in listOrganization)
                {
                    worksheet.Cells[recordIndex, 1].Value = organization.OrganizationName;
                    worksheet.Cells[recordIndex, 2].Value = organization.OrganizationDescription;
                    worksheet.Cells[recordIndex, 3].Value = organization.OrganizationEmail;
                    worksheet.Cells[recordIndex, 4].Value = organization.OrganizationPhoneNumber;
                    worksheet.Cells[recordIndex, 5].Value = organization.OrganizationAddress;
                    worksheet.Cells[recordIndex, 6].Value = organization.StartTime.ToString();
                    worksheet.Cells[recordIndex, 7].Value = organization.ShutdownDay.ToString();
                    worksheet.Cells[recordIndex, 8].Value = organization.OrganizationStatus switch
                    {
                        1 => "Active",
                        0 => "Pending accept",
                        -1 => "Inactive/Banned",
                    };
                    recordIndex++;
                }

                worksheet.Cells.AutoFitColumns(0);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Organization_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
    }
}
