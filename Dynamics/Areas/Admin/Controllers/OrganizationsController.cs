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

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class OrganizationsController : Controller
    {
        private readonly IAdminRepository _adminRepository;

        public OrganizationsController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
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

        // Get all organization information
        [HttpGet]
        public async Task<JsonResult> GetOrganizationInfo(Guid id)
        {
            var organization = await _adminRepository.GetOrganizationInformation(o => o.OrganizationID == id);
            if (organization == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Organization not found"
                });
            }

            var memberCount = await _adminRepository.MemberJoinedOrganization(id); // Count member joined organization

            // return to js with ajax
            return Json(new
            {
                success = true,
                data = new
                {
                    organization.OrganizationName,
                    organization.OrganizationDescription,
                    organization.OrganizationEmail,
                    organization.OrganizationPhoneNumber,
                    organization.OrganizationAddress,
                    organization.StartTime,
                    organization.ShutdownDay,
                    memberCount = memberCount
                }
            });
        }

        // Change organization status
        [HttpPost]
        public async Task<JsonResult> ChangeStatus(Guid id)
        {
            var result = await _adminRepository.ChangeOrganizationStatus(id);
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
