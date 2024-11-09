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
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class ProjectsController : Controller
    {
        private readonly IAdminRepository _adminRepository = null;
        private readonly IWalletService _walletService; 

        public ProjectsController(IAdminRepository adminRepository, IWalletService walletService)
        {
            _adminRepository = adminRepository;
            _walletService = walletService;
        }

        // GET: Admin/Projects
        // View list of projects in the database
        public async Task<IActionResult> Index(int? status)
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                var projects = await _adminRepository.ViewProjects();

                if (status.HasValue)
                {
                    projects = projects.Where(c => c.ProjectStatus == status.Value).ToList();
                }
                return View(projects);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Admin/Projects/Details/5
        // View more details of a project
        public async Task<IActionResult> Details(Guid? id)
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                var project = await _adminRepository.GetProjectInfo(c => c.ProjectID == id);
                return View(project);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Change status of a project
        [HttpPost]
        public async Task<JsonResult> ChangeStatus(Guid id)
        {
            var result = await _adminRepository.BanProject(id);
            var project = await _adminRepository.GetProjects(p => p.ProjectID == id);
            if (result) await _walletService.RefundProjectWalletAsync(project);

            return Json(new
            {
                isBanned = result,
                projectStatus = project.ProjectStatus,

                endTime = project.EndTime?.ToString("MM/dd/yyyy")
            });
        }

        // Export list of projects to Excel file, function is the same as HomeContoller.cs
        public async Task<IActionResult> Export()
        {
            var listProject = await _adminRepository.ViewProjects();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Projects");

                worksheet.Cells[1, 1].Value = "List Project";
                worksheet.Cells["A1:K1"].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells["A1:K1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:K1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:K1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[2, 1].Value = "Project Name";
                worksheet.Cells[2, 2].Value = "Email";
                worksheet.Cells[2, 3].Value = "Phone number";
                worksheet.Cells[2, 4].Value = "Address";
                worksheet.Cells[2, 5].Value = "Start Time";
                worksheet.Cells[2, 6].Value = "End Time";
                worksheet.Cells[2, 7].Value = "Status";
                worksheet.Cells[2, 8].Value = "Description";
                worksheet.Cells[2, 9].Value = "Status";
                worksheet.Cells[2, 10].Value = "Shutdown Reason";

                using (var range = worksheet.Cells["A2:K2"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                int recordIndex = 3;
                foreach (var projects in listProject)
                {
                    worksheet.Cells[recordIndex, 1].Value = projects.ProjectName;
                    worksheet.Cells[recordIndex, 2].Value = projects.ProjectEmail;
                    worksheet.Cells[recordIndex, 3].Value = projects.ProjectPhoneNumber;
                    worksheet.Cells[recordIndex, 4].Value = projects.ProjectAddress;
                    worksheet.Cells[recordIndex, 5].Value = projects.StartTime.ToString();
                    worksheet.Cells[recordIndex, 6].Value = projects.EndTime.ToString();
                    worksheet.Cells[recordIndex, 7].Value = projects.ProjectStatus switch
                    {
                        -1 => "Canceled",
                        0 => "Preparing",
                        1 => "On-going",
                        2 => "Finished",
                    };
                    worksheet.Cells[recordIndex, 8].Value = projects.ProjectDescription;
                    worksheet.Cells[recordIndex, 9].Value = projects.isBanned == true ? "Banned" : "Active";
                    worksheet.Cells[recordIndex, 10].Value = projects.ShutdownReason;
                    recordIndex++;
                }

                worksheet.Cells.AutoFitColumns(0);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Project_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

    }
}