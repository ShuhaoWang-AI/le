using System.Threading.Tasks;
using System.Web.Mvc;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Web.ViewModels.Account;
using Linko.LinkoExchange.Core.Enum;
using NLog;
using System.Web;
using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix("Account")]
    [Route("{action=Index}")]
    public class AccountController : Controller
    {
        #region constructor

        private readonly IAuthenticationService _authenticateService;
        private readonly ILogger _logger;

        public AccountController(IAuthenticationService authenticateService, ILogger logger)
        {
            _authenticateService = authenticateService;
            _logger = logger;
        }

        #endregion


        #region default action

        [AllowAnonymous]
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction(actionName: "UpdateUser"); // TODO: change to appropriate action
            }
            else
            {
                return RedirectToAction(actionName: "SignIn");
            }
        }

        #endregion


        #region sign in action


        // GET: Account/SignIn
        [AllowAnonymous]
        public ActionResult SignIn(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            SignInViewModel model = new SignInViewModel();
            model.UserName = (HttpContext.Request.Cookies["lastSignInName"] != null) ? HttpContext.Request.Cookies.Get(name: "lastSignInName").Value : "";

            return View(model);
        }


        // POST: Account/SignIn
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignIn(SignInViewModel model, string returnUrl)
        {
            var result = await _authenticateService.SignInByUserName(model.UserName, model.Password, model.RememberMe);

            switch (result.AutehticationResult)
            {
                case AuthenticationResult.Success:
                    HttpCookie cookie = new HttpCookie(name: "lastSignInName", value: model.UserName);
                    HttpContext.Response.SetCookie(cookie);
                    _logger.Info(string.Format(format: "SignIn. User={0} has successfully logged in.", arg0: model.UserName));
                    return RedirectToLocal(returnUrl);
                case AuthenticationResult.PasswordLockedOut:
                    _logger.Info(string.Format(format: "SignIn. User={0} has been locked out for exceeding the maximum login attempts.", arg0: model.UserName));
                    return RedirectToAction(actionName: "LockedOut", controllerName: "Account");
                case AuthenticationResult.UserIsLocked:
                    _logger.Info(string.Format(format: "SignIn. User={0} has been locked out.", arg0: model.UserName));
                    return RedirectToAction(actionName: "AccountLocked", controllerName: "Account");
                case AuthenticationResult.UserIsDisabled:
                    _logger.Info(string.Format(format: "SignIn. User={0} has been disabled.", arg0: model.UserName));
                    return RedirectToAction(actionName: "AccountDisabled", controllerName: "Account");
                case AuthenticationResult.RegistrationApprovalPending:
                    _logger.Info(string.Format(format: "SignIn. User={0} registration approval pending.", arg0: model.UserName));
                    return RedirectToAction(actionName: "RegistrationApprovalPending", controllerName: "Account");
                case AuthenticationResult.PasswordExpired:
                    _logger.Info(string.Format(format: "SignIn. User={0} password is expired.", arg0: model.UserName));
                    return RedirectToAction(actionName: "PasswordExpired", controllerName: "Account");
                case AuthenticationResult.UserNotFound:
                case AuthenticationResult.InvalidUserNameOrPassword:
                case AuthenticationResult.Failed:
                default:
                    ModelState.AddModelError(key: "", errorMessage: Core.Resources.Message.InvalidLoginAttempt);
                    ModelState.AddModelError(key: "", errorMessage: Core.Resources.Message.InvalidLoginAttempt);
                    return View(model);
            }
        }

        // Account locked out by Administrator
        // GET: /Account/AccountLocked
        [AllowAnonymous]
        public ActionResult AccountLocked()
        {
            ViewBag.SubTitle = "Account Locked";
            ViewBag.HtmlStr = Core.Resources.Message.AccountLocked + "<br/>";
            // need authority list

            return View(viewName: "Confirmation");
        }

        // account locked out due to several failure login attempt
        // GET: /Account/LockedOut
        [AllowAnonymous]
        public ActionResult LockedOut()
        {
            ViewBag.SubTitle = "Locked out";
            ViewBag.HtmlStr = Core.Resources.Message.ExceedMaximumLoginAttempt + "<br/>";
            ViewBag.HtmlStr += "Use <span class='alert-link'> <a href= " + Url.Action(actionName: "ForgotPassword", controllerName: "Account");
            ViewBag.HtmlStr += ">Forgot Password </a></span> to reset your password or try again later.";

            return View(viewName: "Confirmation");
        }

        // user registration approval pending
        // GET: /Account/RegistrationApprovalPending
        [AllowAnonymous]
        public ActionResult RegistrationApprovalPending()
        {
            ViewBag.SubTitle = "Registration Approval Pending";
            ViewBag.message = Core.Resources.Message.RegistrationApprovalPending;

            return View(viewName: "Confirmation");
        }

        // user account is disabled
        // GET: /Account/AccountDisabled
        [AllowAnonymous]
        public ActionResult AccountDisabled()
        {
            ViewBag.SubTitle = "Account Disabled";
            ViewBag.HtmlStr = Core.Resources.Message.UserAccountDisabled + "<br/>";
            // need authority list

            return View(viewName: "Confirmation");
        }

        // TODO: change password will be in same page
        // user password is expired
        // GET: /Account/PasswordExpired
        [AllowAnonymous]
        public ActionResult PasswordExpired()
        {
            ViewBag.SubTitle = "Locked out";
            ViewBag.HtmlStr = Core.Resources.Message.PasswordExpired + "<br/>";
            ViewBag.HtmlStr += "Use <span class='alert-link'> <a href= " + Url.Action(actionName: "ChangePassword", controllerName: "Account");
            ViewBag.HtmlStr += ">Change Password </a></span> to change your password.";

            return View(viewName: "Confirmation");
        }

        #endregion

        #region SignOut
        //
        // POST: /Account/SignOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignOut()
        {
            _authenticateService.SignOff();
            return RedirectToLocal(returnUrl: "");
        }

        #endregion

        #region Helpers

        private void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(key: "", errorMessage: error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(actionName: "Index", controllerName: "Home");
        }

        #endregion
    }
}