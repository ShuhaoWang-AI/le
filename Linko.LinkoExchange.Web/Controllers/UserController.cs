using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Mapping;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.Shared;
using Linko.LinkoExchange.Web.ViewModels.User;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix(prefix:"User")]
    public class UserController : BaseController
    {
        #region fields

        private readonly IAuthenticationService _authenticateService;
        private readonly IMapHelper _mapHelper;
        private readonly ProfileHelper _profileHelper;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IUserService _userService;

        #endregion

        #region constructors and destructor

        public UserController(
            IAuthenticationService authenticateService,
            IQuestionAnswerService questAnswerService,
            IUserService userService,
            IJurisdictionService jurisdictionService,
            IMapHelper mapHelper,
            IHttpContextService httpContextService,
            IReportPackageService reportPackageService,
            ISampleService sampleService
        )
            : base(httpContextService:httpContextService, userService:userService, reportPackageService:reportPackageService, sampleService:sampleService)
        {
            if (authenticateService == null)
            {
                throw new ArgumentNullException(paramName:nameof(authenticateService));
            }
            if (questAnswerService == null)
            {
                throw new ArgumentNullException(paramName:nameof(questAnswerService));
            }

            _authenticateService = authenticateService;
            _userService = userService;
            _questionAnswerService = questAnswerService;
            _mapHelper = mapHelper;

            _profileHelper = new ProfileHelper(questAnswerService:questAnswerService, userService:userService, jurisdictionService:jurisdictionService, mapHelper:mapHelper,
                                               httpContextService:httpContextService);
        }

        #endregion

        // GET: UserDto
        public ActionResult Index()
        {
            return RedirectToAction(actionName:"UserProfile", controllerName:"User");
        }

        [PortalAuthorize("industry")]
        public ActionResult DownloadSignatory()
        {
            var file = HostingEnvironment.MapPath(virtualPath:"~/Temp/GRESD Electronic Signature Agreement.pdf");
            var fileDownloadName = "GRESD Electronic Signature Agreement.pdf";
            var contentType = "application/pdf";

            var fileStream = System.IO.File.OpenRead(path:file);
            fileStream.Position = 0;
            return File(fileStream:fileStream, contentType:contentType, fileDownloadName:fileDownloadName);
        }

        [PortalAuthorize("industry")]
        public ActionResult RequestSignatory()
        {
            return View();
        }

        [PortalAuthorize("authority", "industry")]
        [AcceptVerbs(verbs:HttpVerbs.Get)]
        [Route(template:"Profile")]
        public ActionResult UserProfile()
        {
            ViewBag.profileCollapsed = false;
            ViewBag.kbqCollapsed = true;
            ViewBag.sqCollapsed = true;
            ViewBag.newRegistration = false;

            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity?.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(s:profileIdStr ?? "0");

            var userProfileViewModel = _profileHelper.GetUserProfileViewModel(userProfileId:userProfileId);
            var userSqViewModel = _profileHelper.GetUserSecurityQuestionViewModel(userProfileId:userProfileId);
            var userKbqViewModel = _profileHelper.GetUserKbqViewModel(userProfileId:userProfileId);

            //set the fake password, just make sure data validation pass
            userProfileViewModel.Password = "Tiger12345";

            var user = new UserViewModel
                       {
                           UserKBQ = userKbqViewModel,
                           UserProfile = userProfileViewModel,
                           UserSQ = userSqViewModel
                       };

            ViewBag.userKBQ = userKbqViewModel;
            ViewBag.userProfile = userProfileViewModel;
            ViewBag.userSQ = userSqViewModel;
            ViewBag.changeEmailSucceed = TempData[key:"ChangeEmailSucceed"];
            ViewBag.changePasswordSucceed = TempData[key:"ChangePasswordSucceed"];
            return View(model:user);
        }

        [PortalAuthorize("authority", "industry")]
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Profile")]
        public ActionResult UserProfile(UserViewModel model, string part, FormCollection form)
        {
            ViewBag.inValidProfile = false;
            ViewBag.inValidKBQ = false;
            ViewBag.inValidSQ = false;

            ViewBag.profileCollapsed = Convert.ToString(value:form[name:"profileCollapsed"]);
            ViewBag.kbqCollapsed = Convert.ToString(value:form[name:"kbqCollapsed"]);
            ViewBag.sqCollapsed = Convert.ToString(value:form[name:"sqCollapsed"]);

            var portalName = _authenticateService.GetClaimsValue(claimType:CacheKey.PortalName);
            portalName = string.IsNullOrWhiteSpace(value:portalName) ? "" : portalName.Trim().ToLower();
            ViewBag.industryPortal = !portalName.Equals(value:"authority");

            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity?.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(s:profileIdStr ?? "0");

            var pristineUser = _profileHelper.GetUserViewModel(userProfileId:userProfileId);
            pristineUser.UserProfile.StateList = _profileHelper.GetStateList();

            if (part == "Profile")
            {
                return SaveUserProfile(model:model, pristineUserModel:pristineUser, userProfileId:userProfileId);
            }
            else if (part == "KBQ")
            {
                return SaveUserKbq(model:model, pristineUserModel:pristineUser, userProfileId:userProfileId);
            }
            else if (part == "SQ")
            {
                return SaveUserSq(model:model, pristineUserModel:pristineUser, userProfileId:userProfileId);
            }

            return View(model:pristineUser);
        }

        [PortalAuthorize("authority", "industry")]
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [Route(template:"Profile/UpdateOneKbq")]
        public JsonResult UpdateOneKbq(KBQViewModel kbqViewModel)
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var profileIdStr = claimsIdentity?.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(s:profileIdStr ?? "0");

            var questionAnswerDto = new AnswerDto
            {
                QuestionId = kbqViewModel.QuestionId,
                UserQuestionAnswerId = kbqViewModel.QuestionAnswerId,
                Content = kbqViewModel.Content,
            };

            string message="";

            var validateResult = _questionAnswerService.ValidateUserKbqToUpdate(userProfileId, questionAnswerDto);
            if (validateResult == RegistrationResult.MissingKBQAnswer)
            {
                message = "Question answer cannot be empty.";
            }
            else if (validateResult == RegistrationResult.DuplicatedKBQ)
            {
                message = "Question cannot be duplicated with others.";
            }
            else if (validateResult == RegistrationResult.DuplicatedKBQAnswer)
            {
                message = "Question answer cannot be duplicated with others.";
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                return Json(data:new
                {
                    result = "false",
                    message = message
                });
            }

            _questionAnswerService.UpdateAnswer(questionAnswerDto);
            
            var result = new
            {
                result ="true",
                message ="Knowledge Based Question updated successfully."
            };

            return Json(data:result); 
        }
        private bool NeedToValidKbq()
        {
            var previousUri = HttpContext.Request.UrlReferrer;
            if (previousUri == null)
            {
                return true;
            }

            if (previousUri.AbsolutePath.ToLower().IndexOf(value:"account/ChangeEmail", comparisonType:StringComparison.OrdinalIgnoreCase) >= 0
                || previousUri.AbsolutePath.ToLower().IndexOf(value:"account/ChangePassword", comparisonType:StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var kbqPass = TempData[key:"KbqPass"] as string;
                return string.IsNullOrWhiteSpace(value:kbqPass) || kbqPass.ToLower() != "true";
            }

            return previousUri.AbsolutePath.ToLower().IndexOf(value:"account/changeaccountsucceed", comparisonType:StringComparison.Ordinal) < 0;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if (filterContext.RouteData.Values[key:"action"].ToString().ToLower() == "userprofile"
            //    && Request.HttpMethod != "POST"
            //    && NeedToValidKbq())
            //{
            //    var kbqPass = TempData[key:"KbqPass"] as string;
            //    if (!string.IsNullOrWhiteSpace(value:kbqPass) && kbqPass.ToLower() == "true")
            //    {
            //        base.OnActionExecuting(filterContext:filterContext);
            //    }
            //    else
            //    {
            //        filterContext.Result = new RedirectToRouteResult(
            //                                                         routeValues:new RouteValueDictionary
            //                                                                     {
            //                                                                         {"action", "KbqChallenge"},
            //                                                                         {"controller", "Account"},
            //                                                                         {"returnUrl", filterContext.HttpContext.Request.Url}
            //                                                                     }
            //                                                        );
            //    }
            //}
            //else
            {
                base.OnActionExecuting(filterContext:filterContext);
            }
        }

        private ActionResult SaveUserProfile(UserViewModel model, UserViewModel pristineUserModel, int userProfileId)
        {
            var validationResult = new List<ValidationResult>();

            var context = new ValidationContext(instance:model.UserProfile, serviceProvider:null, items:null);
            var isValid = Validator.TryValidateObject(instance:model.UserProfile, validationContext:context, validationResults:validationResult, validateAllProperties:true);

            if (!isValid)
            {
                ViewBag.inValidProfile = true;
                return View(model:pristineUserModel);
            }

            var userDto = _mapHelper.GetUserDtoFromUserProfileViewModel(viewModel:model.UserProfile);
            userDto.UserProfileId = userProfileId;

            var validateResult = _userService.ValidateUserProfileData(userProfile:userDto);
            if (validateResult == RegistrationResult.Success)
            {
                _userService.UpdateProfile(dto:userDto);
                ViewBag.SaveProfileSuccessfull = true;
                ViewBag.SuccessMessage = "Save Profile successfully.";
            }
            else
            {
                ModelState.AddModelError(key:string.Empty, errorMessage:@"User profile data is not correct.");
                ViewBag.inValidKBQ = true;
            }

            return View(model:pristineUserModel);
        }

        private ActionResult SaveUserKbq(UserViewModel model, UserViewModel pristineUserModel, int userProfileId)
        {
            pristineUserModel.UserKBQ.QuestionPool = _profileHelper.GetQuestionPool(type:QuestionTypeName.KBQ);

            var validationResult = new List<ValidationResult>();

            var context = new ValidationContext(instance:model.UserKBQ, serviceProvider:null, items:null);
            var isValid = Validator.TryValidateObject(instance:model.UserKBQ, validationContext:context, validationResults:validationResult, validateAllProperties:true);

            if (!isValid)
            {
                ViewBag.inValidKBQ = true;
                return View(model:pristineUserModel);
            }

            pristineUserModel.UserKBQ.UserProfileId = userProfileId;
            var kbqQuestionAnswers = GetPostedUserKbqQuestions(model:model.UserKBQ);
            var validateResult = _questionAnswerService.ValidateUserKbqData(kbqQuestions:kbqQuestionAnswers);
            switch (validateResult)
            {
                case RegistrationResult.Success:
                    _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId:userProfileId, questionAnswers:kbqQuestionAnswers);
                    ViewBag.SaveKBQSuccessfull = true;
                    ViewBag.SuccessMessage = "Save Knowledge Based Questions successfully.";

                    //Reload the view model's kbq questions from database to handle potential new UserQuestionAnswer id's
                    pristineUserModel.UserKBQ = _profileHelper.GetUserKbqViewModel(userProfileId:userProfileId);

                    break;
                case RegistrationResult.DuplicatedKBQ:
                    ModelState.AddModelError(key:string.Empty, errorMessage:@"Duplicated Knowledge Based Questions.");
                    ViewBag.inValidKBQ = true;
                    break;
                case RegistrationResult.DuplicatedKBQAnswer:
                    ModelState.AddModelError(key:string.Empty, errorMessage:@"Duplicated Knowledge Based Question Answers.");
                    ViewBag.inValidKBQ = true;
                    break;
                case RegistrationResult.MissingKBQ:
                    ModelState.AddModelError(key:string.Empty, errorMessage:@"Missing Knowledge Based Questions.");
                    ViewBag.inValidKBQ = true;
                    break;
                case RegistrationResult.MissingKBQAnswer:
                    ModelState.AddModelError(key:string.Empty, errorMessage:@"Missing Knowledge Based Question Answers.");
                    ViewBag.inValidKBQ = true;
                    break;
            }

            return View(model:pristineUserModel);
        }

        private ActionResult SaveUserSq(UserViewModel model, UserViewModel pristineUserModel, int userProfileId)
        {
            pristineUserModel.UserSQ.QuestionPool = _profileHelper.GetQuestionPool(type:QuestionTypeName.SQ);

            var validationResult = new List<ValidationResult>();

            var context = new ValidationContext(instance:model.UserSQ, serviceProvider:null, items:null);
            var isValid = Validator.TryValidateObject(instance:model.UserSQ, validationContext:context, validationResults:validationResult, validateAllProperties:true);

            if (!isValid)
            {
                ViewBag.inValidSQ = true;
                return View(model:pristineUserModel);
            }

            pristineUserModel.UserSQ.UserProfileId = userProfileId;

            var sqQuestionAnswers = GetPostedUserSQQuestionAnswers(model:model.UserSQ);
            var result = _questionAnswerService.ValidateUserSqData(securityQuestions:sqQuestionAnswers);
            switch (result)
            {
                case RegistrationResult.Success:
                    _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId:userProfileId, questionAnswers:sqQuestionAnswers);
                    ViewBag.SaveSQSuccessfull = true;
                    ViewBag.SuccessMessage = "Save Security Questions successfully.";

                    //Reload the view model's security questions from database to handle potential new UserQuestionAnswer id's
                    pristineUserModel.UserSQ = _profileHelper.GetUserSecurityQuestionViewModel(userProfileId:userProfileId);

                    break;

                case RegistrationResult.DuplicatedSecurityQuestion:
                    ModelState.AddModelError(key:string.Empty, errorMessage:@"Duplicated Security Questions.");
                    ViewBag.inValidSQ = true;
                    break;
                case RegistrationResult.DuplicatedSecurityQuestionAnswer:
                    ModelState.AddModelError(key:string.Empty, errorMessage:@"Duplicated Security Question Answers.");
                    ViewBag.inValidSQ = true;
                    break;

                case RegistrationResult.MissingSecurityQuestion:
                    ModelState.AddModelError(key:string.Empty, errorMessage:@"Missing Security Questions.");
                    ViewBag.inValidSQ = true;
                    break;
                case RegistrationResult.MissingSecurityQuestionAnswer:
                    ModelState.AddModelError(key:string.Empty, errorMessage:@"Missing Security Question Answers.");
                    ViewBag.inValidSQ = true;
                    break;
            }

            return View(model:pristineUserModel);
        }

        private List<AnswerDto> GetPostedUserSQQuestionAnswers(UserSQViewModel model)
        {
            var sqQuestionAnswers = new List<AnswerDto>();
            sqQuestionAnswers.Add(item:new AnswerDto
                                       {
                                           QuestionId = model.SecurityQuestion1,
                                           Content = model.SecurityQuestionAnswer1,
                                           UserQuestionAnswerId = model.UserQuestionAnserId_SQ1
                                       });

            sqQuestionAnswers.Add(item:new AnswerDto
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
                                        collection:new[]
                                                   {
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