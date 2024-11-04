using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Utility;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Web.Mvc;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;
using Color = System.Drawing.Color;

namespace Dynamics.Controllers
{
    public class ExcelExportController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IProjectRepository _projectRepo;

        public ExcelExportController(IProjectRepository projectRepository)
        {
            _projectRepo = projectRepository;
        }
        public ActionResult ExportExcel()
        {
            try
            {
                var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
                if (currentOrganization.OrganizationResource.Count == 1)
                {
                    TempData[MyConstants.Error] = "This organization has no resource to donate";
                    return RedirectToAction("ManageOrganizationResource", "Organization");
                }
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Organization Resource");

                    // Add header
                    worksheet.Cells["A1"].Value = "Resource Name";
                    worksheet.Cells["B1"].Value = "Quantity";
                    worksheet.Cells["C1"].Value = "Unit";
                    worksheet.Cells["D1"].Value = "Message";

                    // Style header
                    using (var range = worksheet.Cells["A1:D1"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    // Add data
                    int row = 2;
                 
                    foreach (var item in currentOrganization.OrganizationResource)
                    {
                        if (item.ResourceName.ToUpper().Equals("Money".ToUpper()))
                        {
                            continue;
                        }
                        worksheet.Cells[row, 1].Value = item.ResourceName;
                        worksheet.Cells[row, 2].Value = 0;
                        worksheet.Cells[row, 3].Value = item.Unit;
                        worksheet.Cells[row, 4].Value = "Message...";
                        row++;
                    }

                    worksheet.Cells.AutoFitColumns();

                    // Protect worksheet
                    worksheet.Protection.IsProtected = true;
                    worksheet.Protection.AllowSelectLockedCells = false;
                    worksheet.Cells.Style.Locked = true;
                    worksheet.Cells[2, 2, worksheet.Dimension.End.Row, 2].Style.Locked = false;
                    worksheet.Cells[2, 4, worksheet.Dimension.End.Row, 4].Style.Locked = false;
                    return File(
                        package.GetAsByteArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Donate_for_{@currentOrganization.OrganizationName}.xlsx"
                    );
                }
            }
            catch (Exception ex)
            {
                // Log error here
                return View("Error", new HandleErrorInfo(ex, "ExcelExport", "ExportExcel"));
            }
        }

        public async Task<IActionResult> ExportProjectExcel()
        {
            try
            {
                var currentProjectID = HttpContext.Session.GetString("currentProjectID");
                var currentProjectObj = await _projectRepo.GetProjectAsync(x => x.ProjectID.ToString().Equals(currentProjectID));
                if(currentProjectID == null || currentProjectObj == null)
                {
                    return RedirectToAction("SendDonateRequest", "Project", new { projectID = currentProjectID, donor = "User" });
                }
                if (currentProjectObj.ProjectResource.Count == 1)
                {
                    TempData[MyConstants.Error] = "This project has no resource to donate";
                    return RedirectToAction("SendDonateRequest", "Project", new { projectID = currentProjectID, donor = "User" });
                }
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Project Resource");

                    // Add header
                    worksheet.Cells["A1"].Value = "Resource Name";
                    worksheet.Cells["B1"].Value = "Quantity";
                    worksheet.Cells["C1"].Value = "Current Quantity";
                    worksheet.Cells["D1"].Value = "Expected quantity";
                    worksheet.Cells["E1"].Value = "Unit";
                    worksheet.Cells["F1"].Value = "Message";

                    // Style header
                    using (var range = worksheet.Cells["A1:F1"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    // Add data
                    int row = 2;
                 
                    foreach (var item in currentProjectObj.ProjectResource)
                    {
                        if (item.ResourceName.ToUpper().Equals("Money".ToUpper())||item.Quantity==item.ExpectedQuantity)
                        {
                            continue;
                        }
                        worksheet.Cells[row, 1].Value = item.ResourceName;
                        worksheet.Cells[row, 2].Value = 0;
                        worksheet.Cells[row, 3].Value = item.Quantity;
                        worksheet.Cells[row, 4].Value = item.ExpectedQuantity;
                        worksheet.Cells[row, 5].Value = item.Unit;
                        worksheet.Cells[row, 6].Value = "Message...";
                        row++;
                    }

                    worksheet.Cells.AutoFitColumns();

                    // Protect worksheet
                    worksheet.Protection.IsProtected = true;
                    worksheet.Protection.AllowSelectLockedCells = false;
                    worksheet.Cells.Style.Locked = true;
                    worksheet.Cells[2, 2, worksheet.Dimension.End.Row, 2].Style.Locked = false;
                    worksheet.Cells[2, 6, worksheet.Dimension.End.Row, 6].Style.Locked = false;
                    return File(
                        package.GetAsByteArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Donate_for_{@currentProjectObj.ProjectName}.xlsx"
                    );
                }
            }
            catch (Exception ex)
            {
                // Log error here
                return View("Error", new HandleErrorInfo(ex, "ExcelExport", "ExportProjectExcel"));
            }
        }

    }
}
