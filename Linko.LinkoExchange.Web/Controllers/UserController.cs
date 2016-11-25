using System;
using System.Linq;
using System.Web.Mvc;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.User;
using AutoMapper;
using Linko.LinkoExchange.Web.ViewModels.User;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Core.Enum;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Web.Routing;
using System.Web.Hosting;
using System.IO;
using Linko.LinkoExchange.Web.shared;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthenticationService _authenticateService;
        private readonly ISessionCache _sessionCache;
        private readonly IUserService _userService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IJurisdictionService _jurisdictionService;
        private readonly string fakePassword = "********";
        private readonly IMapper _mapper;
        private readonly ProfileHelper profileHelper;
        public UserController(
            IAuthenticationService authenticateService,
            IQuestionAnswerService questAnswerService,
            ISessionCache sessionCache,
            IUserService userService,
            IJurisdictionService jurisdictionService,
            IMapper mapper)
        {
            if (authenticateService == null) throw new ArgumentNullException("authenticateService");
            if (sessionCache == null) throw new ArgumentNullException("sessionCache");
            if (questAnswerService == null) throw new ArgumentNullException("questAnswerService");

            _authenticateService = authenticateService;
            _sessionCache = sessionCache;
            _userService = userService;
            _jurisdictionService = jurisdictionService;
            _mapper = mapper;
            _questionAnswerService = questAnswerService;

            profileHelper = new ProfileHelper(questAnswerService, sessionCache, userService, jurisdictionService, mapper);
        }

        // GET: UserDto
        public ActionResult Index()
        {
            // TODO: to test get claims 
            //var claims = _authenticationService.GetClaims();
            //var organization = claims?.FirstOrDefault(i => i.Type == "OrganizationName");
            //if (organization != null)
            //{
            //    ViewBag.organizationName = organization.Value;
            //}

            return View();
        }
        
        public ActionResult DownloadSignatory()
        {
            var file = HostingEnvironment.MapPath("~/Temp/GRESD Electronic Signature Agreement.pdf");
            var fileDownloadName = "GRESD Electronic Signature Agreement.pdf";
            var contentType = "application/pdf"; 
            var fileStream = new MemoryStream(); 
            fileStream.Position = 0; 
            return File(fileStream, contentType, fileDownloadName);
        }

        [Authorize]
        public ActionResult RequestSignatory()
        {
            return View();
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Get)]
        public new ActionResult Profile()
        {
            ViewBag.profileCollapsed = false;
            ViewBag.kbqCollapsed = true;
            ViewBag.sqCollapsed = true;
            ViewBag.newRegistration = false;

            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(profileIdStr);

            var userProfileViewModel = profileHelper.GetUserProfileViewModel(userProfileId);
            var userSQViewModel = profileHelper.GetUserSecurityQuestionViewModel(userProfileId);
            var userKbqViewModel = profileHelper.GetUserKbqViewModel(userProfileId);

            //set the fake passsword, just make sure data validataion pass
            userProfileViewModel.Password = "Tiger12345";

            var user = new UserViewModel
            {
                UserKBQ = userKbqViewModel,
                UserProfile = userProfileViewModel,
                UserSQ = userSQViewModel
            };

            ViewBag.userKBQ = userKbqViewModel;
            ViewBag.userProfile = userProfileViewModel;
            ViewBag.userSQ = userSQViewModel;

            return View(user);
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(UserViewModel model, string part, FormCollection form)
        {
            ViewBag.inValidProfile = false;
            ViewBag.inValidKBQ = false;
            ViewBag.inValidSQ = false;

            ViewBag.profileCollapsed = Convert.ToString(form["profileCollapsed"]);
            ViewBag.kbqCollapsed = Convert.ToString(form["kbqCollapsed"]);
            ViewBag.sqCollapsed = Convert.ToString(form["sqCollapsed"]);

            string portalName = _authenticateService.GetClaimsValue(CacheKey.PortalName);
            portalName = string.IsNullOrWhiteSpace(portalName) ? "" : portalName.Trim().ToLower();
            if (portalName.Equals(value: "authority"))
            {
                ViewBag.industryPortal = false;
            }
            else
            {
                ViewBag.industryPortal = true;
            }

            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(profileIdStr);

            var pristineUser = profileHelper.GetUserViewModel(userProfileId);
            pristineUser.UserProfile.StateList = profileHelper.GetStateList();

            if (part == "Profile")
            {
                return SaveUserProfile(model, pristineUser, userProfileId);
            }
            else if (part == "KBQ")
            {
                return SaveUserKbq(model, pristineUser, userProfileId);
            }
            else if (part == "SQ")
            {
                return SaveUserSQ(model, pristineUser, userProfileId);
            }

            return View(pristineUser);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RouteData.Values["action"].ToString().ToLower() == "profile" 
                && Request.HttpMethod != "POST")
            {
                var kbqPass = TempData["KbqPass"] as string;
                if (!string.IsNullOrWhiteSpace(kbqPass) &&
                     kbqPass.ToLower() == "true")
                {
                    base.OnActionExecuting(filterContext);
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary {
                            { "action", "KbqChallenge" },
                            { "controller", "Account" },
                            { "returnUrl", filterContext.HttpContext.Request.Url }
                        }
                    );
                }
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }

        private ActionResult SaveUserProfile(UserViewModel model, UserViewModel pristineUserModel, int userProfileId)
        {
            ValidationContext context = null;
            var validationResult = new List<ValidationResult>();
            bool isValid = true; 
            
            context = new ValidationContext(model.UserProfile, serviceProvider: null, items: null);
            isValid = Validator.TryValidateObject(model.UserProfile, context, validationResult, validateAllProperties: true); 

            if (!isValid)
            {
                ViewBag.inValidProfile = true;
                return View(pristineUserModel);
            }

            var userDto = _mapper.Map<UserDto>(model.UserProfile);
            userDto.UserProfileId = userProfileId; 

            var validateResult = _userService.ValidateUserProfileData(userDto);
            if (validateResult == RegistrationResult.Success)
            {
                _userService.UpdateProfile(userDto);
                ViewBag.SaveProfileSuccessfull = true;
                ViewBag.SuccessMessage = String.Format(format: "Save Profile successfully.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, errorMessage: "User profile data is not correct.");
                ViewBag.inValidKBQ = true;
            }

            return View(pristineUserModel);
        }

        private ActionResult SaveUserKbq(UserViewModel model, UserViewModel pristineUserModel, int userProfileId)
        {
            pristineUserModel.UserKBQ.QuestionPool = profileHelper.GetQuestionPool(QuestionTypeName.KBQ);

            ValidationContext context = null;
            var validationResult = new List<ValidationResult>();
            bool isValid = true; 

            context = new ValidationContext(model.UserKBQ, serviceProvider: null, items: null);
            isValid = Validator.TryValidateObject(model.UserKBQ, context, validationResult, validateAllProperties: true);
            
            if (!isValid)
            {
                ViewBag.inValidKBQ = true;
                return View(pristineUserModel);
            }

            pristineUserModel.UserKBQ.UserProfileId = userProfileId;
            var kbqQuestionAnswers = GetPostedUserKbqQuestions(model.UserKBQ); 
            var validateResult = _questionAnswerService.ValidateUserKbqData(kbqQuestionAnswers);
            switch (validateResult)
            {
                case RegistrationResult.Success:
                    _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId, kbqQuestionAnswers); 
                    ViewBag.SaveKBQSuccessfull = true;
                    ViewBag.SuccessMessage = String.Format(format: "Save Knowledge Based Questions successfully.");
                    break;
                case RegistrationResult.DuplicatedKBQ:
                    ModelState.AddModelError(string.Empty, errorMessage: "Duplicated Knowledage Based Questions");
                    ViewBag.inValidKBQ = true;
                    break;
                case RegistrationResult.DuplicatedKBQAnswer:
                    ModelState.AddModelError(string.Empty, errorMessage: "Duplicated Knowledage Based Question Answers");
                    ViewBag.inValidKBQ = true;
                    break;
                case RegistrationResult.MissingKBQ:
                    ModelState.AddModelError(string.Empty, errorMessage: "Missing Knowledage Based Questions");
                    ViewBag.inValidKBQ = true;
                    break;
                case RegistrationResult.MissingKBQAnswer:
                    ModelState.AddModelError(string.Empty, errorMessage: "Missing Knowledage Based Question Answers");
                    ViewBag.inValidKBQ = true;
                    break;
            }  

            return View(pristineUserModel);
        }

        private ActionResult SaveUserSQ(UserViewModel model, UserViewModel pristineUserModel, int userProfileId)
        {
            pristineUserModel.UserSQ.QuestionPool = profileHelper.GetQuestionPool(QuestionTypeName.SQ);

            ValidationContext context = null;
            var validationResult = new List<ValidationResult>();
            bool isValid = true;

            context = new ValidationContext(model.UserSQ, serviceProvider: null, items: null);
            isValid = Validator.TryValidateObject(model.UserSQ, context, validationResult, validateAllProperties: true);

            if (!isValid)
            {
                ViewBag.inValidSQ = true;
                return View(pristineUserModel);
            } 

            pristineUserModel.UserSQ.UserProfileId = userProfileId;

            var sqQuestionAnswers = GetPostedUserSQQuestionAnswers(model.UserSQ);
            var result = _questionAnswerService.ValidateUserSqData(sqQuestionAnswers);
            switch (result)
            {
                case RegistrationResult.Success:
                    _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId, sqQuestionAnswers); 
                    ViewBag.SaveSQSuccessfull = true;
                    ViewBag.SuccessMessage = String.Format(format: "Save Security Questions successfully.");
                    break;
                case RegistrationResult.DuplicatedSecurityQuestion:
                    ModelState.AddModelError(string.Empty, errorMessage: "Duplicated Security Questions");
                    ViewBag.inValidSQ = true;
                    break;
                case RegistrationResult.DuplicatedSecurityQuestionAnswer:
                    ModelState.AddModelError(string.Empty, errorMessage: "Duplicated Security Question Answers");
                    ViewBag.inValidSQ = true;
                    break;

                case RegistrationResult.MissingSecurityQuestion:
                    ModelState.AddModelError(string.Empty, errorMessage: "Missing Security Questions");
                    ViewBag.inValidSQ = true;
                    break;
                case RegistrationResult.MissingSecurityQuestionAnswer:
                    ModelState.AddModelError(string.Empty, errorMessage: "Missing Security Question Answers");
                    ViewBag.inValidSQ = true;
                    break;
            }

            return View(pristineUserModel);
        }

        private List<AnswerDto> GetPostedUserSQQuestionAnswers(UserSQViewModel model)
        {
            var sqQuestionAnswers = new List<AnswerDto>();
            sqQuestionAnswers.Add(new AnswerDto
            {
                QuestionId = model.SecurityQuestion1,
                Content = model.SecurityQuestionAnswer1,
                UserQuestionAnswerId = model.UserQuestionAnserId_SQ1
            });

            sqQuestionAnswers.Add(new AnswerDto
            {
                QuestionId = model.SecurityQuestion2,
                Content = model.SecurityQuestionAnswer2,
                UserQuestionAnswerId = model.UserQuestionAnserId_SQ2
            });

            return sqQuestionAnswers;
        }

        private List<AnswerDto> GetPostedUserKbqQuestions(UserKBQViewModel model)
        {
            var kbqQuestionAnswers = new List<AnswerDto>();
            kbqQuestionAnswers.AddRange(
                new[] {
                new AnswerDto
                {
                    QuestionId = model.KBQ1,
                    Content = model.KBQAnswer1,
                    UserQuestionAnswerId = model.UserQuestionAnserId_KBQ1
                },
                new AnswerDto
                {
                    QuestionId = model.KBQ2,
                    Content = model.KBQAnswer2,
                    UserQuestionAnswerId = model.UserQuestionAnserId_KBQ2
                },
                new AnswerDto
                {
                    QuestionId = model.KBQ3,
                    Content = model.KBQAnswer3,
                    UserQuestionAnswerId = model.UserQuestionAnserId_KBQ3
                },
                new AnswerDto
                {
                    QuestionId = model.KBQ4,
                    Content = model.KBQAnswer4,
                    UserQuestionAnswerId = model.UserQuestionAnserId_KBQ4
                },
                new AnswerDto
                {
                    QuestionId = model.KBQ5,
                    Content = model.KBQAnswer5,
                    UserQuestionAnswerId = model.UserQuestionAnserId_KBQ5
                 }

                }
            );

            return kbqQuestionAnswers;
        }

       
    }
}