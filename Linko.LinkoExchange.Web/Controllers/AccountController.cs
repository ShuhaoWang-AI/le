using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Resources;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mapping;
using Linko.LinkoExchange.Web.Shared;
using Linko.LinkoExchange.Web.ViewModels.Account;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Linko.LinkoExchange.Web.ViewModels.User;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix(prefix:"Account")]
    [Route(template:"{action=Index}")]
    public class AccountController:BaseController
    {
        #region default action

        [AllowAnonymous]
        public ActionResult Index()
        {
            return Request.IsAuthenticated ? RedirectToAction(actionName:"UserProfile", controllerName:"User") : RedirectToAction(actionName:"SignIn");
        }

        #endregion

        #region SignOut

        // POST: /Account/SignOut
        public ActionResult SignOut()
        {
            _authenticationService.SignOff();
            return RedirectToLocal(returnUrl:"");
        }

        #endregion

        #region constructor

        private readonly IAuthenticationService _authenticationService;
        private readonly IOrganizationService _organizationService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly IInvitationService _invitationService;
        private readonly IJurisdictionService _jurisdictionService;
        private readonly ISettingService _settingService;
        private readonly ProfileHelper _profileHelper;
        private readonly IMapHelper _mapHelper; 

        public AccountController(
            IAuthenticationService authenticationService,
            IOrganizationService organizationService,
            IQuestionAnswerService questionAnswerService,
            ILogger logger,
            IUserService userService,
            IInvitationService invitationService,
            IJurisdictionService jurisdictionService,
            ISettingService settingService,
            IMapHelper mapHelper,
            IHttpContextService httpContextService,
            IReportPackageService reportPackageService,
            ISampleService sampleService
        ):base(httpContextService,userService,reportPackageService,sampleService)
        {
            _authenticationService = authenticationService;
            _organizationService = organizationService;
            _questionAnswerService = questionAnswerService;
            _logger = logger;
            _userService = userService;
            _invitationService = invitationService;
            _jurisdictionService = jurisdictionService;
            _settingService = settingService;
            _mapHelper = mapHelper; 
            _profileHelper = new ProfileHelper(questAnswerService:questionAnswerService, userService:userService, jurisdictionService:jurisdictionService,
                                               mapHelper:mapHelper, httpContextService:httpContextService);
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

            var invitation = _invitationService.GetInvitation(invitationId:token);
            if (invitation == null)
            {
                return View(viewName:"Confirmation", model:new ConfirmationViewModel
                                                           {
                                                               Title = "Invitation Expired",
                                                               Message = "The invitation has expired. Please request a new one."
                                                           });
            }

            var model = new RegistrationViewModel
                        {
                            InvitationEmail = invitation.EmailAddress,
                            UserProfile = new UserProfileViewModel {StateList = GetStateList()},
                            UserKBQ = new UserKBQViewModel(),
                            UserSQ = new UserSQViewModel()
                        };
            model.UserKBQ.QuestionPool = _profileHelper.GetQuestionPool(type:QuestionTypeName.KBQ);
            model.UserSQ.QuestionPool = _profileHelper.GetQuestionPool(type:QuestionTypeName.SQ);

            model.ProgramName = invitation.ProgramName;
            model.IndustryName = invitation.IndustryName;
            model.AuthorityName = invitation.AuthorityName;

            model.UserProfile.FirstName = invitation.FirstName;
            model.UserProfile.LastName = invitation.LastName;
            model.UserProfile.Email = invitation.EmailAddress;

            var user = _userService.GetUserProfileByEmail(emailAddress:invitation.EmailAddress);
            if (user == null)
            {
                model.RegistrationType = RegistrationType.NewRegistration;
                model.UserProfile.ShowConfirmPassword = true;
            }
            else if(invitation.IsResetInvitation)
            {
                model.RegistrationType = RegistrationType.ResetRegistration;
                model.UserProfile = _profileHelper.GetUserProfileViewModel(userProfileId:user.UserProfileId);
                model.UserProfile.Password = "";
                model.UserProfile.ShowConfirmPassword = true;
            }
            else
            {
                model.RegistrationType = RegistrationType.ReRegistration;
                model.UserProfile.ShowConfirmPassword = false;

                model.UserProfile = _profileHelper.GetUserProfileViewModel(userProfileId:user.UserProfileId);
                model.UserKBQ = _profileHelper.GetUserKbqViewModel(userProfileId:user.UserProfileId);
                model.UserSQ = _profileHelper.GetUserSecurityQuestionViewModel(userProfileId:user.UserProfileId);

                // For re-registration, set the kbq questions to be **** so that we do not display gully hashed string.
                model.UserKBQ.KBQAnswer1 = "**********";
                model.UserKBQ.KBQAnswer2 = "**********";
                model.UserKBQ.KBQAnswer3 = "**********";
                model.UserKBQ.KBQAnswer4 = "**********";
                model.UserKBQ.KBQAnswer5 = "**********";

                // For re-registration, we don't update anything to the user's profile, put a fake password here only to by pass the data validation. 
                model.UserProfile.Password = "abcdnxtZ1";
                model.AgreeTermsAndConditions = false;
            }

            model.Token = token;

            return View(model:model);
        }

        [AllowAnonymous]
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public async Task<ActionResult> Register(RegistrationViewModel model, FormCollection form)
        {
            var invitation = _invitationService.GetInvitation(invitationId:model.Token);
            ViewBag.newRegistration = true;
            ViewBag.profileCollapsed = Convert.ToString(value:form[name:"profileCollapsed"]);
            ViewBag.kbqCollapsed = Convert.ToString(value:form[name:"kbqCollapsed"]);
            ViewBag.sqCollapsed = Convert.ToString(value:form[name:"sqCollapsed"]);

            model.UserProfile.StateList = GetStateList();
            model.UserKBQ.QuestionPool = _profileHelper.GetQuestionPool(type:QuestionTypeName.KBQ);
            model.UserSQ.QuestionPool = _profileHelper.GetQuestionPool(type:QuestionTypeName.SQ);
         
            if(model.RegistrationType == RegistrationType.ReRegistration)
            {
                ModelState.Clear();
            }

            var kbqs = new List<AnswerDto>();
            var sqs = new List<AnswerDto>();
            kbqs.Add(item:new AnswerDto {QuestionId = model.UserKBQ.KBQ1, Content = model.UserKBQ.KBQAnswer1});
            kbqs.Add(item:new AnswerDto {QuestionId = model.UserKBQ.KBQ2, Content = model.UserKBQ.KBQAnswer2});
            kbqs.Add(item:new AnswerDto {QuestionId = model.UserKBQ.KBQ3, Content = model.UserKBQ.KBQAnswer3});
            kbqs.Add(item:new AnswerDto {QuestionId = model.UserKBQ.KBQ4, Content = model.UserKBQ.KBQAnswer4});
            kbqs.Add(item:new AnswerDto {QuestionId = model.UserKBQ.KBQ5, Content = model.UserKBQ.KBQAnswer5});
            sqs.Add(item:new AnswerDto {QuestionId = model.UserSQ.SecurityQuestion1, Content = model.UserSQ.SecurityQuestionAnswer1});
            sqs.Add(item:new AnswerDto {QuestionId = model.UserSQ.SecurityQuestion2, Content = model.UserSQ.SecurityQuestionAnswer2});
            
            var userDto = _mapHelper.GetUserDtoFromUserProfileViewModel(viewModel:model.UserProfile);
            userDto.Password = model.UserProfile.Password;
            userDto.AgreeTermsAndConditions = model.AgreeTermsAndConditions; 
            
            var kbqValidationResults = _userService.KbqValidation(kbqQuestions:kbqs);
            var sqValidationResults  = _userService.SecurityValidation(securityQuestions:sqs); 
            
            var inValidUserProfileMessages = ValidUserProfileData(userDto, model.RegistrationType); 
            var inValidKbqMessages = ValidateKbq(kbqValidationResults);
            var inValidSqMessages = ValidateSecurityQuestions(sqValidationResults); 

            if (!ModelState.IsValid)
            {
                ViewBag.inValidProfile = false;
                ViewBag.inValidKBQ = false;
                ViewBag.inValidSQ = false;

                model.ProgramName = invitation.ProgramName;
                model.IndustryName = invitation.IndustryName;
                model.AuthorityName = invitation.AuthorityName;
                model.UserProfile.Password = "";
                model.UserProfile.ConfirmPassword = "";

                // Validate Profile
                ValidationContext context;
                var validationResult = new List<ValidationResult>();

                context = new ValidationContext(instance:model.UserProfile, serviceProvider:null, items:null);
                var isValid = Validator.TryValidateObject(instance:model.UserProfile, validationContext:context, validationResults:validationResult, validateAllProperties:true);

                if (!isValid || inValidUserProfileMessages.Any())
                {
                    ViewBag.inValidProfile = true; 
                }

                // Validate KBQ  
                context = new ValidationContext(instance:model.UserKBQ, serviceProvider:null, items:null);
                isValid = Validator.TryValidateObject(instance:model.UserKBQ, validationContext:context, validationResults:validationResult, validateAllProperties:true);

                if (!isValid || inValidKbqMessages.Any())
                {
                    ViewBag.inValidKBQ = true; 
                }

                // Validate SQ 
                context = new ValidationContext(instance:model.UserSQ, serviceProvider:null, items:null);
                isValid = Validator.TryValidateObject(instance:model.UserSQ, validationContext:context, validationResults:validationResult, validateAllProperties:true);

                if (!isValid || inValidSqMessages.Any())
                {
                    ViewBag.inValidSQ = true; 
                }

                ViewBag.inValidKbqMessages = inValidKbqMessages;
                ViewBag.inValidSqMessages = inValidSqMessages;  
                ViewBag.inValidUserProfileMessages = inValidUserProfileMessages; 

                return View(model:model);
            }

            var result = await _authenticationService.Register(userInfo:userDto, registrationToken:model.Token, securityQuestions:sqs, kbqQuestions:kbqs, registrationType:model.RegistrationType);
            switch (result.Result)
            {
                case RegistrationResult.Success:
                    _logger.Info(message:$"Registration successfully completed. Email={userDto.Email}, FirstName={userDto.FirstName}, LastName={userDto.LastName}.");

                    var authorityProgramSettings = _settingService.GetAuthorityProgramSettingsById(orgRegProgramId:invitation.RecipientOrganizationRegulatoryProgramId);
                    var authorityName = authorityProgramSettings.Settings.First(s => s.TemplateName.Equals(obj:SettingType.EmailContactInfoName)).Value;
                    var authorityEmail = authorityProgramSettings.Settings.First(s => s.TemplateName.Equals(obj:SettingType.EmailContactInfoEmailAddress)).Value;
                    var authorityPhone = authorityProgramSettings.Settings.First(s => s.TemplateName.Equals(obj:SettingType.EmailContactInfoPhone)).Value;
                    var org = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:invitation.RecipientOrganizationRegulatoryProgramId);

                    var messageBody = new StringBuilder();
                    messageBody.Append(value:$"<div>Your LinkoExchange registration has been received and is now under review for the following:</div><br/>"); 
                    messageBody.Append(value:$"<div>");

                    messageBody.Append(value: $"<div>");
                    messageBody.Append(value: $"<div class='col-md-1' style='text-align:right;'>Authority:</div>");
                    var authorityNameInMessage = _organizationService.GetAuthority(orgRegProgramId:invitation.RecipientOrganizationRegulatoryProgramId).OrganizationDto.OrganizationName;
                    messageBody.Append(value: $"<div class='col-md-11'>{authorityNameInMessage}</div>");
                    messageBody.Append(value: $"</div><br/>");


                    if (org.OrganizationDto.OrganizationType.Name.ToLower().IsCaseInsensitiveEqual(comparing: OrganizationTypeName.Industry.ToString()))
                    {
                        messageBody.Append(value: $"<div>");
                        messageBody.Append(value: $"<div class='col-md-1' style='text-align:right'>Facility: </div>");
                        messageBody.Append(value: $"<div class='col-md-11'>{org.OrganizationDto.OrganizationName} </div>");
                        messageBody.Append(value: $"</div>");

                        messageBody.Append(value: $"<div>");
                        messageBody.Append(value: $"<div class='col-md-1' style='text-align:right'></div>");
                        messageBody.Append(value: $"<div class='col-md-11'>{org.OrganizationDto.AddressLine1}</div>");
                        messageBody.Append(value: $"</div>");

                        if (!string.IsNullOrWhiteSpace(value: org.OrganizationDto.AddressLine2))
                        {
                            messageBody.Append(value: $"<div>");
                            messageBody.Append(value: $"<div class='col-md-1' style='text-align:right'></div>");
                            messageBody.Append(value: $"<div class='col-md-11'>{org.OrganizationDto.AddressLine2}</div>");
                            messageBody.Append(value: $"</div>");
                        }

                        messageBody.Append(value: $"<div class='col-md-1' style='text-align:right'></div>");
                        if (!string.IsNullOrWhiteSpace(value: org.OrganizationDto.State))
                        {
                            messageBody.Append(value: $"<div class='col-md-11'>{org.OrganizationDto.CityName}, {org.OrganizationDto.State}</div>");
                        }
                        else
                        {
                            messageBody.Append(value: $"<div class='col-md-11'>{org.OrganizationDto.CityName}</div>");
                        }

                        messageBody.Append(value:$"<br/>&nbsp<br/>");
                    }
                    else
                    {
                        messageBody.Append(value: $"");
                        messageBody.Append(value:$"<br/>");
                    }

                    messageBody.Append(value:$"<div>You will be notified by email when a decision has been made about your account request.</div>");
                    messageBody.Append(value:$"<div>If you have questions or concerns, please contact {authorityName} at {authorityEmail} or {authorityPhone}.</div>");
                    
                    messageBody.Append(value:$"</div>"); 


                    return View(viewName:"Confirmation",
                                model:new ConfirmationViewModel
                                      {
                                          Title = $"Thanks for Registering {model.UserProfile.FirstName} {model.UserProfile.LastName}!",
                                          HtmlStr = messageBody.ToString()
                                      });

                case RegistrationResult.BadUserProfileData:
                    ViewBag.inValidProfile = true;
                    inValidUserProfileMessages.Add("Invalid user profile data.");
                    break;
                case RegistrationResult.BadPassword:
                    ViewBag.inValidProfile = true;
                    inValidUserProfileMessages.Add("Password does not meet criteria.");
                    break;
                case RegistrationResult.CanNotUseLastNumberOfPasswords:
                    model.UserProfile.Password = "";
                    model.UserProfile.ConfirmPassword = "";
                    ViewBag.inValidProfile = true;
                    inValidUserProfileMessages.Add(string.Join(separator:" ", values:result.Errors));
                    break;
                case RegistrationResult.DuplicatedKBQ:
                    ViewBag.inValidKBQ = true;
                    inValidKbqMessages.Add("Knowledge based questions cannot be duplicated."); 
                    break;
                case RegistrationResult.DuplicatedKBQAnswer:
                    ViewBag.inValidKBQ = true;
                    inValidKbqMessages.Add("Knowledge based question answers cannot be duplicated."); 
                    break;
                case RegistrationResult.MissingKBQ:
                    ViewBag.inValidKBQ = true;
                    inValidKbqMessages.Add("Not enough knowledge based questions."); 
                    break;
                case RegistrationResult.MissingKBQAnswer:
                    ViewBag.inValidKBQ = true;
                    inValidKbqMessages.Add("Not enough knowledge based question answers."); 
                    break;
                case RegistrationResult.DuplicatedSecurityQuestion:
                    ViewBag.inValidSQ = true;
                    inValidSqMessages.Add("Security questions cannot be duplicated."); 
                    break;
                case RegistrationResult.MissingSecurityQuestion:
                    ViewBag.inValidSQ = true;
                    inValidSqMessages.Add("Not enough security questions."); 
                    break;
                case RegistrationResult.MissingSecurityQuestionAnswer:
                    ViewBag.inValidSQ = true;
                    inValidSqMessages.Add("Not enough security question answers."); 
                    break;
                case RegistrationResult.DuplicatedSecurityQuestionAnswer:
                    ViewBag.inValidSQ = true;
                    inValidSqMessages.Add("Security question answers cannot be duplicated."); 
                    break;
                case RegistrationResult.BadKBQAndAnswer:
                    ViewBag.inValidKBQ = true;
                    inValidKbqMessages.Add("Invalid knowledge based question and answers."); 
                    break;
                case RegistrationResult.UserNameIsUsed:
                    ViewBag.inValidProfile = true;
                    inValidUserProfileMessages.Add("User Name is already in use on another account. Please select a different User Name.");
                    break;
                case RegistrationResult.EmailIsUsed:
                    ViewBag.inValidProfile = true;
                    inValidUserProfileMessages.Add("Email is being used by another person, please change a different one.");
                    break;
            }

            model.AgreeTermsAndConditions = false;

            ViewBag.invalidKbqMessages = inValidSqMessages.Distinct(); 
            ViewBag.inValidSqMessages = inValidSqMessages.Distinct(); 
            ViewBag.inValidUserProfileMessages = inValidUserProfileMessages.Distinct();
            _logger.Info(message:$"Registration failed. Email={userDto.Email}, FirstName={userDto.FirstName}, LastName={userDto.LastName}, Result={result.Result}");
            return View(model:model);
        }
      
        #endregion

        #region sign in action

        // GET: Account/SignIn
        [AllowAnonymous]
        public ActionResult SignIn(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new SignInViewModel();

            //model.UserName = (HttpContext.Request.Cookies["lastSignInName"] != null) ? HttpContext.Request.Cookies.Get(name: "lastSignInName").Value : "";

            if (Request.IsAuthenticated && !string.IsNullOrWhiteSpace(value:returnUrl))
            {
                var portalName = _authenticationService.GetClaimsValue(claimType:CacheKey.PortalName);
                portalName = string.IsNullOrWhiteSpace(value:portalName) ? "" : portalName.Trim().ToLower();
                if (portalName.Equals(value:"authority") || portalName.Equals(value:"industry"))
                {
                    return Redirect(url:returnUrl);
                }
            }

            return View(model:model);
        }

        // POST: Account/SignIn
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(SignInViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model:model);
            }

            try
            {
                var result = _authenticationService.SignInByUserName(userName:model.UserName, password:model.Password, isPersistent:false).Result;

                switch (result.AutehticationResult)
                {
                    case AuthenticationResult.Success:

                        //HttpCookie cookie = new HttpCookie(name: "lastSignInName", value: model.UserName);
                        //cookie.Expires = DateTime.Now.AddMonths(1);
                        //HttpContext.Response.SetCookie(cookie);
                        _logger.Info(message:string.Format(format:"SignIn. User={0} has successfully logged in.", arg0:model.UserName));
                        return RedirectToAction(actionName:"PortalDirector", controllerName:"Account"); // 6.b
                    case AuthenticationResult.PasswordLockedOut: // 2.c
                        _logger.Info(message:string.Format(format:"SignIn. User={0} has been locked out for exceeding the maximum login attempts.", arg0:model.UserName));
                        return RedirectToAction(actionName:"LockedOut", controllerName:"Account");
                    case AuthenticationResult.AccountResetRequired:
                        _logger.Info(message:string.Format(format:"SignIn. User={0} has been reset and requires re-reg.", arg0:model.UserName));
                        TempData[key:"RegulatoryList"] = result.RegulatoryList;
                        return RedirectToAction(actionName:"AccountReset", controllerName:"Account");
                    case AuthenticationResult.UserIsLocked: // 3.a
                        _logger.Info(message:string.Format(format:"SignIn. User={0} has been locked out.", arg0:model.UserName));
                        TempData[key:"RegulatoryList"] = result.RegulatoryList;
                        return RedirectToAction(actionName:"AccountLocked", controllerName:"Account");
                    case AuthenticationResult.UserIsDisabled: // 5.a
                        _logger.Info(message:string.Format(format:"SignIn. User={0} has been disabled.", arg0:model.UserName));
                        TempData[key:"RegulatoryList"] = result.RegulatoryList;
                        return RedirectToAction(actionName:"AccountDisabled", controllerName:"Account");
                    case AuthenticationResult.AccountIsNotAssociated: // 6.a
                        _logger.Info(message:string.Format(format:"SignIn. User={0} is not associated with an active Industry or Authority.", arg0:model.UserName));
                        TempData[key:"RegulatoryList"] = result.RegulatoryList;
                        return RedirectToAction(actionName:"AccountIsNotAssociated", controllerName:"Account");
                    case AuthenticationResult.RegistrationApprovalPending: // 4.a
                        _logger.Info(message:string.Format(format:"SignIn. User={0} registration approval pending.", arg0:model.UserName));
                        TempData[key:"RegulatoryList"] = result.RegulatoryList;
                        return RedirectToAction(actionName:"RegistrationApprovalPending", controllerName:"Account");
                    case AuthenticationResult.PasswordExpired: // 7.a
                        _logger.Info(message:string.Format(format:"SignIn. User={0} password is expired.", arg0:model.UserName));
                        TempData[key:"UserProfileId"] = result.UserProfileId;
                        TempData[key:"OwinUserId"] = result.OwinUserId;
                        return RedirectToAction(actionName:"ResetExpiredPassword", controllerName:"Account");
                    case AuthenticationResult.UserNotFound: // 2.a
                    case AuthenticationResult.InvalidUserNameOrPassword: // 2.b
                    case AuthenticationResult.Failed:
                    default:
                        _logger.Info(message:string.Format(format:"SignIn. Invalid user name or password for user name ={0}.", arg0:model.UserName));
                        ModelState.AddModelError(key:"", errorMessage:Message.InvalidLoginAttempt);
                        break;
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            // If we got this far, something failed, redisplay form
            return View(model:model);
        }

        // Account locked out by Administrator
        // GET: /Account/AccountLocked
        [AllowAnonymous]
        public ActionResult AccountLocked()
        {
            var model = new ConfirmationViewModel();
            model.Title = "Account Locked";
            model.HtmlStr = Message.AccountLocked + "<br/>";

            if (TempData[key:"RegulatoryList"] != null)
            {
                var regulatoryList = TempData[key:"RegulatoryList"] as IEnumerable<AuthorityDto>;

                model.HtmlStr += "<div class=\"table-responsive\">";
                model.HtmlStr += "<table class=\"table no-margin\">";
                model.HtmlStr += "<tbody>";

                if (regulatoryList != null)
                {
                    foreach (var regulator in regulatoryList)
                    {
                        model.HtmlStr += "<tr><td>" + regulator.EmailContactInfoName + "</td><td>" + regulator.EmailContactInfoEmailAddress + "</td><td>" + regulator.EmailContactInfoPhone
                                         + " </td></tr>";
                    }
                }

                model.HtmlStr += "</tbody>";
                model.HtmlStr += "</table>";
                model.HtmlStr += "</div>";
            }
            else if (TempData[key:"Message"] != null)
            {
                model.HtmlStr += TempData[key:"Message"] as string;
            }

            return View(viewName:"Confirmation", model:model);
        }

        // Account reset by Administrator
        // GET: /Account/AccountReset
        [AllowAnonymous]
        public ActionResult AccountReset()
        {
            var model = new ConfirmationViewModel();
            model.Title = "Account Reset";
            model.HtmlStr = Message.AccountReset + "<br/>";

            if (TempData[key:"RegulatoryList"] != null)
            {
                var regulatoryList = TempData[key:"RegulatoryList"] as IEnumerable<AuthorityDto>;

                model.HtmlStr += "<div class=\"table-responsive\">";
                model.HtmlStr += "<table class=\"table no-margin\">";
                model.HtmlStr += "<tbody>";

                if (regulatoryList != null)
                {
                    foreach (var regulator in regulatoryList)
                    {
                        model.HtmlStr += "<tr><td>" + regulator.EmailContactInfoName + "</td><td>" + regulator.EmailContactInfoEmailAddress + "</td><td>" + regulator.EmailContactInfoPhone
                                         + " </td></tr>";
                    }
                }

                model.HtmlStr += "</tbody>";
                model.HtmlStr += "</table>";
                model.HtmlStr += "</div>";
            }
            else if (TempData[key:"Message"] != null)
            {
                model.HtmlStr += TempData[key:"Message"] as string;
            }

            return View(viewName:"Confirmation", model:model);
        }

        // account locked out due to several failure login attempt
        // GET: /Account/LockedOut
        [AllowAnonymous]
        public ActionResult LockedOut()
        {
            var model = new ConfirmationViewModel();
            model.Title = "Password Lockout";
            model.HtmlStr = Message.ExceedMaximumLoginAttempt + "<br/>";
            model.HtmlStr += "Use <span class='alert-link'> <a href= " + Url.Action(actionName:"ForgotPassword", controllerName:"Account");
            model.HtmlStr += ">Forgot Password </a></span> to reset your password or try again later.";

            return View(viewName:"Confirmation", model:model);
        }

        // user registration approval pending
        // GET: /Account/RegistrationApprovalPending
        [AllowAnonymous]
        public ActionResult RegistrationApprovalPending()
        {
            var model = new ConfirmationViewModel();
            model.Title = "Registration Approval Pending";
            model.Message = Message.RegistrationApprovalPending;

            return View(viewName:"Confirmation", model:model);
        }

        // user account is disabled
        // GET: /Account/AccountDisabled
        [AllowAnonymous]
        public ActionResult AccountDisabled()
        {
            var model = new ConfirmationViewModel();
            model.Title = "Account Disabled";
            model.HtmlStr = Message.UserAccountDisabled + "<br/>";

            if (TempData[key:"RegulatoryList"] != null)
            {
                var regulatoryList = TempData[key:"RegulatoryList"] as IEnumerable<AuthorityDto>;

                model.HtmlStr += "<div class=\"table-responsive\">";
                model.HtmlStr += "<table class=\"table no-margin\">";
                model.HtmlStr += "<tbody>";

                if (regulatoryList != null)
                {
                    foreach (var regulator in regulatoryList)
                    {
                        model.HtmlStr += "<tr><td>" + regulator.EmailContactInfoName + "</td><td>" + regulator.EmailContactInfoEmailAddress + "</td><td>" + regulator.EmailContactInfoPhone
                                         + " </td></tr>";
                    }
                }

                model.HtmlStr += "</tbody>";
                model.HtmlStr += "</table>";
                model.HtmlStr += "</div>";
            }

            return View(viewName:"Confirmation", model:model);
        }

        // user account is not associated with an active Industry or Authority.
        // GET: /Account/AccountIsNotAssociated
        [AllowAnonymous]
        public ActionResult AccountIsNotAssociated()
        {
            var model = new ConfirmationViewModel
                        {
                            Title = "No Active Organizations Found",
                            HtmlStr = Message.AccountIsNotAssociated + "<br/>"
                        };

            if (TempData[key:"RegulatoryList"] != null)
            {
                var regulatoryList = TempData[key:"RegulatoryList"] as IEnumerable<AuthorityDto>;

                model.HtmlStr += "<div class=\"table-responsive\">";
                model.HtmlStr += "<table class=\"table no-margin\">";
                model.HtmlStr += "<tbody>";

                if (regulatoryList != null)
                {
                    foreach (var regulator in regulatoryList)
                    {
                        model.HtmlStr += "<tr><td>" + regulator.EmailContactInfoName + "</td><td>" + regulator.EmailContactInfoEmailAddress + "</td><td>" + regulator.EmailContactInfoPhone
                                         + " </td></tr>";
                    }
                }

                model.HtmlStr += "</tbody>";
                model.HtmlStr += "</table>";
                model.HtmlStr += "</div>";
                model.HtmlStr += "</table>";
            }

            return View(viewName:"Confirmation", model:model);
        }

        // TODO: change password will be in same page
        // user password is expired
        // GET: /Account/PasswordExpired
        [AllowAnonymous]
        public ActionResult PasswordExpired()
        {
            var model = new ConfirmationViewModel();
            model.Title = "Password Expired";
            model.HtmlStr = Message.PasswordExpired + "<br/>";
            model.HtmlStr += "Use <span class='alert-link'> <a href= " + Url.Action(actionName:"ChangePassword", controllerName:"Account");
            model.HtmlStr += ">Change Password </a></span> to change your password.";

            return View(viewName:"Confirmation", model:model);
        }

        // show Portal Director
        // GET: /Account/PortalDirector
        public ActionResult PortalDirector()
        {
            var model = new PortalDirectorViewModel();

            var result = _organizationService.GetUserOrganizations();

            var organizationRegulatoryProgramDtos = result as IList<OrganizationRegulatoryProgramDto> ?? result.ToList();
            if (organizationRegulatoryProgramDtos.Count() == 1)
            {
                _authenticationService.SetClaimsForOrgRegProgramSelection(orgRegProgId:organizationRegulatoryProgramDtos.First().OrganizationRegulatoryProgramId);
                return RedirectToAction(actionName:"Index", controllerName:"Home");
            }
            else if (organizationRegulatoryProgramDtos.Count() > 1)
            {
                model.Authorities =
                    organizationRegulatoryProgramDtos
                        .Where(o => o.OrganizationDto.OrganizationType.Name.Equals(value:"Authority"))
                        .Select(
                                o => new SelectListItem
                                     {
                                         Value = o.OrganizationRegulatoryProgramId.ToString(),
                                         Text = o.OrganizationDto.OrganizationName
                                     }
                               ).ToList();

                model.Industries =
                    organizationRegulatoryProgramDtos
                        .Where(o => o.OrganizationDto.OrganizationType.Name.Equals(value:"Industry"))
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

            return View(model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult PortalDirector(string id)
        {
            try
            {
                var organizationRegulatoryProgramId = int.Parse(s:id);
                _authenticationService.SetClaimsForOrgRegProgramSelection(orgRegProgId:organizationRegulatoryProgramId);

                return Json(data:new
                                 {
                                     redirect = true,
                                     newurl = Url.Action(actionName:"Index", controllerName:"Home")
                                 });
            }
            catch (RuleViolationException rve)
            {
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = MvcValidationExtensions.GetViolationMessages(ruleViolationException:rve)
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
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _authenticationService.RequestResetPassword(username:model.UserName);

                    switch (result.Result)
                    {
                        case AuthenticationResult.Success:
                            _logger.Info(message:string.Format(format:"ForgotPassword. successfully sent reset email for User={0}.", arg0:model.UserName));
                            return RedirectToAction(actionName:"ForgotPasswordConfirmation", controllerName:"Account");

                        case AuthenticationResult.UserNotFound:
                        default:
                            _logger.Info(message:string.Format(format:"ForgotPassword. User name ={0} not found.", arg0:model.UserName));
                            ModelState.AddModelError(key:"", errorMessage:Message.UserNameNotFound);
                            break;
                    }
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model:model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            var model = new ConfirmationViewModel();
            model.Title = "Forgot Password Confirmation";
            model.Message = "Please check your email to reset your password.";

            return View(viewName:"Confirmation", model:model);
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
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotUserName(ForgotUserNameViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _authenticationService.RequestUsernameEmail(email:model.EmailAddress);

                    switch (result.Result)
                    {
                        case AuthenticationResult.Success:
                            _logger.Info(message:string.Format(format:"ForgotUserName. Successfully sent reset email for {0}.", arg0:model.EmailAddress));
                            return RedirectToAction(actionName:"ForgotUserNameConfirmation", controllerName:"Account");

                        case AuthenticationResult.UserNotFound:
                        default:
                            _logger.Info(message:string.Format(format:"ForgotUserName. Email address ={0} not found.", arg0:model.EmailAddress));
                            ModelState.AddModelError(key:"", errorMessage:Message.EmailNotFound);
                            break;
                    }
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model:model);
        }

        // GET: /User/ForgotUserNameConfirmation
        [AllowAnonymous]
        public ActionResult ForgotUserNameConfirmation()
        {
            var model = new ConfirmationViewModel();
            model.Title = "Forgot User Name Confirmation";
            model.Message = "Please check your email for your User Name.";

            return View(viewName:"Confirmation", model:model);
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
                return View(viewName:"Error");
            }
            else if (!_authenticationService.CheckPasswordResetUrlNotExpired(token:token))
            {
                var model = new ConfirmationViewModel();
                model.Title = "Password Reset Link Expiry";
                model.HtmlStr = "The password reset link has expired.  Please use <a href=" +  Url.Action(actionName:"ForgotPassword", controllerName:"Account") + ">Forgot Password</a>";

                return View(viewName:"Confirmation", model:model);
            }
            else
            {
                var userQuestion = _questionAnswerService.GetRandomQuestionAnswerFromToken(token:token, questionType:QuestionTypeName.KBQ);

                var model = new ResetPasswordViewModel();
                model.Token = token;
                model.Id = userQuestion.Answer.UserQuestionAnswerId ?? 0;
                model.Question = userQuestion.Question.Content;
                model.Answer = "";
                model.Password = "";
                model.ConfirmPassword = "";
                model.FailedCount = 0;

                ViewBag.PostedToAction = "ResetPassword";
                return View(model:model);
            }
        }

        //
        // POST: /Account/ResetPassword
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model:model);
            }

            var errorMessage = new List<string>(); 

            var result = await _authenticationService.ResetPasswordAsync(token:model.Token, userQuestionAnswerId:model.Id, answer:model.Answer, attempCount:model.FailedCount, password:model.Password);

            switch (result.Result)
            {
                case AuthenticationResult.Success:
                    _logger.Info(message:string.Format(format:"ResetPassword. Password for {0} has been successfully reset.", arg0:model.Token));
                    return RedirectToAction(actionName:"ResetPasswordConfirmation", controllerName:"Account");

                case AuthenticationResult.PasswordRequirementsNotMet:
                    _logger.Info(message:string.Format(format:"ResetPassword. Password Requirements Not Met for Token = {0}.", arg0:model.Token));
                    foreach (var error in result.Errors)
                    {
                       ModelState.AddModelError(key:"", errorMessage:error);
                       errorMessage.Add(error);
                    }

                    break;

                // Cannot Use Old Password
                case AuthenticationResult.CanNotUseOldPassword:
                    _logger.Info(message:string.Format(format:"ResetPassword. Cannot use old password for Token = {0}.", arg0:model.Token));
                    foreach (var error in result.Errors)
                    {
                       ModelState.AddModelError(key:"", errorMessage:error);
                       errorMessage.Add(error);
                    }

                    break;

                // incorrect answer
                case AuthenticationResult.IncorrectAnswerToQuestion:
                    ModelState.Remove(key:"FailedCount"); // if you don't remove then hidden field does not update on post-back 
                    model.FailedCount++;
                    _logger.Info(message:string.Format(format:"ResetPassword. Failed for Token = {0}.", arg0:model.Token));
                    foreach (var error in result.Errors)
                    {
                      ModelState.AddModelError(key:"", errorMessage:error);
                      errorMessage.Add(error);
                    }

                    break;

                // User is got locked
                case AuthenticationResult.UserIsLocked: // 3.a
                    _logger.Info(message:string.Format(format:"ResetPassword. User has been locked out for Token = {0}.", arg0:model.Token));
                    TempData[key:"Message"] = result.Errors;
                    TempData[key:"RegulatoryList"] = result.RegulatoryList;
                    return RedirectToAction(actionName:"AccountLocked", controllerName:"Account");

                // Token expired
                case AuthenticationResult.ExpiredRegistrationToken:
                default:
                    _logger.Info(message:string.Format(format:"ResetPassword. Failed for Token = {0}.", arg0:model.Token));
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(key:"", errorMessage:error);
                        errorMessage.Add(error);
                    }

                    break; 
            }
            
            ViewBag.errorMessage = errorMessage; 
            return View(model:model);
        }

        //
        // GET: /User/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            var model = new ConfirmationViewModel();
            model.Title = "Reset Password Confirmation";
            model.HtmlStr = "Your Password has been successfully reset. Please click <a href= ";
            model.HtmlStr += Url.Action(actionName:"SignIn", controllerName:"Account") + ">here </a> to Sign in.";

            return View(viewName:"Confirmation", model:model);
        }

        #endregion

        #region  Change password and change email address   

        [AllowAnonymous]
        public ActionResult ResetExpiredPassword()
        {
            var userProfileId = int.Parse(s:TempData[key:"UserProfileId"].ToString());
            var userQuestion = _questionAnswerService.GetRandomQuestionAnswerFromUserProfileId(userProfileId:userProfileId, questionType:QuestionTypeName.KBQ);

            var model = new ResetPasswordViewModel
                        {
                            Id = userQuestion.Answer.UserQuestionAnswerId ?? 0,
                            Question = userQuestion.Question.Content,
                            UserProfileId = userProfileId,
                            OwinUserId = TempData[key:"OwinUserId"].ToString(),
                            Answer = "",
                            Password = "",
                            ConfirmPassword = "",
                            FailedCount = 0
                        };

            ViewBag.PostedToAction = "ResetExpiredPassword";
            ViewBag.ReminderMessage = "Your password has expired and a new password must be created.";

            return View(viewName:"ResetPassword", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetExpiredPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"ResetPassword", model:model);
            }

            // check KBQ question
            if (!_questionAnswerService.ConfirmCorrectAnswer(userQuestionAnswerId:model.Id, answer:model.Answer.ToLower()))
            {
                model.FailedCount++;
                var maxAnswerAttempts =
                    Convert.ToInt32(value:
                                    _settingService.GetOrganizationSettingValueByUserId(userProfileId:model.UserProfileId, settingType:SettingType.FailedKBQAttemptMaxCount, isChooseMin:true,
                                                                                        isChooseMax:null));
                if (maxAnswerAttempts <= model.FailedCount)
                {
                    // Lock the account; 
                    var locckAccountResult = _userService.LockUnlockUserAccount(userProfileId:model.UserProfileId, isAttemptingLock:true,
                                                                                reason:AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringPasswordReset);
                    if (locckAccountResult.IsSuccess)
                    {
                        _logger.Info(message:string.Format(format:"KBQ question. Failed to Answer KBQ Question {0} times. Account is locked. UserProfileId:{1}",
                                                           arg0:maxAnswerAttempts, arg1:model.UserProfileId));

                        var regulatoryList = _organizationService.GetUserRegulators(userId:model.UserProfileId) ?? new List<AuthorityDto>();

                        TempData[key:"RegulatoryList"] = regulatoryList;

                        return RedirectToAction(actionName:"AccountLocked", controllerName:"Account");
                    }
                    else
                    {
                        _logger.Info(message:string.Format(format:"KBQ question. Failed to Answer KBQ Question {0} times. Failed to locked the Account. UserProfileId:{1}",
                                                           arg0:maxAnswerAttempts, arg1:model.UserProfileId));
                    }
                }
                else
                {
                    ModelState.Remove(key:"FailedCount");
                    ModelState.AddModelError(key:"", errorMessage:@"Wrong Answer.");
                    ModelState.Remove(key:"Answer");
                    model.Answer = "";
                }

                return View(viewName:"ResetPassword", model:model);
            }

            var result = _authenticationService.ChangePasswordAsync(userId:model.OwinUserId, newPassword:model.Password).Result;
            if (result.Success)
            {
                return RedirectToAction(actionName:"ResetExpiredPasswordConfirmation");
            }

            var errorMessage = result.Errors.Aggregate((i, j) => { return i + j; });
            ModelState.AddModelError(key:string.Empty, errorMessage:errorMessage);
            return View(viewName:"ResetPassword", model:model);
        }
        
        // GET: /Account/ResetExpiredPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetExpiredPasswordConfirmation()
        {
            var model = new ConfirmationViewModel
                        {
                            Title = "Reset Password Confirmation",
                            Message = "Password successfully changed."
                        };

            return View(viewName:"Confirmation", model:model);
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            if (NeedToValidKbq())
            {
                return RedirectToAction(actionName:"KbqChallenge", routeValues:new
                                                                               {
                                                                                   returnUrl = Request.Url?.ToString()
                                                                               });
            }
            else
            {
                var model = new ChangePasswordViewModel();
                return View(model:model);
            }
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model:model);
            }

            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.Claims.First(i => i.Type == CacheKey.OwinUserId).Value;

            var result = _authenticationService.ChangePasswordAsync(userId:userId, newPassword:model.Password).Result;
            if (result.Success)
            {
                TempData["ChangePasswordSucceed"] = true;
                TempData[key:"KbqPass"] = "true";
                return RedirectToAction(actionName: "UserProfile", controllerName: "User");
            }

            var errorMessage = result.Errors.Aggregate((i, j) => i + j);
            ModelState.AddModelError(key:string.Empty, errorMessage:errorMessage);
            return View(model:model);
        }
 
        [Authorize]
        public ActionResult ChangeEmail()
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var email = claimsIdentity?.Claims.First(i => i.Type == CacheKey.Email).Value;

            if (NeedToValidKbq())
            {
                return RedirectToAction(actionName:"KbqChallenge", routeValues:new
                                                                               {
                                                                                   returnUrl = Request.Url?.ToString()
                                                                               });
            }
            else
            {
                var changeEmailViewModel = new ChangeEmailViewModel {OldEmail = email};
                return View(model:changeEmailViewModel);
            }
        }

        [Authorize]
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult ChangeEmail(ChangeEmailViewModel model)
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity?.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(s:profileIdStr ?? "0");

            if (!ModelState.IsValid)
            {
                return View(model:model);
            }

            var userDto = _userService.GetUserProfileById(userProfileId:userProfileId);
            if (userDto == null || userDto.Email != model.OldEmail)
            {
                ModelState.AddModelError(key:string.Empty, errorMessage:@"The email to change is not your email.");
                ViewBag.inValidData = true;
                return View(model:model);
            }

            var result = _userService.UpdateEmail(userProfileId:userProfileId, newEmailAddress:model.NewEmail);
            if (!result)
            {
                ViewBag.inValidData = true;
                ModelState.AddModelError(key:string.Empty, errorMessage:@"Email is already in use on another account.");

                return View(model:model);
            }
            else
            {
                _authenticationService.UpdateClaim(key:CacheKey.Email, value:model.NewEmail);
                
                TempData["ChangeEmailSucceed"] = true;
                TempData[key:"KbqPass"] = "true";
                return RedirectToAction(actionName: "UserProfile", controllerName: "User"); 
            }
        }
         
        public ActionResult KbqChallenge(string returnUrl)
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity?.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(s:profileIdStr ?? "0");

            var kbqChallange = new KbqChallengeViewModel();
            var questionAndAnswer = _questionAnswerService.GetRandomQuestionAnswerFromUserProfileId(userProfileId:userProfileId, questionType:QuestionTypeName.KBQ);

            kbqChallange.Question = questionAndAnswer.Question.Content;
            kbqChallange.QuestionAnswerId = questionAndAnswer.Answer.UserQuestionAnswerId ?? 0;
            ViewBag.returnUrl = returnUrl;

            return View(model:kbqChallange);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult KbqChallenge(KbqChallengeViewModel model, string returnUrl)
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity?.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(s:profileIdStr ?? "0");
            ViewBag.returnUrl = returnUrl;

            if (!_questionAnswerService.ConfirmCorrectAnswer(userQuestionAnswerId:model.QuestionAnswerId, answer:model.Answer.ToLower()))
            {
                model.FailedCount++;
                var maxAnswerAttempts =
                    Convert.ToInt32(value:
                                    _settingService.GetOrganizationSettingValueByUserId(userProfileId:userProfileId, settingType:SettingType.FailedKBQAttemptMaxCount, isChooseMin:true,
                                                                                        isChooseMax:null));
                if (maxAnswerAttempts <= model.FailedCount)
                {
                    // Logout user
                    _authenticationService.SignOff();

                    // Lock the account; 
                    var result = _userService.LockUnlockUserAccount(userProfileId:userProfileId, isAttemptingLock:true, reason:AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringProfileAccess);
                    if (result.IsSuccess)
                    {
                        _logger.Info(message:string.Format(format:"KBQ question. Failed to Answer KBQ Question {0} times. Account is locked. UserProfileId:{1}",
                                                           arg0:maxAnswerAttempts, arg1:userProfileId));

                        var regulatoryList = _organizationService.GetUserRegulators(userId:userProfileId);
                        if (regulatoryList == null)
                        {
                            regulatoryList = new List<AuthorityDto>();
                        }

                        TempData[key:"RegulatoryList"] = regulatoryList;

                        return RedirectToAction(actionName:"AccountLocked", controllerName:"Account");
                    }
                    else
                    {
                        _logger.Info(message:string.Format(format:"KBQ question. Failed to Answer KBQ Question {0} times. Failed to locked the Account. UserProfileId:{1}",
                                                           arg0:maxAnswerAttempts, arg1:userProfileId));
                    }
                }
                else
                {
                    ModelState.Remove(key:"FailedCount");
                    ModelState.AddModelError(key:"", errorMessage:@"Wrong Answer.");
                    ModelState.Remove(key:"Answer");
                    model.Answer = "";
                }

                return View(model:model);
            }
            else
            {
                TempData[key:"KbqPass"] = "true";
                return Redirect(url:returnUrl);
            }
        }

        #endregion

        #region Helpers

        private List<string> ValidUserProfileData(UserDto userInfo, RegistrationType registrationType)
        {
            var invalidMessages = new List<string>();  
            var results = _authenticationService.ValidateUserProfileForRegistration(userInfo, registrationType); 
            foreach(var result in results)
            {
                switch(result)
                {
                    case RegistrationResult.BadUserProfileData:
                        invalidMessages.Add("Invalid user profile data.");
                        break;
                    case RegistrationResult.MissingUserName:
                        invalidMessages.Add("User name is required.");
                        break;
                    case RegistrationResult.MissingEmailAddress:
                        invalidMessages.Add("Email address is required.");
                        break;

                    default:
                        invalidMessages.Add("Invalid user profile data.");
                        break;
                }
            } 

            return invalidMessages;
        }

        private List<string> ValidateKbq(ICollection<RegistrationResult> kbqValidationResults)
        {
            List<string> invalidMessages = new List<string>(); 

            if (kbqValidationResults.Any())
            {
                ViewBag.inValidKBQ = true;  
                foreach (var item in kbqValidationResults)
                {
                    switch (item)
                    {
                        case RegistrationResult.MissingKBQ:
                            //ModelState.AddModelError(key:"", errorMessage:@"Not enough knowledge based questions."); 
                            invalidMessages.Add("Not enough knowledge based questions.");   
                            break;
                        case RegistrationResult.MissingKBQAnswer:
                            //ModelState.AddModelError(key:"", errorMessage:@"Not enough knowledge based question answers.");
                            invalidMessages.Add("Not enough knowledge based question answers.");   
                            break;
                        case RegistrationResult.DuplicatedKBQ:
                           // ModelState.AddModelError(key:"", errorMessage:@"Knowledge based questions cannot be duplicated.");
                            invalidMessages.Add("Knowledge based questions cannot be duplicated.");   
                            break;
                        case RegistrationResult.DuplicatedKBQAnswer:
                           // ModelState.AddModelError(key:"", errorMessage:@"Knowledge based question answers cannot be duplicated.");
                            invalidMessages.Add("Knowledge based question answers cannot be duplicated.");   
                            break;
                    }
                }
            }

            return invalidMessages; 
        }
        private  List<string>  ValidateSecurityQuestions( ICollection<RegistrationResult> sqValidationResults)
        {
            List<string> invalidMessages = new List<string>();
             
            if (sqValidationResults.Any())
            {
                ViewBag.inValidSQ = true; 
                foreach (var item in sqValidationResults)
                {
                    switch (item)
                    {
                        case RegistrationResult.MissingSecurityQuestion:
                          //  ModelState.AddModelError(key:"", errorMessage:@"Not enough security questions.");
                            invalidMessages.Add("Not enough security questions.");  
                            break;
                        case RegistrationResult.MissingSecurityQuestionAnswer:
                          //  ModelState.AddModelError(key:"", errorMessage:@"Not enough security question answers.");
                            invalidMessages.Add("Not enough security question answers.");   
                            break;
                        case RegistrationResult.DuplicatedSecurityQuestion:
                        //    ModelState.AddModelError(key:"", errorMessage:@"Security questions cannot be duplicated.");
                            invalidMessages.Add("Security questions cannot be duplicated.");   
                            break;
                        case RegistrationResult.DuplicatedSecurityQuestionAnswer:
                        //    ModelState.AddModelError(key:"", errorMessage:@"Security question answers cannot be duplicated.");
                            invalidMessages.Add("Security question answers cannot be duplicated.");   
                            break;
                    }
                }
            }

            return invalidMessages;
        }
        
        private bool NeedToValidKbq()
        {
            var kbqPass = TempData[key:"KbqPass"] as string;
            var previousUri = HttpContext.Request.UrlReferrer;
            return (previousUri == null || previousUri.AbsolutePath.ToLower().IndexOf(value:"user/profile", comparisonType:StringComparison.Ordinal) < 0) &&
                   (string.IsNullOrWhiteSpace(value:kbqPass) || kbqPass != "true");
        }

        private void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(key:"", errorMessage:error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(url:returnUrl))
            {
                return Redirect(url:returnUrl);
            }

            return RedirectToAction(actionName:"Index", controllerName:"Home");
        }

        private List<JurisdictionViewModel> GetStateList()
        {
            var list = _jurisdictionService.GetStateProvs(countryId:(int) Country.USA);

            return list.Select(jur => new JurisdictionViewModel
                                      {
                                          JurisdictionId = jur.JurisdictionId,
                                          StateName = jur.Name
                                      }).ToList();
        }

        #endregion
    }
}