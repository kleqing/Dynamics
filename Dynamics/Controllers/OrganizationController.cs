using Dynamics.DataAccess.Repository;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Web.Helpers;
using AutoMapper;

namespace Dynamics.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IOrganizationVMService _organizationService;
        private readonly IUserToOragnizationTransactionHistoryVMService _userToOragnizationTransactionHistoryVMService;
        private readonly IProjectVMService _projectVMService;
        private readonly IOrganizationToProjectHistoryVMService _organizationToProjectHistoryVMService;
        private readonly CloudinaryUploader _cloudinaryUploader;
        private readonly IOrganizationService _orgDisplayService;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IOrganizationResourceRepository _organizationResourceRepository;
        private readonly INotificationService _notificationService;

        private readonly IUserToOrganizationTransactionHistoryRepository
            _userToOrganziationTransactionHistoryRepository;

        private readonly IOrganizationToProjectTransactionHistoryRepository
            _organizationToProjectTransactionHistoryRepository;

        private readonly ITransactionViewService _transactionViewService;
        private readonly IPagination _pagination;
        private readonly IRoleService _roleService;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IMapper _mapper;
        private readonly IProjectResourceRepository _projectResourceRepository;
        private readonly IProjectService _projectService;
        private readonly IWalletService _walletService;

        public OrganizationController(IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IOrganizationVMService organizationService,
            IUserToOragnizationTransactionHistoryVMService userToOragnizationTransactionHistoryVMService,
            IProjectVMService projectVMService,
            IOrganizationToProjectHistoryVMService organizationToProjectHistoryVMService,
            CloudinaryUploader cloudinaryUploader, IOrganizationService orgDisplayService,
            IOrganizationMemberRepository organizationMemberRepository,
            IOrganizationResourceRepository organizationResourceRepository, INotificationService notificationService,
            IUserToOrganizationTransactionHistoryRepository userToOrganizationTransactionHistoryRepository,
            IOrganizationToProjectTransactionHistoryRepository organizationToProjectTransactionHistoryRepository,
            IProjectService projectService,
            IMapper mapper, IProjectResourceRepository projectResourceRepository,
            ITransactionViewService transactionViewService, IPagination pagination, IRoleService roleService,
            IProjectMemberRepository projectMemberRepository, IWalletService walletService)
        {
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _organizationService = organizationService;
            _userToOragnizationTransactionHistoryVMService = userToOragnizationTransactionHistoryVMService;
            _projectVMService = projectVMService;
            _organizationToProjectHistoryVMService = organizationToProjectHistoryVMService;
            _cloudinaryUploader = cloudinaryUploader;
            _orgDisplayService = orgDisplayService;
            _organizationMemberRepository = organizationMemberRepository;
            _organizationResourceRepository = organizationResourceRepository;
            _notificationService = notificationService;
            _userToOrganziationTransactionHistoryRepository = userToOrganizationTransactionHistoryRepository;
            _organizationToProjectTransactionHistoryRepository = organizationToProjectTransactionHistoryRepository;
            _transactionViewService = transactionViewService;
            _pagination = pagination;
            _roleService = roleService;
            _projectMemberRepository = projectMemberRepository;
            _mapper = mapper;
            _projectResourceRepository = projectResourceRepository;
            _walletService = walletService;
            _projectService = projectService;
        }

        //The index use the cards at homepage to display instead - Kiet
        public async Task<IActionResult> Index()
        {
            var orgs = _organizationRepository.GetAll(org => org.OrganizationStatus == 1);
            var organizationVMs = _orgDisplayService.MapToOrganizationOverviewDtoList(await orgs.ToListAsync());
            return View(organizationVMs);
        }

        //GET: /Organization/Create
        [HttpGet]
        public IActionResult Create()
        {
            //get current user
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            if (currentUser.UserAddress == null || currentUser.PhoneNumber == null)
            {
                TempData[MyConstants.Error] = "You need update Your profile before Create new organization";
                return RedirectToAction("Edit", "User");
            }

            var organization = new Organization()
            {
                OrganizationID = Guid.NewGuid(),
                StartTime = DateOnly.FromDateTime(DateTime.UtcNow),
                OrganizationEmail = currentUser.Email,
                OrganizationPhoneNumber = currentUser.PhoneNumber,
                OrganizationAddress = currentUser.UserAddress,
            };
            return View(organization);
        }

        //POST: /Organizations/Create
        [HttpPost]
        public async Task<IActionResult> Create(Organization organization, List<IFormFile> images)
        {
            // Check if user already has an organization
            //get current user
            try
            {
                var userString = HttpContext.Session.GetString("user");
                User currentUser = null;
                if (userString != null)
                {
                    currentUser = JsonConvert.DeserializeObject<User>(userString);
                }

                var existOrg = await _organizationRepository.GetOrganizationUserLead(currentUser.Id);
                if (existOrg != null)
                {
                    TempData[MyConstants.Error] = "Create organization failed!";
                    TempData[MyConstants.Subtitle] = "You already a leader of an organization";
                    return View(organization);
                }

                //set picture for Organization
                if (images.Count != 0)
                {
                    // organization.OrganizationPictures = Util.UploadImage(image, @"images\Organization");
                    var resImages = await _cloudinaryUploader.UploadMultiImagesAsync(images);
                    if (!(resImages.Equals("Wrong extension") || resImages.Equals("No file")))
                    {
                        organization.OrganizationPictures = resImages;
                    }
                    else
                    {
                        TempData[MyConstants.Error] = resImages.Equals("No file")
                            ? "No file to upload!"
                            : "Extension of some files is wrong!";
                    }
                }
                else
                {
                    organization.OrganizationPictures = "/images/defaultOrg.jpg";
                }

                if (userString != null)
                {
                    currentUser = JsonConvert.DeserializeObject<User>(userString);
                }

                //set contact for Organization
                if (organization.OrganizationEmail == null)
                {
                    organization.OrganizationEmail = currentUser.Email;
                }

                if (organization.OrganizationPhoneNumber == null)
                {
                    organization.OrganizationPhoneNumber = currentUser.PhoneNumber;
                }

                if (organization.OrganizationAddress == null)
                {
                    organization.OrganizationAddress = currentUser.UserAddress;
                }

                var result = await _organizationRepository.AddOrganizationAsync(organization);
                if (result)
                {
                    var organizationResource = new OrganizationResource()
                    {
                        OrganizationID = organization.OrganizationID,
                        ResourceName = "Money",
                        Quantity = 0,
                        Unit = "VND",
                    };
                    await _organizationRepository.AddOrganizationResourceSync(organizationResource);
                    await _roleService.AddUserToRoleAsync(currentUser.Id, RoleConstants.HeadOfOrganization);
                    TempData[MyConstants.Success] = "Create organization successfully!";
                    return RedirectToAction(nameof(JoinOrganization),
                        new
                        {
                            organizationId = organization.OrganizationID, status = 2, userId = currentUser.Id
                        }); //status 2 : CEOID   0 : processing   1 : membert
                }
            }
            catch (Exception e)
            {
                TempData[MyConstants.Error] = "Create organization failed!";
                TempData[MyConstants.Subtitle] = e.Message;
            }

            TempData[MyConstants.Error] = "Create organization failed!";
            return View(organization);
        }

        public async Task<IActionResult> MyOrganization(Guid userId)
        {
            // Get all organization that user joined / leader of
            var myOrganizationMembers = await _organizationMemberRepository.GetAllAsync(om => om.UserID == userId);
            if (myOrganizationMembers.IsNullOrEmpty()) return RedirectToAction("Index", "Organization");
            var myOrgs = new List<Organization>();
            var otherOrgs = new List<Organization>();
            // Find organization where the user is not the CEO 
            foreach (var organizationMember in myOrganizationMembers)
            {
                if (organizationMember.Status == 2) myOrgs.Add(organizationMember.Organization);
                else otherOrgs.Add(organizationMember.Organization);
            }
            // Get real organizations based on the project members

            var MyOrgDtos = _orgDisplayService.MapToOrganizationOverviewDtoList(myOrgs);
            var OtherOrgDtos = _orgDisplayService.MapToOrganizationOverviewDtoList(otherOrgs);
            return View(new MyOrganizationVM
            {
                JoinedOrgs = OtherOrgDtos,
                MyOrg = MyOrgDtos,
            });
        } //fix session done

        public async Task<IActionResult> Detail(Guid organizationId)
        {
            //Creat current organization
            var organizationVM =
                await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(organizationId));
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);
            return View(organizationVM);
        }

        public async Task<IActionResult> Edit(Guid organizationId)
        {
            // can be use Session current organization
            var organization =
                await _organizationRepository.GetOrganizationAsync(o => o.OrganizationID.Equals(organizationId));
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Organization organization, List<IFormFile> images)
        {
            // can be use Session current organization
            if (organization != null)
            {
                var resImages = await _cloudinaryUploader.UploadMultiImagesAsync(images);
                if (!(resImages.Equals("Wrong extension") || resImages.Equals("No file")))
                {
                    organization.OrganizationPictures += "," + resImages;
                }
                else
                {
                    TempData[MyConstants.Error] = resImages.Equals("No file")
                        ? "No file to upload!"
                        : "Extension of some files is wrong!";
                }

                if (await _organizationRepository.UpdateOrganizationAsync(organization))
                {
                    var link = Url.Action(nameof(Detail), "Organization",
                        new { organizationId = organization.OrganizationID },
                        Request.Scheme);
                    await _notificationService.UpdateOrganizationNotificationAsync(organization.OrganizationID, link);
                    TempData[MyConstants.Success] = "Update organization successfully!";
                    return RedirectToAction("Detail", new { organizationId = organization.OrganizationID });
                }
            }

            TempData[MyConstants.Error] = "Update organization failed!";
            return View(organization);
        }

        public async Task<IActionResult> ShutDown(Guid organizationId)
        {
            var organizationVM =
                await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(organizationId));

            bool boolOrganizationResource = true;
            foreach (var or in organizationVM.OrganizationResource)
            {
                if (or.Quantity > 0)
                {
                    boolOrganizationResource = false;
                    break;
                }
            }

            var boolProject = true;
            foreach (var p in organizationVM.Project)
            {
                if (p.ProjectStatus != 2 && p.ProjectStatus != -1)
                {
                    boolProject = false;
                    break;
                }
            }

            var organization =
                await _organizationRepository.GetOrganizationAsync(o => o.OrganizationID.Equals(organizationId));
            if (boolOrganizationResource && boolProject)
            {
                organization.ShutdownDay = DateOnly.FromDateTime(DateTime.Now);
                organization.OrganizationStatus = -2;
                if (await _organizationRepository.UpdateOrganizationAsync(organization))
                {
                    await _walletService.RefundOrganizationWalletAsync(organization);
                    TempData[MyConstants.Success] = "Shut Down organization successfully!";
                    return RedirectToAction(nameof(MyOrganization), new { organizationId = organizationId });
                }
                else
                {
                    TempData[MyConstants.Error] = "Shut Down organization Failed!";
                    return RedirectToAction("Detail", new { organizationId = organization.OrganizationID });
                }
            }
            else if (!boolOrganizationResource)
            {
                TempData[MyConstants.Error] =
                    "Shut Down organization Failed because exist at least a organization resource available!";
                return RedirectToAction("Detail", new { organizationId = organization.OrganizationID });
            }
            else
            {
                TempData[MyConstants.Error] =
                    "Shut Down organization Failed because exist at least a project available!";
                return RedirectToAction("Detail", new { organizationId = organization.OrganizationID });
            }
        }

        //Organization Member
        public async Task<IActionResult> ManageOrganizationMember()
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            //Creat current organization
            var organizationVM =
                await _organizationService.GetOrganizationVMAsync(o =>
                    o.OrganizationID.Equals(currentOrganization.OrganizationID));
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

            return View(organizationVM);
        }

        [Authorize]
        public IActionResult sendRequestJoinOrganization(Guid organizationId, Guid Id)
        {
            // Get the id from session here, no need to pass it from the view - Kiet
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            TempData[MyConstants.Success] = "Send request successfully!";
            return RedirectToAction(nameof(JoinOrganization),
                new { organizationId = organizationId, status = 0, userId = currentUser.Id });
        }

        public IActionResult ManageRequestJoinOrganization(Guid organizationId)
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            return View(currentOrganization);
        }


        public async Task<IActionResult> AcceptRquestJoin(Guid organizationId, Guid userId)
        {
            return RedirectToAction(nameof(JoinOrganization),
                new { organizationId = organizationId, status = 1, userId = userId });
        }


        public async Task<IActionResult> JoinOrganization(Guid organizationId, int status, Guid userId)
        {
            var organizationMember = new OrganizationMember()
            {
                UserID = userId,
                OrganizationID = organizationId,
                Status = status,
            };

            try
            {
                if (status == 0)
                {
                    await _organizationRepository.AddOrganizationMemberSync(organizationMember);
                    // Prevent send join request to be replaced by this one
                    if (TempData[MyConstants.Success] == null)
                    {
                        TempData[MyConstants.Success] = "You have successfully joined the organization.";
                    }

                    var link = Url.Action(nameof(Detail), "Organization", new { organizationId },
                        Request.Scheme);
                    await _notificationService.ProcessOrganizationJoinRequestNotificationAsync(userId, organizationId,
                        link, "send");
                }
                else if (status == 2)
                {
                    await _organizationRepository.AddOrganizationMemberSync(organizationMember);
                }
                else
                {
                    await _organizationRepository.UpdateOrganizationMemberAsync(organizationMember);
                    TempData[MyConstants.Success] = "Your membership status has been updated successfully.";
                    var link = Url.Action(nameof(Detail), "Organization", new { organizationId },
                        Request.Scheme);
                    await _notificationService.ProcessOrganizationJoinRequestNotificationAsync(userId, organizationId,
                        link, "join");
                }

                var organizationVM =
                    await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(organizationId));
                HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY,
                    organizationVM);

                if (status == 1)
                {
                    return RedirectToAction(nameof(ManageRequestJoinOrganization),
                        new { organizationId = organizationId });
                }

                return RedirectToAction(nameof(MyOrganization), new { organizationId = organizationId });
            }
            catch (Exception ex)
            {
                // If there's an error, notify the user
                TempData[MyConstants.Error] = "An error occurred while processing your request.";
                return RedirectToAction(nameof(Detail), new { organizationId = organizationId });
            }
        }


        [HttpPost]
        public async Task<IActionResult> OutOrganization(Guid organizationId, Guid userId)
        {
            var organizationMember =
                await _organizationRepository.GetOrganizationMemberAsync(om =>
                    om.OrganizationID == organizationId && om.UserID == userId);
            var statusUserOut = organizationMember.Status;

            await _organizationRepository.DeleteOrganizationMemberByOrganizationIDAndUserIDAsync(organizationId, userId);

            var organizationVM =
                await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(organizationId));
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            if (statusUserOut == 0 && currentUser.Id.Equals(organizationVM.CEO.Id))
            {
                TempData[MyConstants.Success] = "User request denied successfully.";
                var link = Url.Action(nameof(Detail), "Organization", new { organizationId },
                    Request.Scheme);
                await _notificationService.ProcessOrganizationJoinRequestNotificationAsync(userId, organizationId, link,
                    "deny");
                return RedirectToAction(nameof(ManageRequestJoinOrganization), new { organizationId = organizationId });
            }
            else if (statusUserOut == 0)
            {
                TempData[MyConstants.Success] = "You have successfully left the organization.";
                var link = Url.Action(nameof(Detail), "Organization", new { organizationId },
                    Request.Scheme);
                await _notificationService.ProcessOrganizationLeaveNotificationAsync(userId, organizationId, link, "left");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData[MyConstants.Success] = "User has been removed or banned from the organization.";
                var link = Url.Action(nameof(Detail), "Organization", new { organizationId },
                    Request.Scheme);
                await _notificationService.ProcessOrganizationLeaveNotificationAsync(userId, organizationId, link,
                    "remove");
                return RedirectToAction(nameof(ManageOrganizationMember), new { organizationId = organizationId });
            }
        }


        public IActionResult TransferCeoOrganization()
        {
            // send to form to save vallue not change
            // can be use Session current organization
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            return View(currentOrganization);
        }

        [HttpPost]
        public async Task<IActionResult> TransferCeoOrganization(Guid organizationId, Guid currentCEOID, Guid newCEOID)
        {
            //var newCEO = await _userRepository.GetAsync(u => u.Id.Equals(newCEOID));
            //var currentCEO = await _userRepository.GetAsync(u => u.Id.Equals(currentCEOID));

            if (!newCEOID.Equals(currentCEOID))
            {
                //Get All Projects in Organization 
                var projects =
                    await _projectRepository.GetAllProjectsByOrganizationIDAsync(p =>
                        p.OrganizationID.Equals(organizationId));
                if (await _roleService.IsInRoleAsync(newCEOID, RoleConstants.ProjectLeader) &&
                    await _roleService.IsInRoleAsync(currentCEOID, RoleConstants.ProjectLeader))
                {
                    TempData[MyConstants.Error] = "both is being head of project so not transfer!.";
                    return RedirectToAction(nameof(ManageOrganizationMember), new { organizationId = organizationId });
                }

                var organizationMemberCurrent = new OrganizationMember()
                {
                    UserID = currentCEOID,
                    OrganizationID = organizationId,
                    Status = 1,
                };
                await _organizationRepository.UpdateOrganizationMemberAsync(organizationMemberCurrent);

                var organizationMemberNew = new OrganizationMember()
                {
                    UserID = newCEOID,
                    OrganizationID = organizationId,
                    Status = 2,
                };
                await _organizationRepository.UpdateOrganizationMemberAsync(organizationMemberNew);

                await _roleService.AddUserToRoleAsync(newCEOID, RoleConstants.HeadOfOrganization);
                await _roleService.DeleteRoleFromUserAsync(currentCEOID, RoleConstants.HeadOfOrganization);

                foreach (var project in projects)
                {
                    var projectMember = await _projectMemberRepository.GetAsync(pm =>
                        pm.UserID.Equals(newCEOID) && pm.ProjectID.Equals(project.ProjectID));
                    //newCeo is membber of project
                    if (projectMember != null && projectMember.Status == 1)
                    {
                        projectMember.Status = 2;
                        await _projectMemberRepository.UpdateAsync(projectMember);
                    }

                    if (projectMember != null && projectMember.Status == 3)
                    {
                        projectMember.Status = 2;
                        await _projectMemberRepository.UpdateAsync(projectMember);
                    }

                    if (projectMember == null) // new Ceo is outside Project
                    {
                        var newProjectMember = new ProjectMember()
                        {
                            UserID = newCEOID,
                            ProjectID = project.ProjectID,
                            Status = 2,
                        };

                        await _projectRepository.AddProjectMemberAsync(newProjectMember);
                    }

                    //Delete role Project Leader of leader organization current
                    var leaderProject = await _projectMemberRepository.GetAsync(pm =>
                        pm.Status == 3 && pm.ProjectID.Equals(project.ProjectID));
                    if (leaderProject == null)
                    {
                        await _roleService.AddUserToRoleAsync(newCEOID, RoleConstants.ProjectLeader);
                        await _roleService.DeleteRoleFromUserAsync(currentCEOID, RoleConstants.ProjectLeader);
                    }
                    
                    var projectMember1 = await _projectMemberRepository.GetAsync(pm =>
                        pm.UserID.Equals(currentCEOID) && pm.ProjectID.Equals(project.ProjectID));
                    projectMember1.Status = 1;
                    await _projectMemberRepository.UpdateAsync(projectMember1);
                }

                var link = Url.Action(nameof(Detail), "Organization", new { organizationId },
                    Request.Scheme);
                await _notificationService.TransferOrganizationCeoNotificationAsync(newCEOID, currentCEOID,
                    organizationId, link);
                TempData[MyConstants.Success] = "CEO transferred successfully.";
                return RedirectToAction(nameof(ManageOrganizationMember), new { organizationId = organizationId });
            }

            TempData[MyConstants.Error] = "New CEO must be different from the current CEO.";
            return RedirectToAction(nameof(ManageOrganizationMember), new { organizationId = organizationId });
        }


        ////Manage Project
        public async Task<IActionResult> ManageOrganizationProject()
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            //Create current organization
            var organizationVM =
                await _organizationService.GetOrganizationVMAsync(o =>
                    o.OrganizationID.Equals(currentOrganization.OrganizationID));
            var orgPrj =
                await _projectService.GetProjectsWithExpressionAsync(pr =>
                    pr.OrganizationID.Equals(currentOrganization.OrganizationID));
            var projectDtos = _projectService.MapToListProjectOverviewDto(orgPrj);
            ViewBag.prjDto = projectDtos; // Chua chay
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);
            return View(organizationVM);
        }


        //Manage history
        public async Task<IActionResult> ManageOrganizationTranactionHistory(
            SearchRequestDto userSearchRequestDto, PaginationRequestDto userPaginationRequestDto,
            SearchRequestDto organizationSearchRequestDto, PaginationRequestDto organizationPaginationRequestDto)
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            if (userSearchRequestDto.Filter == null)
            {
                userSearchRequestDto.Filter = SearchOptionsConstants.StatusAccepted;
            }

            if (organizationSearchRequestDto.Filter == null)
            {
                organizationSearchRequestDto.Filter = SearchOptionsConstants.StatusAccepted;
            }
            var userToOrgQueryable = _userToOrganziationTransactionHistoryRepository.GetAllAsQueryable(uto =>
                uto.OrganizationResource.OrganizationID.Equals(currentOrganization.OrganizationID) &&
                uto.Status != 0); // Dont get the pending ones
            var orgToPrjQueryable = _organizationToProjectTransactionHistoryRepository.GetAllAsQueryable(uto =>
                uto.OrganizationResource.OrganizationID.Equals(currentOrganization.OrganizationID));

            var totalUserDonations =
                await _transactionViewService.SetupUserToOrgTransactionDtosWithSearchParams(userSearchRequestDto,
                    userToOrgQueryable);
            var totalOrgAllocations =
                await _transactionViewService.SetupOrgToPrjTransactionDtosWithSearchParams(organizationSearchRequestDto,
                    orgToPrjQueryable);
            
            var paginatedUserDonations = _pagination.Paginate(totalUserDonations, HttpContext, userPaginationRequestDto, userSearchRequestDto);
            var paginatedOrganizationDonations = _pagination.Paginate(totalOrgAllocations, HttpContext, organizationPaginationRequestDto, organizationSearchRequestDto);

            return View(new ManageOrganizationTransactionHistoryVM
            {
                OrganizationTransactions = paginatedOrganizationDonations,
                OrganizationSearchRequestDto = organizationSearchRequestDto,
                OrganizationPaginationRequestDto = organizationPaginationRequestDto,
                
                UserTransactions = paginatedUserDonations,
                UserSearchRequestDto = userSearchRequestDto,
                UserPaginationRequestDto = userPaginationRequestDto,
            });
        }

        //Manage Resource
        public async Task<IActionResult> ManageOrganizationResource()
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            var organizationVM =
                await _organizationService.GetOrganizationVMAsync(o =>
                    o.OrganizationID.Equals(currentOrganization.OrganizationID));
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

            var OrganizationToProjectHistorysPending =
                await _organizationToProjectHistoryVMService.GetAllOrganizationToProjectHistoryAsync(currentOrganization
                    .OrganizationID);
            HttpContext.Session.Set<List<OrganizationToProjectHistory>>(
                MySettingSession.SESSION_OrganizzationToProjectHistory_For_Organization_Pending_Key,
                OrganizationToProjectHistorysPending);
            return View(organizationVM);
        }

        [HttpGet]
        public IActionResult AddNewOrganizationResource()
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            var organizationResource = new OrganizationResource()
            {
                ResourceID = new Guid(),
                OrganizationID = currentOrganization.OrganizationID,
                Quantity = 0,
            };

            return View(organizationResource);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewOrganizationResource(OrganizationResource organizationResource)
        {
            if (organizationResource != null)
            {
                if (await _organizationRepository.AddOrganizationResourceSync(organizationResource))
                {
                    var organizationVM = await _organizationService.GetOrganizationVMAsync(o =>
                        o.OrganizationID.Equals(organizationResource.OrganizationID));
                    HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY,
                        organizationVM);

                    TempData[MyConstants.Success] = "Organization resource added successfully.";
                    // var link = Url.Action(nameof(Detail), "Organization", new {organizationId = organizationResource.OrganizationID},
                    //     Request.Scheme);
                    // await _notificationService.ProcessOrganizationResourceNotificationAsync(organizationResource.ResourceID, link, "add");
                    return RedirectToAction(nameof(ManageOrganizationResource));
                }
            }

            TempData[MyConstants.Error] = "Invalid organization resource data.";
            return View(organizationResource);
        }


        public async Task<IActionResult> RemoveOrganizationResource(Guid resourceId)
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            if (currentOrganization != null)
            {
                var organizationVM = await _organizationService.GetOrganizationVMAsync(o =>
                    o.OrganizationID.Equals(currentOrganization.OrganizationID));
                HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY,
                    organizationVM);

                TempData[MyConstants.Success] = "Organization resource removed successfully.";
                // var link = Url.Action(nameof(Detail), "Organization", new {organizationId = currentOrganization.OrganizationID},
                //     Request.Scheme);
                // await _notificationService.ProcessOrganizationResourceNotificationAsync(resourceId, link, "remove");
                await _organizationRepository.DeleteOrganizationResourceAsync(resourceId);
                return RedirectToAction(nameof(ManageOrganizationResource));
            }

            TempData[MyConstants.Error] = "Failed to remove organization resource.";
            return RedirectToAction(nameof(ManageOrganizationResource));
        }


        [HttpGet]
        public async Task<IActionResult> SendResourceOrganizationToProject(Guid resourceId)
        {
            // get current resource
            var currentResource =
                await _organizationRepository.GetOrganizationResourceAsync(or => or.ResourceID.Equals(resourceId));
            HttpContext.Session.Set<OrganizationResource>(MySettingSession.SESSION_Current_Organization_Resource_KEY,
                currentResource);

            var organizationToProjectHistory = new OrganizationToProjectHistory()
            {
                OrganizationResourceID = currentResource.ResourceID,
                Status = 0,
                Time = DateOnly.FromDateTime(DateTime.Now),
            };

            return View(organizationToProjectHistory);
        }

        [HttpPost]
        public async Task<IActionResult> SendResourceOrganizationToProject(OrganizationToProjectHistory transaction,
            Guid projectId, List<IFormFile> file)
        {
            var resourceSent =
                await _organizationRepository.GetOrganizationResourceAsync(or =>
                    or.ResourceID.Equals(transaction.OrganizationResourceID));

            if (transaction != null && projectId != Guid.Empty && transaction.Amount > 0 &&
                transaction.Amount <= resourceSent.Quantity)
            {
                var project = await _projectVMService.GetProjectAsync(p => p.ProjectID.Equals(projectId));
                bool duplicate = false;
                var projectResource = new ProjectResource();

                foreach (var item in project.ProjectResource)
                {
                    if (item.ResourceName.ToUpper().Contains(resourceSent.ResourceName.ToUpper()) &&
                        item.Unit.ToUpper().Contains(resourceSent.Unit.ToUpper()))
                    {
                        duplicate = true;
                        projectResource = item;
                        break;
                    }
                }

                // If there are already resource on project, just assign the transaction with the resource Id
                if (duplicate)
                {
                    transaction.ProjectResourceID = projectResource.ResourceID;

                    if (projectResource.ExpectedQuantity - projectResource.Quantity == 0)
                    {
                        TempData[MyConstants.Error] = "Resource is full not recieves donate";
                        return RedirectToAction(nameof(ManageOrganizationResource));
                    }

                    if (transaction.Amount > (projectResource.ExpectedQuantity - projectResource.Quantity))
                    {
                        ViewBag.MessageExcessQuantity =
                            $"*Quantity more than 0 and less than equal {projectResource.ExpectedQuantity - projectResource.Quantity}";
                        return View(transaction);
                    }
                }
                else
                {
                    var newProjectResource = new ProjectResource()
                    {
                        ProjectID = project.ProjectID,
                        ResourceName = resourceSent.ResourceName,
                        Quantity = 0,
                        ExpectedQuantity = transaction.Amount,
                        Unit = resourceSent.Unit,
                    };

                    await _projectRepository.AddProjectResourceAsync(newProjectResource);

                    project = await _projectVMService.GetProjectAsync(p => p.ProjectID.Equals(projectId));
                    var projectResource1 = new ProjectResource();

                    foreach (var item in project.ProjectResource)
                    {
                        if (item.ResourceName.ToUpper().Contains(resourceSent.ResourceName.ToUpper()) &&
                            item.Unit.ToUpper().Contains(resourceSent.Unit.ToUpper()))
                        {
                            duplicate = true;
                            projectResource1 = item;
                            break;
                        }
                    }

                    if (duplicate)
                    {
                        transaction.ProjectResourceID = projectResource1.ResourceID;
                    }
                }

                var resImages = await _cloudinaryUploader.UploadMultiImagesAsync(file);
                if (resImages.Equals("Wrong extension") || resImages.Equals("No file"))
                {
                    TempData[MyConstants.Error] = resImages.Equals("No file")
                        ? "No file to upload!"
                        : "Extension of some files is wrong!";

                    ViewBag.Images = resImages.Equals("Wrong extension")
                        ? "*Wrong extension"
                        : "*Please upload at least one image proof";
                    return View(transaction);
                }

                transaction.Attachments = resImages;

                if (await _organizationRepository.AddOrganizationToProjectHistoryAsync(transaction))
                {
                    resourceSent.Quantity -= transaction.Amount;

                    if (await _organizationRepository.UpdateOrganizationResourceAsync(resourceSent))
                    {
                        TempData[MyConstants.Success] = "Resource sent to project successfully.";
                        var link = Url.Action(nameof(Detail), "Organization",
                            new { organizationId = resourceSent.OrganizationID },
                            Request.Scheme);
                        await _notificationService.OrganizationSendToProjectNotificationAsync(resourceSent, projectId,
                            link, "sent");
                        return RedirectToAction(nameof(ManageOrganizationResource));
                    }
                }
            }

            if (transaction.Amount <= 0 || transaction.Amount > resourceSent.Quantity)

                ViewBag.MessageExcessQuantity = $"*Quantity more than 0 and less than equal {resourceSent.Quantity}";

            if (projectId == Guid.Empty)
            {
                ViewBag.MessageProject = "*Choose project to send";
            }

            return View(transaction);
        }

        public async Task<IActionResult> CancelSendResource(Guid transactionId)
        {
            var transactionHistory =
                await _organizationRepository.GetOrganizationToProjectHistoryAsync(otp =>
                    otp.TransactionID.Equals(transactionId));
            if (transactionHistory != null)
            {
                var OrganizationResource = await _organizationRepository.GetOrganizationResourceAsync(or =>
                    or.ResourceID.Equals(transactionHistory.OrganizationResourceID));
                OrganizationResource.Quantity += transactionHistory.Amount;
                await _organizationRepository.UpdateOrganizationResourceAsync(OrganizationResource);
                await _organizationRepository.DeleteOrganizationToProjectHistoryAsync(transactionId);

                TempData[MyConstants.Success] = "Resource sending transaction canceled successfully.";
                var link = Url.Action(nameof(Detail), "Organization",
                    new { organizationId = OrganizationResource.OrganizationID },
                    Request.Scheme);
                await _notificationService.OrganizationSendToProjectNotificationAsync(OrganizationResource,
                    transactionHistory.ProjectResource.ProjectID, link, "unsent");
                return RedirectToAction(nameof(ManageOrganizationResource));
            }

            TempData[MyConstants.Error] = "Transaction not found. Unable to cancel.";
            return RedirectToAction(nameof(ManageOrganizationResource));
        }


        // This method is shared between user donate to org and org allocate to project
        [Authorize]
        public IActionResult DonateByMoney(string organizationId, string resourceId)
        {
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            // Setup for display
            ViewBag.donatorName = currentUser.UserName;
            ViewBag.returnUrl = Url.Action("Detail", "Organization", new { organizationId }, Request.Scheme) ?? "~/";
            var vnPayRequestModel = new PayRequestDto
            {
                FromID = currentUser.Id,
                ResourceID = new Guid(resourceId),
                TargetId = new Guid(organizationId),
                TargetType = MyConstants.Organization,
            };
            return View(vnPayRequestModel);
        }

        // This action is only accessible by CEO
        [Authorize]
        public async Task<IActionResult> AllocateMoney(string organizationId, string resourceId)
        {
            // Skip the project that is banned or completed
            var projects = await _projectRepository
                .GetAllProjectsByOrganizationIDAsync(p => p.OrganizationID.Equals(new Guid(organizationId)));
            ViewData["projects"] = projects; // Cast to list of projects obj later
            // Get the current money resource first
            var organizationMoneyResource =
                await _organizationResourceRepository.GetAsync(or => or.ResourceID == new Guid(resourceId));
            if (organizationMoneyResource == null)
                throw new Exception("WARNING: Organization MONEY resource not found");
            ViewData["limitAmount"] = organizationMoneyResource.Quantity;
            ViewBag.returnUrl = Url.Action("Detail", "Organization", new { organizationId }, Request.Scheme) ?? "~/";
            var payRequestDto = new PayRequestDto
            {
                // Target project id will be rendered in a form of options
                FromID = new Guid(organizationId),
                ResourceID = new Guid(resourceId),
                TargetType = MyConstants.Allocation,
            };
            // same view as user donate to organization but with more options
            return View(nameof(DonateByMoney), payRequestDto);
        }

        [HttpPost]
        public async Task<IActionResult> AllocateMoney(PayRequestDto payRequestDto)
        {
            var returnUrl = ViewBag.returnUrl ?? Url.Action("Detail", "Organization",
                new { organizationId = payRequestDto.FromID }, Request.Scheme);
            try
            {
                var result = _mapper.Map<OrganizationToProjectHistory>(payRequestDto);
                result.Time = DateOnly.FromDateTime(DateTime.Now); // Manual convert because of... Time
                result.Status = 1;
                var projectMoneyResource = await _projectResourceRepository.GetAsync(pr =>
                    pr.ProjectID == payRequestDto.TargetId &&
                    pr.ResourceName.ToLower().Equals("money"));
                var organizationMoneyResource =
                    await _organizationResourceRepository.GetAsync(or => or.ResourceID == payRequestDto.ResourceID);
                if (organizationMoneyResource == null)
                    throw new Exception("PAYMENT: Organization resource not found (Is this one lacking money?)");
                if (projectMoneyResource == null)
                    throw new Exception("PAYMENT: Project resource not found (Is this one lacking money?)");

                result.OrganizationResourceID =
                    payRequestDto.ResourceID; // Pay request holds the organizationResource ID
                result.ProjectResourceID = projectMoneyResource.ResourceID; // The resourceId we searched for 

                // Insert to database:
                await _organizationToProjectTransactionHistoryRepository.AddAsync(result);

                // Update in project resource and organization resource
                // One gains, another one loss
                projectMoneyResource.Quantity += payRequestDto.Amount;
                organizationMoneyResource.Quantity -= payRequestDto.Amount;
                await _projectResourceRepository.UpdateResourceTypeAsync(projectMoneyResource);
                await _organizationResourceRepository.UpdateAsync(organizationMoneyResource);
                TempData[MyConstants.Success] = "Allocated money success!";
            }
            catch (Exception e)
            {
                TempData[MyConstants.Error] = "Something wrong happened. Please try again.";
                TempData[MyConstants.Subtitle] = e.Message;
            }

            return Redirect(returnUrl);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DonateByResource(Guid resourceId)
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            //get current user
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            // get current resource
            var currentResource =
                await _organizationRepository.GetOrganizationResourceAsync(or => or.ResourceID.Equals(resourceId));
            HttpContext.Session.Set<OrganizationResource>(MySettingSession.SESSION_Current_Organization_Resource_KEY,
                currentResource);

            var userToOrganizationTransactionHistory = new UserToOrganizationTransactionHistory()
            {
                ResourceID = resourceId,
                UserID = currentUser.Id,
                Status = 0,
                Time = DateOnly.FromDateTime(DateTime.Now),
            };
            return View(userToOrganizationTransactionHistory);
        }

        [HttpPost]
        public async Task<IActionResult> DonateByResource(UserToOrganizationTransactionHistory transactionHistory,
            List<IFormFile> file)
        {
            if (transactionHistory != null)
            {
                var resImages = await _cloudinaryUploader.UploadMultiImagesAsync(file);
                if (!(resImages.Equals("Wrong extension") || resImages.Equals("No file")) &&
                    transactionHistory.Amount > 0)
                {
                    transactionHistory.Attachments = resImages;
                    if (await _organizationRepository.AddUserToOrganizationTransactionHistoryASync(transactionHistory))
                    {
                        var currentOrganization =
                            HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
                        var link = Url.Action(nameof(Detail), "Organization",
                            new { organizationId = currentOrganization.OrganizationID },
                            Request.Scheme);
                        await _notificationService.ProcessOrganizationDonationNotificationAsync(
                            currentOrganization.OrganizationID,
                            transactionHistory.TransactionID, link, "donate");
                        TempData[MyConstants.Success] = "Resource donated successfully.";
                        return RedirectToAction(nameof(ManageOrganizationResource));
                    }
                }
                else if (resImages.Equals("Wrong extension"))
                {
                    ViewBag.Images = "*Wrong extension";
                }
                else if (resImages.Equals("No file"))
                {
                    ViewBag.Images = "*Please upload at least one image proof";
                }

                if (transactionHistory.Amount <= 0)
                {
                    ViewBag.Amount = "*Amount > 0";
                }

                TempData[MyConstants.Error] = "Failed to donate resource.";
            }

            return View(transactionHistory);
        }

        public async Task<IActionResult> ReviewDonateRequest()
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            var userToOrganizationTransactionHistoryInAOrganizations =
                await _userToOragnizationTransactionHistoryVMService.GetTransactionHistory(currentOrganization
                    .OrganizationID);

            return View(userToOrganizationTransactionHistoryInAOrganizations);
        }

        [Authorize]
        public async Task<IActionResult> MyDonors(Guid OrganizationID)
        {
            var currentOrganization =
                HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            if (currentOrganization == null && !Guid.Empty.Equals(OrganizationID))
            {
                currentOrganization =
                    await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(OrganizationID));
            }

            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            var listUserToOrg = await _userToOragnizationTransactionHistoryVMService
                .GetTransactionHistory(uto =>
                    uto.UserID.Equals(currentUser.Id) &&
                    uto.OrganizationResource.Organization.OrganizationID.Equals(currentOrganization.OrganizationID));
            return View(listUserToOrg);
        }

        public async Task<IActionResult> DenyRequestDonate(Guid transactionId, string reason)
        {
            if (reason != null)
            {
                //update table UserToOrganizationTransactionHistory
                var userToOrganizationTransactionHistory =
                    await _organizationRepository.GetUserToOrganizationTransactionHistoryByTransactionIDAsync(uto =>
                        uto.TransactionID.Equals(transactionId));
                userToOrganizationTransactionHistory.Status = -1;
                userToOrganizationTransactionHistory.Message += $"\nReason Deny Your Request: {reason}";
                var updateTransactionSuccess =
                    await _organizationRepository.UpdateUserToOrganizationTransactionHistoryAsync(
                        userToOrganizationTransactionHistory);

                if (updateTransactionSuccess)
                {
                    TempData[MyConstants.Success] = "Request denied successfully.";
                    var link = Url.Action(nameof(MyDonors), "Organization", "",
                        Request.Scheme);
                    await _notificationService.ProcessOrganizationDonationNotificationAsync(
                        userToOrganizationTransactionHistory.OrganizationResource.OrganizationID,
                        userToOrganizationTransactionHistory.TransactionID, link, "deny");
                }
            }
            else TempData[MyConstants.Error] = "Failed to deny request.";

            return RedirectToAction(nameof(ReviewDonateRequest));
        }

        public async Task<IActionResult> AcceptRquestDonate(Guid transactionId, List<IFormFile> proofImages)
        {
            var resImages = await _cloudinaryUploader.UploadMultiImagesAsync(proofImages);
            if (!(resImages.Equals("Wrong extension") || resImages.Equals("No file")))
            {
                //update table UserToOrganizationTransactionHistory
                var userToOrganizationTransactionHistory =
                    await _organizationRepository.GetUserToOrganizationTransactionHistoryByTransactionIDAsync(uto =>
                        uto.TransactionID.Equals(transactionId));
                userToOrganizationTransactionHistory.Status = 1;
                userToOrganizationTransactionHistory.Attachments = resImages;
                var updateTransactionSuccess =
                    await _organizationRepository.UpdateUserToOrganizationTransactionHistoryAsync(
                        userToOrganizationTransactionHistory);

                var currentOrganization =
                    HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

                //update table resource
                var organizationResource =
                    await _organizationRepository.GetOrganizationResourceByOrganizationIDAndResourceIDAsync(
                        currentOrganization.OrganizationID, userToOrganizationTransactionHistory.ResourceID);
                organizationResource.Quantity += userToOrganizationTransactionHistory.Amount;
                var updateResourceSuccess =
                    await _organizationRepository.UpdateOrganizationResourceAsync(organizationResource);
                if (updateTransactionSuccess && updateResourceSuccess)
                {
                    TempData[MyConstants.Success] = "Request accepted successfully.";
                    var link = Url.Action(nameof(MyDonors), "Organization",
                        new { userToOrganizationTransactionHistory.OrganizationResource.OrganizationID },
                        Request.Scheme);
                    await _notificationService.ProcessOrganizationDonationNotificationAsync(
                        userToOrganizationTransactionHistory.OrganizationResource.OrganizationID,
                        userToOrganizationTransactionHistory.TransactionID, link, "accept");
                    return RedirectToAction(nameof(ManageOrganizationResource));
                }
                else
                {
                    TempData[MyConstants.Error] = "Failed to accept request.";
                    return RedirectToAction(nameof(ReviewDonateRequest));
                }
            }
            else
            {
                TempData[MyConstants.Error] = resImages.Equals("No file")
                    ? "No file to upload!"
                    : "Extension of some files is wrong!";
                return RedirectToAction(nameof(ReviewDonateRequest));
            }
        }
    }
}