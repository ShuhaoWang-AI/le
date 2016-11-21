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

        [Authorize] 
        [AcceptVerbs(HttpVerbs.Get)]
        public new ActionResult Profile()
        { 
            ViewBag.profileCollapsed = false;
            ViewBag.kbqCollapsed = true;
            ViewBag.sqCollapsed = true;

            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(profileIdStr);

            var userProfileViewModel = GetUserProfileViewModel(userProfileId);
            var userSQViewModel = GetUserSecurityQuestionViewModel(userProfileId);
            var userKbqViewModel = GetUserKbqViewModel(userProfileId);

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

            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(profileIdStr);

            var pristineUser = GetUserViewModel(userProfileId);
            pristineUser.UserProfile.StateList = GetStateList();

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
            if (filterContext.RouteData.Values["action"].ToString().ToLower() == "profile")
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
                            { "returnUrl", "Profile" }
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
            pristineUserModel.UserKBQ.QuestionPool = GetQuestionPool(QuestionTypeName.KBQ);

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
            pristineUserModel.UserSQ.QuestionPool = GetQuestionPool(QuestionTypeName.SQ);

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
                QuestionId = model.SecuritryQuestion1,
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

        private UserViewModel GetUserViewModel(int userProfileId)
        {
            var userProfileViewModel = GetUserProfileViewModel(userProfileId);
            var userSQViewModel = GetUserSecurityQuestionViewModel(userProfileId);
            var userKbqViewModel = GetUserKbqViewModel(userProfileId);

            return new UserViewModel
            {
                UserKBQ = userKbqViewModel,
                UserProfile = userProfileViewModel,
                UserSQ = userSQViewModel
            };  
        }

        private UserProfileViewModel GetUserProfileViewModel(int userProfileId)
        {

            var userProileDto = _userService.GetUserProfileById(userProfileId);
            var userProfileViewModel = _mapper.Map<UserProfileViewModel>(userProileDto);

            // set password to be stars 
            userProfileViewModel.Password = fakePassword;

            // Get state list   
            userProfileViewModel.StateList = GetStateList();
            userProfileViewModel.Role = _sessionCache.GetClaimValue(CacheKey.UserRole);  

            return userProfileViewModel;
        }

        private UserKBQViewModel GetUserKbqViewModel(int userProfileId)
        {

            var userKbqViewModel = new UserKBQViewModel();
            userKbqViewModel.UserProfileId = userProfileId;   

            var kbqQuestions = _questionAnswerService.GetUsersQuestionAnswers(userProfileId, QuestionTypeName.KBQ);
          
            var kbqs = kbqQuestions.Select(i => _mapper.Map<QuestionAnswerPairViewModel>(i)).ToList();

            userKbqViewModel.QuestionPool = GetQuestionPool(QuestionTypeName.KBQ);

            ////  KBQ questions 
            userKbqViewModel.KBQ1 = kbqs[0].Question.QuestionId.Value;
            userKbqViewModel.KBQ2 = kbqs[1].Question.QuestionId.Value;
            userKbqViewModel.KBQ3 = kbqs[2].Question.QuestionId.Value;
            userKbqViewModel.KBQ4 = kbqs[3].Question.QuestionId.Value;
            userKbqViewModel.KBQ5 = kbqs[4].Question.QuestionId.Value;


            userKbqViewModel.KBQAnswer1 = kbqs[0].Answer.Content;
            userKbqViewModel.KBQAnswer2 = kbqs[1].Answer.Content;
            userKbqViewModel.KBQAnswer3 = kbqs[2].Answer.Content;
            userKbqViewModel.KBQAnswer4 = kbqs[3].Answer.Content;
            userKbqViewModel.KBQAnswer5 = kbqs[4].Answer.Content;

            //// keep track UserQuestionAnswerId
            userKbqViewModel.UserQuestionAnserId_KBQ1 = kbqs[0].Answer.UserQuestionAnswerId.Value;
            userKbqViewModel.UserQuestionAnserId_KBQ2 = kbqs[1].Answer.UserQuestionAnswerId.Value;
            userKbqViewModel.UserQuestionAnserId_KBQ3 = kbqs[2].Answer.UserQuestionAnswerId.Value;
            userKbqViewModel.UserQuestionAnserId_KBQ4 = kbqs[3].Answer.UserQuestionAnswerId.Value;
            userKbqViewModel.UserQuestionAnserId_KBQ5 = kbqs[4].Answer.UserQuestionAnswerId.Value;

            return userKbqViewModel;
        }

        private UserSQViewModel GetUserSecurityQuestionViewModel(int userProfileId)
        {
            var userSQViewModel = new UserSQViewModel();
            userSQViewModel.UserProfileId = userProfileId;     
            
            var securityQeustions = _questionAnswerService.GetUsersQuestionAnswers(userProfileId, QuestionTypeName.SQ);
            var sqs = securityQeustions.Select(i => _mapper.Map<QuestionAnswerPairViewModel>(i)).ToList();

            userSQViewModel.QuestionPool = GetQuestionPool(QuestionTypeName.SQ);

            ////  Security questions 
            userSQViewModel.SecuritryQuestion1 = sqs[0].Question.QuestionId.Value;
            userSQViewModel.SecurityQuestion2 = sqs[1].Question.QuestionId.Value;

            userSQViewModel.SecurityQuestionAnswer2 = sqs[1].Answer.Content;
            userSQViewModel.SecurityQuestionAnswer1 = sqs[0].Answer.Content;

            //// Keep track UserQuestionAnswerId 
            userSQViewModel.UserQuestionAnserId_SQ1 = sqs[0].Answer.UserQuestionAnswerId.Value;
            userSQViewModel.UserQuestionAnserId_SQ2 = sqs[1].Answer.UserQuestionAnswerId.Value; 
          
            return userSQViewModel;
        }

        private List<QuestionViewModel> GetQuestionPool(QuestionTypeName type)
        {
            return _questionAnswerService.GetQuestions().Select(i => _mapper.Map<QuestionViewModel>(i)).ToList()
                .Where(i => i.QuestionType == type).ToList();
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
    }
}