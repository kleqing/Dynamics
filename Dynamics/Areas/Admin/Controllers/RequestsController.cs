﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Dynamics.DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;
using Dynamics.Utility;
using OfficeOpenXml;

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class RequestsController : Controller
    {
        private readonly IAdminRepository _adminRepository;

        public RequestsController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        // GET: Admin/Requests
        // View list of requests in the database
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                return View(await _adminRepository.ViewRequest());
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Change status of request
        [HttpPost]
        public async Task<JsonResult> ChangeStatus(Guid id, int status)
        {
            var result = await _adminRepository.ChangeRequestStatus(id);
            return Json(new
            {
                status = result
            });
        }

        // Delete request
        [HttpPost]
        public async Task<JsonResult> Delete(Guid id)
        {
            var request = await _adminRepository.DeleteRequest(id);
            if (request == null)
            {
                return Json(new { success = false }); // return delete notification to js with ajax
            }

            return Json(new { success = true }); 
        }

        // Export request to excel file
        public async Task<IActionResult> Export()
        {
            var listRequest = await _adminRepository.ViewRequest();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Requests");

                worksheet.Cells[1, 1].Value = "List Request";
                worksheet.Cells["A1:G1"].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells["A1:G1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[2, 1].Value = "Title";
                worksheet.Cells[2, 2].Value = "Content";
                worksheet.Cells[2, 3].Value = "Location";
                worksheet.Cells[2, 4].Value = "Phone Number";
                worksheet.Cells[2, 5].Value = "Emergency";
                worksheet.Cells[2, 6].Value = "Created Day";
                worksheet.Cells[2, 7].Value = "Status";

                using (var range = worksheet.Cells["A2:G2"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                int recordIndex = 3;
                foreach (var requests in listRequest)
                {
                    worksheet.Cells[recordIndex, 1].Value = requests.RequestTitle;
                    worksheet.Cells[recordIndex, 2].Value = requests.Content;
                    worksheet.Cells[recordIndex, 3].Value = requests.Location;
                    worksheet.Cells[recordIndex, 4].Value = requests.RequestPhoneNumber;
                    worksheet.Cells[recordIndex, 5].Value = requests.isEmergency == 1 ? "Emergency" : "None";
                    worksheet.Cells[recordIndex, 6].Value = requests.CreationDate.ToString();
                    worksheet.Cells[recordIndex, 7].Value = requests.Status switch
                    {
                        1 => "Accepted",
                        -1 => "Canceled",
                        0 => "Pending"
                    };
                    recordIndex++;
                }

                worksheet.Cells.AutoFitColumns(0);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Request_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        // Get request information using ajax
        [HttpGet]
        public async Task<JsonResult> GetRequestInformation(Guid id)
        {
            var request = await _adminRepository.GetRequestInfo(r => r.RequestID == id);
            if (request == null)
            {
                // request not found
                return Json(new
                    {
                        success = false,
                        message = "Request not found"
                    }
                );
            }

            // return request information to js with ajax
            return Json(new
            {
                success = true,
                data = new
                {
                    request.RequestTitle,
                    request.Content,
                    request.Location,
                    request.RequestEmail,
                    request.RequestPhoneNumber,
                    request.CreationDate,
                    Attachment = request.Attachment
                }
            });
        }
    }
}