using System.Text;
using System.Text.Encodings.Web;
using Dynamics.Areas.Identity.Pages.Account;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;

namespace Dynamics.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
            IUserRepository userRepo, IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepo;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ResendConfirmationEmail(string email, string? returnUrl = "~/")
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData[MyConstants.Error] = "Please enter your email address inorder to continue";
                return Redirect("/Identity/Account/Login");
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                TempData[MyConstants.Success] = "Confirmation email sent!, please check your mail box";
            }
            catch (Exception e)
            {
                TempData[MyConstants.Error] = "Confirmation email could not be sent.";
                TempData[MyConstants.Subtitle] = e.Message;
                throw;
            }

            return Redirect("/Identity/Account/Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            HttpContext.Session.Clear(); // Clear the session
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}