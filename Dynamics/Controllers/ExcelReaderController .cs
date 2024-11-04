using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using Dynamics.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Utility;
using Newtonsoft.Json;
using Dynamics.DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;



namespace Dynamics.Controllers
{
    public class ExcelReaderController : Controller
    {

        IOrganizationRepository _organizationRepository;
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectResourceRepository _projectResourceRepo;
        private readonly IUserToProjectTransactionHistoryRepository _userToProjectTransactionHistoryRepo;
        private readonly CloudinaryUploader _cloudinaryUploader;

        public ExcelReaderController(IOrganizationRepository organizationRepository,
            IProjectRepository projectRepository,
            IProjectResourceRepository projectResourceRepository,
            IUserToProjectTransactionHistoryRepository userToProjectTransactionHistoryRepository,
            CloudinaryUploader cloudinaryUploader)
        {
            _organizationRepository = organizationRepository;
            _projectRepo = projectRepository;
            _projectResourceRepo = projectResourceRepository;
            _userToProjectTransactionHistoryRepo = userToProjectTransactionHistoryRepository;
            _cloudinaryUploader = cloudinaryUploader;
        }
        [Authorize]
        public async Task<ActionResult> Upload(IFormFile file, List<IFormFile> images)
        {
            if (file != null && file.Length > 0)
            {
                try
                {
                    var resImage = await _cloudinaryUploader.UploadMultiImagesAsync(images);
                    if (resImage.Equals("Wrong extension"))
                    {
                        TempData[MyConstants.Error] = "Wrong extension of some proof images file.";
                        return RedirectToAction("ManageOrganizationResource", "Organization");
                    }
                    else if (resImage.Equals("No file"))
                    {
                        TempData[MyConstants.Error] = "Please select at least a proof image to upload.";
                        return RedirectToAction("ManageOrganizationResource", "Organization");
                    }

                    var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

                    //Get current user
                    var userString = HttpContext.Session.GetString("user");
                    User currentUser = null;
                    if (userString != null)
                    {
                        currentUser = JsonConvert.DeserializeObject<User>(userString);
                    }
                    int isValidDonationFile = 0;
                    using (var package = new ExcelPackage(file.OpenReadStream()))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet != null)
                        {
                            int rowCount = worksheet.Dimension.Rows;

                            for (int row = 2; row <= rowCount; row++) // Assuming first row is header
                            {
                                var resource = new OrganizationResource()
                                {
                                    ResourceName = worksheet.Cells[row, 1].Value?.ToString(),
                                    Quantity = Convert.ToInt32(worksheet.Cells[row, 2].Value) >= 0 ? Convert.ToInt32(worksheet.Cells[row, 2].Value) : 0,
                                    Unit = worksheet.Cells[row, 3].Value?.ToString(),
                                };

                                if(resource.Quantity == 0)
                                {
                                    continue;
                                }

                                // get current resource
                                var currentResource = await _organizationRepository.GetOrganizationResourceAsync(or => or.ResourceName.Equals(resource.ResourceName) && or.Unit.Equals(resource.Unit));

                                var userToOrganizationTransactionHistory = new UserToOrganizationTransactionHistory()
                                {
                                    ResourceID = currentResource.ResourceID,
                                    UserID = currentUser.Id,
                                    Status = 0,
                                    Time = DateOnly.FromDateTime(DateTime.UtcNow),
                                    Amount = resource.Quantity,
                                    Message = worksheet.Cells[row, 4].Value?.ToString(),
                                    Attachments = resImage,
                                };

                                await _organizationRepository.AddUserToOrganizationTransactionHistoryASync(userToOrganizationTransactionHistory);
                                isValidDonationFile++;
                            }
                        }
                    }
                    if (isValidDonationFile == 0)
                    {
                        TempData[MyConstants.Error] = "There is no valid donation. Fail to send your donation request!";
                        return RedirectToAction("ManageOrganizationResource", "Organization");
                    }
                    TempData[MyConstants.Success] = "Send donate requests successfully.";
                    return RedirectToAction("ManageOrganizationResource", "Organization");
                }
                catch (Exception ex)
                {
                    //ViewBag.Error = "Error: " + ex.Message;
                     TempData[MyConstants.Error] = "Error: " + ex.Message;
                    return RedirectToAction("ManageOrganizationResource", "Organization");
                }
            }
             TempData[MyConstants.Error] = "Please select a file to upload.";
            //ViewBag.Error = "Please select a file to upload.";
            return RedirectToAction("ManageOrganizationResource", "Organization");
        }
        public async Task<ActionResult> UploadProjectExcel(IFormFile file,List<IFormFile> images)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var currentProjectObj = await _projectRepo.GetProjectAsync(x => x.ProjectID.ToString().Equals(currentProjectID));

            if (file != null && file.Length > 0)
            {
                try
                {
                    var resImage = await _cloudinaryUploader.UploadMultiImagesAsync(images);
                    if (resImage.Equals("No file") || resImage.Equals("Wrong extension"))
                    {
                        return Json(new
                        {
                            success = false,
                            message = resImage.Equals("No file")
                                ? "Please upload at least one proof image!"
                                : "Some files have the wrong extension!"
                        });
                    }

                    //get current user
                    var currentUserID = HttpContext.Session.GetString("currentUserID");
                    string resourceCannotDonate = null;
                    int isValidDonationFile = 0;
                    using (var package = new ExcelPackage(file.OpenReadStream()))

                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet != null)
                        {
                            int rowCount = worksheet.Dimension.Rows;

                            for (int row = 2; row <= rowCount; row++) // Assuming first row is header
                            {
                                var resource = new ProjectResource()
                                {
                                    ResourceName = worksheet.Cells[row, 1].Value?.ToString(),
                                    Quantity = Convert.ToInt32(worksheet.Cells[row, 2].Value) >= 0 ? Convert.ToInt32(worksheet.Cells[row, 2].Value) : 0,
                                    Unit = worksheet.Cells[row, 5].Value?.ToString(),
                                };
                                var currentResource = await _projectResourceRepo.GetAsync(or => or.ResourceName.Equals(resource.ResourceName) && or.Unit.Equals(resource.Unit));
                                if (resource.Quantity == 0)
                                {
                                    resourceCannotDonate += currentResource.ResourceName + "-" + currentResource.Unit + ", ";
                                    continue;
                                }

                                // get current resource

                                var userToProjectTransactionHistory = new UserToProjectTransactionHistory()
                                {
                                    ProjectResourceID = currentResource.ResourceID,
                                    UserID = new Guid(currentUserID),
                                    Status = 0,
                                    Time = DateOnly.FromDateTime(DateTime.UtcNow),
                                    Amount = resource.Quantity.Value,
                                    Message = worksheet.Cells[row, 6].Value?.ToString(),
                                };
                                var quantityAfterDonate = currentResource.Quantity + resource.Quantity.Value;
                                if(quantityAfterDonate > currentResource.ExpectedQuantity)
                                {
                                    resourceCannotDonate += currentResource.ResourceName + "-" + currentResource.Unit + ", ";
                                }
                                else if (quantityAfterDonate < currentResource.ExpectedQuantity && resource.Quantity > 0)
                                {
                                    userToProjectTransactionHistory.Attachments = resImage;
                                    await _userToProjectTransactionHistoryRepo.AddUserDonateRequestAsync(userToProjectTransactionHistory);
                                    isValidDonationFile ++;
                                }
                            }
                        }
                    }
                    if (isValidDonationFile == 0)
                    {
                        return Json(new { success = false, message = "There is no valid donation. Fail to send your donation request!" });
                    }
                    if (!string.IsNullOrEmpty(resourceCannotDonate))
                    {
                        return Json(new { success = true, message = "Send donate requests successfully.\nBut donate request of  " + resourceCannotDonate.TrimEnd(',', ' ') + " cannot send because of invalid quantity of donation." });
                    }
                    else
                    {
                        return Json(new { success = true, message = "Your donation request was sent successfully!" });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Fail to send your donation request!" });
                }
            }
            return Json(new { success = false, message = "Fail to send your donation request!" });
        }
    }
}
