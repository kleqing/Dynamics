#nullable disable

using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ILogger = Serilog.ILogger;

namespace Dynamics.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserRepository _userRepo;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IUserRepository repository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _userRepo = repository;
        }

        // Declares that incoming http request will be bind to this input
        // This only appear in Razor page because they have no controller
        [BindProperty] public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (Input.Password != Input.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords don't match.");
                return Page();
            }
            // Try to get existing user (If we might have) that is in the system
            var existingUserFullName = await _userRepo.GetAsync(u => u.UserFullName.Equals(Input.Name));
            var existingUserEmail = await _userRepo.GetAsync(u => u.UserEmail.Equals(Input.Email));
            // If one of these 2 exists, it means that another user is already has the same name or email
            if (existingUserEmail != null || existingUserFullName != null)
            {
                ModelState.AddModelError("", "Username or email is already taken.");
                return Page();
            }
            if (ModelState.IsValid)
            {
                // If role not exist, create all of our possible role
                // also, getAwaiter is the same as writing await keyword
                _logger.LogWarning("REGISTER: CREATING ROLES");
                if (!_roleManager.RoleExistsAsync(RoleConstants.User).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(RoleConstants.User)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(RoleConstants.Admin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(RoleConstants.HeadOfOrganization)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(RoleConstants.ProjectLeader)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(RoleConstants.Banned)).GetAwaiter().GetResult();
                }

                var user = new IdentityUser();
                await _userManager.AddToRoleAsync(user, RoleConstants.User); // Default role
                user.UserName = Input.Name;
                user.Email = Input.Email;
                _logger.LogWarning("REGISTER: CREATING IDENTITY USER");
                // Create a user with email and input password
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogWarning("REGISTER: ADDING USER TO DATABASE");
                    // Add real user to database
                    await _userRepo.AddAsync(new User
                    {
                        // Note: Identity user id is string while normal user ID is Guid
                        UserID = new Guid(user.Id), // The link between 2 user table should be this id
                        UserFullName = Input.Name,
                        UserEmail = Input.Email,
                        UserAvatar = MyConstants.DefaultAvatarUrl,
                        UserRole = RoleConstants.User,
                    });

                    _logger.LogInformation("User created a new account with password.");
                    
                    // Where the email sending begins
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation",
                            new { email = Input.Email, returnUrl = returnUrl });
                    }
                    // This part is only for debug purpose, because user will be redirect to register confirmation instead
                    var businessUser = await _userRepo.GetAsync(u => u.UserID.ToString() == user.Id);
                    HttpContext.Session.SetString("user", JsonConvert.SerializeObject(businessUser));
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Redirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}