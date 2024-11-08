using System.Text.RegularExpressions;
using AutoMapper;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using ILogger = Serilog.ILogger;

namespace Dynamics.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITransactionViewService _transactionViewService;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserToOrganizationTransactionHistoryRepository _userToOrgRepo;
        private readonly IUserToProjectTransactionHistoryRepository _userToPrjRepo;
        private readonly CloudinaryUploader _cloudinaryUploader;
        private readonly ISearchService _searchService;
        private readonly IPagination _pagination;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;
        private readonly IRoleService _roleService;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationService _organizationService;

        public UserController(IUserRepository userRepo, UserManager<User> userManager,
            SignInManager<User> signInManager, ITransactionViewService transactionViewService,
            IProjectMemberRepository projectMemberRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserToOrganizationTransactionHistoryRepository userToOrgRepo,
            IUserToProjectTransactionHistoryRepository userToPrjRepo, CloudinaryUploader cloudinaryUploader,
            ISearchService searchService, IPagination pagination, IMapper mapper, IOrganizationRepository organizationRepository,
            ILogger<UserController> logger, IRoleService roleService)
        {
            _userRepository = userRepo;
            _userManager = userManager;
            _signInManager = signInManager;
            _transactionViewService = transactionViewService;
            _projectMemberRepository = projectMemberRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userToOrgRepo = userToOrgRepo;
            _userToPrjRepo = userToPrjRepo;
            _cloudinaryUploader = cloudinaryUploader;
            _searchService = searchService;
            _pagination = pagination;
            _mapper = mapper;
            _logger = logger;
            _roleService = roleService;
            _organizationRepository = organizationRepository;
        }

        // View a user profile (including user's own profile)
        // [Route("User/{username}")]
        public async Task<IActionResult> Index(string username)
        {
            var currentUser = await _userRepository.GetAsync(u => u.UserName.Equals(username));
            if (currentUser == null) return NotFound();
            var userVM = _mapper.Map<UserVM>(currentUser);
            var isCEO = await _roleService.IsInRoleAsync(currentUser.Id, RoleConstants.HeadOfOrganization);
            {
                var orgMember = await _organizationMemberRepository.GetAsync(om => om.Status == 2 && om.UserID == currentUser.Id);
                if (orgMember != null)
                {
                    userVM.OrganizationName = orgMember.Organization.OrganizationName;
                    userVM.OrganizationId = orgMember.Organization.OrganizationID;
                }
            }
            return View(userVM);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }
            else
            {
                return RedirectToAction("Logout", "Auth");
            }

            var user = await _userRepository.GetAsync(u => u.Id.Equals(currentUser.Id));
            if (user == null)
            {
                return NotFound();
            }

            // Convert user DOB to correct date for display purpose
            if (user.UserDOB != null)
            {
                ViewBag.UserDOB = user.UserDOB.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd");
            }

            return View(_mapper.Map<UserVM>(currentUser));
        }

        // POST: Client/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserVM user, IFormFile? image)
        {
            try
            {
                var currentUser = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("user"));
                if (currentUser == null) return Unauthorized();
                // Try to get existing user (If we might have) that is in the system
                var existingUserFullName = await _userRepository.GetAsync(u =>
                    (u.UserName.Equals(user.UserName) && u.UserName != currentUser.UserName) ||
                    (u.Email.Equals(user.Email) && u.Email != currentUser.Email));
                // If one of these 2 exists, it means that another user is already has the same name or email
                if (existingUserFullName != null)
                {
                    TempData[MyConstants.Error] = "Username or email is already taken.";
                    return View(user);
                }

                if (user.UserName == null || user.Email == null)
                {
                    TempData[MyConstants.Error] = "User name and email cannot be empty";
                    return View(user);
                }

                if (image != null)
                {
                    var imgUrl = await _cloudinaryUploader.UploadImageAsync(image);
                    if (imgUrl.Equals("Wrong file extension", StringComparison.OrdinalIgnoreCase))
                    {
                        TempData[MyConstants.Error] = "Wrong file extension";
                        TempData[MyConstants.Subtitle] = "Support formats are: jpg, jpeg, png, gif, webp.";
                        return View(user);
                    }

                    if (imgUrl != null) user.UserAvatar = imgUrl;
                }

                await _userRepository.UpdateAsync(_mapper.Map<User>(user));

                // Update the session as well
                HttpContext.Session.SetString("user", JsonConvert.SerializeObject(user));
                TempData[MyConstants.Success] = "User updated!";
                // Set for display
                if (user.UserDOB != null)
                    ViewBag.UserDOB = user.UserDOB.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong, please try again later.");
                return View(user);
            }

            return View(user);
        }

        public async Task<IActionResult> Account()
        {
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
            {
                currentUser = JsonConvert.DeserializeObject<User>(userString);
            }
            else
            {
                return RedirectToAction("Logout", "Auth");
            }

            var user = await _userManager.FindByIdAsync(currentUser.Id.ToString());
            if (user.PasswordHash == null)
            {
                TempData["Google"] = "Your account is bounded with Google account.";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePassword)
        {
            if (!ModelState.IsValid)
            {
                TempData[MyConstants.Error] = "Change password failed...";
                return RedirectToAction("Account", "User");
            }

            var currentUser = await _userManager.FindByIdAsync(changePassword.UserId.ToString());
            if (currentUser == null) return Unauthorized();

            if (changePassword.NewPassword != changePassword.ConfirmPassword)
            {
                TempData[MyConstants.Error] = "The password and confirmation password do not match.";
                return RedirectToAction("Account", "User");
            }

            if (currentUser.PasswordHash == null)
            {
                TempData["Google"] = "Your account is bound with google account.";
                return RedirectToAction("Account", "User");
            }

            // Check if user enter the old password and new password the same
            var passwordHasher = _userManager.PasswordHasher.VerifyHashedPassword(currentUser, currentUser.PasswordHash,
                changePassword.NewPassword);
            if (passwordHasher == PasswordVerificationResult.Success)
            {
                TempData[MyConstants.Error] = "New password cannot be the same as old password.";
                return RedirectToAction("Account", "User");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(currentUser, changePassword.OldPassword,
                changePassword.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    TempData[MyConstants.Error] = error.Description;
                }

                return RedirectToAction("Account", "User");
            }

            // Add a message and refresh the page
            TempData[MyConstants.Success] = "Password changed!";
            // Sign in the user again
            await _signInManager.RefreshSignInAsync(currentUser);
            // return RedirectToAction("Logout", "Auth");
            return RedirectToAction("Account", "User");
        }

        /**
         * Display all accepted user donations
         */
        public async Task<IActionResult> History(SearchRequestDto searchOptions,
            PaginationRequestDto paginationRequestDto)
        {
            _logger.LogWarning("STARTING HISTORY VIEW");
            // Get current userID
            var userString = HttpContext.Session.GetString("user");
            User currentUser;
            if (userString != null)
                currentUser = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("user"));
            else return RedirectToAction("Logout", "Auth");

            // Get base query for both type of transactions
            var userToOrgQueryable =
                _userToOrgRepo.GetAllAsQueryable(ut => ut.UserID.Equals(currentUser.Id) && ut.Status == 1);
            var userToPrjQueryable =
                _userToPrjRepo.GetAllAsQueryable(ut => ut.UserID.Equals(currentUser.Id) && ut.Status == 1);

            var total = await _transactionViewService.SetupUserTransactionDtosWithSearchParams(searchOptions,
                userToPrjQueryable, userToOrgQueryable);
            var paginated = _pagination.Paginate(total, HttpContext, paginationRequestDto, searchOptions);

            var final = new UserHistoryViewModel
            {
                UserTransactions =
                    paginated, // Display descending by time (The ordering should already be default in search)
                PaginationRequestDto = paginationRequestDto,
                SearchRequestDto = searchOptions,
            };
            return View(final);
        }

        public async Task<IActionResult> RequestsStatus(SearchRequestDto searchRequestDto,
            PaginationRequestDto paginationRequestDto)
        {
            _logger.LogWarning("STARTING REQUEST STATUS");
            // Get current userID
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
                currentUser = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("user"));
            else return RedirectToAction("Logout", "Auth");
            // Get join requests except for the one with status of 2 (User is already the leader of)
            var userRequestProjects =
                await _projectMemberRepository.GetAllAsync(pm => pm.UserID.Equals(currentUser.Id) && pm.Status < 2);
            var userRequestOrganizations =
                await _organizationMemberRepository.GetAllAsync(om =>
                    om.UserID.Equals(currentUser.Id) && om.Status < 2);

            // Donation requests only get the pending / denied ones (Money is automatically accepted so it should not be here.)
            var userToOrgQueryable =
                _userToOrgRepo.GetAllAsQueryable(ut => ut.UserID.Equals(currentUser.Id) && ut.Status < 1);
            var userToPrjQueryable =
                _userToPrjRepo.GetAllAsQueryable(ut => ut.UserID.Equals(currentUser.Id) && ut.Status < 1);

            var total = await _transactionViewService.SetupUserTransactionDtosWithSearchParams(searchRequestDto,
                userToPrjQueryable, userToOrgQueryable);
            var paginated = _pagination.Paginate(total, HttpContext, paginationRequestDto, searchRequestDto);
            ViewBag.totalPages = paginationRequestDto.TotalPages;

            return View(new UserRequestsStatusViewModel
            {
                OrganizationJoinRequests = userRequestOrganizations,
                ProjectJoinRequests = userRequestProjects,
                ResourcesDonationRequests = paginated,
                SearchRequestDto = searchRequestDto,
                PaginationRequestDto = paginationRequestDto
            });
        }

        // Cancel a user pending donation
        public async Task<IActionResult> CancelDonation(Guid transactionID, string type)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "project":
                    {
                        var result = await _userToPrjRepo.DeleteTransactionByIdAsync(transactionID);
                        if (result == null) throw new Exception();
                        break;
                    }
                    case "organization":
                    {
                        var result = await _userToOrgRepo.DeleteTransactionByIdAsync(transactionID);
                        if (result == null) throw new Exception();
                        break;
                    }
                    default:
                        throw new ArgumentException("Invalid type");
                }

                TempData[MyConstants.Success] = "Donation cancelled successful!";
            }
            catch (Exception e)
            {
                TempData[MyConstants.Error] = "Something went wrong, please try again later.";
            }

            return RedirectToAction("RequestsStatus", "User");
        }

        public async Task<IActionResult> CancelJoinRequest(Guid userID, Guid targetID, string type)
        {
            var msg = "Something went wrong, please try again later.";
            try
            {
                switch (type.ToLower())
                {
                    case "project":
                    {
                        var result =
                            await _projectMemberRepository.DeleteAsync(pm =>
                                pm.UserID == userID && pm.ProjectID == targetID);
                        if (result == null)
                        {
                            msg = "No request found for this transaction.";
                            throw new Exception("Cancel failed.");
                        }

                        break;
                    }
                    case "organization":
                    {
                        var result = await _organizationMemberRepository.DeleteAsync(om =>
                            om.UserID == userID && om.OrganizationID == targetID);
                        if (result == null)
                        {
                            msg = "No request found for this transaction.";
                            throw new Exception("Cancel failed.");
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException("Invalid type");
                }

                TempData[MyConstants.Success] = "Cancel successful!";
            }
            catch (Exception e)
            {
                TempData[MyConstants.Error] = msg;
            }

            return RedirectToAction("RequestsStatus", "User");
        }
    }
}