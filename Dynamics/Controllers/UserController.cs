using Dynamics.DataAccess.Repository;
using Dynamics.Models;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dynamics.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITransactionViewService _transactionViewService;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserToOrganizationTransactionHistoryRepository _userToOrgRepo;
        private readonly IUserToProjectTransactionHistoryRepository _userToPrjRepo;
        private readonly CloudinaryUploader _cloudinaryUploader;

        public UserController(IUserRepository userRepo, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ITransactionViewService transactionViewService,
            IProjectMemberRepository projectMemberRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserToOrganizationTransactionHistoryRepository userToOrgRepo,
            IUserToProjectTransactionHistoryRepository userToPrjRepo, CloudinaryUploader cloudinaryUploader)
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
        }

        // View a user profile (including user's own profile)
        public async Task<IActionResult> Index(string username)
        {
            var currentUser = await _userRepository.GetAsync(u => u.UserFullName.Equals(username));
            if (currentUser == null) return NotFound();
            return View(currentUser);
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

            var user = await _userRepository.GetAsync(u => u.UserID.Equals(currentUser.UserID));
            if (user == null)
            {
                return NotFound();
            }

            // Convert user DOB to correct date for display purpose
            if (user.UserDOB != null)
            {
                ViewBag.UserDOB = user.UserDOB.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd");
            }

            return View(user);
        }

        // POST: Client/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user, IFormFile? image)
        {
            try
            {
                var currentUser = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("user"));
                // Try to get existing user (If we might have) that is in the system
                var existingUserFullName = await _userRepository.GetAsync(u =>
                    u.UserFullName.Equals(user.UserFullName) && u.UserFullName != currentUser.UserFullName);
                var existingUserEmail = await _userRepository.GetAsync(u =>
                    u.UserEmail.Equals(user.UserEmail) && u.UserEmail != currentUser.UserEmail);
                // If one of these 2 exists, it means that another user is already has the same name or email
                if (existingUserEmail != null || existingUserFullName != null)
                {
                    TempData[MyConstants.Error] = "Username or email is already taken.";
                    return View(user);
                }

                if (user.UserFullName == null || user.UserEmail == null)
                {
                    TempData[MyConstants.Error] = "Update failed...";
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

                await _userRepository.UpdateAsync(user);

                // Update the session as well
                HttpContext.Session.SetString("user", JsonConvert.SerializeObject(user));
                TempData[MyConstants.Success] = "User updated!";
                if (currentUser.UserDOB != null)
                    ViewBag.UserDOB = currentUser.UserDOB.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd");
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

            var user = await _userManager.FindByIdAsync(currentUser.UserID.ToString());
            if (user.PasswordHash == null)
            {
                TempData["Google"] = "Your account is bounded with google account.";
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

            if (changePassword.NewPassword != changePassword.ConfirmPassword)
            {
                TempData[MyConstants.Error] = "The password and confirmation password do not match.";
                return RedirectToAction("Account", "User");
            }

            var currentUser = await _userManager.FindByIdAsync(changePassword.UserId.ToString());
            if (currentUser.PasswordHash == null)
            {
                TempData["Google"] = "Your account is bound with google account.";
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
        public async Task<IActionResult> History()
        {
            // Get current userID
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
                currentUser = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("user"));
            else return RedirectToAction("Logout", "Auth");
            var userToOrgTransaction = await _transactionViewService
                .GetUserToOrganizationTransactionDTOsAsync(ut =>
                    ut.UserID.Equals(currentUser.UserID) && ut.Status == 1);
            var userToPrjTransaction = await _transactionViewService
                .GetUserToProjectTransactionDTOsAsync(ut => ut.UserID.Equals(currentUser.UserID) && ut.Status == 1);
            // Merge into a list and display
            var total = new List<UserTransactionDto>();
            total.AddRange(userToOrgTransaction);
            total.AddRange(userToPrjTransaction);
            var final = new UserHistoryViewModel
            {
                userTransactions = total.OrderByDescending(ut => ut.Time).ToList() // Display descending by time
            };
            return View(final);
        }

        public async Task<IActionResult> RequestsStatus()
        {
            // Get current userID
            var userString = HttpContext.Session.GetString("user");
            User currentUser = null;
            if (userString != null)
                currentUser = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("user"));
            else return RedirectToAction("Logout", "Auth");
            // Get join requests except for the one with status of 2 (User is already the leader of)
            var userRequestProjects =
                await _projectMemberRepository.GetAllAsync(pm => pm.UserID.Equals(currentUser.UserID) && pm.Status < 2);
            var userRequestOrganizations =
                await _organizationMemberRepository.GetAllAsync(om =>
                    om.UserID.Equals(currentUser.UserID) && om.Status < 2);

            // Donation requests only get the pending ones (Money is automatically accepted so it should not be here.)
            var userToOrgTransaction = await _transactionViewService
                .GetUserToOrganizationTransactionDTOsAsync(ut =>
                    ut.UserID.Equals(currentUser.UserID) && ut.Status == 0);
            var userToPrjTransaction = await _transactionViewService
                .GetUserToProjectTransactionDTOsAsync(ut => ut.UserID.Equals(currentUser.UserID) && ut.Status == 0);
            userToPrjTransaction.AddRange(userToOrgTransaction);

            return View(new UserRequestsStatusViewModel
            {
                OrganizationJoinRequests = userRequestOrganizations,
                ProjectJoinRequests = userRequestProjects,
                ResourcesDonationRequests = userToPrjTransaction,
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