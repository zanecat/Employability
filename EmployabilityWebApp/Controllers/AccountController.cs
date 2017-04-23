using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.Services;
using EmployabilityWebApp.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;


namespace EmployabilityWebApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService accountService;

        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        ///
        /// GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                if (User.IsInRole(BasicUser.Role))
                {
                    return RedirectToAction("Start", "SelfAssessment");
                }
                if (User.IsInRole(AdminUser.Role))
                {
                    return RedirectToAction("Index", "SelfAssessment");
                }
                if (User.IsInRole(OrganisationUser.Role))
                {
                    return RedirectToAction("Index", "Manage");
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ViewName = "Login page";
            return View();
        }

        ///
        /// POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result =
                await
                    accountService.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
                        shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Login");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new {ReturnUrl = returnUrl, model.RememberMe});
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        ///
        /// GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await accountService.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel {Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe});
        }

        ///
        /// POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            // You can configure the account lockout settings in IdentityConfig
            var result =
                await
                    accountService.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe,
                        rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        ///
        /// GET: /Account/RegisterOrganization
        [HttpGet]
        [Authorize(Roles = AdminUser.Role)]
        [Route("Account/RegisterOrganization")]
        public ActionResult RegisterOrganization()
        {
            ViewBag.ViewName = "Register page";
            RegisterOrganizationViewModel
                viewModel = new RegisterOrganizationViewModel();

            return View(viewModel);
        }

        ///
        /// GET: /Account/RegisterBasic
        [HttpGet]
        [AllowAnonymous]
        [Route("Account/RegisterBasic")]
        public ActionResult RegisterBasic()
        {

            var isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                if (User.IsInRole(BasicUser.Role))
                {
                    return RedirectToAction("Start", "SelfAssessment");
                }
                if (User.IsInRole(AdminUser.Role))
                {
                    return RedirectToAction("Index", "SelfAssessment");
                }
                if (User.IsInRole(OrganisationUser.Role))
                {
                    return RedirectToAction("Index", "Manage");
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ViewName = "Register page";
            RegisterBasicUserViewModel
                viewModel = new RegisterBasicUserViewModel();

            return View(viewModel);
        }

        ///
        /// POST: /Account/RegisterBasic
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("Account/RegisterBasic")]
        public async Task<ActionResult> RegisterBasic(RegisterBasicUserViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await accountService.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }

                user = new BasicUser(model.Email)
                {
                    GivenName = model.GivenName,
                    FamilyName = model.FamilyName
                };

                var result = await accountService.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var code = await accountService.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action(
                        "ConfirmEmail", "Account",
                        new {userId = user.Id, code},
                        protocol: Request.Url?.Scheme);

                    await accountService.SendEmailAsync(user.Id,
                        "Confirm your account",
                        callbackUrl);
                    return View("DisplayEmail");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        ///
        /// POST: /Account/RegisterOrganization
        [HttpPost]
        [Authorize(Roles = AdminUser.Role)]
        [ValidateAntiForgeryToken]
        [Route("Account/RegisterOrganization")]
        public async Task<ActionResult> RegisterOrganization(RegisterOrganizationViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await accountService.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }

                user = new OrganisationUser(model.Email)
                {
                    Domain = model.Domain,
                    OrganisationName = model.OrganizationName
                };

                var result = await accountService.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var code = await accountService.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action(
                        "ConfirmEmailOrganisation", "Account",
                        new { userId = user.Id, code },
                        protocol: Request.Url?.Scheme);

                    await accountService.SendEmailAsync(user.Id,
                        "Confirm your account",
                        callbackUrl);
                    return View("DisplayEmail");

                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmailOrganisation(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await accountService.ConfirmEmailAsync(userId, code);

            if (!result.Succeeded)
            {
                return View("Error");
            }
            await accountService.ConfirmEmailOrganisationUser(userId, code);
            return RedirectToAction("Index", "Manage");
        }

        ///
        /// GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await accountService.ConfirmEmailAsync(userId, code);

            if (!result.Succeeded)
            {
                return View(result.Succeeded ? "ConfirmEmail" : "Error");
            }
            await accountService.ConfirmEmailBasicUser(userId, code);
            return RedirectToAction("Start", "SelfAssessment");
        }

        ///
        /// GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        ///
        /// POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var isConfirmed = await accountService.IsEmailConfirmedAsync(model.Email);

            return isConfirmed ? View("ForgotPasswordConfirmation") : View(model);
        }

        ///
        /// GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        ///
        /// GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        ///
        /// POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await accountService.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await accountService.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        ///
        /// GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        ///
        /// POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        ///
        /// GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await accountService.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await accountService.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        ///
        /// POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await accountService.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe });
        }

        ///
        /// GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await accountService.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        ///
        /// POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser(model.Email);
                var result = await accountService.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await accountService.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await accountService.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        ///
        /// POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        ///
        /// GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }


        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}
