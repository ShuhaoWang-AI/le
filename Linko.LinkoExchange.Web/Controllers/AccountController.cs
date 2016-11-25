using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.ViewModels.Account;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Linko.LinkoExchange.Web.ViewModels.User;
using NLog;
using Linko.LinkoExchange.Services.Settings;
using System;
using System.ComponentModel.DataAnnotations;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Web.shared;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix("Account")]
    [Route("{action=Index}")] 
    public class AccountController : Controller
    {
        #region constructor

        private readonly IAuthenticationService _authenticationService;
        private readonly IOrganizationService _organizationService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IRequestCache _requestCache;
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly IInvitationService _invitationService;
        private readonly IJurisdictionService _jurisdictionService;
        private readonly ISettingService _settingService;
        private readonly IProgramService _programService;
        private readonly IMapper _mapper;
        private readonly ISessionCache _sessionCache;
        private readonly ProfileHelper profileHelper; 

        public AccountController(

            IAuthenticationService authenticationService, 
            IOrganizationService organizationService,
            IQuestionAnswerService questionAnswerService, 
            IRequestCache requestCache, 
            ILogger logger, 
            IUserService userService,
            IInvitationService invitationService,
            IJurisdictionService jurisdictionService, 
            ISettingService settingService,
            IProgramService programService,
            ISessionCache sessionCache,
            IMapper mapper)
        {
            _authenticationService = authenticationService;
            _organizationService = organizationService;
            _questionAnswerService = questionAnswerService;
            _requestCache = requestCache;
            _logger = logger;
            _userService = userService;
            _invitationService = invitationService;
            _jurisdictionService = jurisdictionService;
            _settingService = settingService;
            _programService = programService;
            _sessionCache = sessionCache;
            _mapper = mapper;
            profileHelper = new ProfileHelper(questionAnswerService, sessionCache, userService, jurisdictionService, mapper);
        }

        #endregion

        #region Register
        [AllowAnonymous]
        public ActionResult Register(string token)
        {
            ViewBag.newRegistration = true;
            ViewBag.profileCollapsed = false;
            ViewBag.kbqCollapsed = false;
            ViewBag.sqCollapsed = false;

            var invitation = _invitationService.GetInvitation(token);
            if (invitation == null)
            {
                ModelState.AddModelError("Invitation", "Invalid invitation link.");
                return View("Error");
            }

            var model = new RegistrationViewModel();
            model.Email = invitation.EmailAddress; 
            model.UserProfile = new UserProfileViewModel();
            model.UserProfile.StateList = GetStateList();
            model.UserKBQ = new UserKBQViewModel();
            model.UserSQ = new UserSQViewModel();
            model.UserKBQ.QuestionPool = profileHelper.GetQuestionPool(QuestionTypeName.KBQ);
            model.UserSQ.QuestionPool = profileHelper.GetQuestionPool(QuestionTypeName.SQ);

            model.ProgramName = invitation.ProgramName;
            model.IndustryName = invitation.IndustryName;
            model.AuthorityName = invitation.AuthorityName; 
             
            model.UserProfile.FirstName = invitation.FirstName;
            model.UserProfile.LastName = invitation.LastName;
            model.UserProfile.Email = invitation.EmailAddress;
 
            var user = _userService.GetUserProfileByEmail(invitation.EmailAddress);
            if (user == null)
            {
                model.RegistrationType = RegistrationType.NewRegistration;
            }
            else if (user.IsAccountResetRequired)
            {
                model.RegistrationType = RegistrationType.ResetRegistration;
            }
            else
            {
                model.RegistrationType = RegistrationType.ReRegistration; 
                
                //TODO fill out the kbq questions and security questions 
                model.UserProfile = profileHelper.GetUserProfileViewModel(user.UserProfileId);
                model.UserKBQ = profileHelper.GetUserKbqViewModel(user.UserProfileId);
                model.UserSQ  = profileHelper.GetUserSecurityQuestionViewModel(user.UserProfileId);
                // For re-registration, set the kbq questions to be **** so that we do not display guly hashed string.
                model.UserKBQ.KBQAnswer1 = "**********";
                model.UserKBQ.KBQAnswer2 = "**********";
                model.UserKBQ.KBQAnswer3 = "**********";
                model.UserKBQ.KBQAnswer4 = "**********";
                model.UserKBQ.KBQAnswer5 = "**********"; 

                // For Registration, we don't update anything to the user's profile, put a fake password here only to by pass the data validation. 
                model.UserProfile.Password = "FakePassowrd!001";
                model.AgreeTermsAndConditions = true; 
            }

            model.Token = token;

            return View(model);
        }

        [AllowAnonymous]
        [AcceptVerbs(HttpVerbs.Post)]
        async public Task<ActionResult> Register(RegistrationViewModel model, FormCollection form)
        {
            ViewBag.newRegistration = true;
            ViewBag.profileCollapsed = Convert.ToString(form["profileCollapsed"]);
            ViewBag.kbqCollapsed = Convert.ToString(form["kbqCollapsed"]);
            ViewBag.sqCollapsed = Convert.ToString(form["sqCollapsed"]);

            model.UserProfile.StateList = GetStateList();
            model.UserKBQ.QuestionPool = profileHelper.GetQuestionPool(QuestionTypeName.KBQ);
            model.UserSQ.QuestionPool = profileHelper.GetQuestionPool(QuestionTypeName.SQ);
            if (!ModelState.IsValid)
            {
                ViewBag.inValidProfile = false;
                ViewBag.inValidKBQ = false;
                ViewBag.inValidSQ = false;

                // Validate Profile
                ValidationContext context = null;
                var validationResult = new List<ValidationResult>();
                bool isValid = true;

                context = new ValidationContext(model.UserProfile, serviceProvider: null, items: null);
                isValid = Validator.TryValidateObject(model.UserProfile, context, validationResult, validateAllProperties: true);

                if (!isValid)
                {
                    ViewBag.inValidProfile = true;
                    return View(model);
                }

                // Validate KBQ  
                context = new ValidationContext(model.UserKBQ, serviceProvider: null, items: null);
                isValid = Validator.TryValidateObject(model.UserKBQ, context, validationResult, validateAllProperties: true);

                if (!isValid)
                {
                    ViewBag.inValidKBQ = true;
                    return View(model);
                }

                // Validate SQ 
                context = new ValidationContext(model.UserSQ, serviceProvider: null, items: null);
                isValid = Validator.TryValidateObject(model.UserSQ, context, validationResult, validateAllProperties: true);

                if (!isValid)
                {
                    ViewBag.inValidSQ = true;
                    return View(model);
                }

                return View(model);
            }

            UserDto userDto = _mapper.Map<UserProfileViewModel, UserDto>(model.UserProfile);
            userDto.Password = model.UserProfile.Password;
            userDto.AgreeTermsAndConditions = model.AgreeTermsAndConditions;

            var kbqs = new List<AnswerDto>();
            var sqs = new List<AnswerDto>();
            kbqs.Add(new AnswerDto() { QuestionId = model.UserKBQ.KBQ1, Content = model.UserKBQ.KBQAnswer1 });
            kbqs.Add(new AnswerDto() { QuestionId = model.UserKBQ.KBQ2, Content = model.UserKBQ.KBQAnswer2 });
            kbqs.Add(new AnswerDto() { QuestionId = model.UserKBQ.KBQ3, Content = model.UserKBQ.KBQAnswer3 });
            kbqs.Add(new AnswerDto() { QuestionId = model.UserKBQ.KBQ4, Content = model.UserKBQ.KBQAnswer4 });
            kbqs.Add(new AnswerDto() { QuestionId = model.UserKBQ.KBQ5, Content = model.UserKBQ.KBQAnswer5 });
            sqs.Add(new AnswerDto() { QuestionId = model.UserSQ.SecurityQuestion1, Content = model.UserSQ.SecurityQuestionAnswer1 });
            sqs.Add(new AnswerDto() { QuestionId = model.UserSQ.SecurityQuestion2, Content = model.UserSQ.SecurityQuestionAnswer2 });

            var result = await _authenticationService.Register(userDto, model.Token, sqs, kbqs, model.RegistrationType);
            switch (result.Result)
            {
                case RegistrationResult.Success:
                    _logger.Info($"Registration successfully completed. Email={userDto.Email}, FirstName={userDto.FirstName}, LastName={userDto.LastName}.");
                    return View(viewName: "Confirmation",
                        model: new ConfirmationViewModel() { Title = "Registration Completed", Message = "Thank you for completing registration." }); 

                case RegistrationResult.BadUserProfileData:
                   
                    ViewBag.inValidProfile = true;
                    ModelState.AddModelError(key: "", errorMessage: "Invalid user profile data.");
                    break;
                case RegistrationResult.BadPassword:
                    ViewBag.inValidProfile = true;
                    ModelState.AddModelError(key: "", errorMessage: "Password does not meet criteria.");
                    break;
                case RegistrationResult.CanNotUseLastNumberOfPasswords:
                    ViewBag.inValidProfile = true;
                    ModelState.AddModelError(key: "", errorMessage: String.Join(separator: " ", values: result.Errors));
                    break;
                case RegistrationResult.DuplicatedKBQ:
                    ViewBag.inValidKBQ = true;
                    ModelState.AddModelError(key: "", errorMessage: "Knowledge based questions can not be duplicated.");
                    break;
                case RegistrationResult.DuplicatedKBQAnswer:
                    ViewBag.inValidKBQ = true;
                    ModelState.AddModelError(key: "", errorMessage: "Knowledge based question answers can not be duplicated.");
                    break;
                case RegistrationResult.MissingKBQ:
                    ViewBag.inValidKBQ = true;
                    ModelState.AddModelError(key: "", errorMessage: "Not enough knowledge based questions.");
                    break;
                case RegistrationResult.DuplicatedSecurityQuestion:
                    ViewBag.inValidSQ = true;
                    ModelState.AddModelError(key: "", errorMessage: "Security questions can not be duplicated.");
                    break;
                case RegistrationResult.MissingSecurityQuestion:
                    ViewBag.inValidSQ = true;
                    ModelState.AddModelError(key: "", errorMessage: "Not enough security questions.");
                    break; 
                case RegistrationResult.DuplicatedSecurityQuestionAnswer:
                    ViewBag.inValidSQ = true;
                    ModelState.AddModelError(key: "", errorMessage: "Security question answers can not be duplicated.");
                    break; 
                case RegistrationResult.BadKBQAndAnswer:
                    ViewBag.inValidKBQ = true;
                    ModelState.AddModelError(key: "", errorMessage: "Invalid knowledge based question and answers.");
                    break;
                default:
                    break; 
            }

             _logger.Info($"Registration failed. Email={userDto.Email}, FirstName={userDto.FirstName}, LastName={userDto.LastName}, Result={result.Result.ToString()}");
            return View(model);
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
        [AcceptVerbs(HttpVerbs.Post)]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(SignInViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = _authenticationService.SignInByUserName(model.UserName, model.Password, isPersistent: false).Result;

                switch (result.AutehticationResult)
                {
                    case AuthenticationResult.Success:
                        HttpCookie cookie = new HttpCookie(name: "lastSignInName", value: model.UserName);
                        HttpContext.Response.SetCookie(cookie);
                        _logger.Info(string.Format(format: "SignIn. User={0} has successfully logged in.", arg0: model.UserName));
                        return RedirectToAction(actionName: "PortalDirector", controllerName: "Account");       // 6.b
                    case AuthenticationResult.PasswordLockedOut:            // 2.c
                        _logger.Info(string.Format(format: "SignIn. User={0} has been locked out for exceeding the maximum login attempts.", arg0: model.UserName));
                        return RedirectToAction(actionName: "LockedOut", controllerName: "Account");
                    case AuthenticationResult.UserIsLocked:                 // 3.a
                        _logger.Info(string.Format(format: "SignIn. User={0} has been locked out.", arg0: model.UserName));
                        TempData["RegulatoryList"] = result.RegulatoryList;
                        return RedirectToAction(actionName: "AccountLocked", controllerName: "Account");
                    case AuthenticationResult.UserIsDisabled:               // 5.a
                        _logger.Info(string.Format(format: "SignIn. User={0} has been disabled.", arg0: model.UserName));
                        TempData["RegulatoryList"] = result.RegulatoryList;
                        return RedirectToAction(actionName: "AccountDisabled", controllerName: "Account");
                    case AuthenticationResult.AccountIsNotAssociated:               // 6.a
                        _logger.Info(string.Format(format: "SignIn. User={0} is not associated with an active Industry or Authority.", arg0: model.UserName));
                        TempData["RegulatoryList"] = result.RegulatoryList;
                        return RedirectToAction(actionName: "AccountIsNotAssociated", controllerName: "Account");
                    case AuthenticationResult.RegistrationApprovalPending:  // 4.a
                        _logger.Info(string.Format(format: "SignIn. User={0} registration approval pending.", arg0: model.UserName));
                        return RedirectToAction(actionName: "RegistrationApprovalPending", controllerName: "Account");
                    case AuthenticationResult.PasswordExpired:              // 7.a
                        _logger.Info(string.Format(format: "SignIn. User={0} password is expired.", arg0: model.UserName));
                        return RedirectToAction(actionName: "PasswordExpired", controllerName: "Account");
                    case AuthenticationResult.UserNotFound:                 // 2.a
                    case AuthenticationResult.InvalidUserNameOrPassword:    // 2.b
                    case AuthenticationResult.Failed:
                    default:
                        _logger.Info(string.Format(format: "SignIn. Invalid user name or password for user name ={0}.", arg0: model.UserName));
                        ModelState.AddModelError(key: "", errorMessage: Core.Resources.Message.InvalidLoginAttempt);
                        break;
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

            if (TempData["RegulatoryList"] != null)
            {
                var regulatoryList = TempData["RegulatoryList"] as IEnumerable<AuthorityDto>;

                model.HtmlStr += "<div class=\"table-responsive\">";
                model.HtmlStr += "<table class=\"table no-margin\">";
                model.HtmlStr += "<tbody>";

                foreach (AuthorityDto regulator in regulatoryList)
                {
                    model.HtmlStr += "<tr><td>" + regulator.EmailContactInfoName + "</td><td>" + regulator.EmailContactInfoEmailAddress + "</td><td>" + regulator.EmailContactInfoPhone + " </td></tr>";
                }

                model.HtmlStr += "</tbody>";
                model.HtmlStr += "</table>";
                model.HtmlStr += "</div>";
            }
            else if (TempData["Message"] != null)
            {
                model.HtmlStr += TempData["Message"] as string;
            }

            return View(viewName: "Confirmation", model: model);
        }

        // account locked out due to several failure login attempt
        // GET: /Account/LockedOut
        [AllowAnonymous]
        public ActionResult LockedOut()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Locked Out";
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

            if (TempData["RegulatoryList"] != null)
            {
                var regulatoryList = TempData["RegulatoryList"] as IEnumerable<AuthorityDto>;

                model.HtmlStr += "<div class=\"table-responsive\">";
                model.HtmlStr += "<table class=\"table no-margin\">";
                model.HtmlStr += "<tbody>";

                foreach (AuthorityDto regulator in regulatoryList)
                {
                    model.HtmlStr += "<tr><td>" + regulator.EmailContactInfoName + "</td><td>" + regulator.EmailContactInfoEmailAddress + "</td><td>" + regulator.EmailContactInfoPhone + " </td></tr>";
                }

                model.HtmlStr += "</tbody>";
                model.HtmlStr += "</table>";
                model.HtmlStr += "</div>";
            }

            return View(viewName: "Confirmation", model: model);
        }


        // user account is not associated with an active Industry or Authority.
        // GET: /Account/AccountIsNotAssociated
        [AllowAnonymous]
        public ActionResult AccountIsNotAssociated()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Account Is Not Associated";
            model.HtmlStr = Core.Resources.Message.AccountIsNotAssociated + "<br/>";

            if (TempData["RegulatoryList"] != null)
            {
                var regulatoryList = TempData["RegulatoryList"] as IEnumerable<AuthorityDto>;

                model.HtmlStr += "<div class=\"table-responsive\">";
                model.HtmlStr += "<table class=\"table no-margin\">";
                model.HtmlStr += "<tbody>";

                foreach (AuthorityDto regulator in regulatoryList)
                {
                    model.HtmlStr += "<tr><td>" + regulator.EmailContactInfoName + "</td><td>" + regulator.EmailContactInfoEmailAddress + "</td><td>" + regulator.EmailContactInfoPhone + " </td></tr>";
                }

                model.HtmlStr += "</tbody>";
                model.HtmlStr += "</table>";
                model.HtmlStr += "</div>";
                model.HtmlStr += "</table>";
            }

            return View(viewName: "Confirmation", model: model);
        }

        // TODO: change password will be in same page
        // user password is expired
        // GET: /Account/PasswordExpired
        [AllowAnonymous]
        public ActionResult PasswordExpired()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "Password Expired";
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

            var result = _organizationService.GetUserOrganizations();

            if (result.Count() == 1)
            {
                _authenticationService.SetClaimsForOrgRegProgramSelection(result.First().OrganizationRegulatoryProgramId);
                return RedirectToAction(actionName: "Index", controllerName: "Home");
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

            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult PortalDirector(string id)
        {
            try
            {
                int organizationRegulatoryProgramId = int.Parse(id);
                _authenticationService.SetClaimsForOrgRegProgramSelection(organizationRegulatoryProgramId);

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
        [AcceptVerbs(HttpVerbs.Post)]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _authenticationService.RequestResetPassword(model.UserName);

                    switch (result.Result)
                    {
                        case AuthenticationResult.Success:
                            _logger.Info(string.Format(format: "ForgotPassword. successfully sent reset email for User={0}.", arg0: model.UserName));
                            return RedirectToAction(actionName: "ForgotPasswordConfirmation", controllerName: "Account");

                        case AuthenticationResult.UserNotFound:
                        default:
                            _logger.Info(string.Format(format: "ForgotPassword. User name ={0} not found.", arg0: model.UserName));
                            ModelState.AddModelError(key: "", errorMessage: Core.Resources.Message.UserNameNotFound);
                            break;
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
        [AcceptVerbs(HttpVerbs.Post)]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotUserName(ForgotUserNameViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _authenticationService.RequestUsernameEmail(model.EmailAddress);

                    switch (result.Result)
                    {
                        case AuthenticationResult.Success:
                            _logger.Info(string.Format(format: "ForgotUserName. Successfully sent reset email for {0}.", arg0: model.EmailAddress));
                            return RedirectToAction(actionName: "ForgotUserNameConfirmation", controllerName: "Account");

                        case AuthenticationResult.UserNotFound:
                        default:
                            _logger.Info(string.Format(format: "ForgotUserName. Email address ={0} not found.", arg0: model.EmailAddress));
                            ModelState.AddModelError(key: "", errorMessage: Core.Resources.Message.EmailNotFound);
                            break;
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
                var userQuestion = _questionAnswerService.GetRandomQuestionAnswerFromToken(token, QuestionTypeName.KBQ);

                ResetPasswordViewModel model = new ResetPasswordViewModel();
                model.Token = token;
                model.Id = userQuestion.Answer.UserQuestionAnswerId.Value;
                model.Question = userQuestion.Question.Content;
                model.Answer = "";
                model.Password = "";
                model.ConfirmPassword = "";
                model.FailedCount = 0;

                return View(model);
            }
        } 

        //
        // POST: /Account/ResetPassword
        [AcceptVerbs(HttpVerbs.Post)]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authenticationService.ResetPasswordAsync(model.Token, model.Id, model.Answer, model.FailedCount, model.Password);

            switch (result.Result)
            {
                case AuthenticationResult.Success:
                    _logger.Info(string.Format(format: "ResetPassword. Password for {0} has been successfully reset.", arg0: model.Token));
                    return RedirectToAction(actionName: "ResetPasswordConfirmation", controllerName: "Account");

                case AuthenticationResult.PasswordRequirementsNotMet:
                    _logger.Info(string.Format(format: "ResetPassword. Password Requirements Not Met for Token = {0}.", arg0: model.Token));
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(key: "", errorMessage: error);
                    }
                    return View(model);

                // Can Not Use Old Password
                case AuthenticationResult.CanNotUseOldPassword:
                    _logger.Info(string.Format(format: "ResetPassword. Can not use old password for Token = {0}.", arg0: model.Token));
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(key: "", errorMessage: error);
                    }
                    return View(model);

                // incorrect answer
                case AuthenticationResult.IncorrectAnswerToQuestion:
                    ModelState.Remove(key: "FailedCount"); // if you don't remove then hidden field does not update on post-back 
                    model.FailedCount++;
                    _logger.Info(string.Format(format: "ResetPassword. Failed for Token = {0}.", arg0: model.Token));
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(key: "", errorMessage: error);
                    }
                    return View(model);

                // User is got locked
                case AuthenticationResult.UserIsLocked:                 // 3.a
                    _logger.Info(string.Format(format: "ResetPassword. User has been locked out for Token = {0}.", arg0: model.Token));
                    TempData["Message"] = result.Errors;
                    return RedirectToAction(actionName: "AccountLocked", controllerName: "Account");

                // Token expired
                case AuthenticationResult.ExpiredRegistrationToken:
                default:
                    _logger.Info(string.Format(format: "ResetPassword. Failed for Token = {0}.", arg0: model.Token));
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(key: "", errorMessage: error);
                    }
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

        #region  Change password and change email address   

        [Authorize]
        public ActionResult ChangeAccountSucceed()
        {
            ViewBag.SuccessMessage = TempData["Message"];
            ViewBag.SubTitle = TempData["SubTitle"];
            TempData["SubTitle"] = "Change Email";
            TempData["Message"] = "Change email address succeeded.";

            return View();
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;

            if (NeedToValidKbq())
            {
                return RedirectToAction("KbqChallenge", new { returnUrl = Request.Url.ToString() });
            }
            else
            {
                var model = new ChangePasswordViewModel(); 
                return View(model);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.Claims.First(i => i.Type == CacheKey.OwinUserId).Value;

            var result = _authenticationService.ChangePasswordAsync(userId, model.Password).Result;
            if (result.Success)
            {
                TempData["SubTitle"] = "Change Password";
                TempData["Message"] = "Change password succeeded.";
                return RedirectToAction(actionName: "ChangeAccountSucceed");
            }
            var errorMessage = result.Errors.Aggregate((i, j) => { return i + j; }); 
            ModelState.AddModelError(string.Empty, errorMessage: errorMessage);
            return View(model); 
        }

        [Authorize]
        public ActionResult ChangeEmail()
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.Claims.First(i => i.Type == CacheKey.OwinUserId).Value;
            var email = claimsIdentity.Claims.First(i => i.Type == CacheKey.Email).Value; 
          
            if (NeedToValidKbq())
            {
                return RedirectToAction("KbqChallenge", new { returnUrl = Request.Url.ToString() });
            }
            else
            {
                var changeEmailViewModel = new ChangeEmailViewModel();
                changeEmailViewModel.OldEmail = email;
                return View(changeEmailViewModel);
            }
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangeEmail(ChangeEmailViewModel model)
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(profileIdStr);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userDto = _userService.GetUserProfileById(userProfileId);
            if (userDto == null ||
                userDto != null && userDto.UserProfileId != userProfileId)
            {
                ModelState.AddModelError(string.Empty, errorMessage: "The email to change is not your email.");
                ViewBag.inValidData = true;
                return View(model);
            }

            var result = _userService.UpdateEmail(userProfileId, model.NewEmail);
            if (!result)
            {
                ViewBag.inValidData = true;
                ModelState.AddModelError(string.Empty, errorMessage: "Change email address failed."); 

                return View(model);
            }
            else
            {
                _authenticationService.UpdateClaim(CacheKey.Email, model.NewEmail);

                TempData["SubTitle"] = "Change Email";
                TempData["Message"] = "Change email address succeeded.";
                return RedirectToAction(actionName: "ChangeAccountSucceed");
            }
            
        }

        public ActionResult KbqChallenge(string returnUrl)
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(profileIdStr);

            KbqChallengeViewModel kbqChallange = new KbqChallengeViewModel(); 
            var questionAndAnswer = _questionAnswerService.GetRandomQuestionAnswerFromUserProfileId(userProfileId, QuestionTypeName.KBQ); 

            kbqChallange.Question = questionAndAnswer.Question.Content;
            kbqChallange.QuestionAnswerId = questionAndAnswer.Answer.UserQuestionAnswerId.Value;
            ViewBag.returnUrl = returnUrl; 

            return View(kbqChallange);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult KbqChallenge(KbqChallengeViewModel model, string returnUrl)
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(profileIdStr);
            ViewBag.returnUrl = returnUrl; 

            if (!_questionAnswerService.ConfirmCorrectAnswer(model.QuestionAnswerId, model.Answer.ToLower()))
            {
                model.FailedCount++; 
                int maxAnswerAttempts = Convert.ToInt32(_settingService.GetOrganizationSettingValueByUserId(userProfileId, SettingType.FailedKBQAttemptMaxCount, true, null));
                if (maxAnswerAttempts <= model.FailedCount)
                {
                    // Logout user
                    _authenticationService.SignOff(); 

                    // Lock the account; 
                    var result = _userService.LockUnlockUserAccount(userProfileId, true, true);
                    if (result.IsSuccess)
                    {
                        _logger.Info(string.Format(format: "KBQ question. Failed to Answer KBQ Question {0} times. Account is locked. UserProfileId:{1}",
                                     arg0: maxAnswerAttempts, arg1:userProfileId));

                        var regulatoryList = _organizationService.GetUserRegulators(userProfileId);
                        if (regulatoryList == null)
                        {
                            regulatoryList = new List<AuthorityDto>();
                        }

                        TempData["RegulatoryList"] = regulatoryList;

                        return RedirectToAction(actionName: "AccountLocked", controllerName: "Account");
                    }
                    else
                    {
                        _logger.Info(string.Format(format: "KBQ question. Failed to Answer KBQ Question {0} times. Failed to locked the Account. UserProfileId:{1}",
                                    arg0: maxAnswerAttempts, arg1: userProfileId));
                    } 
                }
                else
                {
                    ModelState.Remove(key: "FailedCount");
                    ModelState.AddModelError(key: "", errorMessage: "Wrong Answer."); 
                }
                return View(model);
            }
            else
            {
                TempData["KbqPass"] = "true";
                return Redirect(returnUrl);
            }
        }

        #endregion

        #region SignOut
        //
        // POST: /Account/SignOut
        public ActionResult SignOut()
        {
            _authenticationService.SignOff();
            return RedirectToLocal(returnUrl: "");
        }

        #endregion

        #region Helpers

        private bool NeedToValidKbq()
        {
            var kbqPass = TempData["KbqPass"] as string;
            var previousUri = HttpContext.Request.UrlReferrer;
            return ( previousUri == null ||  previousUri.AbsolutePath.ToLower().IndexOf("user/profile") < 0 ) &&
                   (string.IsNullOrWhiteSpace(kbqPass) || kbqPass != "true");
        }

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

        private List<JurisdictionViewModel> GetStateList()
        {
            var list = _jurisdictionService.GetStateProvs((int)(Country.USA));
            var stateList = new List<JurisdictionViewModel>();
            foreach (var jur in list)
            {
                stateList.Add(new JurisdictionViewModel
                {
                    JurisdictionId = jur.JurisdictionId,
                    StateName = jur.Name
                });
            };

            return stateList;
        }
        //private List<QuestionViewModel> GetQuestionPool(QuestionTypeName type)
        //{
        //    return _questionAnswerService.GetQuestions().Select(i => _mapper.Map<QuestionViewModel>(i)).ToList()
        //        .Where(i => i.QuestionType == type).ToList();
        //}         
        #endregion
    }
}