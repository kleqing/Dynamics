using Aspose.Cells;
using Dynamics.Areas.Admin.Models;
using Dynamics.Areas.Admin.Ultility;
using Dynamics.DataAccess.Repository;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class HomeController : Controller
    {
        public readonly IAdminRepository _adminRepository;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(IAdminRepository adminRepository, SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
            _adminRepository = adminRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Check if the user is in the Admin role
            if (User.IsInRole(RoleConstants.Admin))
            {
                // Get top 5 user and organization
                var top5User = await _adminRepository.GetTop5User();
                var top5Organization = await _adminRepository.GetTop5Organization();

                // View recent item (request)
                var recentRequest = await _adminRepository.ViewRecentItem();

                // Count user
                var userCount = await _adminRepository.CountUser();

                // Count organization
                var organizationCount = await _adminRepository.CountOrganization();

                // Count request
                var requestCount = await _adminRepository.CountRequest();

                // Count project
                var projectCount = await _adminRepository.CountProject();

                var model = new Dashboard
                {
                    // Count of user, organization, request, project in database
                    UserCount = userCount,
                    OrganizationCount = organizationCount,
                    RequestCount = requestCount,
                    ProjectCount = projectCount,

                    // View recent request (Recent item in dashoard page)
                    GetRecentRequest = recentRequest,

                    // List of top 5 user and organization
                    TopUser = top5User,
                    TopOrganization = top5Organization,
                };

                return View(model);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<ActionResult> ExportTop5User()
        {
            // Get top 5 user
            var top5User = await _adminRepository.GetTop5User();

            // using EPPlus to create excel file
            using (var package = new ExcelPackage())
            {
                // Add worksheet
                var workSheet = package.Workbook.Worksheets.Add("Top 5 Users");

                // Add header to the worksheet
                workSheet.Cells[1, 1].Value = "Top 5 User"; // cell 1
                workSheet.Cells["A1:D1"].Merge = true; // merge cell
                workSheet.Cells[1, 1].Style.Font.Size = 14;
                workSheet.Cells[1, 1].Style.Font.Bold = true;
                workSheet.Cells["A1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells["A1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                workSheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Add header content to the worksheet
                workSheet.Cells[2, 1].Value = "Full Name";
                workSheet.Cells[2, 2].Value = "Email";
                workSheet.Cells[2, 3].Value = "Phone Number";
                workSheet.Cells[2, 4].Value = "Address";

                using (var range = workSheet.Cells["A2:D2"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Add user data to the worksheet
                int recordIndex = 3;
                foreach (var user in top5User)
                {
                    workSheet.Cells[recordIndex, 1].Value = user.UserFullName;
                    workSheet.Cells[recordIndex, 2].Value = user.UserEmail;
                    workSheet.Cells[recordIndex, 3].Value = user.UserPhoneNumber;
                    workSheet.Cells[recordIndex, 4].Value = user.UserAddress;
                    recordIndex++;
                }

                // Auto fit column width
                workSheet.Cells.AutoFitColumns(0);

                // Save the excel file using MemoryStream
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Top5User_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        // For other exporting, the process is similar above
        public async Task<ActionResult> ExportTop5Organization()
        {
            var top5Organization = await _adminRepository.GetTop5Organization();

            using (var package = new ExcelPackage())
            {
                var workSheet = package.Workbook.Worksheets.Add("Top 5 Organizations");

                workSheet.Cells[1, 1].Value = "Organization Name";

                using (var range = workSheet.Cells["A1:A1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int recordIndex = 2;
                foreach (var organization in top5Organization)
                {
                    workSheet.Cells[recordIndex, 1].Value = organization.OrganizationName;
                    recordIndex++;
                }

                workSheet.Cells.AutoFitColumns(0);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Top5Organization_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public async Task<ActionResult> ExportRecentActivity()
        {
            var recentRequest = await _adminRepository.ViewRecentItem();

            using (var package = new ExcelPackage())
            {
                var workSheet = package.Workbook.Worksheets.Add("Recent Activity");

                workSheet.Cells[1, 1].Value = "Recent Activity";
                workSheet.Cells["A1:D1"].Merge = true;
                workSheet.Cells[1, 1].Style.Font.Size = 14;
                workSheet.Cells[1, 1].Style.Font.Bold = true;
                workSheet.Cells["A1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                workSheet.Cells[2, 1].Value = "Request Name";
                workSheet.Cells[2, 2].Value = "Request Description";
                workSheet.Cells[2, 3].Value = "Request Date";
                workSheet.Cells[2, 4].Value = "Requested by";

                using (var range = workSheet.Cells["A2:D2"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                int recordIndex = 3;
                foreach (var request in recentRequest)
                {
                    workSheet.Cells[recordIndex, 1].Value = request.RequestTitle;
                    workSheet.Cells[recordIndex, 2].Value = request.Content;
                    workSheet.Cells[recordIndex, 3].Value = request.CreationDate;
                    workSheet.Cells[recordIndex, 4].Value = request.User.UserFullName;
                    recordIndex++;
                }

                workSheet.Cells.AutoFitColumns(0);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"RecentActivity_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);

            }
        }
    }
}
