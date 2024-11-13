using AutoMapper;
using AutoMapper.Execution;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using ILogger = Serilog.ILogger;
using Util = Dynamics.Utility.Util;
using Dynamics.Models.Dto;
using Project = Dynamics.Models.Models.Project;
using Microsoft.AspNetCore.Http;

namespace Dynamics.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IOrganizationRepository _organizationRepo;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IRequestRepository _requestRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IProjectResourceRepository _projectResourceRepo;
        private readonly IUserToProjectTransactionHistoryRepository _userToProjectTransactionHistoryRepo;
        private readonly IOrganizationToProjectTransactionHistoryRepository _organizationToProjectTransactionHistoryRepo;
        private readonly IProjectHistoryRepository _projectHistoryRepo;
        private readonly IReportRepository _reportRepo;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly CloudinaryUploader _cloudinaryUploader;
        private readonly ILogger<ProjectController> _logger;
        private readonly IUserRepository _userRepository;
        IOrganizationVMService _organizationService;
        private readonly ITransactionViewService _transactionViewService;
        private readonly IPagination _pagination;
        private readonly INotificationService _notificationService;
        private readonly IRoleService _roleService;
        private readonly IWalletService _walletService;

        public ProjectController(IProjectRepository _projectRepo,
            IOrganizationRepository _organizationRepo,
            IOrganizationMemberRepository _organizationMemberRepository,
            IRequestRepository _requestRepo,
            IProjectMemberRepository _projectMemberRepo,
            IProjectResourceRepository _projectResourceRepo,
            IUserToProjectTransactionHistoryRepository _userToProjectTransactionHistoryRepo,
            IOrganizationToProjectTransactionHistoryRepository _organizationToProjectTransactionHistoryRepo,
            IProjectHistoryRepository projectHistoryRepository,
            IReportRepository reportRepository,
            IWebHostEnvironment hostEnvironment,
            IMapper mapper, IPagination pagination,
            IProjectService projectService,
            CloudinaryUploader cloudinaryUploader, ILogger<ProjectController> logger,
            IUserRepository userRepository,
            IOrganizationVMService organizationService, INotificationService notificationService,
            ITransactionViewService transactionViewService,
            IRoleService roleService, IWalletService walletService)
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
            this._organizationMemberRepository = _organizationMemberRepository;
            _pagination = pagination;
            this._mapper = mapper;
            this._projectService = projectService;
            _reportRepo = reportRepository;
            _cloudinaryUploader = cloudinaryUploader;
            _logger = logger;
            _userRepository = userRepository;
            this._organizationService = organizationService;
            _transactionViewService = transactionViewService;
            _roleService = roleService;
            _walletService = walletService;
            _notificationService = notificationService;
        }

        [Route("Project/Index/{userID:guid}")]
        public async Task<IActionResult> Index(Guid userID,
            string searchQuery, string filterQuery,
            DateOnly? dateFrom, DateOnly? dateTo,
            int pageNumberPIL = 1, int pageNumberPIM = 1, int pageNumberPOT = 1, int pageSize = 6)
        {
            //get project that user has joined
            var projectMemberList = _projectMemberRepo.FilterProjectMember(x =>
                x.UserID.Equals(userID) && x.Status >= 1 &&
                x.Project.ProjectStatus >= 0); // (Don't show the ones that are pending)
            List<Project> projectsIAmMember = new List<Project>();
            List<Project> projectsILead = new List<Project>();
            foreach (var projectMember in projectMemberList)
            {
                var project = await _projectRepo.GetProjectAsync(x => x.ProjectID.Equals(projectMember.ProjectID));
                if (project != null)
                {
                    var leaderOfProject = await _projectService.GetProjectLeaderAsync(project.ProjectID);
                    if (leaderOfProject.Id.Equals(userID))
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

            //search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                projectsIAmMember =
                    await _projectRepo.SearchIndexFilterAsync(_pagination.ToQueryable(projectsIAmMember),
                        searchQuery, filterQuery);
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
            var paginatedProjectsIAmMemberDtos =
                _projectService.MapToListProjectOverviewDto(paginatedProjectsIAmMember);
            return View(new MyProjectVM()
            {
                ProjectsILead = paginatedProjectsILeadDtos,
                ProjectsIAmMember = paginatedProjectsIAmMemberDtos,
                // OtherProjects = paginatedOtherProjects
            });
        }

        public IActionResult NoData(string msg)
        {
            return View((object)msg);
        }

        public async Task<IActionResult> ViewAllProjects()
        {
            var projects = await _projectService.ReturnAllProjectsVMsAsync();
            var ongoingProject = projects.allActiveProjects.Where(p => p.ProjectStatus >= 0 && p.ProjectStatus < 2);
            projects.allActiveProjects = ongoingProject.ToList();
            return View(projects);
        }

        public async Task<IActionResult> ViewAllSuccessfulProjects()
        {
            var projects = await _projectService.ReturnAllProjectsVMsAsync();
            var finished = projects.allActiveProjects.Where(p => p.ProjectStatus == 2).ToList();
            projects.allActiveProjects = finished.ToList();
            return View(projects);
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
            try
            {
                _logger.LogWarning("ImportFileProject");
                var projectObj =
                    await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(finishProjectVM.ProjectID));
                if (projectObj?.ProjectStatus == -1)
                {
                    TempData[MyConstants.Warning] = "You cannot finish project while this project is already shutdown!";
                    return RedirectToAction(nameof(ManageProject), new { id = finishProjectVM.ProjectID });
                }

                var projectID = finishProjectVM.ProjectID;
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
                var leaderID = HttpContext.Session.GetString("currentProjectLeaderID");
                await _roleService.DeleteRoleFromUserAsync(new Guid(leaderID), RoleConstants.ProjectLeader);
                var res = await _projectRepo.FinishProjectAsync(finishProjectVM);
                if (res)
                {
                    var link = Url.Action(nameof(ManageProject), "Project", new { id = projectID.ToString() },
                        Request.Scheme);
                    await _notificationService.UpdateProjectNotificationAsync(projectID, link, "finish", "");
                    TempData[MyConstants.Success] = "Finish project successfully!";
                    return RedirectToAction(nameof(ManageProject), new { id = projectID });
                }

                TempData[MyConstants.Error] =
                    "Failed to finish the project.\nRemember that a project must have at least one phase report before it can be finished!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
            _logger.LogWarning("UpdateProjectProfile");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(id));
            if (projectObj == null)
            {
                return NotFound();
            }
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProject), new { id = projectObj.ProjectID });
            }
            //prevent user from updating project that is not in progress
            var projectDto = _mapper.Map<UpdateProjectProfileRequestDto>(projectObj);
            projectDto.NewLeaderID = new Guid(HttpContext.Session.GetString("currentProjectLeaderID"));
            var currentProjectCEO = new Guid(HttpContext.Session.GetString("currentProjectCEOID"));
            IEnumerable<SelectListItem> StatusList = new List<SelectListItem>()
            {
                new SelectListItem { Text = "Pending", Value = "0" },
                new SelectListItem { Text = "In Progress", Value = "1" }
            };
            ViewData["StatusList"] = StatusList;

            ICollection<SelectListItem> MemberList = new List<SelectListItem>() { };

            foreach (var member in _projectService.FilterMemberOfProject(p => p.ProjectID.Equals(id) && p.Status >= 1))
            {
                if (member.Id != projectDto.NewLeaderID && member.Id != currentProjectCEO)
                {
                    if (await _roleService.IsInRoleAsync(member.Id, RoleConstants.ProjectLeader) 
                          || await _roleService.IsInRoleAsync(member.Id, RoleConstants.HeadOfOrganization))
                    {
                        continue;
                    }
                }

                MemberList.Add(new SelectListItem { Text = member.UserName, Value = member.Id.ToString() });
            }

            ViewData["MemberList"] = MemberList;
            return View(projectDto);
        }

        //POST: Project/UpdateProjectProfile
        [HttpPost]
        public async Task<IActionResult> UpdateProjectProfile(UpdateProjectProfileRequestDto updateProject,
            List<IFormFile> images)
        {
            _logger.LogWarning("UpdateProjectProfile post");
            var resUpdate = await _projectService.UpdateProjectProfileAsync(updateProject, images);
            if (resUpdate.Equals("No file") || resUpdate.Equals("Wrong extension"))
            {
                TempData[MyConstants.Error] = resUpdate.Equals("No file")
                    ? "No file to upload!"
                    : "Extension of some files is wrong!";
            }
            else if (resUpdate.Equals(MyConstants.Success))
            {
                var link = Url.Action(nameof(ManageProject), "Project", new { id = updateProject.ProjectID.ToString() },
                    Request.Scheme);
                await _notificationService.UpdateProjectNotificationAsync(updateProject.ProjectID, link, "update", "");
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
            _logger.LogWarning("ShutdownProject post");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(shutdownProjectVM.ProjectID));
            if (projectObj?.ProjectStatus == 2)
            {
                return Json(new { success = false, message = "You cannot shut down while this project is finished!" });
            }

            var userIDString = HttpContext.Session.GetString("currentUserID");
            var leaderID = HttpContext.Session.GetString("currentProjectLeaderID");
            await _roleService.DeleteRoleFromUserAsync(new Guid(leaderID), RoleConstants.ProjectLeader);

            var res = await _projectRepo.ShutdownProjectAsync(shutdownProjectVM);
            if (res && !string.IsNullOrEmpty(userIDString))
            {
                var link = Url.Action(nameof(ManageProject), "Project",
                    new { id = shutdownProjectVM.ProjectID.ToString() },
                    Request.Scheme);
                await _notificationService.UpdateProjectNotificationAsync(shutdownProjectVM.ProjectID, link, "shutdown",
                    shutdownProjectVM.Reason);
                await _walletService.RefundProjectWalletAsync(projectObj);
                return Json(new
                {
                    success = true, message = "Shutdown project successful!",
                    remind = "You just have shut down a project for \"" + shutdownProjectVM.Reason + "\"",
                    userIDString = userIDString
                });
            }

            return Json(new { success = false, message = "Fail to shutdown project!" });
        }

        [Authorize]
        public async Task<IActionResult> SendReportProjectRequest(Report report)
        {
            _logger.LogWarning("Send report project request");
            var projectObj =
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
            _logger.LogWarning("ManageProject get");
            if (string.IsNullOrEmpty(id.ToString()) || id.Equals(Guid.Empty))
            {
                return NotFound("id is empty!");
            }
            var detailProject = await _projectService.ReturnDetailProjectVMAsync(new Guid(id), HttpContext);
            if (detailProject != null)
            {
                return View(detailProject);
            }
            TempData[MyConstants.Error] = "Fail to get project!";
            return RedirectToAction(nameof(Index), new { id = HttpContext.Session.GetString("currentUserID") });
        }

        //----------------------manage project member -------------
        [Route("Project/ManageProjectMember/{projectID}")]
        public async Task<IActionResult> ManageProjectMember([FromRoute] Guid projectID,
            PaginationRequestDto paginationRequestDto)
        {
            _logger.LogWarning("ManageProjectMember get");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("currentProjectID")))
            {
                HttpContext.Session.SetString("currentProjectID", projectID.ToString());
                var leaderOfProject = await _projectService.GetProjectLeaderAsync(projectID);
                HttpContext.Session.SetString("currentProjectLeaderID", leaderOfProject.Id.ToString());
                var ceoOfProject =
                    _projectService.FilterMemberOfProject(x => x.Status == 2 && x.ProjectID == projectID);
                HttpContext.Session.SetString("currentProjectCEOID", ceoOfProject[0].Id.ToString());
            }

            var allProjectMember =
                _projectMemberRepo.FilterProjectMember(p =>
                    p.ProjectID.Equals(projectID) && p.Status >= 1 && p.Status < 4);
            if (allProjectMember == null)
            {
                throw new Exception("No member in this project!");
            }

            // var totalPM = allProjectMember.Count();
            // var totalPagePM = (int)Math.Ceiling((double)totalPM / pageSize);
            // var paginatedPM = _pagination.Paginate(allProjectMember, pageNumberPM, pageSize);
            // ViewBag.currentPagePM = pageNumberPM;
            // ViewBag.totalPagesPM = totalPagePM;
            var paginatedPM = _pagination.Paginate(query: allProjectMember, paginationRequestDto: paginationRequestDto,
                context: HttpContext);

            var joinRequests =
                _projectMemberRepo.FilterProjectMember(p => p.ProjectID.Equals(projectID) && p.Status == 0) ??
                Enumerable.Empty<ProjectMember>();
            var nums = joinRequests.Count();
            ViewData["hasJoinRequest"] = nums > 0;
            return View(new ManageProjectMemberVM
            {
                PaginationRequestDto = paginationRequestDto,
                ProjectMembers = allProjectMember
            });
        }

        public async Task<IActionResult> GetUsersNotInProject(string key)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            // Fetch the list of users who are not in the project
            var users = await _userRepository.GetAllUsersAsync();
            var usersInProject =
                _projectService.FilterMemberOfProject(p =>
                    p.ProjectID.Equals(new Guid(currentProjectID)) && p.Status >= 0 || p.Status ==-2);
            var usersNotInProject = users.Where(u => !usersInProject.Any(up => up.Id == u.Id));
            // Return as JSON (you could also return a PartialView)
            if (!string.IsNullOrEmpty(key))
            {
                usersNotInProject =
                    usersNotInProject.Where(u => u.UserName.Contains(key, StringComparison.OrdinalIgnoreCase));
            }

            return Json(usersNotInProject.Select(u => new
            {
                id = u.Id,
                name = u.UserName,
                email = u.Email,
                avatarUrl = u.UserAvatar
            }));
        }
        public async Task<IActionResult> InviteMembers(string userIds)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(new Guid(currentProjectID)));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
            }

            if (string.IsNullOrEmpty(userIds))
            {
                TempData[MyConstants.Error] = "No user selected!";
                return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
            }

            var userIdList = userIds.Split(',').Select(Guid.Parse).ToList();
            foreach (var userId in userIdList)
            {
                var user = _userRepository.GetAsync(u => u.Id == userId).Result;
                if (user != null && user.isBanned)
                {
                    TempData[MyConstants.Error] = $"Failed to send invitations because user {user.UserName} is banned!";
                    return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
                }

                var res = await _projectMemberRepo.InviteMemberAsync(userId, new Guid(currentProjectID));
                if (!res)
                {
                    TempData[MyConstants.Error] = "Failed to send invitation!";
                    return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
                }

                var linkUser = Url.Action(nameof(AcceptJoinInvitation), "Project",
                    new { projectId = new Guid(currentProjectID), memberId = userId }, Request.Scheme);
                var linkLeader = Url.Action(nameof(CancelJoinInvitation), "Project",
                    new { projectId = new Guid(currentProjectID), memberId = userId }, Request.Scheme);
                //send to user and leader
                var userString = HttpContext.Session.GetString("user");
                User currentUser = null;
                if (userString != null)
                {
                    currentUser = JsonConvert.DeserializeObject<User>(userString);
                }
                await _notificationService.InviteProjectMemberRequestNotificationAsync(projectObj, user, currentUser,linkUser,
                    linkLeader);
            }

            TempData[MyConstants.Success] = "Invite members successful!";
            return RedirectToAction(nameof(ManageProjectMember), new { id = new Guid(currentProjectID) });
        }

        public async Task<IActionResult> AcceptJoinInvitation(Guid projectId, Guid memberId)
        {
            var res = await _projectMemberRepo.AcceptJoinRequestAsync(memberId, projectId);
            var projectObj = await _projectRepo.GetProjectAsync(x => x.ProjectID.Equals(projectId));
            var user = _userRepository.GetAsync(u => u.Id == memberId).Result;
            if (res)
            {
                var userName = _userRepository.GetAsync(u => u.Id == memberId).Result.UserName;

                //send notification to accepted member
                var link = Url.Action(nameof(ManageProject), "Project", new { id = projectId.ToString() },
                    Request.Scheme);
                //send noti to leader
                await _notificationService.ProcessInviteProjectMemberRequestNotificationAsync(projectObj, user, link,
                    "join");
                TempData[MyConstants.Success] =
                    $"Welcome! You are now officially a member of the {projectObj.ProjectName} project.";
                return RedirectToAction(nameof(ManageProject), "Project", new { id = projectId.ToString() });
            }

            TempData[MyConstants.Error] =
                $"Apologies! You have not succeeded in joining the {projectObj.ProjectName} project.";
            return RedirectToAction(nameof(ManageProject), "Project", new { id = projectId.ToString() });
        }
        public async Task<IActionResult> CancelJoinInvitation(Guid projectId, Guid memberId)
        {
            var res = await _projectMemberRepo.DeleteAsync(x =>
                x.UserID == memberId && x.ProjectID == projectId && x.Status == -2);
            var user = _userRepository.GetAsync(u => u.Id == memberId).Result;
            var projectObj = await _projectRepo.GetProjectAsync(x => x.ProjectID.Equals(projectId));
            if (res != null)
            {
                var link = Url.Action(nameof(ManageProject), "Project", new { id = projectId.ToString() },
                    Request.Scheme);
                //send noti cancelled to user
                await _notificationService.ProcessInviteProjectMemberRequestNotificationAsync(projectObj, user, link,
                    "cancel");

                TempData[MyConstants.Success] = $"The invitation to {user.UserName} has been successfully canceled.";
                return RedirectToAction(nameof(ManageProject), "Project", new { id = projectId.ToString() });
            }

            TempData[MyConstants.Error] = $"Failed to cancel the invitation for {user.UserName}.";
            return RedirectToAction(nameof(ManageProject), "Project", new { id = projectId.ToString() });
        }

        [Route("Project/DeleteProjectMember/{memberID}")]
        public async Task<IActionResult> DeleteProjectMember([FromRoute] Guid memberID)
        {
            _logger.LogWarning("DeleteProjectMember get");

            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(new Guid(currentProjectID)));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
            }

            var res = await _projectMemberRepo.DeleteAsync(x =>
                x.UserID.Equals(memberID) && x.ProjectID.Equals(new Guid(currentProjectID)));
            if (res != null)
            {
                var link = Url.Action(nameof(ManageProject), "Project", new { id = currentProjectID },
                    Request.Scheme);
                await _notificationService.DeleteProjectMemberNotificationAsync(memberID, link, projectObj.ProjectName);
                TempData[MyConstants.Success] = "Delete project member successfully!";
                return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
            }

            TempData[MyConstants.Error] = "Fail to delete project member!";
            return RedirectToAction(nameof(ManageProjectMember), new { id = currentProjectID });
        }

        //----manage join request-----
        //create request
        [Authorize]
        public async Task<IActionResult> JoinProjectRequest(Guid memberID, Guid projectID)
        {
            _logger.LogWarning("JoinProjectRequest get");
            var projectObj = await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
            if (projectObj?.ProjectStatus == -1 || projectObj.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
            }
            

            var res = await _projectService.SendJoinProjectRequestAsync(projectID, memberID);

            if (res.Equals(MyConstants.Success))
            {
                //send notification and save it to database
                var link = Url.Action(nameof(ManageProject), "Project", new { id = projectObj.ProjectID.ToString() },
                    Request.Scheme);
                await _notificationService.JoinProjectRequestNotificationAsync(projectObj, link);

                TempData[MyConstants.Success] = "Join request sent successfully!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
            }
            else if (res.Equals(MyConstants.Warning))
            {
                TempData[MyConstants.Warning] = "Already sent another join request or has received an invitation from this project!";
                TempData[MyConstants.Subtitle] = "Please wait for the project leader response!";
            }
            else if (res.Equals(MyConstants.Error))
            {
                TempData[MyConstants.Error] = "Fail to send join request!";
            }

            return RedirectToAction(nameof(ManageProject), new { id = projectID });
        }

        //send notification to move out from project
        public async Task<IActionResult> LeaveProjectRequest(Guid projectID)
        {
            _logger.LogWarning("LeaveProjectRequest get");
            var currentUserID = HttpContext.Session.GetString("currentUserID");
            if (currentUserID != null)
            {
                var currentProjectLeaderID = HttpContext.Session.GetString("currentProjectLeaderID");
                var currentProjectCEOID = HttpContext.Session.GetString("currentProjectCEOID");
                var checkIsLeader = currentUserID.Equals(currentProjectLeaderID);
                var checkIsCEO = currentUserID.Equals(currentProjectCEOID);
                if (checkIsLeader || checkIsCEO)
                {
                    //TempData[MyConstants.Warning] = "You can not leave the project!";
                    TempData[MyConstants.Info] = checkIsLeader
                        ? "Transfer team leader rights if you still want to leave the project."
                        : (checkIsCEO ? "Leave project is not allowed while you are CEO." : null);
                    return RedirectToAction(nameof(ManageProject), new { id = projectID });
                }

                var res = await _projectMemberRepo.DeleteAsync(x =>
                    x.UserID == new Guid(currentUserID) && x.ProjectID == projectID);
                if (res != null)
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
        public async Task<IActionResult> ReviewJoinRequest([FromRoute] Guid projectID, int pageSizeJR = 10,
            int pageNumberJR = 1)
        {
            _logger.LogWarning("ReviewJoinRequest get");
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var projectObj = await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectMember), new { id = projectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectMember), new { id = projectID });
            }

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

        [Route("Project/AcceptJoinRequest/{memberID}")]
        public async Task<IActionResult> AcceptJoinRequest([FromRoute] Guid memberID)
        {
            _logger.LogWarning("AcceptJoinRequest get");
            var currentProjectID = new Guid(HttpContext.Session.GetString("currentProjectID"));
            var res = await _projectMemberRepo.AcceptJoinRequestAsync(memberID, currentProjectID);
            if (res)
            {
                //send notification to accepted member
                var link = Url.Action(nameof(ManageProject), "Project", new { id = currentProjectID.ToString() },
                    Request.Scheme);
                await _notificationService.ProcessJoinRequestNotificationAsync(memberID, link, "join");

                TempData[MyConstants.Success] = "Join request accepted successfully!";
                return RedirectToAction(nameof(ReviewJoinRequest), new { id = currentProjectID });
            }

            TempData[MyConstants.Error] = "Failed to accept the join request!";
            return RedirectToAction(nameof(ReviewJoinRequest), new { id = currentProjectID });
        }

        [Route("Project/DenyJoinRequest/{memberID}")]
        public async Task<IActionResult> DenyJoinRequest([FromRoute] Guid memberID)
        {
            _logger.LogWarning("DenyJoinRequest get");
            var currentProjectID = new Guid(HttpContext.Session.GetString("currentProjectID"));
            var res = await _projectMemberRepo.DenyJoinRequestAsync(memberID, currentProjectID);
            if (res)
            {
                //send notification to denied member
                var link = Url.Action(nameof(ManageProject), "Project", new { id = currentProjectID.ToString() },
                    Request.Scheme);
                await _notificationService.ProcessJoinRequestNotificationAsync(memberID, link, "deny");

                TempData[MyConstants.Success] = "Join request denied successfully!";
                return RedirectToAction(nameof(ReviewJoinRequest), new { id = currentProjectID });
            }

            TempData[MyConstants.Error] = "Failed to deny the join request!";
            return RedirectToAction(nameof(ReviewJoinRequest), new { id = currentProjectID });
        }

        //-------------------manage transaction history of project------------------------
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SendDonateRequest(Guid projectID, string donor)
        {
            _logger.LogWarning("SendDonateRequest get");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "This project is not in progress!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
            }
            else if (projectObj?.ProjectStatus == 1)
            {
                TempData[MyConstants.Warning] = "This project has stopped receiving donations!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "This project is finished!";
                return RedirectToAction(nameof(ManageProject), new { id = projectID });
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
            sendDonateRequestVM.PayRequestDto = new PayRequestDto
            {
                TargetId = projectID,
                TargetType = MyConstants.Project,
                ResourceID = moneyResource.ResourceID,
            };
            // Last but not least, set up a return url:
            var url = Url.Action("ManageProject", "Project", new { id = projectID }, Request.Scheme);
            ViewBag.returnUrl = url ?? "~/";
            return View(sendDonateRequestVM);
        }

        [HttpPost]
        public async Task<IActionResult> SendDonateRequest(SendDonateRequestVM sendDonateRequestVM,
            List<IFormFile> images)
        {
            _logger.LogWarning("SendDonateRequest post");
            var res = await _projectService.SendDonateRequestAsync(sendDonateRequestVM, images);
            if (!string.IsNullOrEmpty(res))
            {
                if (res.Equals("No file") || res.Equals("Wrong extension"))
                {
                    return Json(new
                    {
                        success = false,
                        message = res.Equals("No file")
                            ? "Please upload at least one proof image!"
                            : "Some files have the wrong extension!"
                    });
                }

                if (res.Equals("Exceed"))
                {
                    // Return JSON response with failure message
                    return Json(new
                    {
                        success = false,
                        message = "Cannot send resource that causes current quantity exceed expected quantity !"
                    });
                }
                else if (res.Equals(MyConstants.Success))
                {
                    var link = Url.Action(nameof(ManageProject), "Project", new { id = sendDonateRequestVM.ProjectID },
                        Request.Scheme);
                    await _notificationService.ProcessProjectDonationNotificationAsync(sendDonateRequestVM.ProjectID,
                        Guid.Empty, link, "donate");
                    return Json(new { success = true, message = "Your donation request was sent successfully!" });
                }
            }

            return Json(new { success = false, message = "Fail to send your donation request!" });
        }

        [Route("Project/ManageProjectDonor/{projectID}")]
        public async Task<IActionResult> ManageProjectDonor(Guid projectID, SearchRequestDto searchRequestDto,
            PaginationRequestDto paginationRequestDto)
        {
            _logger.LogWarning("ManageProjectDonor get");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("currentProjectID")))
            {
                HttpContext.Session.SetString("currentProjectID", projectID.ToString());
                var leaderOfProject = await _projectService.GetProjectLeaderAsync(projectID);
                HttpContext.Session.SetString("currentProjectLeaderID", leaderOfProject.Id.ToString());
                var ceoOfProject =
                    _projectService.FilterMemberOfProject(x => x.Status == 2 && x.ProjectID == projectID);
                HttpContext.Session.SetString("currentProjectCEOID", ceoOfProject[0].Id.ToString());
            }
            // If the search request dto is empty, then we set the default filter to accepted
            if (searchRequestDto.Filter == null)
            {
                searchRequestDto.Filter = SearchOptionsConstants.StatusAccepted;
            }
            
            // Base query:
            var userToPrjQueryable = _userToProjectTransactionHistoryRepo.GetAllAsQueryable(utp =>
                utp.ProjectResource.ProjectID.Equals(projectID) && utp.Status != 0);
            var orgToPrjQueryable = _organizationToProjectTransactionHistoryRepo.GetAllAsQueryable(utp =>
                utp.ProjectResource.ProjectID.Equals(projectID) && utp.Status != 0);

            // Setup search query and pagination
            searchRequestDto.Filter = string.IsNullOrEmpty(searchRequestDto.Filter) ?"Accepted":searchRequestDto.Filter;
            var transactionDtos =
                await _transactionViewService.SetupProjectTransactionDtosWithSearchParams(searchRequestDto,
                    userToPrjQueryable, orgToPrjQueryable);
            var paginated = _pagination.Paginate(transactionDtos, HttpContext, paginationRequestDto, searchRequestDto);
           
            var projectTransactionHistoryVM = new ProjectTransactionHistoryVM
            {
                Transactions = paginated,
                PaginationRequestDto = paginationRequestDto,
                SearchRequestDto = searchRequestDto
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
            _logger.LogWarning("ReviewUserDonateRequest get");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectDonor), new { id = projectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectDonor), new { id = projectID });
            }

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
            _logger.LogWarning("ReviewOrgDonateRequest get");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectDonor), new { id = projectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectDonor), new { id = projectID });
            }

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

        [HttpPost]
        public async Task<IActionResult> AcceptDonateRequest(ReviewProjectDonateRequestVM reviewProjectDonateRequestVM,
            List<IFormFile> proofImages)
        {
            _logger.LogWarning("AcceptDonateRequest get");
            var res = false;
            var resImage = await _cloudinaryUploader.UploadMultiImagesAsync(proofImages);

            if (resImage.Equals("Wrong extension") || resImage.Equals("No file"))
            {
                TempData[MyConstants.Error] = resImage.Equals("No file")
                    ? "No file to upload!"
                    : "Extension of some files is wrong!";
            }
            else
            {
                switch (reviewProjectDonateRequestVM.TypeDonor)
                {
                    case "User":
                        var transactionObj = await _userToProjectTransactionHistoryRepo.GetAsync(x =>
                            x.TransactionID.Equals(reviewProjectDonateRequestVM.TransactionID));
                        if (transactionObj != null)
                        {
                            transactionObj.Attachments = resImage;
                            res = await _userToProjectTransactionHistoryRepo.AcceptUserDonateRequestAsync(
                                transactionObj);
                        }

                        var link = Url.Action(nameof(ManageProjectDonor), "Project",
                            new { projectID = transactionObj.ProjectResource.ProjectID },
                            Request.Scheme);
                        await _notificationService.ProcessProjectDonationNotificationAsync
                        (transactionObj.ProjectResource.ProjectID, transactionObj.TransactionID, link,
                            "AcceptUserDonate");
                        break;
                    case "Organization":
                        var transactionOrgObj = await _organizationToProjectTransactionHistoryRepo.GetAsync(x =>
                            x.TransactionID.Equals(reviewProjectDonateRequestVM.TransactionID));
                        if (transactionOrgObj != null)
                        {
                            transactionOrgObj.Attachments = resImage;
                            res = await _organizationToProjectTransactionHistoryRepo.AcceptOrgDonateRequestAsync(
                                transactionOrgObj);
                        }

                        var link2 = Url.Action(nameof(ManageProjectDonor), "Project",
                            new { projectID = transactionOrgObj.ProjectResource.ProjectID },
                            Request.Scheme);
                        await _notificationService.ProcessProjectDonationNotificationAsync
                        (transactionOrgObj.ProjectResource.ProjectID, transactionOrgObj.TransactionID, link2,
                            "AcceptOrgDonate");
                        break;
                    default:
                        return NotFound();
                }
            }

            if (res)
            {
                TempData[MyConstants.Success] = "Donation request accepted successfully!";
                return (reviewProjectDonateRequestVM.TypeDonor.Equals("User"))
                    ? RedirectToAction(nameof(ReviewUserDonateRequest),
                        new { id = HttpContext.Session.GetString("currentProjectID") })
                    : RedirectToAction(nameof(ReviewOrgDonateRequest),
                        new { id = HttpContext.Session.GetString("currentProjectID") });
            }

            if (TempData[MyConstants.Error] == null)
            {
                TempData[MyConstants.Error] = "Failed to accept the donation request!";
            }

            return (reviewProjectDonateRequestVM.TypeDonor.Equals("User"))
                ? RedirectToAction(nameof(ReviewUserDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") })
                : RedirectToAction(nameof(ReviewOrgDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptDonateRequestAll(string donor, List<IFormFile> proofImages)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var link = Url.Action(nameof(ManageProjectDonor), "Project", new { projectID = new Guid(currentProjectID) },
                Request.Scheme);
            var res = await _projectService.AcceptDonateProjectRequestAllAsync(new Guid(currentProjectID), donor,
                proofImages, link);
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
        public async Task<IActionResult> DenyDonateRequest(ReviewProjectDonateRequestVM reviewProjectDonateRequestVM)
        {
            _logger.LogWarning("DenyDonateRequest get");
            var res = false;
            switch (reviewProjectDonateRequestVM.TypeDonor)
            {
                case "User":
                    var transactionObj = await _userToProjectTransactionHistoryRepo.GetAsync(x =>
                        x.TransactionID.Equals(reviewProjectDonateRequestVM.TransactionID));
                    if (transactionObj != null)
                    {
                        transactionObj.Message += "\nReason: " + reviewProjectDonateRequestVM.ReasonToDeny;
                        res = await _userToProjectTransactionHistoryRepo.DenyUserDonateRequestAsync(transactionObj);
                    }

                    var link = Url.Action(nameof(ManageProjectDonor), "Project",
                        new { projectID = transactionObj.ProjectResource.ProjectID },
                        Request.Scheme);
                    await _notificationService.ProcessProjectDonationNotificationAsync
                    (transactionObj.ProjectResource.ProjectID, transactionObj.TransactionID, link,
                        "DenyUserDonate");
                    break;
                case "Organization":
                    var transactionOrgObj = await _organizationToProjectTransactionHistoryRepo.GetAsync(x =>
                        x.TransactionID.Equals(reviewProjectDonateRequestVM.TransactionID));
                    if (transactionOrgObj != null)
                    {
                        transactionOrgObj.Message += "\nReason: " + reviewProjectDonateRequestVM.ReasonToDeny;
                        res = await _organizationToProjectTransactionHistoryRepo.DenyOrgDonateRequestAsync(
                            transactionOrgObj);
                    }

                    var link2 = Url.Action(nameof(ManageProjectDonor), "Project",
                        new { projectID = transactionOrgObj.ProjectResource.ProjectID },
                        Request.Scheme);
                    await _notificationService.ProcessProjectDonationNotificationAsync
                    (transactionOrgObj.ProjectResource.ProjectID, transactionOrgObj.TransactionID, link2,
                        "DenyOrgDonate");
                    break;
            }

            if (res)
            {
                TempData[MyConstants.Success] = "Donation request denied successfully!";
                return (reviewProjectDonateRequestVM.TypeDonor.Equals("User"))
                    ? RedirectToAction(nameof(ReviewUserDonateRequest),
                        new { id = HttpContext.Session.GetString("currentProjectID") })
                    : RedirectToAction(nameof(ReviewOrgDonateRequest),
                        new { id = HttpContext.Session.GetString("currentProjectID") });
            }

            TempData[MyConstants.Error] = "Failed to deny the donation request!";
            return (reviewProjectDonateRequestVM.TypeDonor.Equals("User"))
                ? RedirectToAction(nameof(ReviewUserDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") })
                : RedirectToAction(nameof(ReviewOrgDonateRequest),
                    new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        public async Task<IActionResult> DenyDonateRequestAll(string donor, string reasonToDeny)
        {
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var link = Url.Action(nameof(ManageProjectDonor), "Project", new { projectID = new Guid(currentProjectID) },
                Request.Scheme);
            var res = await _projectService.DenyDonateProjectRequestAllAsync(new Guid(currentProjectID), donor,
                reasonToDeny, link);
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

        [Route("Project/ManageProjectResource/{projectID}")]
        public async Task<IActionResult> ManageProjectResource(Guid projectID, PaginationRequestDto paginationRequestDto)
        {
            _logger.LogWarning("ManageProjectResource get");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("currentProjectID")))
            {
                HttpContext.Session.SetString("currentProjectID", projectID.ToString());
                var leaderOfProject = await _projectService.GetProjectLeaderAsync(projectID);
                HttpContext.Session.SetString("currentProjectLeaderID", leaderOfProject.Id.ToString());
                var ceoOfProject =
                    _projectService.FilterMemberOfProject(x => x.Status == 2 && x.ProjectID == projectID);
                HttpContext.Session.SetString("currentProjectCEOID", ceoOfProject[0].Id.ToString());
            }

            var allResource = await _projectResourceRepo.FilterProjectResourceAsync(
                p => p.ProjectID.Equals(projectID));
            if (allResource.Count() == 0)
            {
                return RedirectToAction("NoData", new { msg = "No resource has been created" });
            }
            // Pagination
            var paginated =
                _pagination.Paginate<ProjectResource>(allResource.ToList(), HttpContext, paginationRequestDto, null);
            ViewBag.PaginationRequestDto = paginationRequestDto;
            return View(paginated);
        }

        [HttpPost]
        public async Task<IActionResult> AddProjectResourceType(ProjectResource projectResource)
        {
            _logger.LogWarning("AddProjectResourceType post");

            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectResource.ProjectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectResource), new { id = projectResource.ProjectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectResource), new { id = projectResource.ProjectID });
            }

            if (ModelState.IsValid)
            {
                var resAddResource = await _projectResourceRepo.AddResourceTypeAsync(projectResource);
                if (resAddResource)
                {
                    TempData[MyConstants.Success] = "Add resource type successfully!";
                    return RedirectToAction(nameof(ManageProjectResource), new { id = projectResource.ProjectID });
                }
            }

            TempData[MyConstants.Error] = "Fail to add new resource type!";
            return RedirectToAction(nameof(ManageProjectResource),
                new { id = HttpContext.Session.GetString("currentProjectID") });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateResourceType(ProjectResource projectResource)
        {
            _logger.LogWarning("UpdateResourceType post");

            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectResource.ProjectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectResource), new { id = projectResource.ProjectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectResource), new { id = projectResource.ProjectID });
            }

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
            _logger.LogWarning("DeleteResourceType post");
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(new Guid(currentProjectID)));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectResource), new { id = currentProjectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectResource), new { id = currentProjectID });
            }

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
            _logger.LogWarning("ManageProjectPhaseReport get");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("currentProjectID")))
            {
                HttpContext.Session.SetString("currentProjectID", projectID.ToString());
                var leaderOfProject = await _projectService.GetProjectLeaderAsync(projectID);
                HttpContext.Session.SetString("currentProjectLeaderID", leaderOfProject.Id.ToString());
                var ceoOfProject =
                    _projectService.FilterMemberOfProject(x => x.Status == 2 && x.ProjectID == projectID);
                HttpContext.Session.SetString("currentProjectCEOID", ceoOfProject[0].Id.ToString());
            }

            var allUpdate =
                await _projectHistoryRepo.GetAllPhaseReportsAsync(u => u.ProjectID.Equals(projectID));
            if (allUpdate.ToList().Count() == 0)
            {
                return RedirectToAction("NoData", new { msg = "No update has been created" });
            }

            allUpdate = allUpdate.OrderByDescending(x => x.Date).ToList();
            return View(allUpdate);
        }

        [HttpGet]
        public async Task<IActionResult> AddProjectPhaseReport(Guid projectID)
        {
            _logger.LogWarning("AddProjectPhaseReport get");

            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectPhaseReport), new { id = projectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectPhaseReport), new { id = projectID });
            }

            var existingDates = await _projectService.GetExistingReportDatesAsync(projectID);
            // Convert to a list of string dates in "YYYY-MM-DD" format
            var disabledDates = existingDates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
            ViewBag.DisabledDates = disabledDates;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProjectPhaseReport(History history, List<IFormFile> images)
        {
            _logger.LogWarning("AddProjectPhaseReport post");
            var resAdd = await _projectService.AddProjectPhaseReportAsync(history, images);
            if (resAdd.Equals("No file") || resAdd.Equals("Wrong extension"))
            {
                TempData[MyConstants.Error] = resAdd.Equals("No file")
                    ? "A report file must be upload!"
                    : "Some files have the wrong extension!";
            }
            else if (resAdd.Equals(MyConstants.Success))
            {
                TempData[MyConstants.Success] = "Add project update successfully!";
                var link = Url.Action(nameof(ManageProjectPhaseReport), "Project", new { id = history.ProjectID },
                    Request.Scheme);
                await _notificationService.ProcessProjectPhaseNotificationAsync(history.ProjectID, link, "add");
            }
            else
            {
                TempData[MyConstants.Error] = "Fail to add project update!";
            }

            return RedirectToAction(nameof(ManageProjectPhaseReport), new { id = history.ProjectID });
        }

        public async Task<IActionResult> EditProjectPhaseReport(Guid historyID, Guid projectID)
        {
            _logger.LogWarning("EditProjectPhaseReport get");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectPhaseReport), new { id = projectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectPhaseReport), new { id = projectID });
            }

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
            _logger.LogWarning("EditProjectPhaseReport post");
            var resEdit = await _projectService.EditProjectPhaseReportAsync(history, images);
            if (resEdit.Equals("No file") || resEdit.Equals("Wrong extension"))
            {
                TempData[MyConstants.Error] = resEdit.Equals("No file")
                    ? "Please upload at least one image!"
                    : "Some files have the wrong extension!";
            }
            else if (resEdit.Equals(MyConstants.Success))
            {
                TempData[MyConstants.Success] = "Update project update successfully!";
                var link = Url.Action(nameof(ManageProjectPhaseReport), "Project", new { id = history.ProjectID },
                    Request.Scheme);
                await _notificationService.ProcessProjectPhaseNotificationAsync(history.ProjectID, link, "update");
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
            _logger.LogWarning("DeleteProjectPhaseReport get");
            var currentProjectID = HttpContext.Session.GetString("currentProjectID");
            var projectObj =
                await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(new Guid(currentProjectID)));
            if (projectObj?.ProjectStatus == -1)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is not in progress!";
                return RedirectToAction(nameof(ManageProjectPhaseReport), new { id = currentProjectID });
            }
            else if (projectObj?.ProjectStatus == 2)
            {
                TempData[MyConstants.Warning] = "Action is not allowed once the project is finished!";
                return RedirectToAction(nameof(ManageProjectPhaseReport), new { id = currentProjectID });
            }

            var res = await _projectHistoryRepo.DeletePhaseReportAsync(id);
            if (res)
            {
                TempData[MyConstants.Success] = "Delete project update successfully!";
                var link = Url.Action(nameof(ManageProjectPhaseReport), "Project",
                    new { id = new Guid(currentProjectID) },
                    Request.Scheme);
                await _notificationService.ProcessProjectPhaseNotificationAsync(new Guid(currentProjectID), link,
                    "delete");
                return RedirectToAction(nameof(ManageProjectPhaseReport),
                    new { id = currentProjectID });
            }

            TempData[MyConstants.Error] = "Fail to delete project update!";
            return RedirectToAction(nameof(ManageProjectPhaseReport),
                new { id = currentProjectID });
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

            var currentOrganization = await _organizationService.GetOrganizationVMByUserIDAsync(currentUser.Id);
            // Only get the person that is not the CEO of other organization
            // Because a CEO can only lead the project of that CEO
            var memberToAssignToProject = new List<OrganizationMember>();
            foreach (var mem in currentOrganization.OrganizationMember)
            {
                var isPrjLeader = await _roleService.IsInRoleAsync(mem.UserID, RoleConstants.ProjectLeader);
                var isCEO = await _roleService.IsInRoleAsync(mem.UserID, RoleConstants.HeadOfOrganization);
                // To become a project leader, the member must: Not leading other organization && not a project leader of a project
                // If the user is CEO of the CURRENT organization, then the CEO must currently not leading any project 
                if (mem.Status == 2 && !isPrjLeader)
                {
                    memberToAssignToProject.Add(mem);
                }
                // If user is not a project leader, but is other organization CEO, don't allow them
                else if (!isPrjLeader && !isCEO)
                {
                    memberToAssignToProject.Add(mem);
                }
            }

            currentOrganization.OrganizationMember = memberToAssignToProject;
            var projectVM = new ProjectVM()
            {
                ProjectID = Guid.NewGuid(),
                OrganizationID = currentOrganization.OrganizationID,
                ProjectStatus = 0,
                StartTime = DateOnly.FromDateTime(DateTime.UtcNow),
                OrganizationVM = currentOrganization,
                RequestID = requestId,
                Attachment = request.Attachment ?? "/images/defaultPrj.jpg",
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
        [Authorize]
        public async Task<IActionResult> CreateProject(Guid? requestId)
        {
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            if (requestId != null)
            {
                currentOrganization = await _organizationService.GetOrganizationVMByUserIDAsync(currentUser.Id);
            }

            if (currentOrganization.OrganizationStatus < 1)
            {
                TempData[MyConstants.Error] =
                    "Your organization needs to be approved by an admin before accepting a request.";
                return RedirectToAction("MyOrganization", "Organization", new { userId = currentUser.Id });
            }

            // Only get the person that is not the CEO of OTHER organization
            // Because a CEO can only lead the project of that CEO
            var memberToAssignToProject = new List<OrganizationMember>();
            foreach (var mem in currentOrganization.OrganizationMember)
            {
                var isPrjLeader = await _roleService.IsInRoleAsync(mem.UserID, RoleConstants.ProjectLeader);
                var isCEO = await _roleService.IsInRoleAsync(mem.UserID, RoleConstants.HeadOfOrganization);
                // To become a project leader, the member must: Not leading other organization && not a project leader of a project
                // If the user is CEO of the CURRENT organization, then the CEO must currently not leading any project 
                if (mem.Status == 2 && !isPrjLeader)
                {
                    memberToAssignToProject.Add(mem);
                }
                // If user is not a project leader, but is other organization CEO, don't allow them
                else if (!isPrjLeader && !isCEO)
                {
                    memberToAssignToProject.Add(mem);
                }
            }

            currentOrganization.OrganizationMember = memberToAssignToProject;
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
        public async Task<IActionResult> CreateProject(ProjectVM projectVM, List<IFormFile> images,
            int expectedQuantity,
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
                currentOrganization = await _organizationService.GetOrganizationVMByUserIDAsync(currentUser.Id);
            }

            projectVM.OrganizationVM = currentOrganization;

            if (!await _roleService.IsInRoleAsync(currentUser, RoleConstants.ProjectLeader) &&
                projectVM.LeaderID == Guid.Empty)
            {
                projectVM.LeaderID = currentUser.Id;
            }

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
                    if (images.Count != 0)
                    {
                        // organization.OrganizationPictures = Util.UploadImage(image, @"images\Organization");
                        var resImages = await _cloudinaryUploader.UploadMultiImagesAsync(images);
                        if (!(resImages.Equals("Wrong extension") || resImages.Equals("No file")))
                        {
                            projectVM.Attachment = resImages;

                        }
                        else
                        {
                            TempData[MyConstants.Error] = resImages.Equals("No file")
                                ? "No file to upload!"
                                : "Extension of some files is wrong!";
                        }
                    }

                    if (projectVM.ProjectEmail == null)
                    {
                        projectVM.ProjectEmail = Leader.Email;
                    }

                    if (projectVM.ProjectPhoneNumber == null)
                    {
                        projectVM.ProjectPhoneNumber = Leader.PhoneNumber;
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
                            Attachment = projectVM.Attachment ?? "/images/defaultPrj.jpg",
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

                            await _roleService.AddUserToRoleAsync(projectVM.LeaderID, RoleConstants.ProjectLeader);
                            return RedirectToAction(nameof(AutoJoinProject),
                                new { projectId = project.ProjectID, leaderId = projectVM.LeaderID });
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "You have no leader to lead this project!");
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

            if (!currentUser.Id.Equals(leaderId))
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
                UserID = currentUser.Id,
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
            var link = Url.Action(nameof(ManageProjectResource), "Project", new { id = projectResource.ProjectID },
                Request.Scheme);
            await _notificationService.AddProjectResourceNotificationAsync(projectResource.ProjectID, link);
            return RedirectToAction(nameof(AddProjectResource));
        }

        public async Task<IActionResult> ViewAllSuccessProject()
        {
            var successfulProjecs =
                await _projectRepo.GetAllAsync(p => p.ProjectStatus == 2); // Get all finished project
            return View(successfulProjecs);
        }
    }
}