using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.ViewModels.Account;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using NLog;
using Linko.LinkoExchange.Core.Extensions;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix("Account")]
    [Route("{action=Index}")]
    public class AccountController : Controller
    {
        #region constructor

        private readonly IAuthenticationService _authenticateService;
        private readonly IOrganizationService _organizationService;
        private readonly IRequestCache _requestCache;
        private readonly ILogger _logger;
        //private readonly ICurrentUser _currentUser;

        public AccountController(IAuthenticationService authenticateService, IOrganizationService organizationService, 
            IRequestCache requestCache, ILogger logger, ICurrentUser currentUser)
        {
            _authenticateService = authenticateService;
            _organizationService = organizationService;
            _requestCache = requestCache;
            _logger = logger;
            //_currentUser = currentUser;
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
        public async Task<ActionResult> SignIn(SignInViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authenticateService.SignInByUserName(model.UserName, model.Password, false);

                switch (result.AutehticationResult)
                {
                    case AuthenticationResult.Success:
                        HttpCookie cookie = new HttpCookie(name: "lastSignInName", value: model.UserName);
                        HttpContext.Response.SetCookie(cookie);
                        _logger.Info(string.Format(format: "SignIn. User={0} has successfully logged in.", arg0: model.UserName));
                        return RedirectToAction(actionName: "PortalDirector", controllerName: "Account");
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
                        _logger.Info(string.Format(format: "SignIn. Invalid user name or password for user name ={0}.", arg0: model.UserName));
                        ModelState.AddModelError(key: "", errorMessage: Core.Resources.Message.InvalidLoginAttempt);
                        return View(model);
                }

            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // Account locked out by Administrator
        // GET: /Account/AccountLocked
        [AllowAnonymous]
        public ActionResult AccountLocked()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Account Locked";
            model.HtmlStr = Core.Resources.Message.AccountLocked + "<br/>";
            // need authority list

            return View(viewName: "Confirmation", model: model);
        }

        // account locked out due to several failure login attempt
        // GET: /Account/LockedOut
        [AllowAnonymous]
        public ActionResult LockedOut()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Locked out";
            model.HtmlStr = Core.Resources.Message.ExceedMaximumLoginAttempt + "<br/>";
            model.HtmlStr += "Use <span class='alert-link'> <a href= " + Url.Action(actionName: "ForgotPassword", controllerName: "Account");
            model.HtmlStr += ">Forgot Password </a></span> to reset your password or try again later.";

            return View(viewName: "Confirmation", model: model);
        }

        // user registration approval pending
        // GET: /Account/RegistrationApprovalPending
        [AllowAnonymous]
        public ActionResult RegistrationApprovalPending()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Registration Approval Pending";
            model.Message = Core.Resources.Message.RegistrationApprovalPending;

            return View(viewName: "Confirmation", model: model);
        }

        // user account is disabled
        // GET: /Account/AccountDisabled
        [AllowAnonymous]
        public ActionResult AccountDisabled()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Account Disabled";
            model.HtmlStr = Core.Resources.Message.UserAccountDisabled + "<br/>";
            // need authority list

            return View(viewName: "Confirmation", model: model);
        }

        // TODO: change password will be in same page
        // user password is expired
        // GET: /Account/PasswordExpired
        [AllowAnonymous]
        public ActionResult PasswordExpired()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Locked out";
            model.HtmlStr = Core.Resources.Message.PasswordExpired + "<br/>";
            model.HtmlStr += "Use <span class='alert-link'> <a href= " + Url.Action(actionName: "ChangePassword", controllerName: "Account");
            model.HtmlStr += ">Change Password </a></span> to change your password.";

            return View(viewName: "Confirmation", model: model);
        }
        
        // show Portal Director
        // GET: /Account/PortalDirector
        public ActionResult PortalDirector()
        {
            PortalDirectorViewModel model = new PortalDirectorViewModel();
            //PortalDirectorViewModel model = new PortalDirectorViewModel();
            string currentUserId = "1"; //_currentUser.GetClaimsValue(CacheKey.UserProfileId);
            string userFullName = HttpContext.User.Identity.UserFullName();

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var result = _organizationService.GetUserOrganizations(Convert.ToInt32(currentUserId));

                if (result.Count() == 1)
                {
                    // TODO: call service to set current OrganizationRegulatoryProgramId
                    RedirectToAction(actionName: "Index", controllerName: "Home");
                }
                else if (result.Count() > 1)
                {
                    model.Authorities =
                        result
                        .Where(o => o.OrganizationDto.OrganizationType.Name.Equals(value: "Authority"))
                        .Select(
                            o => new SelectListItem
                            {
                                Value = o.OrganizationRegulatoryProgramId.ToString(),
                                Text = o.OrganizationDto.OrganizationName
                            }
                        ).ToList();

                    model.Industries =
                        result
                        .Where(o => o.OrganizationDto.OrganizationType.Name.Equals(value: "Industry"))
                        .Select(
                            o => new SelectListItem
                            {
                                Value = o.OrganizationRegulatoryProgramId.ToString(), 
                                Text = o.OrganizationDto.OrganizationName
                            }
                        ).ToList();
                }
                else
                {
                    // user has no access and should be catch by SignIn action 
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult PortalDirector(string id)
        {
            try
            {
                int OrganizationRegulatoryProgramId = int.Parse(id);
                // TODO: call service to set current OrganizationRegulatoryProgramId

                return Json(new
                {
                    redirect = true,
                    newurl = Url.Action(actionName: "Index", controllerName: "Home")
                });
            }
            catch (RuleViolationException rve)
            {
                return Json(new
                {
                    redirect = false,
                    message = MvcValidationExtensions.GetViolationMessages(rve)
                });
            }
        }
        #endregion

        #region forgot password action

        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _authenticateService.RequestResetPassword(model.UserName); // TODO: update with correct service

                    switch (result.Result)
                    {
                        case AuthenticationResult.Success:
                            _logger.Info(string.Format(format: "ForgotPassword. successfully sent reset email for User={0}.", arg0: model.UserName));
                            return RedirectToAction(actionName: "ForgotPasswordConfirmation", controllerName: "Account");

                        case AuthenticationResult.UserNotFound:
                        case AuthenticationResult.Failed:
                        default:
                            _logger.Info(string.Format(format: "ForgotPassword. User name ={0} not found.", arg0: model.UserName));
                            ModelState.AddModelError(key: "", errorMessage: Core.Resources.Message.UserNameNotFound);
                            return View(model);
                    }
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Forgot password confirmation";
            model.Message = "Please check your email to reset your password.";

            return View(viewName: "Confirmation", model: model);
        }
        #endregion

        #region forgot user name action

        // GET: /Account/ForgotUserName
        [AllowAnonymous]
        public ActionResult ForgotUserName()
        {
            return View();
        }

        // POST: /Account/ForgotUserName
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotUserName(ForgotUserNameViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _authenticateService.RequestResetPassword(model.EmailAddress); // TODO: update with correct service

                    switch (result.Result)
                    {
                        case AuthenticationResult.Success:
                            _logger.Info(string.Format(format: "ForgotUserName. successfully sent reset email for {0}.", arg0: model.EmailAddress));
                            return RedirectToAction(actionName: "ForgotUserNameConfirmation", controllerName: "Account");

                        case AuthenticationResult.UserNotFound:
                        case AuthenticationResult.Failed:
                        default:
                            _logger.Info(string.Format(format: "ForgotUserName. Email address ={0} not found.", arg0: model.EmailAddress));
                            ModelState.AddModelError(key: "", errorMessage: Core.Resources.Message.EmailNotFound);
                            return View(model);
                    }
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /User/ForgotUserNameConfirmation
        [AllowAnonymous]
        public ActionResult ForgotUserNameConfirmation()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Forgot user name confirmation";
            model.Message = "Please check your email for your User Name.";

            return View(viewName: "Confirmation", model: model);
        }
        #endregion

        #region Reset Password

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string token)
        {
            if (token == null)
            {
               return View(viewName: "Error");
            }
            else
            {
                ResetPasswordViewModel model = new ResetPasswordViewModel();
                model.Token = token;
                model.Id = 1; // TODO: call service to set UserQuestionAnswerId as Id
                model.Question = "test question?";
                model.Answer = "";
                model.Password = "";
                model.ConfirmPassword = "";
                model.FailedCount = 0;

                return View(model);
            }
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //TODO: Call proper service and replace following code accordingly... 
            //AuthenticationResultDto ResetPassword (model.Token, UserQuestionAnswerId:  model.Id, model.answer, model.FailedCount, model.Password);
            var resultFromService = await _authenticateService.ResetPasswordAsync(model.Token, model.Id, model.Answer, model.FailedCount, model.Password);

            resultFromService.Errors = new List<string> { "You cannot use the last <?> passwords." };
            resultFromService.Result = AuthenticationResult.CanNotUseOldPassword;

            var result = resultFromService;

            switch (result.Result)
            {
                case AuthenticationResult.Success:
                    _logger.Info(string.Format(format: "ResetPassword. Password for {0} has been successfully reset.", arg0: model.Token));
                    return RedirectToAction(actionName: "ResetPasswordConfirmation", controllerName: "Account");

                // Can Not Use Old Password
                case AuthenticationResult.CanNotUseOldPassword:
                    _logger.Info(string.Format(format: "ResetPassword. Can not use old password for Token = {0}.", arg0: model.Token));
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(key: "", errorMessage: error);
                    }
                    return View(model);

                // incorrect answer
                    //model.FailedCount++;

                // User is got locked
                // Token invalid
                // Token expired
                case AuthenticationResult.Failed:
                default:
                    _logger.Info(string.Format(format: "ResetPassword. Failed for Token = {0}.", arg0: model.Token));
                    ModelState.AddModelError(key: "", errorMessage: ""); // should come from service as ErrorMessage
                    return View(model);
            }
        }

        //
        // GET: /User/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Reset password confirmation";
            model.HtmlStr = "Your password has been successfully reset. Please click <a href= ";
            model.HtmlStr += Url.Action(actionName: "SignIn", controllerName: "Account") + ">here </a> to Sign in.";

            return View(viewName: "Confirmation", model: model);
        }


        #endregion

        #region SignOut
        //
        // POST: /Account/SignOut
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