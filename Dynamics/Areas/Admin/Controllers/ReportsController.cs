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

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class ReportsController : Controller
    {
        private readonly IAdminRepository _adminRepository;

        public ReportsController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        // GET: Admin/Reports
        // View list of reports in the database
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                return View(await _adminRepository.ViewReport());
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Export report to excel file
        public async Task<IActionResult> Export()
        {
            var listReport = await _adminRepository.ViewReport();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Reports");

                worksheet.Cells[1, 1].Value = "List Report";
                worksheet.Cells["A1:D1"].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells["A1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[2, 1].Value = "Reporter";
                worksheet.Cells[2, 2].Value = "Type";
                worksheet.Cells[2, 3].Value = "Reason";
                worksheet.Cells[2, 4].Value = "Report day";

                using (var range = worksheet.Cells["A2:D2"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                int recordIndex = 3;
                foreach (var reports in listReport)
                {
                    worksheet.Cells[recordIndex, 1].Value = reports.Reporter.UserFullName;
                    worksheet.Cells[recordIndex, 2].Value = reports.Type;
                    worksheet.Cells[recordIndex, 3].Value = reports.Reason;
                    worksheet.Cells[recordIndex, 4].Value = reports.ReportDate.ToString();
                    recordIndex++;
                }

                worksheet.Cells.AutoFitColumns(0);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Report_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
    }
}
