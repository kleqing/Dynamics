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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using Aspose.Cells;

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly IAdminRepository _adminRepository = null;

        public UsersController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        // GET: Admin/Users
        // View list of users in the database
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                var users = await _adminRepository.ViewUser();
                return View(users);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Ban a user using ajax
        [HttpPost]
        public async Task<JsonResult> BanUser(Guid id)
        {
            var result = await _adminRepository.BanUserById(id);
            return Json(new
            {
                isBanned = result
            });
        }

        // Gain user role as admin or admin to user using ajax
        [HttpPost]
        public async Task<JsonResult> UserAsAdmin(Guid id)
        {
            await _adminRepository.ChangeUserRole(id);

            var userRole = await _adminRepository.GetUserRole(id);

            return Json(new
            {
                isAdmin = userRole == RoleConstants.Admin
            });
        }

        // Export user to excel file
        public async Task<IActionResult> Export()
        {
            var listUser = await _adminRepository.ViewUser();

            using (var package = new ExcelPackage())
            {
                var workSheet = package.Workbook.Worksheets.Add("Users");

                workSheet.Cells[1, 1].Value = "List User";
                workSheet.Cells["A1:I1"].Merge = true;
                workSheet.Cells[1, 1].Style.Font.Size = 14;
                workSheet.Cells[1, 1].Style.Font.Bold = true;
                workSheet.Cells["A1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                workSheet.Cells["A1:GI"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                workSheet.Cells[2, 1].Value = "Full Name";
                workSheet.Cells[2, 2].Value = "Email";
                workSheet.Cells[2, 3].Value = "Phone";
                workSheet.Cells[2, 4].Value = "Address";
                workSheet.Cells[2, 5].Value = "DoB";
                workSheet.Cells[2, 6].Value = "Description";
                workSheet.Cells[2, 7].Value = "Created date";
                workSheet.Cells[2, 8].Value = "Status";
                workSheet.Cells[2, 9].Value = "Role";


                using (var range = workSheet.Cells["A2:I2"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                int recordIndex = 3;
                foreach (var user in listUser)
                {
                    workSheet.Cells[recordIndex, 1].Value = user.UserFullName;
                    workSheet.Cells[recordIndex, 2].Value = user.UserEmail;
                    workSheet.Cells[recordIndex, 3].Value = user.UserPhoneNumber;
                    workSheet.Cells[recordIndex, 4].Value = user.UserAddress;
                    workSheet.Cells[recordIndex, 5].Value = user.UserDOB;
                    workSheet.Cells[recordIndex, 6].Value = user.UserDescription;
                    workSheet.Cells[recordIndex, 7].Value = user.CreatedDate.ToString();
                    workSheet.Cells[recordIndex, 8].Value = user.isBanned == true ? "Banned" : "Active";
                    workSheet.Cells[recordIndex, 9].Value = user.UserRole == "admin" ? "Admin" : "User";
                    recordIndex++;
                }

                workSheet.Cells.AutoFitColumns(0);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                
                string excelName = $"User_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }

        }
    }
}
