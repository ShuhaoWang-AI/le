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

namespace Linko.LinkoExchange.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthenticationService _authenticateService;
        private readonly ISessionCache _sessionCache;
        private readonly IUserService _userService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IJurisdictionService _jurisdictionService;
        private string FakePassword = "********";
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
        public ActionResult Profile()
        {
            var userProfileViewModel = GetUserProfileViewModel();

            return View(userProfileViewModel);
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)] 
        [ValidateAntiForgeryToken]
        public ActionResult Profile(UserProfileViewModel model, FormCollection form)
        {
            model.QuestionPool = GetQuestionPool();
            model.StateList = GetStateList();

            if (!ModelState.IsValid)
            {
                var errors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
                return View(model);
            }

            #region Create Question Answer Dto 

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
                UserQuestionAnswerId = model.UserQuestionAnserId_SQ1
            });

            #endregion create Question Answer Dto  

            var profileIdStr = _sessionCache.GetClaimValue(CacheKey.UserProfileId) as string;
            var userProfileId = int.Parse(profileIdStr);
            var userProileDto = _userService.GetUserProfileById(userProfileId);

            model.UserProfileId = userProfileId; 

            var userDto = _mapper.Map<UserDto>(model);  
             
            var result = _userService.UpdateProfile(userDto, sqQuestionAnswers, kbqQuestionAnswers);
            switch (result)
            {
                case RegistrationResult.BadSecurityQuestionAndAnswer:
                    ModelState.AddModelError(string.Empty, "Bad Security Question and Anwsers.");
                    break;
                case RegistrationResult.DuplicatedKBQ:
                    ModelState.AddModelError(string.Empty, "Duplicated Knowledage Based Questions");
                    break;
                case RegistrationResult.DuplicatedKBQAnswer:
                    ModelState.AddModelError(string.Empty, "Duplicated Knowledage Based Question Answers");
                    break;
                case RegistrationResult.DuplicatedSecurityQuestion:
                    ModelState.AddModelError(string.Empty, "Duplicated Security Questions");
                    break;
                case RegistrationResult.DuplicatedSecurityQuestionAnswer:
                    ModelState.AddModelError(string.Empty, "Duplicated Security Question Answers");
                    break;
                case RegistrationResult.MissingKBQ:
                    ModelState.AddModelError(string.Empty, "Missing Knowledage Based Questions");
                    break;
                case RegistrationResult.MissingKBQAnswer:
                    ModelState.AddModelError(string.Empty, "Missing Knowledage Based Question Answers");
                    break;
                case RegistrationResult.MissingSecurityQuestion:
                    ModelState.AddModelError(string.Empty, "Missing Security Question Answers");
                    break;
                case RegistrationResult.BadUserProfileData:
                    ModelState.AddModelError(string.Empty, "Bad User Profile Data");
                    break;

            }
             
            return View(model);
        }

        private UserProfileViewModel GetUserProfileViewModel()
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity; 
            var profileIdStr = claimsIdentity.Claims.First(i => i.Type == CacheKey.UserProfileId).Value;
            var userProfileId = int.Parse(profileIdStr);
            var userProileDto = _userService.GetUserProfileById(userProfileId);
            var userProfileViewModel = _mapper.Map<UserProfileViewModel>(userProileDto);

            // set password to be stars 
            userProfileViewModel.Password = FakePassword;

            // Get state list   
            userProfileViewModel.StateList = GetStateList();

            userProfileViewModel.Role = _sessionCache.GetClaimValue(CacheKey.UserRole); 

            var kbqQuestions = _questionAnswerService.GetUsersQuestionAnswers(userProfileId, Services.Dto.QuestionType.KnowledgeBased);
            var securityQeustions = _questionAnswerService.GetUsersQuestionAnswers(userProfileId, Services.Dto.QuestionType.Security);

            var kbqs = kbqQuestions.Select(i => _mapper.Map<QuestionAnswerPairViewModel>(i)).ToList();
            var sqs = securityQeustions.Select(i => _mapper.Map<QuestionAnswerPairViewModel>(i)).ToList();

            userProfileViewModel.QuestionPool = GetQuestionPool();

            ////  Security questions 
            userProfileViewModel.SecuritryQuestion1 = sqs[0].Question.QuestionId.Value;
            userProfileViewModel.SecurityQuestion2 = sqs[1].Question.QuestionId.Value;

            userProfileViewModel.SecurityQuestionAnswer2 = sqs[1].Answer.Content;
            userProfileViewModel.SecurityQuestionAnswer1 = sqs[0].Answer.Content;

            //// Keep track UserQuestionAnswerId 
            userProfileViewModel.UserQuestionAnserId_SQ1 = sqs[0].Answer.UserQuestionAnswerId.Value;
            userProfileViewModel.UserQuestionAnserId_SQ2 = sqs[1].Answer.UserQuestionAnswerId.Value;


            ////  KBQ questions 
            userProfileViewModel.KBQ1 = kbqs[0].Question.QuestionId.Value;
            userProfileViewModel.KBQ2 = kbqs[1].Question.QuestionId.Value;
            userProfileViewModel.KBQ3 = kbqs[2].Question.QuestionId.Value;
            userProfileViewModel.KBQ4 = kbqs[3].Question.QuestionId.Value;
            userProfileViewModel.KBQ5 = kbqs[4].Question.QuestionId.Value;


            userProfileViewModel.KBQAnswer1 = kbqs[0].Answer.Content;
            userProfileViewModel.KBQAnswer2 = kbqs[1].Answer.Content; 
            userProfileViewModel.KBQAnswer3 = kbqs[2].Answer.Content; 
            userProfileViewModel.KBQAnswer4 = kbqs[3].Answer.Content; 
            userProfileViewModel.KBQAnswer5 = kbqs[4].Answer.Content;

            //// keep track UserQuestionAnswerId
            userProfileViewModel.UserQuestionAnserId_KBQ1 = kbqs[0].Answer.UserQuestionAnswerId.Value;
            userProfileViewModel.UserQuestionAnserId_KBQ2 = kbqs[1].Answer.UserQuestionAnswerId.Value;
            userProfileViewModel.UserQuestionAnserId_KBQ3 = kbqs[2].Answer.UserQuestionAnswerId.Value;
            userProfileViewModel.UserQuestionAnserId_KBQ4 = kbqs[3].Answer.UserQuestionAnswerId.Value;
            userProfileViewModel.UserQuestionAnserId_KBQ5 = kbqs[4].Answer.UserQuestionAnswerId.Value; 

            return userProfileViewModel;
        }

        public List<QuestionViewModel> GetQuestionPool()
        {
           return _questionAnswerService.GetQuestions().Select(i => _mapper.Map<QuestionViewModel>(i)).ToList();
        }

        public List<JurisdictionViewModel> GetStateList()
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