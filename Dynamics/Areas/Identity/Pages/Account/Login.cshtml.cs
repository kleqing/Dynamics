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
using Microsoft.AspNetCore.Http;
using Dynamics.Utility;
using Dynamics.Models.Models;
using Dynamics.Utility;
using Dynamics.Models.Models;
using AutoMapper.Execution;
using Microsoft.CodeAnalysis;

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

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                }
                else
                {
                    //Check if verified first before sign in
                    var isEmailConfirmedAsync = await _userManager.IsEmailConfirmedAsync(user);
                    if (!isEmailConfirmedAsync)
                    {
                        ModelState.AddModelError(string.Empty, "User account is not confirmed!");
                        return Page();
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe,
                        lockoutOnFailure: false);
                    // SerializeObject for session
                    var businessUser = await _userRepository.GetAsync(u => u.UserEmail == user.Email);
                    HttpContext.Session.SetString("user", JsonConvert.SerializeObject(businessUser));
                    HttpContext.Session.SetString("currentUserID", businessUser.UserID.ToString());
                    
                    //var isBan = _userRepository.GetBanAsync(businessUser.UserID);

                    //if (isBan.Result)
                    //{
                    //    ModelState.AddModelError(string.Empty, "User account is banned!");
                    //    return Page();
                    //}
                    // Login as administrator
                    if (User.IsInRole(RoleConstants.Admin) && result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                    else if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        if (returnUrl.Contains("JoinProjectRequest"))
                        {
                            returnUrl = returnUrl.Replace("memberid=00000000-0000-0000-0000-000000000000", $"memberid={businessUser.UserID.ToString()}");
                        }
                        return Redirect(returnUrl);
                    }

                    // TODO: Ban user in da future
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    // If we get here, something went wrong.
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }
            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}