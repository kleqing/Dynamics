// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Dynamics.DataAccess.Repository;
using Newtonsoft.Json;
using Dynamics.Utility;
using Serilog;
using ILogger = Serilog.ILogger;
using Dynamics.Models.Models;

namespace Dynamics.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager, IUserRepository userRepository)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        [BindProperty] public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData] public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required] [EmailAddress] public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")] public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ReturnUrl = returnUrl;
        }

        // Login
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Wrong email or password");
                }
                else
                {
                    if (await _userManager.IsInRoleAsync(user, RoleConstants.Banned))
                    {
                        ModelState.AddModelError(string.Empty, "User account is banned!");
                        return Page();
                    }

                    //Check if verified first before sign in
                    var isEmailConfirmedAsync = await _userManager.IsEmailConfirmedAsync(user);
                    if (!isEmailConfirmedAsync)
                    {
                        var resendConfirmationEmail =
                            Url.Action("ResendConfirmationEmail", "Auth", new { area = "", email = Input.Email },
                                Request.Scheme); // Request scheme is used so that it generate the whole url instead of just relative
                        TempData["ConfirmationEmail"] = resendConfirmationEmail;
                        ModelState.AddModelError(string.Empty, "Your account is not confirmed!");
                        return Page();
                    }

                    _logger.LogWarning("LOGIN: PasswordSignInAsync");
                    var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe,
                        lockoutOnFailure: true);
                    if (!result.Succeeded)
                    {
                        if (await _userManager.IsLockedOutAsync(user))
                        {
                            ModelState.AddModelError(string.Empty,
                                "You have too many invalid login attempts, please try again later.");
                            return Page();
                        }

                        ModelState.AddModelError(string.Empty, "Wrong email or password");
                        return Page();
                    }

                    // SerializeObject for session
                    _logger.LogWarning("LOGIN: GET BUSINESS USER");
                    var businessUser = await _userRepository.GetAsync(u => u.UserEmail == user.Email);
                    HttpContext.Session.SetString("user", JsonConvert.SerializeObject(businessUser));
                    HttpContext.Session.SetString("currentUserID", businessUser.UserID.ToString());
                    // Login as administrator
                    if (User.IsInRole(RoleConstants.Admin) && result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return Redirect(returnUrl);
                    }

                    // If we get here, something went wrong.
                    ModelState.AddModelError(string.Empty, "Something went wrong, please try again.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}