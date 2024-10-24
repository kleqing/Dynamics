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

namespace Dynamics.Controllers
{
    public class OrganizationController : Controller
    {

        IOrganizationRepository _organizationRepository;
        IUserRepository _userRepository;
        IProjectRepository _projectRepository;
        IOrganizationVMService _organizationService;
        IUserToOragnizationTransactionHistoryVMService _userToOragnizationTransactionHistoryVMService;
        IProjectVMService _projectVMService;
        IOrganizationToProjectHistoryVMService _organizationToProjectHistoryVMService;
        private readonly CloudinaryUploader _cloudinaryUploader;
        private readonly IOrganizationService _orgDisplayService;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IOrganizationResourceRepository _organizationResourceRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserToOrganizationTransactionHistoryRepository _userToOrganziationTransactionHistoryRepository;
        private readonly IOrganizationToProjectTransactionHistoryRepository _organizationToProjectTransactionHistoryRepository;
        private readonly ITransactionViewService _transactionViewService;

        public OrganizationController(IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IOrganizationVMService organizationService,
            IUserToOragnizationTransactionHistoryVMService userToOragnizationTransactionHistoryVMService,
            IProjectVMService projectVMService,
            IOrganizationToProjectHistoryVMService organizationToProjectHistoryVMService,
            CloudinaryUploader cloudinaryUploader, IOrganizationService orgDisplayService, IOrganizationMemberRepository organizationMemberRepository
            , IOrganizationResourceRepository organizationResourceRepository, INotificationService notificationService,
            IUserToOrganizationTransactionHistoryRepository userToOrganizationTransactionHistoryRepository,
            IOrganizationToProjectTransactionHistoryRepository organizationToProjectTransactionHistoryRepository,
            ITransactionViewService transactionViewService)
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
        }

        //The index use the cards at homepage to display instead - Kiet
        public async Task<IActionResult> Index()
        {
            var orgs = _organizationRepository.GetAll();
            var organizationVMs = _orgDisplayService.MapToOrganizationOverviewDtoList(orgs.ToList());
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


            var organization = new Organization()
            {
                OrganizationID = Guid.NewGuid(),
                StartTime = DateOnly.FromDateTime(DateTime.UtcNow),
                OrganizationEmail = currentUser.UserEmail,
                OrganizationPhoneNumber = currentUser.UserPhoneNumber,
                OrganizationAddress = currentUser.UserAddress,
            };


            return View(organization);
        }

        //POST: /Organizations/Create
        [HttpPost]
        public async Task<IActionResult> Create(Organization organization, List<IFormFile> images)
        {
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

            //get current user
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            //set contact for Organization
            if (organization.OrganizationEmail == null)
            {
                organization.OrganizationEmail = currentUser.UserEmail;
            }

            if (organization.OrganizationPhoneNumber == null)
            {
                organization.OrganizationPhoneNumber = currentUser.UserPhoneNumber;
            }

            if (organization.OrganizationAddress == null)
            {
                organization.OrganizationAddress = currentUser.UserAddress;
            }

            if (await _organizationRepository.AddOrganizationAsync(organization))
            {
                var organizationResource = new OrganizationResource()
                {
                    OrganizationID = organization.OrganizationID,
                    ResourceName = "Money",
                    Quantity = 0,
                    Unit = "VND",
                };
                await _organizationRepository.AddOrganizationResourceSync(organizationResource);
                TempData[MyConstants.Success] = "Create organization successfully!";
                return RedirectToAction(nameof(JoinOrganization), new { organizationId = organization.OrganizationID, status = 2, userId = currentUser.UserID });//status 2 : CEOID   0 : processing   1 : membert
            }
            else
                TempData[MyConstants.Error] = "Create organization failed!";
            return View(organization);
        }

        public async Task<IActionResult> MyOrganization(Guid userId)
        {
            // All organizations
            var orgs = await _organizationRepository.GetAll().ToListAsync();
            var organizationVMs = _orgDisplayService.MapToOrganizationOverviewDtoList(orgs);
            // My organizations where the user joined / owned
            var myOrganizationMembers = await _organizationMemberRepository.GetAllAsync(om => om.UserID == userId);
            if (myOrganizationMembers.IsNullOrEmpty()) return RedirectToAction("Index", "Organization");
            var myOrgs = new List<Organization>();
            foreach (var organizationMember in myOrganizationMembers)
            {
                myOrgs.Add(organizationMember.Organization);
            }

            var MyOrgDtos = _orgDisplayService.MapToOrganizationOverviewDtoList(myOrgs);
            ViewBag.MyOrgs = MyOrgDtos;
            return View(organizationVMs);

        }//fix session done

        public async Task<IActionResult> Detail(Guid organizationId)
        {
            //Creat current organization
            var organizationVM = await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(organizationId));
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

            return View(organizationVM);
        }

        public async Task<IActionResult> Edit(Guid organizationId)
        {
            // can be use Session current organization
            var organization = await _organizationRepository.GetOrganizationAsync(o => o.OrganizationID.Equals(organizationId));
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
                    TempData[MyConstants.Success] = "Update organization successfully!";
                    return RedirectToAction("Detail", new { organizationId = organization.OrganizationID });
                }
                

            }
            TempData[MyConstants.Error] = "Update organization failed!";
            return View(organization);
        }


        //Organization Member
        public async Task<IActionResult> ManageOrganizationMember()
        {
            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            //Creat current organization
            var organizationVM = await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(currentOrganization.OrganizationID));
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

            return View(organizationVM);
        }

        public IActionResult sendRequestJoinOrganization(Guid organizationId, Guid userId)
        {
            // Get the id from session here, no need to pass it from the view - Kiet
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }
            TempData[MyConstants.Success] = "Send request successfully!";
            return RedirectToAction(nameof(JoinOrganization), new { organizationId = organizationId, status = 0, userId = currentUser.UserID});
        }

        public IActionResult ManageRequestJoinOrganization(Guid organizationId)
        {
            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            return View(currentOrganization);
        }


        public async Task<IActionResult> AcceptRquestJoin(Guid organizationId, Guid userId)
        {       
            return RedirectToAction(nameof(JoinOrganization), new { organizationId = organizationId, status = 1, userId = userId});
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
                if (status == 2 || status == 0)
                {
                    await _organizationRepository.AddOrganizationMemberSync(organizationMember);
                     TempData[MyConstants.Success] = "You have successfully joined the organization.";
                }
                else
                {
                    await _organizationRepository.UpdateOrganizationMemberAsync(organizationMember);
                     TempData[MyConstants.Success] = "Your membership status has been updated successfully.";
                }

                var organizationVM = await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(organizationId));
                HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

                if (status == 1)
                {
                    return RedirectToAction(nameof(ManageRequestJoinOrganization), new { organizationId = organizationId });
                }

                return RedirectToAction(nameof(Detail), new { organizationId = organizationId });
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
            var organizationMember = await _organizationRepository.GetOrganizationMemberAsync(om => om.OrganizationID == organizationId && om.UserID == userId);
            var statusUserOut = organizationMember.Status;

            await _organizationRepository.DeleteOrganizationMemberByOrganizationIDAndUserIDAsync(organizationId, userId);

            var organizationVM = await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(organizationId));
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            if (statusUserOut == 0 && currentUser.UserID.Equals(organizationVM.CEO.UserID))
            {
                 TempData[MyConstants.Success] = "User request denied successfully.";
                return RedirectToAction(nameof(ManageRequestJoinOrganization), new { organizationId = organizationId });
            }
            else if (statusUserOut == 0)
            {
                 TempData[MyConstants.Success] = "You have successfully left the organization.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                 TempData[MyConstants.Success] = "User has been removed or banned from the organization.";
                return RedirectToAction(nameof(ManageOrganizationMember), new { organizationId = organizationId });
            }
        }


        public IActionResult TransferCeoOrganization()
        {
            // send to form to save vallue not change
            // can be use Session current organization
            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            return View(currentOrganization);
        }

        [HttpPost]
        public async Task<IActionResult> TransferCeoOrganization(Guid organizationId, Guid currentCEOID, Guid newCEOID)
        {
            if (!newCEOID.Equals(currentCEOID))
            {
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

                 TempData[MyConstants.Success] = "CEO transferred successfully.";
                return RedirectToAction("Index", "EditUser");
            }

             TempData[MyConstants.Error] = "New CEO must be different from the current CEO.";
            return RedirectToAction(nameof(ManageOrganizationMember), new { organizationId = organizationId });
        }



        ////Manage Project
        public async Task<IActionResult> ManageOrganizationProject()
        {

            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            //Creat current organization
            var organizationVM = await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(currentOrganization.OrganizationID));
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

            return View(organizationVM);
        }


        //Manage history
        public async Task<IActionResult> ManageOrganizationTranactionHistory()
        {

            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            //var UserToOrganizationTransactionHistoryInAOrganizations = await _userToOragnizationTransactionHistoryVMService.GetTransactionHistoryIsAccept(currentOrganization.OrganizationID);

            //var OrganizationToProjectHistorysPending = await _organizationToProjectHistoryVMService.GetAllOrganizationToProjectHistoryAsync(currentOrganization.OrganizationID);
            //var OrganizationToProjectHistorysAccepting = await _organizationToProjectHistoryVMService.GetAllOrganizationToProjectHistoryByAcceptingAsync(currentOrganization.OrganizationID);

            //HttpContext.Session.Set<List<OrganizationToProjectHistory>>(MySettingSession.SESSION_OrganizzationToProjectHistory_For_Organization_Pending_Key, OrganizationToProjectHistorysPending);
            //HttpContext.Session.Set<List<OrganizationToProjectHistory>>(MySettingSession.SESSION_OrganizzationToProjectHistory_For_Organization_Accepting_Key, OrganizationToProjectHistorysAccepting);

            var userToOrgQueryable = _userToOrganziationTransactionHistoryRepository.GetAllAsQueryable(uto => uto.OrganizationResource.OrganizationID.Equals(currentOrganization.OrganizationID) && uto.Status != 0);
            var orgToPrj = _organizationToProjectTransactionHistoryRepository.GetAllAsQueryable(uto => uto.OrganizationResource.OrganizationID.Equals(currentOrganization.OrganizationID));

            var userToOrgTransactionDtos = await _transactionViewService.GetUserToOrganizationTransactionDtosAsync(userToOrgQueryable);
            var orgToPrjTransactionDtos = await _transactionViewService.GetOrganizationToProjectTransactionDtosAsync(orgToPrj);

            var total = new List<OrganizationTransactionDto>();
            total.AddRange(userToOrgTransactionDtos);
            total.AddRange(orgToPrjTransactionDtos);

            return View(total);
        }

        //Manage Resource
        public async Task<IActionResult> ManageOrganizationResource()
        {
            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);
            var organizationVM = await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(currentOrganization.OrganizationID));
            HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

            var OrganizationToProjectHistorysPending = await _organizationToProjectHistoryVMService.GetAllOrganizationToProjectHistoryAsync(currentOrganization.OrganizationID);
            HttpContext.Session.Set<List<OrganizationToProjectHistory>>(MySettingSession.SESSION_OrganizzationToProjectHistory_For_Organization_Pending_Key, OrganizationToProjectHistorysPending);
            return View(organizationVM);
        }

        [HttpGet]
        public IActionResult AddNewOrganizationResource()
        {
            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

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
                    var organizationVM = await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(organizationResource.OrganizationID));
                    HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

                     TempData[MyConstants.Success] = "Organization resource added successfully.";
                    return RedirectToAction(nameof(ManageOrganizationResource));
                }
            }
             TempData[MyConstants.Error] = "Invalid organization resource data.";
            return View(organizationResource);
        }


        public async Task<IActionResult> RemoveOrganizationResource(Guid resourceId)
        {
            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            if (currentOrganization != null)
            {
                await _organizationRepository.DeleteOrganizationResourceAsync(resourceId);
                var organizationVM = await _organizationService.GetOrganizationVMAsync(o => o.OrganizationID.Equals(currentOrganization.OrganizationID));
                HttpContext.Session.Set<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY, organizationVM);

                 TempData[MyConstants.Success] = "Organization resource removed successfully.";
                return RedirectToAction(nameof(ManageOrganizationResource));
            }

             TempData[MyConstants.Error] = "Failed to remove organization resource.";
            return RedirectToAction(nameof(ManageOrganizationResource));
        }


        [HttpGet]
        public async Task<IActionResult> SendResourceOrganizationToProject(Guid resourceId)
        {
            // get current resource
            var currentResource = await _organizationRepository.GetOrganizationResourceAsync(or => or.ResourceID.Equals(resourceId));
            HttpContext.Session.Set<OrganizationResource>(MySettingSession.SESSION_Current_Organization_Resource_KEY, currentResource);

            var organizationToProjectHistory = new OrganizationToProjectHistory()
            {
                OrganizationResourceID = currentResource.ResourceID,
                Status = 0,
                Time = DateOnly.FromDateTime(DateTime.UtcNow),
            };

            return View(organizationToProjectHistory);
        }

        [HttpPost]
        public async Task<IActionResult> SendResourceOrganizationToProject(OrganizationToProjectHistory transaction, Guid projectId, List<IFormFile> file)
        {
            var resourceSent = await _organizationRepository.GetOrganizationResourceAsync(or => or.ResourceID.Equals(transaction.OrganizationResourceID));
            var resImages = await _cloudinaryUploader.UploadMultiImagesAsync(file);


            if (transaction != null && projectId != Guid.Empty && transaction.Amount > 0 && transaction.Amount <= resourceSent.Quantity && !(resImages.Equals("Wrong extension") || resImages.Equals("No file")))
            {
                transaction.Attachments = resImages;

                var project = await _projectVMService.GetProjectAsync(p => p.ProjectID.Equals(projectId));
                bool duplicate = false;
                var projectResource = new ProjectResource();

                foreach (var item in project.ProjectResource)
                {
                    if (item.ResourceName.ToUpper().Contains(resourceSent.ResourceName.ToUpper()) && item.Unit.ToUpper().Contains(resourceSent.Unit.ToUpper()))
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
                        if (item.ResourceName.ToUpper().Contains(resourceSent.ResourceName.ToUpper()) && item.Unit.ToUpper().Contains(resourceSent.Unit.ToUpper()))
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

                if (await _organizationRepository.AddOrganizationToProjectHistoryAsync(transaction))
                {
                    resourceSent.Quantity -= transaction.Amount;

                    if (await _organizationRepository.UpdateOrganizationResourceAsync(resourceSent))
                    {
                         TempData[MyConstants.Success] = "Resource sent to project successfully.";
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
            if (resImages.Equals("Wrong extension") || resImages.Equals("No file"))
            {
                TempData[MyConstants.Error] = resImages.Equals("No file")
                  ? "No file to upload!"
                  : "Extension of some files is wrong!";

                ViewBag.Images =  resImages.Equals("Wrong extension") ? "*Wrong extension" :  "*Please upload at least one image proof";
            }
           
            return View(transaction);
        }
        public async Task<IActionResult> CancelSendResource(Guid transactionId)
        {
            var transactionHistory = await _organizationRepository.GetOrganizationToProjectHistoryAsync(otp => otp.TransactionID.Equals(transactionId));
            if (transactionHistory != null)
            {
                var OrganizationResource = await _organizationRepository.GetOrganizationResourceAsync(or => or.ResourceID.Equals(transactionHistory.OrganizationResourceID));
                OrganizationResource.Quantity += transactionHistory.Amount;
                await _organizationRepository.UpdateOrganizationResourceAsync(OrganizationResource);
                await _organizationRepository.DeleteOrganizationToProjectHistoryAsync(transactionId);
                 TempData[MyConstants.Success] = "Resource sending transaction canceled successfully.";
                return RedirectToAction(nameof(ManageOrganizationResource));
            }
             TempData[MyConstants.Error] = "Transaction not found. Unable to cancel.";
            return RedirectToAction(nameof(ManageOrganizationResource));
        }


        // This method is shared between user donate to org and org allocate to project
        public IActionResult DonateByMoney(string organizationId, string resourceId)
        {
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }
            // Setup for display
            ViewBag.donatorName = currentUser.UserFullName;
            ViewBag.returnUrl = Url.Action("Detail", "Organization", new { organizationId }, Request.Scheme) ?? "~/";
            var vnPayRequestModel = new VnPayRequestDto
            {
                FromID = currentUser.UserID,
                ResourceID = new Guid(resourceId),
                TargetId = new Guid(organizationId),
                TargetType = MyConstants.Organization,
            };
            return View(vnPayRequestModel);
        }

        // TODO: Handle roles
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
            if (organizationMoneyResource == null) throw new Exception("WARNING: Organization MONEY resource not found");
            ViewData["limitAmount"] = organizationMoneyResource.Quantity;
            ViewBag.returnUrl = Url.Action("Detail", "Organization", new { organizationId }, Request.Scheme) ?? "~/";
            var vnPayRequestModel = new VnPayRequestDto
            {
                // Target project id will be rendered in a form of options
                FromID = new Guid(organizationId),
                ResourceID = new Guid(resourceId),
                TargetType = MyConstants.Allocation,
            };
            // same view as user donate to organization but with more options
            return View(nameof(DonateByMoney), vnPayRequestModel);
        }
        [HttpGet]
        public async Task<IActionResult> DonateByResource(Guid resourceId)
        {
            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            //get current user
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            // get current resource
            var currentResource = await _organizationRepository.GetOrganizationResourceAsync(or => or.ResourceID.Equals(resourceId));
            HttpContext.Session.Set<OrganizationResource>(MySettingSession.SESSION_Current_Organization_Resource_KEY, currentResource);

            var userToOrganizationTransactionHistory = new UserToOrganizationTransactionHistory()
            {
                ResourceID = resourceId,
                UserID = currentUser.UserID,
                Status = 0,
                Time = DateOnly.FromDateTime(DateTime.UtcNow),
            };
            return View(userToOrganizationTransactionHistory);
        }
        [HttpPost]
        public async Task<IActionResult> DonateByResource(UserToOrganizationTransactionHistory transactionHistory, List<IFormFile> file)
        {
            if (transactionHistory != null)
            { 
                var resImages = await _cloudinaryUploader.UploadMultiImagesAsync(file);
                if (!(resImages.Equals("Wrong extension") || resImages.Equals("No file")) && transactionHistory.Amount > 0)
                {
                    transactionHistory.Attachments = resImages;
                    if (await _organizationRepository.AddUserToOrganizationTransactionHistoryASync(transactionHistory))
                    {
                        TempData[MyConstants.Success] = "Resource donated successfully.";
                        return RedirectToAction(nameof(ManageOrganizationResource));
                    }
                }    
                else if(resImages.Equals("Wrong extension"))
                {
                    ViewBag.Images = "*Wrong extension";
                }
                else if(resImages.Equals("No file"))
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
            var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

            var userToOrganizationTransactionHistoryInAOrganizations = await _userToOragnizationTransactionHistoryVMService.GetTransactionHistory(currentOrganization.OrganizationID);

            return View(userToOrganizationTransactionHistoryInAOrganizations);
        }

        public async Task<IActionResult> MyDonors()
        {
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }

            var userToOrganizationTransactionHistoryInAOrganizations = await _userToOragnizationTransactionHistoryVMService.GetTransactionHistoryByUserID(currentUser.UserID);
            return View(userToOrganizationTransactionHistoryInAOrganizations);
        }

        public async Task<IActionResult> DenyRequestDonate(Guid transactionId, string reason)
        {
            if(reason != null)
            {
                //update table UserToOrganizationTransactionHistory
                var userToOrganizationTransactionHistory = await _organizationRepository.GetUserToOrganizationTransactionHistoryByTransactionIDAsync(uto => uto.TransactionID.Equals(transactionId));
                userToOrganizationTransactionHistory.Status = -1;
                userToOrganizationTransactionHistory.Message += $"\nReason Deny Your Request: {reason}";
                var updateTransactionSuccess = await _organizationRepository.UpdateUserToOrganizationTransactionHistoryAsync(userToOrganizationTransactionHistory);

                if (updateTransactionSuccess) TempData[MyConstants.Success] = "Request denied successfully.";
               
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
                var userToOrganizationTransactionHistory = await _organizationRepository.GetUserToOrganizationTransactionHistoryByTransactionIDAsync(uto => uto.TransactionID.Equals(transactionId));
                userToOrganizationTransactionHistory.Status = 1;
                userToOrganizationTransactionHistory.Attachments = resImages;
                var updateTransactionSuccess = await _organizationRepository.UpdateUserToOrganizationTransactionHistoryAsync(userToOrganizationTransactionHistory);

                var currentOrganization = HttpContext.Session.Get<OrganizationVM>(MySettingSession.SESSION_Current_Organization_KEY);

                //update table resource
                var organizationResource = await _organizationRepository.GetOrganizationResourceByOrganizationIDAndResourceIDAsync(currentOrganization.OrganizationID, userToOrganizationTransactionHistory.ResourceID);
                organizationResource.Quantity += userToOrganizationTransactionHistory.Amount;
                var updateResourceSuccess = await _organizationRepository.UpdateOrganizationResourceAsync(organizationResource);
                if (updateTransactionSuccess && updateResourceSuccess)
                {
                    TempData[MyConstants.Success] = "Request accepted successfully.";
                    return RedirectToAction(nameof(ManageOrganizationResource));
                }

                else
                {
                    
                    TempData[MyConstants.Error] = "Failed to accept request.";
                    return RedirectToAction( nameof(ReviewDonateRequest));
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