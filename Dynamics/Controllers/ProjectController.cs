using AutoMapper;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Dynamics.Models.Models.DTO;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Newtonsoft.Json;
using Serilog;
using System.Composition;
using Dynamics.Models.Models.Dto;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using ILogger = Serilog.ILogger;
using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.WebSockets;
using Dynamics.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;
using Util = Dynamics.Utility.Util;

namespace Dynamics.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IOrganizationRepository _organizationRepo;
        private readonly IRequestRepository _requestRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IProjectResourceRepository _projectResourceRepo;
        private readonly IUserToProjectTransactionHistoryRepository _userToProjectTransactionHistoryRepo;

        private readonly IOrganizationToProjectTransactionHistoryRepository
            _organizationToProjectTransactionHistoryRepo;

        private readonly IProjectHistoryRepository _projectHistoryRepo;
        private readonly IReportRepository _reportRepo;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly CloudinaryUploader _cloudinaryUploader;
        private readonly ILogger<ProjectController> _logger;
        IOrganizationVMService _organizationService;
        private readonly IPagination _pagination;

        public ProjectController(IProjectRepository _projectRepo,
            IOrganizationRepository _organizationRepo,
            IRequestRepository _requestRepo,
            IProjectMemberRepository _projectMemberRepo,
            IProjectResourceRepository _projectResourceRepo,
            IUserToProjectTransactionHistoryRepository _userToProjectTransactionHistoryRepo,
            IOrganizationToProjectTransactionHistoryRepository _organizationToProjectTransactionHistoryRepo,
            IProjectHistoryRepository projectHistoryRepository,
            IReportRepository reportRepository,
            IWebHostEnvironment hostEnvironment,
            IMapper mapper,
            IPagination pagination,
            IProjectService projectService,
            CloudinaryUploader cloudinaryUploader,
            ILogger<ProjectController> logger,
            IOrganizationVMService organizationService
        )
        {
            this._projectRepo = _projectRepo;
            this._organizationRepo = _organizationRepo;
            this._requestRepo = _requestRepo;
            this._projectMemberRepo = _projectMemberRepo;
            this._projectResourceRepo = _projectResourceRepo;
            this._userToProjectTransactionHistoryRepo = _userToProjectTransactionHistoryRepo;
            this._organizationToProjectTransactionHistoryRepo = _organizationToProjectTransactionHistoryRepo;
            this._projectHistoryRepo = projectHistoryRepository;
            this.hostEnvironment = hostEnvironment;
            _pagination = pagination;
            this._mapper = mapper;
            this._projectService = projectService;
            _reportRepo = reportRepository;
            _cloudinaryUploader = cloudinaryUploader;
            _logger = logger;
            this._organizationService = organizationService;
        }

        [Route("Project/Index/{userID:guid}")]
        public async Task<IActionResult> Index(Guid userID, int pageNumberPIL = 1, int pageNumberPIM = 1,
            int pageNumberPOT = 1, int pageSize = 3)
        {
            //get project that user has joined
            var projectMemberList = _projectMemberRepo.FilterProjectMember(x => x.UserID.Equals(userID) && x.Status >= 0 && x.Project.ProjectStatus >= 0);
            List<Models.Models.Project> projectsIAmMember = new List<Models.Models.Project>();
            List<Models.Models.Project> projectsILead = new List<Models.Models.Project>();
            foreach (var projectMember in projectMemberList)
            {
                var project = await _projectRepo.GetProjectAsync(x => x.ProjectID.Equals(projectMember.ProjectID));
                if (project != null)
                {
                    var leaderOfProject = await _projectService.GetProjectLeaderAsync(project.ProjectID);
                    if (leaderOfProject.UserID.Equals(userID))
                    {
                        //get project that user join as a leader
                        projectsILead.Add(project);
                    }
                    else
                    {
                        //get project that user join as a member
                        projectsIAmMember.Add(project);
                    }
                }
            }

            //pagination
            var totalProjectsILead = projectsILead.Count();
            var totalPagesProjectsILead = (int)Math.Ceiling((double)totalProjectsILead / pageSize);
            var paginatedProjectsILead = _pagination.Paginate(projectsILead, pageNumberPIL, pageSize);
            ViewBag.currentPagePIL = pageNumberPIL;
            ViewBag.totalPagesPIL = totalPagesProjectsILead;

            var totalProjectsIAmMember = projectsIAmMember.Count();
            var totalPagesProjectsIAmMember = (int)Math.Ceiling((double)totalProjectsIAmMember / pageSize);
            var paginatedProjectsIAmMember = _pagination.Paginate(projectsIAmMember, pageNumberPIM, pageSize);
            ViewBag.currentPagePIM = pageNumberPIM;
            ViewBag.totalPagesPIM = totalPagesProjectsIAmMember;
            
            // Set page size
            ViewBag.pageSize = pageSize;
            // Convert to dtos
            var paginatedProjectsILeadDtos = _projectService.MapToListProjectOverviewDto(paginatedProjectsILead);
            var paginatedProjectsIAmMemberDtos = _projectService.MapToListProjectOverviewDto(paginatedProjectsIAmMember);
            return View(new MyProjectVM()
            {
                ProjectsILead = paginatedProjectsILeadDtos,
                ProjectsIAmMember = paginatedProjectsIAmMemberDtos,
                // OtherProjects = paginatedOtherProjects
            });
        }
        
        public async Task<IActionResult> ViewAllProjects()
        {
            return View(await _projectService.ReturnAllProjectsVMsAsync());
        }

        //update project profile
        public async Task<IActionResult> DeleteImage(string imgPath, Guid phaseID)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            if (phaseID != Guid.Empty)
            {
                var res = await _projectService.DeleteImageAsync(imgPath, phaseID);
                if (!res)
                {
                    TempData[MyConstants.Error] = $"Fail to delete image {imgPath}!";
                    return RedirectToAction(nameof(EditProjectPhaseReport),
                        new { historyID = phaseID, projectID = currentProjectID });
                }

                TempData[MyConstants.Success] = $"Delete image {imgPath} successful!";
                return RedirectToAction(nameof(EditProjectPhaseReport),
                    new { historyID = phaseID, projectID = currentProjectID });
            }
            else
            {
                var res = await _projectService.DeleteImageAsync(imgPath, phaseID);
                if (!res)
                {
                    TempData[MyConstants.Error] = $"Fail to delete image {imgPath}!";
                    return RedirectToAction(nameof(UpdateProjectProfile), new { id = currentProjectID });
                }

                TempData[MyConstants.Success] = $"Delete image {imgPath} successful!";
                return RedirectToAction(nameof(UpdateProjectProfile), new { id = currentProjectID });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportFileProject(FinishProjectVM finishProjectVM, IFormFile reportFile)
        {
            var projectID = finishProjectVM.ProjectID;
            if (reportFile != null)
            {
                var resReportFile = await Util.UploadFiles(new List<IFormFile> { reportFile }, @"files\Project");
                if (resReportFile.Equals("No file"))
                {
                    TempData[MyConstants.Error] = "No file to upload!";
                    return RedirectToAction(nameof(ManageProject), new { id = projectID });
                }

                if (resReportFile.Equals("Wrong extension"))
                {
                    TempData[MyConstants.Error] = "Extension of some files is wrong!";
                    return RedirectToAction(nameof(ManageProject), new { id = projectID });
                }

                finishProjectVM.ReportFile = resReportFile;
                var res = await _projectRepo.FinishProjectAsync(finishProjectVM);
                if (res)
                {
                    TempData[MyConstants.Success] = "Finish project successfully!";
                    return RedirectToAction(nameof(ManageProject), new { id = projectID });
                }
            }

            TempData[MyConstants.Error] = "Fail to finish project!";
            return RedirectToAction(nameof(ManageProject), new { id = projectID });
        }

        public IActionResult DownloadFile(string fileWebPath)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            if (!string.IsNullOrEmpty(fileWebPath))
            {
                var fileName = fileWebPath.Substring(fileWebPath.LastIndexOf('/') + 1);
                var filepath = Path.Combine(hostEnvironment.WebRootPath, "files\\Project", fileName);
                var fileExtension = Path.GetExtension(filepath);
                if (!string.IsNullOrEmpty(fileExtension))
                {
                    switch (fileExtension)
                    {
                        case ".xlsx":
                            return File(System.IO.File.ReadAllBytes(filepath),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                System.IO.Path.GetFileName(filepath));
                        case ".xls":
                            return File(System.IO.File.ReadAllBytes(filepath), "application/vnd.ms-excel",
                                System.IO.Path.GetFileName(filepath));
                        case ".pdf":
                            return File(System.IO.File.ReadAllBytes(filepath), "application/pdf",
                                System.IO.Path.GetFileName(filepath));
                        case ".docx":
                            return File(System.IO.File.ReadAllBytes(filepath),
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                System.IO.Path.GetFileName(filepath));
                        case ".doc":
                            return File(System.IO.File.ReadAllBytes(filepath), "application/msword",
                                System.IO.Path.GetFileName(filepath));
                        case ".txt":
                            return File(System.IO.File.ReadAllBytes(filepath), "text/plain",
                                System.IO.Path.GetFileName(filepath));
                        case ".csv":
                            return File(System.IO.File.ReadAllBytes(filepath), "text/csv",
                                System.IO.Path.GetFileName(filepath));
                    }

                    TempData[MyConstants.Success] = "Download file successful!";
                    return RedirectToAction(nameof(UpdateProjectProfile), new { id = new Guid(currentProjectID) });
                }
            }

            TempData[MyConstants.Info] = "There is no file to download!";
            return RedirectToAction(nameof(UpdateProjectProfile), new { id = new Guid(currentProjectID) });
        }

        public async Task<IActionResult> UpdateProjectProfile(Guid id)
        {
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(id));
            if (projectObj == null)
            {
                return NotFound();
            }

            var projectDto = _mapper.Map<UpdateProjectProfileRequestDto>(projectObj);
            IEnumerable<SelectListItem> StatusList = new List<SelectListItem>()
            {
                new SelectListItem { Text = "Pending", Value = "0" },
                new SelectListItem { Text = "In Progress", Value = "1" },
                new SelectListItem { Text = "Completed", Value = "2" }
            };
            ViewData["StatusList"] = StatusList;

            ICollection<SelectListItem> MemberList = new List<SelectListItem>() { };
            foreach (var member in _projectService.FilterMemberOfProject(p => p.ProjectID.Equals(id) && p.Status >= 1))
            {
                MemberList.Add(new SelectListItem { Text = member.UserFullName, Value = member.UserID.ToString() });
            }

            ViewData["MemberList"] = MemberList;

            //get leader id from project member to updateProjectDto
            projectDto.NewLeaderID = new Guid(HttpContext.Session.GetString("currentProjectLeaderID"));
            return View(projectDto);
        }

        //POST: Project/UpdateProjectProfile
        [HttpPost]
        public async Task<IActionResult> UpdateProjectProfile(UpdateProjectProfileRequestDto updateProject,
            List<IFormFile> images)
        {
            var resUpdate = await _projectService.UpdateProjectProfileAsync(updateProject, images);
            if (resUpdate.Equals("No file") || resUpdate.Equals("Wrong extension"))
            {
                TempData[MyConstants.Error] =
                    resUpdate.Equals("No file") ? "No file to upload!" : "Extension of some files is wrong!";
            }
            else if (resUpdate.Equals(MyConstants.Success))
            {
                TempData[MyConstants.Success] = "Update project successfully!";
                return RedirectToAction(nameof(ManageProject),
                    new { id = HttpContext.Session.GetString("currentProjectID") });
            }
            else
            {
                TempData[MyConstants.Error] = "Fail to update project!";
            }

            return RedirectToAction(nameof(UpdateProjectProfile),
                new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        [HttpPost]
        public async Task<IActionResult> ShutdownProject(ShutdownProjectVM shutdownProjectVM)
        {
            var userIDString = HttpContext.Session.GetString("currentUserID");
            var res = await _projectRepo.ShutdownProjectAsync(shutdownProjectVM);
            if (res && !string.IsNullOrEmpty(userIDString))
            {
                return Json(new
                {
                    success = true, message = "Shutdown project successful!",
                    remind = "You just have shut down a project for \"" + shutdownProjectVM.Reason + "\"",
                    userIDString = userIDString
                });
            }

            return Json(new { success = false, message = "Fail to shutdown project!" });
        }

        public async Task<IActionResult> SendReportProjectRequest(Report report)
        {
            _logger.LogWarning("Send report project request");
            report.ReporterID = new Guid(HttpContext.Session.GetString("currentUserID"));
            report.Type = ReportObjectConstant.Project;
            var res = await _reportRepo.SendReportProjectRequestAsync(report);
            if (res)
            {
                TempData[MyConstants.Success] = "Send report project request successfully!";
                return RedirectToAction(nameof(ManageProject), new { id = report.ObjectID });
            }

            TempData[MyConstants.Error] = "Fail to send report project request!";
            return RedirectToAction(nameof(ManageProject), new { id = report.ObjectID });
        }

        //show tổng quan project
        public async Task<IActionResult> ManageProject(string id)
        {
            if (string.IsNullOrEmpty(id.ToString()) || id.Equals(00000000 - 0000 - 0000 - 0000 - 000000000000))
            {
                return NotFound("id is empty!");
            }

            var detailProject = await _projectService.ReturnDetailProjectVMAsync(new Guid(id));
            if (detailProject != null)
            {
                return View(detailProject);
            }

            TempData[MyConstants.Error] = "Fail to get project!";
            return RedirectToAction(nameof(Index), new { id = HttpContext.Session.GetString("currentUserID") });
        }

        //----------------------manage project member -------------
        [Route("Project/ManageProjectMember/{projectID}")]
        public async Task<IActionResult> ManageProjectMember([FromRoute] Guid projectID, int pageNumberPM = 1,
            int pageSize = 10)
        {
            var allProjectMember =
                _projectMemberRepo.FilterProjectMember(p => p.ProjectID.Equals(projectID) && p.Status >= 1);
            if (allProjectMember == null)
            {
                throw new Exception("No member in this project!");
            }

            var totalPM = allProjectMember.Count();
            var totalPagePM = (int)Math.Ceiling((double)totalPM / pageSize);
            var paginatedPM = _pagination.Paginate(allProjectMember, pageNumberPM, pageSize);
            ViewBag.currentPagePM = pageNumberPM;
            ViewBag.totalPagesPM = totalPagePM;

            var joinRequests =
                _projectMemberRepo.FilterProjectMember(p => p.ProjectID.Equals(projectID) && p.Status == 0) ??
                Enumerable.Empty<ProjectMember>();
            var nums = joinRequests.Count();
            ViewData["hasJoinRequest"] = nums > 0;
            return View(paginatedPM);
        }

        [Route("Project/DeleteProjectMember/{memberID}")]
        public async Task<IActionResult> DeleteProjectMember([FromRoute] Guid memberID)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var res = await _projectMemberRepo.DeleteAsync(x =>
                x.UserID.Equals(memberID) && x.ProjectID.Equals(new Guid(currentProjectID)));
            if (res != null)
            {
                TempData[MyConstants.Success] = "Delete project member successfully!";
                return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
            }

            TempData[MyConstants.Error] = "Fail to delete project member!";
            return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
        }

        //----manage join request-----
        //create request
        public async Task<IActionResult> JoinProjectRequest(Guid memberID, Guid projectID)
        {
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "This project is not in progress!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
            }

            var res = await _projectService.SendJoinProjectRequestAsync(projectID, memberID);
            _logger.LogWarning(res);
            if (res != null)
            {
                if (res.Equals(MyConstants.Success))
                {
                    TempData[MyConstants.Success] = "Join request sent successfully!";
                }
                else if (res.Equals(MyConstants.Warning))
                {
                    TempData[MyConstants.Warning] = "Already sent another join request!";
                    TempData[MyConstants.Subtitle] = "Please wait for the project leader response!";
                }
                else if (res.Equals(MyConstants.Error))
                {
                    TempData[MyConstants.Error] = "Fail to send join request!";
                }
            }

            return RedirectToAction(nameof(ManageProject), new { id = projectID });
        }

        //send notification to move out from project
        public async Task<IActionResult> LeaveProjectRequest(Guid projectID)
        {
            var currentUserID = HttpContext.Session.GetString("currentUserID");
            if (currentUserID != null)
            {
                var currentProjectLeaderID = HttpContext.Session.GetString("currentProjectLeaderID");
                if (currentUserID.Equals(currentProjectLeaderID))
                {
                    TempData[MyConstants.Warning] = "You can not leave the project while you are a project leader !";
                    TempData[MyConstants.Info] = "Transfer team leader rights if you still want to leave the project.";
                    return RedirectToAction(nameof(ManageProject), new { id = projectID });
                }

                var res = await _projectMemberRepo.DenyJoinRequestAsync(new Guid(currentUserID), projectID);
                if (res)
                {
                    TempData[MyConstants.Success] = "Move out project successfull!";
                    return RedirectToAction(nameof(ManageProject), new { id = projectID });
                }
            }

            TempData[MyConstants.Error] = "Fail to move out project!";
            return RedirectToAction(nameof(ManageProject), new { id = projectID });
        }

        //review request
        [Route("Project/ReviewJoinRequest/{projectID}")]
        public async Task<IActionResult> ReviewJoinRequest([FromRoute] Guid projectID, int pageNumberJR = 1,
            int pageSizeJR = 10)
        {
            var allJoinRequest =
                _projectMemberRepo.FilterProjectMember(p => p.ProjectID.Equals(projectID) && p.Status == 0);

            if (allJoinRequest == null)
            {
                throw new Exception("List join request is null!");
            }

            //pagination
            var totalJR = allJoinRequest.Count();
            var totalPageJR = (int)Math.Ceiling((double)totalJR / pageSizeJR);
            var paginatedJR = _pagination.Paginate(allJoinRequest, pageNumberJR, pageSizeJR);
            ViewBag.currentPagejR = pageNumberJR;
            ViewBag.totalPagesJR = totalPageJR;

            return View(paginatedJR);
        }

        [Route("Project/AcceptedJoinRequest/{memberID}")]
        public async Task<IActionResult> AcceptedJoinRequest([FromRoute] Guid memberID)
        {
            var currentProjectID = new Guid(HttpContext.Session.GetString("currentProjectID"));
            var res = await _projectMemberRepo.AcceptedJoinRequestAsync(memberID, currentProjectID);
            if (res)
            {
                TempData[MyConstants.Success] = "Join request accepted successfully!";
                return RedirectToAction(nameof(ReviewJoinRequest), new { id = currentProjectID });
            }

            TempData[MyConstants.Error] = "Failed to accept the join request!";
            return RedirectToAction(nameof(ReviewJoinRequest), new { id = currentProjectID });
        }

        [Route("Project/DenyJoinRequest/{memberID}")]
        public async Task<IActionResult> DenyJoinRequest([FromRoute] Guid memberID)
        {
            var currentProjectID = new Guid(HttpContext.Session.GetString("currentProjectID"));
            var res = await _projectMemberRepo.DenyJoinRequestAsync(memberID, currentProjectID);
            if (res)
            {
                TempData[MyConstants.Success] = "Join request denied successfully!";
                return RedirectToAction(nameof(ReviewJoinRequest), new { id = currentProjectID });
            }

            TempData[MyConstants.Error] = "Failed to deny the join request!";
            return RedirectToAction(nameof(ReviewJoinRequest), new { id = currentProjectID });
        }

        public async Task<IActionResult> AcceptedJoinRequestAll()
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");

            var res = await _projectService.AcceptJoinProjectRequestAllAsync(new Guid(currentProjectID));
            if (!res)
            {
                TempData[MyConstants.Error] = "Failed to accept the join request!";
                return RedirectToAction(nameof(ReviewJoinRequest), new { id = new Guid(currentProjectID) });
            }

            TempData[MyConstants.Success] = "All join request accepted successfully!";
            return RedirectToAction(nameof(ReviewJoinRequest), new { id = new Guid(currentProjectID) });
        }

        public async Task<IActionResult> DenyJoinRequestAll()
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");

            var res = await _projectService.DenyJoinProjectRequestAllAsync(new Guid(currentProjectID));
            if (!res)
            {
                TempData[MyConstants.Error] = "Failed to deny the join request!";
                return RedirectToAction(nameof(ReviewJoinRequest), new { id = new Guid(currentProjectID) });
            }

            TempData[MyConstants.Success] = "All join request denied successfully!";
            return RedirectToAction(nameof(ReviewJoinRequest), new { id = new Guid(currentProjectID) });
        }

        //-------------------manage transaction history of project------------------------
        [HttpGet]
        public async Task<IActionResult> SendDonateRequest(Guid projectID, string donor)
        {
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "This project is not in progress!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
            }

            if (!ModelState.IsValid)
            {
                TempData[MyConstants.Error] = "Fail to send donate request!";
                return RedirectToAction(nameof(SendDonateRequest), new { projectID = projectID, donor = donor });
            }

            var allResource = await _projectResourceRepo.FilterProjectResourceAsync(p =>
                p.ProjectID.Equals(projectID) && !p.ResourceName.Equals("Money") && p.Quantity < p.ExpectedQuantity);
            if (allResource == null)
            {
                return NotFound();
            }

            IEnumerable<SelectListItem> ResourceTypeList = allResource.Select(x => new SelectListItem
            {
                Text = x.ResourceName + " - " + x.Unit,
                Value = x.ResourceID.ToString(),
            }).ToList();
            ViewData["ResourceTypeList"] = ResourceTypeList;
            //set value for View Model
            SendDonateRequestVM sendDonateRequestVM =
                await _projectService.ReturnSendDonateRequestVMAsync(projectID, donor);
            if (sendDonateRequestVM == null)
            {
                TempData[MyConstants.Error] = "Fail to get history donate of user/organization!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
            }

            // Setup for payment by money
            var moneyResource = await _projectResourceRepo.GetAsync(pr =>
                pr.ResourceName.ToLower().Equals("money") && pr.ProjectID.Equals(projectID));
            if (moneyResource == null) throw new Exception("warning: PROJECT DOES NOT HAVE MONEY RESOURCE");
            sendDonateRequestVM.VnPayRequestDto = new VnPayRequestDto
            {
                TargetId = projectID,
                TargetType = MyConstants.Project,
                ResourceID = moneyResource.ResourceID,
            };
            // Last but not least, setup a return url:
            var url = Url.Action("ManageProject", "Project", new { id = projectID }, Request.Scheme);
            ViewBag.returnUrl = url ?? "~/";
            return View(sendDonateRequestVM);
        }

        [HttpPost]
        public async Task<IActionResult> SendDonateRequest(SendDonateRequestVM sendDonateRequestVM)
        {
            var res = await _projectService.SendDonateRequestAsync(sendDonateRequestVM);
            if (!string.IsNullOrEmpty(res))
            {
                if (res.Equals("Exceed"))
                {
                    // Return JSON response with failure message
                    return Json(new
                    {
                        success = false,
                        message = "The quantity of resource you want to donate is more than expected quantity!"
                    });
                }
                else if (res.Equals(MyConstants.Success))
                {
                    return Json(new { success = true, message = "Your donation request was sent successfully!" });
                }
            }

            return Json(new { success = false, message = "Fail to send your donation request!" });
        }

        [Route("Project/ManageProjectDonor/{projectID}")]
        public async Task<IActionResult> ManageProjectDonor(Guid projectID, int pageNumberUD = 1, int pageNumberOD = 1, int pageSize = 10)
        {
            ProjectTransactionHistoryVM projectTransactionHistoryVM = await _projectService.ReturnProjectTransactionHistoryVMAsync(projectID);
            if (projectTransactionHistoryVM == null)
            {
                TempData[MyConstants.Error] = "Fail to get history donate of user/organization!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
            }

            // This one is special bc we are paginating on client side, so unfortunately, no async here
            var totalUD = projectTransactionHistoryVM.UserDonate.Count();
            var totalPageUD = (int)Math.Ceiling((double)totalUD / pageSize);
            var paginatedUD = _pagination.Paginate(projectTransactionHistoryVM.UserDonate, pageNumberUD, pageSize);
            ViewBag.currentPageUD = pageNumberUD;
            ViewBag.totalPagesUD = totalPageUD;

            var totalOD = projectTransactionHistoryVM.OrganizationDonate.Count();
            var totalPageOD = (int)Math.Ceiling((double)totalOD / pageSize);
            var paginatedOD = _pagination.Paginate(projectTransactionHistoryVM.OrganizationDonate, pageNumberOD, pageSize);
            ViewBag.currentPageOD = pageNumberOD;
            ViewBag.totalPagesOD = totalPageOD;


            ProjectTransactionHistoryVM PaginateAsyncdVM = new ProjectTransactionHistoryVM()
            {
                UserDonate = paginatedUD,
                OrganizationDonate = paginatedOD
            };
            int nums =
                (await _userToProjectTransactionHistoryRepo.GetAllUserDonateAsync(u =>
                     u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 0) ??
                 new List<UserToProjectTransactionHistory>()).Count();
            var hasUserDonateRequest = nums > 0;

            int nums2 =
                (await _organizationToProjectTransactionHistoryRepo.GetAllOrganizationDonateAsync(
                     u => u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 0) ??
                 new List<OrganizationToProjectHistory>()).Count();
            var hasOrgDonateRequest = nums2 > 0;

            ViewData["hasUserDonateRequest"] = hasUserDonateRequest;
            ViewData["hasOrgDonateRequest"] = hasOrgDonateRequest;
            return View(projectTransactionHistoryVM);
        }

        [HttpGet]
        [Route("Project/ReviewUserDonateRequest/{projectID}")]
        public async Task<IActionResult> ReviewUserDonateRequest(Guid projectID, int pageNumberUD = 1,
            int pageSizeUD = 10)
        {
            var allUserDonate =
                await _userToProjectTransactionHistoryRepo.GetAllUserDonateAsync(u =>
                    u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 0);
            if (allUserDonate == null)
            {
                return NotFound();
            }

            //pagination
            var totalUD = allUserDonate.Count();
            var totalPageUD = (int)Math.Ceiling((double)totalUD / pageSizeUD);
            allUserDonate = _pagination.Paginate(allUserDonate, pageNumberUD, pageSizeUD);
            ViewBag.currentPageUD = pageNumberUD;
            ViewBag.totalPagesUD = totalPageUD;

            return View(allUserDonate);
        }

        [HttpGet]
        [Route("Project/ReviewOrgDonateRequest/{projectID}")]
        public async Task<IActionResult> ReviewOrgDonateRequest(Guid projectID, int pageNumberOD = 1,
            int pageSizeOD = 10)
        {
            var allOrgDonate =
                await _organizationToProjectTransactionHistoryRepo.GetAllOrganizationDonateAsync(
                    u => u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 0);
            if (allOrgDonate == null)
            {
                return NotFound();
            }

            //pagination
            var totalOD = allOrgDonate.Count();
            var totalPageOD = (int)Math.Ceiling((double)totalOD / pageSizeOD);
            allOrgDonate = _pagination.Paginate(allOrgDonate, pageNumberOD, pageSizeOD);
            ViewBag.currentPageOD = pageNumberOD;
            ViewBag.totalPagesOD = totalPageOD;

            return View(allOrgDonate);
        }

        //-----accept request donate-----
        public async Task<IActionResult> AcceptedDonateRequest(Guid transactionID, string donor)
        {
            var res = false;
            switch (donor)
            {
                case "User":
                    res = await _userToProjectTransactionHistoryRepo.AcceptedUserDonateRequestAsync(transactionID);
                    break;
                case "Organization":
                    res = await _organizationToProjectTransactionHistoryRepo.AcceptedOrgDonateRequestAsync(
                        transactionID);
                    break;
                default:
                    return NotFound();
            }

            if (res)
            {
                TempData[MyConstants.Success] = "Donation request accepted successfully!";
                return (donor.Equals("User"))
                    ? RedirectToAction(nameof(ReviewUserDonateRequest),
                        new { id = HttpContext.Session.GetString("currentProjectID") })
                    : RedirectToAction(nameof(ReviewOrgDonateRequest),
                        new { id = HttpContext.Session.GetString("currentProjectID") });
            }

            TempData[MyConstants.Error] = "Failed to accept the donation request!";
            return (donor.Equals("User"))
                ? RedirectToAction(nameof(ReviewUserDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") })
                : RedirectToAction(nameof(ReviewOrgDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        public async Task<IActionResult> AcceptedDonateRequestAll(string donor)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var res = await _projectService.AcceptDonateProjectRequestAllAsync(new Guid(currentProjectID), donor);
            if (!res)
            {
                TempData[MyConstants.Error] = "Failed to accept all the donation request!";
            }
            else
            {
                TempData[MyConstants.Success] = "Donation all request accepted successfully!";
            }

            return (donor.Equals("User"))
                ? RedirectToAction(nameof(ReviewUserDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") })
                : RedirectToAction(nameof(ReviewOrgDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        //-----deny request donate-----
        public async Task<IActionResult> DenyDonateRequest(Guid transactionID, string donor)
        {
            var res = false;
            switch (donor)
            {
                case "User":
                    res = await _userToProjectTransactionHistoryRepo.DenyUserDonateRequestAsync(transactionID);
                    break;
                case "Organization":
                    res = await _organizationToProjectTransactionHistoryRepo.DenyOrgDonateRequestAsync(transactionID);
                    break;
            }

            if (res)
            {
                TempData[MyConstants.Success] = "Donation request denied successfully!";
                return (donor.Equals("User"))
                    ? RedirectToAction(nameof(ReviewUserDonateRequest),
                        new { id = HttpContext.Session.GetString("currentProjectID") })
                    : RedirectToAction(nameof(ReviewOrgDonateRequest),
                        new { id = HttpContext.Session.GetString("currentProjectID") });
            }

            TempData[MyConstants.Error] = "Failed to deny the donation request!";
            return (donor.Equals("User"))
                ? RedirectToAction(nameof(ReviewUserDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") })
                : RedirectToAction(nameof(ReviewOrgDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        public async Task<IActionResult> DenyDonateRequestAll(string donor)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var res = await _projectService.DenyDonateProjectRequestAllAsync(new Guid(currentProjectID), donor);
            if (!res)
            {
                TempData[MyConstants.Error] = "Failed to deny all the donation request!";
            }
            else
            {
                TempData[MyConstants.Success] = "All donation request denied successfully!";
            }

            return (donor.Equals("User"))
                ? RedirectToAction(nameof(ReviewUserDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") })
                : RedirectToAction(nameof(ReviewOrgDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        //---------------------------manage ProjectResource--------------------------
        public IActionResult CreateProjectDonateRequestAsync(Guid projectID)
        {
            IEnumerable<SelectListItem> ResourceTypeList = _projectResourceRepo.GetAllProjectResourceQuery().Select(x =>
                new SelectListItem
                {
                    Text = x.ResourceName,
                    Value = x.ResourceID.ToString(),
                });
            ResourceTypeList.Append(new SelectListItem() { Text = "Other", Value = "None" });
            ViewData["ResourceTypeList"] = ResourceTypeList;
            return View();
        }

        [Route("Project/ManageProjectResource/{projectID}")]
        public async Task<IActionResult> ManageProjectResource(Guid projectID)
        {
            var allResource = await _projectResourceRepo.FilterProjectResourceAsync(p => p.ProjectID.Equals(projectID));
            if (allResource == null)
            {
                return NotFound();
            }

            return View(allResource);
        }

        [HttpPost]
        public async Task<IActionResult> AddProjectResourceType(ProjectResource projectResource)
        {
            if (ModelState.IsValid)
            {
                var projectObj =
                    await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectResource.ProjectID));
                if (projectObj != null)
                {
                    var resAddResource = await _projectResourceRepo.AddResourceTypeAsync(projectResource);
                    if (resAddResource)
                    {
                        TempData[MyConstants.Success] = "Add resource type successfully!";
                        return RedirectToAction(nameof(ManageProjectResource), new { id = projectObj.ProjectID });
                    }

                    return RedirectToAction(nameof(ManageProjectResource), new { id = projectObj.ProjectID });
                }
            }

            TempData[MyConstants.Error] = "Fail to add new resource type!";
            return RedirectToAction(nameof(ManageProjectResource),
                new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateResourceType(ProjectResource projectResource)
        {
            var res = await _projectService.UpdateProjectResourceTypeAsync(projectResource);
            if (!string.IsNullOrEmpty(res))
            {
                if ("Existed".Equals(res))
                {
                    TempData[MyConstants.Error] = "Resource type has the same unit is existed!";
                }
                else if (MyConstants.Success.Equals(res))
                {
                    TempData[MyConstants.Success] = "Update resource type successfully!";
                }
                else
                {
                    TempData[MyConstants.Error] = "Fail to update resource type!";
                }
            }

            return RedirectToAction(nameof(ManageProjectResource), new { id = projectResource.ProjectID });
        }

        public async Task<IActionResult> DeleteResourceType(Guid resourceID)
        {
            var res = await _projectResourceRepo.DeleteResourceTypeAsync(resourceID);
            if (res)
            {
                TempData[MyConstants.Success] = "Delete resource type successfully!";
                return RedirectToAction(nameof(ManageProjectResource),
                    new { id = HttpContext.Session.GetString("currentProjectID") });
            }

            TempData[MyConstants.Error] = "Fail to delete resource type!";
            return RedirectToAction(nameof(ManageProjectResource),
                new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        //-----------------manage project phase report -------------------
        [Route("Project/ManageProjectPhaseReport/{projectID}")]
        public async Task<IActionResult> ManageProjectPhaseReport(Guid projectID)
        {
            var allUpdate =
                await _projectHistoryRepo.GetAllPhaseReportsAsync(u => u.ProjectID.Equals(projectID));
            if (allUpdate == null)
            {
                return NotFound();
            }

            allUpdate = allUpdate.OrderByDescending(x => x.Date).ToList();
            return View(allUpdate);
        }

        [HttpGet]
        public async Task<IActionResult> AddProjectPhaseReport(Guid projectID)
        {
            var existingDates = await _projectService.GetExistingReportDatesAsync(projectID);
            // Convert to a list of string dates in "YYYY-MM-DD" format
            var disabledDates = existingDates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
            ViewBag.DisabledDates = disabledDates;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProjectPhaseReport(History history, List<IFormFile> images)
        {
            var resAdd = await _projectService.AddProjectPhaseReportAsync(history, images);
            if (resAdd.Equals("No file") || resAdd.Equals("Wrong extension"))
            {
                TempData[MyConstants.Error] =
                    resAdd.Equals("No file") ? "No file to upload!" : "Extension of some files is wrong!";
            }
            else if (resAdd.Equals(MyConstants.Success))
            {
                TempData[MyConstants.Success] = "Add project update successfully!";
            }
            else
            {
                TempData[MyConstants.Error] = "Fail to add project update!";
            }

            return RedirectToAction(nameof(ManageProjectPhaseReport), new { id = history.ProjectID });
        }

        public async Task<IActionResult> EditProjectPhaseReport(Guid historyID, Guid projectID)
        {
            var projectUpdates =
                await _projectHistoryRepo.GetAllPhaseReportsAsync(u => u.HistoryID.Equals(historyID));
            if (projectUpdates == null)
            {
                return NotFound();
            }

            var projectUpdate = projectUpdates.FirstOrDefault();
            var existingDates = await _projectService.GetExistingReportDatesAsync(projectID);
            // Convert to a list of string dates in "YYYY-MM-DD" format
            var disabledDates = existingDates.Select(d => d.ToString("yyyy-MM-dd"))
                .Except(new[] { projectUpdate?.Date.ToString("yyyy-MM-dd") }).ToList();
            ViewBag.DisabledDates = disabledDates;
            return View(projectUpdate);
        }

        [HttpPost]
        public async Task<IActionResult> EditProjectPhaseReport(History history, List<IFormFile> images)
        {
            var resEdit = await _projectService.EditProjectPhaseReportAsync(history, images);
            if (resEdit.Equals("No file") || resEdit.Equals("Wrong extension"))
            {
                TempData[MyConstants.Error] =
                    resEdit.Equals("No file") ? "No file to upload!" : "Extension of some files is wrong!";
            }
            else if (resEdit.Equals(MyConstants.Success))
            {
                TempData[MyConstants.Success] = "Update project update successfully!";
                return RedirectToAction(nameof(ManageProjectPhaseReport), new { id = history.ProjectID });
            }
            else
            {
                TempData[MyConstants.Error] = "Fail to update project update!";
            }

            return RedirectToAction(nameof(EditProjectPhaseReport),
                new { projectID = history.ProjectID, historyID = history.HistoryID });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProjectPhaseReport(Guid id)
        {
            var res = await _projectHistoryRepo.DeletePhaseReportAsync(id);
            if (res)
            {
                TempData[MyConstants.Success] = "Delete project update successfully!";
                return RedirectToAction(nameof(ManageProjectPhaseReport),
                    new { id = new Guid(HttpContext.Session.GetString("currentProjectID")) });
            }

            TempData[MyConstants.Error] = "Fail to delete project update!";
            return RedirectToAction(nameof(ManageProjectPhaseReport),
                new { id = new Guid(HttpContext.Session.GetString("currentProjectID")) });
        }


        public async Task<IActionResult> CreateProjectByImportData(Guid requestId)
        {
            Request request = await _requestRepo.GetAsync(r => r.RequestID.Equals(requestId));

            //get current user
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            var currentOrganization = await _organizationService.GetOrganizationVMByUserIDAsync(currentUser.UserID);

            var projectVM = new ProjectVM()
            {
                ProjectID = Guid.NewGuid(),
                OrganizationID = currentOrganization.OrganizationID,
                ProjectStatus = 0,
                StartTime = DateOnly.FromDateTime(DateTime.UtcNow),
                OrganizationVM = currentOrganization,
                RequestID = requestId,
                Attachment = request.Attachment,
                ProjectName = request.RequestTitle,
                ProjectDescription = request.Content,
                ProjectEmail = request.RequestEmail,
                ProjectPhoneNumber = request.RequestPhoneNumber,
                ProjectAddress = request.Location
            };

            ViewBag.ExpectedQuantity = 0;
            ViewBag.RequestId = requestId;
            return View(projectVM);
        }


        //Repo of tuan
        [HttpGet]
        public async Task<IActionResult> CreateProject(Guid? requestId)
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            if (requestId != null)
            {
                currentOrganization = await _organizationService.GetOrganizationVMByUserIDAsync(currentUser.UserID);
            }

            var projectVM = new ProjectVM()
            {
                ProjectID = Guid.NewGuid(),
                OrganizationID = currentOrganization.OrganizationID,
                ProjectStatus = 0,
                StartTime = DateOnly.FromDateTime(DateTime.UtcNow),
                OrganizationVM = currentOrganization,
                RequestID = requestId != Guid.Empty ? requestId : null,
            };

            ViewBag.ExpectedQuantity = 0;
            return View(projectVM);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(ProjectVM projectVM, IFormFile image, int expectedQuantity,
            string Unit)
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            if (currentOrganization == null)
            {
                currentOrganization = await _organizationService.GetOrganizationVMByUserIDAsync(currentUser.UserID);
            }

            projectVM.OrganizationVM = currentOrganization;

            if (projectVM.LeaderID != Guid.Empty)
            {
                var Leader = new User();
                foreach (var item in currentOrganization.OrganizationMember)
                {
                    if (item.UserID.Equals(projectVM.LeaderID))
                    {
                        Leader = item.User;
                    }
                }

                if (projectVM != null)
                {
                    if (image != null)
                    {
                        projectVM.Attachment = await _cloudinaryUploader.UploadImageAsync(image);
                    }

                    if (projectVM.ProjectEmail == null)
                    {
                        projectVM.ProjectEmail = Leader.UserEmail;
                    }

                    if (projectVM.ProjectPhoneNumber == null)
                    {
                        projectVM.ProjectPhoneNumber = Leader.UserPhoneNumber;
                    }

                    if (projectVM.ProjectAddress == null)
                    {
                        projectVM.ProjectAddress = Leader.UserAddress;
                    }

                    // user not update profile.
                    if (expectedQuantity > 0)
                    {
                        var project = new Models.Models.Project()
                        {
                            ProjectID = projectVM.ProjectID,
                            OrganizationID = projectVM.OrganizationID,
                            RequestID = projectVM.RequestID,
                            ProjectName = projectVM.ProjectName,
                            ProjectEmail = projectVM.ProjectEmail,
                            ProjectPhoneNumber = projectVM.ProjectPhoneNumber,
                            ProjectAddress = projectVM.ProjectAddress,
                            ProjectStatus = projectVM.ProjectStatus,
                            Attachment = projectVM.Attachment,
                            ProjectDescription = projectVM.ProjectDescription,
                            StartTime = projectVM.StartTime,
                            EndTime = projectVM.EndTime,
                        };


                        if (await _projectRepo.AddProjectAsync(project))
                        {
                            var projectResource = new ProjectResource()
                            {
                                ProjectID = project.ProjectID,
                                ResourceName = "Money",
                                Quantity = 0,
                                ExpectedQuantity = expectedQuantity,
                                Unit = Unit,
                            };
                            await _projectRepo.AddProjectResourceAsync(projectResource);

                            if (project.RequestID != null)
                            {
                                Request request =
                                    await _requestRepo.GetAsync(r => r.RequestID.Equals(project.RequestID));
                                request.Status = 2;
                                await _requestRepo.UpdateAsync(request);
                            }


                            return RedirectToAction(nameof(AutoJoinProject),
                                new { projectId = project.ProjectID, leaderId = projectVM.LeaderID });
                        }
                    }
                }
            }


            if (expectedQuantity <= 0)
                ViewBag.MessageExpectedQuantity = "*ExpectedQuantity more than > 0";

            ViewBag.ExpectedQuantity = expectedQuantity;

            return View(projectVM);
        }

        public async Task<IActionResult> AutoJoinProject(Guid projectId, Guid leaderId)
        {
            //get current user
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            //join Project

            if (!currentUser.UserID.Equals(leaderId))
            {
                var projectMember1 = new ProjectMember()
                {
                    UserID = leaderId,
                    ProjectID = projectId,
                    Status = 3,
                };
                await _projectRepo.AddProjectMemberAsync(projectMember1);
            }

            var projectMember = new ProjectMember()
            {
                UserID = currentUser.UserID,
                ProjectID = projectId,
                Status = 2,
            };
            await _projectRepo.AddProjectMemberAsync(projectMember);

            var currentProject = await _projectRepo.GetProjectByProjectIDAsync(p => p.ProjectID.Equals(projectId));
            HttpContext.Session.Set<Models.Models.Project>(MySettingSession.SESSION_Current_Project_KEY,
                currentProject);
            return RedirectToAction(nameof(AddProjectResource));
        }

        [HttpGet]
        public async Task<IActionResult> DetailProject(Guid projectId)
        {
            var currentProject = await _projectRepo.GetProjectByProjectIDAsync(p => p.ProjectID.Equals(projectId));
            HttpContext.Session.Set<Models.Models.Project>(MySettingSession.SESSION_Current_Project_KEY,
                currentProject);
            return RedirectToAction(nameof(AddProjectResource));
        }

        public async Task<IActionResult> AddProjectResource()
        {
            var currentProject =
                HttpContext.Session.Get<Models.Models.Project>(MySettingSession.SESSION_Current_Project_KEY);

            var projectResource = new ProjectResource()
            {
                ProjectID = currentProject.ProjectID,
                Quantity = 0,
            };

            var projectResources =
                await _projectRepo.GetAllResourceByProjectIDAsync(pr => pr.ProjectID.Equals(currentProject.ProjectID));
            HttpContext.Session.Set<List<ProjectResource>>(MySettingSession.SESSION_Resources_In_A_PRoject_KEY,
                projectResources);

            return View(projectResource);
        }

        [HttpPost]
        public async Task<IActionResult> AddProjectResource(ProjectResource projectResource)
        {
            await _projectRepo.AddProjectResourceAsync(projectResource);
            return RedirectToAction(nameof(AddProjectResource));
        }
    }
}