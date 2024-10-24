#nullable disable

using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using ILogger = Serilog.ILogger;

namespace Dynamics.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IUserRepository _userRepo;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            IUserRepository userRepo)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _logger = logger;
            _emailSender = emailSender;
            _userRepo = userRepo;
        }

        [BindProperty] public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData] public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required] [EmailAddress] public string Email { get; set; }
        }

        // Prevent unauthorized access to the page
        public IActionResult OnGet() => RedirectToPage("./Login");

        // When user click on the button
        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        // Login
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            // await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme); // Clear cookies for clean process
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Checking if the user already have an account with the same email, we will use that account to log in instead
            var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
            var businessUser = await _userRepo.GetAsync(u => u.UserEmail == userEmail);
            if (businessUser != null && businessUser.UserRole.Equals(RoleConstants.Banned))
            {
                ModelState.AddModelError("", "User account is banned!");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var existingUser = await _userManager.FindByEmailAsync(userEmail ?? "No email");
            if (existingUser != null)
            {
                // Sign in using that user instead of Google
                HttpContext.Session.SetString("user", JsonConvert.SerializeObject(businessUser));
                HttpContext.Session.SetString("currentUserID", businessUser.UserID.ToString());
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name,
                    info.LoginProvider);
                await _signInManager.SignInAsync(existingUser, isPersistent: false, authenticationMethod: null);
                if (User.IsInRole(RoleConstants.Admin))
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                return Redirect(returnUrl);
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                if (User.IsInRole(RoleConstants.Admin) && result.Succeeded)
                {
                    return Redirect("~/Admin/");
                }

                // Set the session
                HttpContext.Session.SetString("user", JsonConvert.SerializeObject(businessUser));
                HttpContext.Session.SetString("currentUserID", businessUser.UserID.ToString());
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name,
                    info.LoginProvider);
                if (User.IsInRole(RoleConstants.Admin) && result.Succeeded)
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                return Redirect(returnUrl);
            }

            // If the user does not have an account, then ask the user to create an account.
            // Do we need this though ?
            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName; // This one is needed so that register can work
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                Input = new InputModel
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                };
            }

            return Page();
        }

        // Register
        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                // Create a new identity user base on Google information
                var user = new IdentityUser();
                user.UserName = Input.Email;
                user.Email = Input.Email;

                var result = await _userManager.CreateAsync(user); // Create user with no password bc of Google login

                if (result.Succeeded)
                {
                    // add external login to user so that he will be able to sign in using external login
                    result = await _userManager.AddLoginAsync(user, info);
                    // No need for this anymore because if user exist, it will use the exist account to log in instead (Look at the login section)
                    // var existed = await _userManager.FindByEmailAsync(user.Email);
                    if (result.Succeeded)
                    {
                        // Add user to the database after creating the user with external login
                        await _userRepo.AddAsync(new User
                        {
                            UserID = new Guid(user
                                .Id), // This and email is the only thing that connects between 2 tables, the user name IS NOT the same
                            UserFullName = Input.Email,
                            UserEmail = info.Principal.FindFirstValue(ClaimTypes.Email), // Get user's email from Google
                            UserAvatar = info.Principal.FindFirstValue("picture"),
                            UserRole = RoleConstants.User
                        });
                        await _userRepo.AddToRoleAsync(new Guid(user.Id), RoleConstants.User); // Init default role
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        // existed.EmailConfirmed = true;
                        // await _userManager.UpdateAsync(existed);

                        // We don't need to confirm the email if the user use Google auth
                        await _emailSender.SendEmailAsync(Input.Email, "Register Confirmation",
                            $"You have register successfully to Dynamics");

                        // Set the session for the app:
                        var businessUser = await _userRepo.GetAsync(u => u.UserEmail == user.Email);
                        HttpContext.Session.SetString("user", JsonConvert.SerializeObject(businessUser));
                        HttpContext.Session.SetString("currentUserID", businessUser.UserID.ToString());
                        // Provide user with default role as well
                        result = _userManager.AddToRoleAsync(user, RoleConstants.User).GetAwaiter().GetResult();
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                            return Redirect(returnUrl);
                        }
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}